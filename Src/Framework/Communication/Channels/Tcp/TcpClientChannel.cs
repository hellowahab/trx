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
    public class TcpClientChannel : TcpBaseSenderReceiverChannel, IClientChannel
    {
        private AddressFamily _addressFamily = AddressFamily.InterNetwork;
        protected ChannelRequestCtrl CurrentConnectAttempt;
        private ChannelRequestCtrl _lastSuccessfulConnect;
        private string _localInterface = "0.0.0.0";
        private int _localPort;
        private string _remoteInterface;
        private int _remotePort;

        /// <summary>
        ///   Builds a channel to send messages.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        public TcpClientChannel(Pipeline pipeline)
            : base(pipeline)
        {
        }

        /// <summary>
        ///   Buils a channel to send and receive messages.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        /// <param name = "tupleSpace">
        ///   Tuple space to store received messages.
        /// </param>
        public TcpClientChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
        }

        /// <summary>
        ///   Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        /// <param name = "tupleSpace">
        ///   Tuple space to store received messages.
        /// </param>
        /// <param name = "messagesIdentifier">
        ///   Messages identifier to compute keys to match requests with responses.
        /// </param>
        public TcpClientChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        ///   Remote port to connect to.
        /// </summary>
        public int RemotePort
        {
            get { return _remotePort; }

            set
            {
                if (_remotePort == value)
                    return;

                _remotePort = value;
                RemoteEndPoint = null;
            }
        }

        /// <summary>
        ///   It returns or sets the name or IP address of the remote host to connect to.
        /// </summary>
        public string RemoteInterface
        {
            get { return _remoteInterface; }

            set
            {
                if (_remoteInterface == value)
                    return;

                _remoteInterface = value;
                RemoteEndPoint = null;
            }
        }

        /// <summary>
        ///   Local port to connect from, if 0 the system will automatically assign one.
        /// </summary>
        public int LocalPort
        {
            get { return _localPort; }

            set
            {
                if (_localPort == value)
                    return;

                _localPort = value;
                LocalEndPoint = null;
            }
        }

        /// <summary>
        ///   It returns or sets the name or IP address of the interface
        ///   over which connection is binded.
        /// </summary>
        public string LocalInterface
        {
            get { return _localInterface; }

            set
            {
                if (_localInterface == value)
                    return;

                _localInterface = value;
                LocalEndPoint = null;
            }
        }

        /// <summary>
        ///   Address family to use.
        /// </summary>
        public AddressFamily AddressFamily
        {
            get { return _addressFamily; }
            set { _addressFamily = value; }
        }

        /// <summary>
        ///   Local IP end point.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; internal set; }

        /// <summary>
        ///   Remote IP end point.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; internal set; }

        /// <summary>
        ///   Starts the connection of the channel.
        /// </summary>
        /// <returns>
        ///   A connection request handler.
        /// </returns>
        public ChannelRequestCtrl Connect()
        {
            CheckDisposed();

            if (IsConnected)
                return _lastSuccessfulConnect;

            lock (SyncRoot)
            {
                if (IsConnected)
                    return _lastSuccessfulConnect;

                if (CurrentConnectAttempt != null)
                    // A connection attempt is active, return the same channel control operation.
                    return CurrentConnectAttempt;

                _lastSuccessfulConnect = null;

                try
                {
                    if (LocalEndPoint == null)
                    {
                        // Resolve local end point with LocalPort and LocalInterface properties.
                        if (_localInterface == null)
                            throw new ChannelException("Invalid local interface: null.");

                        if (_localPort != 0 && !NetUtilities.IsValidTcpPort(_localPort))
                            throw new ChannelException(string.Format("Invalid local port number {0}.", _localPort));

                        IPAddress addr;
                        LocalEndPoint = IPAddress.TryParse(_localInterface, out addr)
                            ? new IPEndPoint(addr, _localPort)
                            : new IPEndPoint(ResolveHostEntry(GetChannelTitle(), _localInterface, AddressFamily),
                                _localPort);
                    }

                    Socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    // Socket will linger for 10 seconds after close is called.
                    var lingerOption = new LingerOption(true, 10);
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, -1);

                    Socket.Bind(LocalEndPoint);

                    if (RemoteEndPoint == null)
                    {
                        // Resolve remote end point with RemotePort and RemoteInterface properties.
                        if (_remoteInterface == null)
                            throw new ChannelException("Invalid remote interface: null.");

                        if (!NetUtilities.IsValidTcpPort(_remotePort))
                            throw new ChannelException(string.Format("Invalid remote port number {0}.", _remotePort));

                        IPAddress addr;
                        RemoteEndPoint = IPAddress.TryParse(_remoteInterface, out addr)
                            ? new IPEndPoint(addr, _remotePort)
                            : new IPEndPoint(ResolveHostEntry(GetChannelTitle(), _remoteInterface, AddressFamily),
                                _remotePort);
                    }

                    // Use local variable because sometimes BeginConnect calls it's callback in the calling
                    // thread (mainly fast connections like localhost i guess)
                    var ctrl = new ChannelRequestCtrl();
                    CurrentConnectAttempt = ctrl;

                    if (LocalEndPoint.AddressFamily != RemoteEndPoint.AddressFamily)
                        throw new ChannelException(string.Format("{0}: incompatible local address family type {1} " +
                            "with remote address family type {2}, please specify the same type in both end points.",
                            GetChannelTitle(), LocalEndPoint.AddressFamily, RemoteEndPoint.AddressFamily));

                    Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.ConnectionRequested),
                        true, Logger);

                    Logger.Info(string.Format("{0}: connecting to {1} from local end point {2}.", GetChannelTitle(),
                        RemoteEndPoint, LocalEndPoint));

                    Socket.BeginConnect(RemoteEndPoint, AsyncConnectionRequestHandler, Socket);

                    return ctrl;
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        string.Format("{0}: error caught trying to setup connection attempt.", GetChannelTitle()), ex);
                    IsConnected = false;
                    CurrentConnectAttempt = null;
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }
            }
        }

        protected virtual void OnSocketConnection()
        {
            IsConnected = true;
            _lastSuccessfulConnect = CurrentConnectAttempt;

            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true,
                Logger);

            CurrentConnectAttempt.MarkAsCompleted(true);
            CurrentConnectAttempt = null;

            Logger.Info(string.Format("{0}: connected to {1} from local end point {2}.", GetChannelTitle(),
                Socket.RemoteEndPoint, Socket.LocalEndPoint));
        }

        protected void OnSocketConnectionException(Exception ex)
        {
            Pipeline.ProcessChannelEvent(PipelineContext,
                new ChannelEvent(ChannelEventType.ConnectionFailed),
                true, Logger);

            if (CurrentConnectAttempt != null)
            {
                CurrentConnectAttempt.MarkAsCompleted(false);
                CurrentConnectAttempt.Error = ex;
                CurrentConnectAttempt = null;
            }

            if (ex is SocketException)
                LogSocketException((SocketException) ex);
            else
                Logger.Error(string.Format("{0}: exception caught handling asynchronous connect.",
                    GetChannelTitle()), ex);
        }

        protected virtual void OnSuccesfulSocketConnection()
        {
            try
            {
                StartAsyncReceive();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("{0}: exception caught handling asynchronous connect.",
                    GetChannelTitle()), ex);
                OnDisconnection();
            }
        }

        private void AsyncConnectionRequestHandler(IAsyncResult asyncResult)
        {
            lock (SyncRoot)
            {
                var asyncSocket = asyncResult.AsyncState as Socket;

                if (asyncSocket == null || !ReferenceEquals(asyncSocket, Socket))
                    // Someone called Close over socket.
                    return;

                try
                {
                    Socket.EndConnect(asyncResult);
                    OnSocketConnection();
                }
                catch (Exception ex)
                {
                    OnSocketConnectionException(ex);
                    return;
                }

                OnSuccesfulSocketConnection();
            }
        }
    }
}