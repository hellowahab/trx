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
using System.Reflection;
using System.Threading;
using Trx.Messaging.Channels;
using Trx.Utilities;
using log4net;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// This class implements a server capable of accepting connection requests
    /// that use TCP/IP protocol.
    /// Each new connection is encapsulated in an new object that is or inherits
    /// of <see cref="Trx.Messaging.Channels.TcpChannel"/>.
    /// </summary>
    public class TcpListener : IListener
    {
        private IChannelPool _channelPool;
        private bool _listening;
        private IPEndPoint _localEndPoint;
        private string _localInterface = "0.0.0.0";
        private ILog _logger;
        private string _name;
        private int _port;
        private Socket _socket;

        /// <summary>
        /// Initialize a new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        public TcpListener()
        {
            _localInterface = "localhost";
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="TcpListener"/> class.
        /// </summary>
        /// <param name="port">
        /// It's the local port where the connection requests will be listened.
        /// </param>
        public TcpListener(int port)
        {
            _localInterface = "localhost";

            if (!NetUtilities.IsValidTcpPort(port))
                throw new ArgumentOutOfRangeException("port");

            _port = port;
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="TcpListener"/> class,
        /// ready to listen requests over a given IP address and port.
        /// </summary>
        /// <param name="localAddress">
        /// It's the local address where the connection requests will be listened.
        /// </param>
        /// <param name="port">
        /// It's the local port where the connection requests will be listened.
        /// </param>
        public TcpListener(IPAddress localAddress, int port)
        {
            if (localAddress == null)
                throw new ArgumentNullException("localAddress");

            if (!NetUtilities.IsValidTcpPort(port))
                throw new ArgumentOutOfRangeException("port");

            CreateServerSocket();
        }

        /// <summary>
        /// It returns the logger used by the class.
        /// </summary>
        public ILog Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType));
            }

            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        MethodBase.GetCurrentMethod().DeclaringType);
                else
                    _logger = value;
            }
        }

        /// <summary>
        /// It returns the logger name used by the class.
        /// </summary>
        public string LoggerName
        {
            set {
                Logger = string.IsNullOrEmpty(value) ? null : LogManager.GetLogger(value);
            }

            get { return Logger.Logger.Name; }
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
                if (_port != value)
                {
                    _port = value;
                    _localEndPoint = null;
                }
            }
        }

        /// <summary>
        /// It returns or sets the channel name.
        /// </summary>
        public string Name
        {
            get { return _name; }

            set { _name = value; }
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
                if (_localInterface != value)
                {
                    _localInterface = value;
                    _localEndPoint = null;
                }
            }
        }

        /// <summary>
        /// It indicates if the channel is waiting for connection requests.
        /// </summary>
        public bool Listening
        {
            get { return _listening; }
        }

        /// <summary>
        /// It returns or sets the pool of channels from which the listener
        /// gets the channels that associates with new connections.
        /// </summary>
        public IChannelPool ChannelPool
        {
            get { return _channelPool; }

            set { _channelPool = value; }
        }

        /// <summary>
        /// Occurs when a connection request arrives. It allows to choose
        /// if the connection will be accepted or rejected.
        /// </summary>
        public event ListenerConnectionRequestEventHandler ConnectionRequest;

        /// <summary>
        /// Occurs when a connection has been created.
        /// </summary>
        public event ListenerConnectedEventHandler Connected;

        /// <summary>
        /// Occurs when an error has been taken in the internal listener's
        /// processing.
        /// </summary>
        /// <remarks>
        /// This event is received from the listener when a taken error has
        /// disabled it, it's necessary to call <see cref="Start"/>
        /// to enable it again.
        /// </remarks>
        public event ListenerErrorEventHandler Error;

        /// <summary>
        /// Start to listen connection requests.
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (_listening)
                    return;

                if (_localEndPoint == null)
                {
                    // Local end point unspecified, try to solve it trought
                    // Port and LocalInterface properties.
                    if (_localInterface == null)
                        throw new MessagingException("Invalid local interface.");

                    if (!NetUtilities.IsValidTcpPort(_port))
                        throw new MessagingException("Invalid port number.");

                    IPAddress addr;
                    if (IPAddress.TryParse(_localInterface, out addr))
                        // Create server socket, over the first located address.
                        _localEndPoint = new IPEndPoint(addr, _port);
                    else
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(_localInterface);

                        if (hostEntry.AddressList.Length == 0)
                            throw new MessagingException("Can't resolve local interface name.");

                        // Create server socket, over the first located address.
                        _localEndPoint = new IPEndPoint(hostEntry.AddressList[0], _port);
                    }
                }

                if (_socket == null)
                    CreateServerSocket();

                // Bind and start listening.
                if (_socket != null)
                {
                    _socket.Bind(_localEndPoint);
                    _socket.Listen(10);
                }

                _listening = true;

                // Start an asynchronous Accept operation.
                if (_socket != null)
                    _socket.BeginAccept(AsyncAcceptRequestHandler, _socket);
            }
        }

        /// <summary>
        /// Finish to listen connection requests.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (!_listening)
                    return;

                _listening = false;

                if (_socket != null)
                {
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

        /// <summary>
        /// It informs if there are pending request connections.
        /// </summary>
        /// <returns>
        /// Returns true if there are pending request connections,
        /// otherwise returns false.
        /// </returns>
        public bool Pending()
        {
            lock (this)
            {
                if (!_listening)
                    throw new InvalidOperationException("The server isn't started.");

                return _socket.Poll(0, SelectMode.SelectRead);
            }
        }

        /// <summary>
        /// It fires the <see cref="Error"/> event.
        /// </summary>
        /// <param name="exception">
        /// It's the exception produced by the error.
        /// </param>
        private void OnError(Exception exception)
        {
            if (Logger.IsErrorEnabled)
                Logger.Error(exception);

            if (Error != null)
                Error(this, new ErrorEventArgs(exception));
        }

        /// <summary>
        /// Fires the <see cref="ConnectionRequest"/> event.
        /// </summary>
        private bool OnConnectionRequest(EndPoint remoteEndPoint)
        {
            bool accept = true;

            if (ConnectionRequest != null)
            {
                var eventArgs =
                    new ListenerConnectionRequestEventArgs(remoteEndPoint);

                ConnectionRequest(this, eventArgs);

                accept = eventArgs.Accept;
            }

            return accept;
        }

        /// <summary>
        /// Fires the <see cref="Connected"/> event.
        /// </summary>
        /// <param name="channel">
        /// It's the accepted channel.
        /// </param>
        private void OnConnected(IChannel channel)
        {
            if (Connected != null)
                Connected(this, new ListenerConnectedEventArgs(channel));
        }

        /// <summary>
        /// This method creates the socket that is going to listen
        /// over a given interface and port.
        /// </summary>
        private void CreateServerSocket()
        {
            _socket = new Socket(_localEndPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Socket will linger for 10 seconds after close is called.
            var lingerOption = new LingerOption(true, 10);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, -1);

            _port = _localEndPoint.Port;
            _localInterface = _localEndPoint.Address.ToString();
        }

        private string GetNdcName()
        {
            string ndcName = string.Empty;

            if (_name == null)
            {
                if (_localEndPoint != null)
                    ndcName = _localEndPoint.ToString();
            }
            else if (_localEndPoint == null)
                ndcName = _name;
            else
                ndcName = string.Format("{0}-{1}", _name, _localEndPoint);

            return ndcName;
        }

        /// <summary>
        /// It's the handler of the asynchronous reading.
        /// </summary>
        /// <param name="asyncResult">
        /// It is the result of the asynchronous reading.
        /// </param>
        private void AsyncAcceptRequestHandler(IAsyncResult asyncResult)
        {
            if (asyncResult.AsyncState == _socket)
            {
                Socket socket = null;
                lock (this)
                {
                    if (asyncResult.AsyncState == _socket)
                    {
                        if (!_listening)
                        {
                            // Someone called Close over socket. EndAccept and return.
                            try
                            {
                                _socket.EndAccept(asyncResult);
                            }
                            catch
                            {
                            }
                            Monitor.Pulse(this);
                            return;
                        }

                        try
                        {
                            // Accept connection.
                            socket = _socket.EndAccept(asyncResult);
                        }
                        catch (Exception e)
                        {
                            OnError(e);
                            return;
                        }

                        try
                        {
                            // Start an asynchronous accept operation.
                            _socket.BeginAccept(AsyncAcceptRequestHandler, _socket);
                        }
                        catch (Exception e)
                        {
                            Stop();
                            OnError(e);
                        }
                    }
                }

                if (socket == null)
                    return;

                IChannel channel = null;
                try
                {
                    NDC.Push(socket.RemoteEndPoint == null
                        ? string.Format("{0}<-?", GetNdcName())
                        : string.Format("{0}<-{1}", GetNdcName(), socket.RemoteEndPoint));

                    if ((_channelPool != null) &&
                        OnConnectionRequest(socket.RemoteEndPoint))
                    {
                        channel = _channelPool.Remove();
                        if (channel == null)
                        {
                            if (Logger.IsInfoEnabled)
                                Logger.Info(string.Format(
                                    "Connection request from {0}, not accepted because channel pool is empty.",
                                    socket.RemoteEndPoint));

                            // Can't get a channel for the new connection, close it.
                            socket.Shutdown(SocketShutdown.Both);
                            socket = null;
                        }
                        else
                            channel.BeginBind(socket);
                    }
                    else
                    {
                        if (Logger.IsInfoEnabled)
                            if (_channelPool == null)
                                Logger.Info(string.Format(
                                    "Connection request from {0}, not accepted because channel pool is null.",
                                    socket.RemoteEndPoint));
                            else
                                Logger.Info(string.Format(
                                    "Connection request from {0}, not accepted.",
                                    socket.RemoteEndPoint));

                        // Connection not accepted, close it.
                        socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        OnError(e);

                        if (socket != null)
                            socket.Shutdown(SocketShutdown.Both);

                        // Return channel to the pool.
                        if (channel != null)
                        {
                            _channelPool.Add(channel);
                            channel = null;

                            if (Logger.IsInfoEnabled)
                                Logger.Info(string.Format(
                                    "Channel returned to pool, {0} channel/s remaining in channel pool.",
                                    _channelPool.Length));
                        }
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    NDC.Pop();
                }

                if (channel != null)
                    try
                    {
                        NDC.Push(string.Format("{0}<-{1}", GetNdcName(), socket.RemoteEndPoint));

                        // It is necessary first to raise the event, and soon the EndBind. Doing this
                        // the components that receive the event can initialize before the channel
                        // begins to receive.
                        OnConnected(channel);
                        channel.EndBind();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            channel.Close();

                            OnError(e);

                            // Return channel to the pool.
                            _channelPool.Add(channel);

                            if (Logger.IsInfoEnabled)
                                Logger.Info(string.Format(
                                    "Channel returned to pool, {0} channel/s remaining in channel pool.",
                                    _channelPool.Length));
                        }
                        catch
                        {
                        }
                    }
                    finally
                    {
                        NDC.Pop();
                    }
            }
        }
    }
}