#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
//
// Trx Framework - <http://www.trxframework.org/>
// Copyright (C) 2004-2012  Zabaleta Asociados SRL
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using System;
using System.Net;
using System.Net.Sockets;
using Trx.Coordination.TupleSpace;
using Trx.Utilities;

namespace Trx.Communication.Channels.Tcp
{
    public class TcpServerChannel : BaseServerChannel
    {
        private AddressFamily _addressFamily = AddressFamily.InterNetwork;
        private bool _isListening;
        private ChannelRequestCtrl _lastSuccessfulStartListening;
        private IPEndPoint _localEndPoint;
        private string _localInterface = "0.0.0.0";
        private int _port;
        private int _sendMaxRequestSize = TcpBaseSenderReceiverChannel.DefaultSendMaxRequestSize;
        private Socket _socket;

        /// <summary>
        /// Builds a server channel with childs intended to send messages only.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        public TcpServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory)
            : base(pipeline, pipelineFactory)
        {
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        public TcpServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, pipelineFactory, tupleSpace)
        {
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier used by childs to compute keys to match requests with responses.
        /// </param>
        public TcpServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier)
            : base(pipeline, pipelineFactory, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        /// True if the server channel is listening for new connections. Otherwise false.
        /// </summary>
        public override bool IsListening
        {
            get { return _isListening; }
        }

        /// <summary>
        /// It returns or sets the port number over which connection
        /// requests are listened.
        /// </summary>
        public int Port
        {
            get { return _port; }

            set
            {
                if (_port == value)
                    return;

                _port = value;
                _localEndPoint = null;
            }
        }

        /// <summary>
        /// It returns or sets the name or IP address of the interface
        /// over which connection requests are listened.
        /// </summary>
        public string LocalInterface
        {
            get { return _localInterface; }

            set
            {
                if (_localInterface == value)
                    return;

                _localInterface = value;
                _localEndPoint = null;
            }
        }

        /// <summary>
        /// Local IP end point.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
        }

        /// <summary>
        /// Address family to use.
        /// </summary>
        public AddressFamily AddressFamily
        {
            get { return _addressFamily; }
            set { _addressFamily = value; }
        }

        /// <summary>
        /// Send maximum request size for childs.
        /// </summary>
        public int SendMaxRequestSize
        {
            get { return _sendMaxRequestSize; }
            set { _sendMaxRequestSize = value; }
        }

        protected string GetChannelTitle()
        {
            return Name ?? "TCP server channel";
        }

        /// <summary>
        /// Ask the current channel to start listening connections.
        /// </summary>
        /// <returns>
        /// The operation request control.
        /// </returns>
        public override ChannelRequestCtrl StartListening()
        {
            if (_isListening)
                return _lastSuccessfulStartListening;

            lock (SyncRoot)
            {
                if (_isListening)
                    return _lastSuccessfulStartListening;

                try
                {
                    if (_localEndPoint == null)
                    {
                        // Local end point unspecified, try to resolve it trought
                        // Port and LocalInterface properties.
                        if (_localInterface == null)
                            throw new ChannelException(string.Format("Invalid local interface: null."));

                        if (!NetUtilities.IsValidTcpPort(_port))
                            throw new ChannelException(string.Format("Invalid port number {0}.", _port));

                        IPAddress addr;
                        _localEndPoint = IPAddress.TryParse(_localInterface, out addr)
                            ? new IPEndPoint(addr, _port)
                            : new IPEndPoint(
                                TcpBaseSenderReceiverChannel.ResolveHostEntry(GetChannelTitle(), _localInterface,
                                    AddressFamily), _port);
                    }

                    _socket = new Socket(_localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // Socket will linger for 10 seconds after close is called.
                    var lingerOption = new LingerOption(true, 10);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, -1);

                    // Bind and start listening.
                    _socket.Bind(_localEndPoint);
                    _socket.Listen(10);

                    _isListening = true;
                    _lastSuccessfulStartListening = new ChannelRequestCtrl(true);

                    // Start an asynchronous Accept operation.
                    _socket.BeginAccept(AsyncAcceptRequestHandler, _socket);

                    Logger.Info(string.Format("{0}: accepting connections on local end point {1}.", GetChannelTitle(),
                        _socket.LocalEndPoint));

                    return _lastSuccessfulStartListening;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: cannot start listening.", GetChannelTitle()), ex);
                    StopListening();
                    _lastSuccessfulStartListening = null;
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }
            }
        }

        protected virtual IServerChildChannel CreateChild(Socket acceptedSocket)
        {
            TcpServerChildChannel child;
            string childName = string.Format("{0} child-{1}", GetChannelTitle(), NextChildId);
            if (TupleSpace == null)
                child = new TcpServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName, true)
                {
                    SendMaxRequestSize = _sendMaxRequestSize
                };
            else if (MessagesIdentifier == null)
                child = new TcpServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName,
                    TupleSpace, true)
                {
                    SendMaxRequestSize = _sendMaxRequestSize
                };
            else
                child = new TcpServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName,
                    TupleSpace, MessagesIdentifier, true)
                {
                    SendMaxRequestSize = _sendMaxRequestSize
                };

            if (!ReferenceEquals(ChannelAddress, ReferenceChannelAddress))
                // We have an addressing mechanism wich isn't the default. Create an address of the same kind
                // for the child.
                child.SetChannelAddressWithoutFiringEvent(ChannelAddress.GetAddress(child));

            child.LoggerName = LoggerName;
            child.Logger = Logger;

            return child;
        }

        private void AsyncAcceptRequestHandler(IAsyncResult asyncResult)
        {
            lock (SyncRoot)
            {
                var asyncSocket = asyncResult.AsyncState as Socket;
                if (asyncSocket == null || !ReferenceEquals(asyncSocket, _socket))
                    // Someone called Close over socket.
                    return;
                Socket acceptedSocket;
                try
                {
                    acceptedSocket = asyncSocket.EndAccept(asyncResult);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught ending asynchronous accept.", GetChannelTitle()),
                        ex);
                    return;
                }
                finally
                {
                    try
                    {
                        // Start an asynchronous accept operation.
                        _socket.BeginAccept(AsyncAcceptRequestHandler, _socket);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(
                            string.Format("{0}: exception caught starting and asynchronous accept.", GetChannelTitle()),
                            ex);
                        StopListening();
                        try
                        {
                            StartListening();
                        }
                        catch
                        {
                        }
                    }
                }

                try
                {
                    var evt = new SocketConnectionRequestChannelEvent(acceptedSocket);

                    Pipeline.ProcessChannelEvent(PipelineContext, evt, false, null);

                    if (!evt.Accept)
                    {
                        acceptedSocket.Shutdown(SocketShutdown.Both);
                        Logger.Info(
                            string.Format(
                                "{0}: connection request from {1} on local end point {2}, not accepted by the pipeline.",
                                GetChannelTitle(), acceptedSocket.RemoteEndPoint, acceptedSocket.LocalEndPoint));
                        return;
                    }

                    // Socket will linger for 10 seconds after close is called.
                    var lingerOption = new LingerOption(true, 10);
                    acceptedSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                    acceptedSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);

                    Logger.Info(string.Format("{0}: connection accepted from {1} on local end point {2}.",
                        GetChannelTitle(), acceptedSocket.RemoteEndPoint, acceptedSocket.LocalEndPoint));

                    ChildConnection(CreateChild(acceptedSocket));
                }
                catch (Exception ex)
                {
                    try
                    {
                        acceptedSocket.Shutdown(SocketShutdown.Both);
                        Logger.Error(
                            string.Format(
                                "{0}: exception caught creating child channel with remote end point {1} and local end point {2}.",
                                GetChannelTitle(), acceptedSocket.RemoteEndPoint, acceptedSocket.LocalEndPoint), ex);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Ask the current channel to stop listening new connections.
        /// </summary>
        public override void StopListening()
        {
            lock (SyncRoot)
            {
                _isListening = false;

                if (_socket == null)
                    return;

                try
                {
                    _socket.Close();
                }
                catch
                {
                }

                _socket = null;
            }
        }
    }
}