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
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Trx.Messaging.FlowControl;
using Trx.Utilities;
using log4net;

namespace Trx.Messaging.Channels
{
    /// <summary>
    /// This class implements a channel capable of interchanging messages
    /// with another system, using the TCP/IP communication protocol.
    /// </summary>
    public class TcpChannel : IFilteredChannel, IMessageSource, IDisposable
    {
        /// <summary>
        /// Default reconnection interval (in milliseconds).
        /// </summary>
        /// <remarks>
        /// When the reconnection interval has elapsed, and the channel
        /// doesn't have communications and must start a connection
        /// request, it does it.
        /// </remarks>
        public const int DEFAULT_RECONNECT_INTERVAL = 60000; // Default to one minute.

        /// <summary>
        /// Default innactivity reception interval (in milliseconds).
        /// </summary>
        /// <remarks>
        /// If in the indicated interval doesn't arrives data from
        /// the remote system, the channel hang up the connection and
        /// it tries to reconnect whether it's configurated to do so.
        /// </remarks>
        public const int DEFAULT_INACTIVITY_INTERVAL = 300000; // Five minutes.

        public const int DEFAULT_MAX_PACKET_LENGTH = 10240;
        private readonly IMessageFormatter _formatter;
        private readonly Timer _timer;
        private bool _connected;
        private int _currentReconnectInterval;

        private bool _disposed;
        private bool _enabled;
        private int _expectedDataLength;
        private bool _expectingDataLength;
        private AddressFamily _family;
        private FormatterContext _formatterContext;
        private string _hostName;
        private int _inactivityInterval;
        private IMessageFilter _incomingFilters;
        private DateTime _lastConnectionDateTime = DateTime.MinValue;
        private IPEndPoint _localEndPoint;
        private string _localInterface;
        private ILog _logger;
        private IMessageProcessor _messageProcessor;
        private string _name;
        private IMessageFilter _outgoingFilters;
        private ParserContext _parserContext;
        private int _port;
        private bool _reconnect;
        private int _reconnectInterval;
        private IPEndPoint _remoteEndPoint;
        private byte[] _sessionHeader;
        private Socket _socket;

        /// <summary>
        /// It initializes a new instance of <see cref="TcpChannel"/> class.
        /// </summary>
        /// <param name="formatter">
        /// It's the messages formatter to use.
        /// </param>
        public TcpChannel(IMessageFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            _socket = null;
            _reconnect = true;
            _enabled = false;
            _connected = false;
            _remoteEndPoint = null;
            _formatter = formatter;
            _hostName = null;
            _port = int.MinValue;
            _family = AddressFamily.InterNetwork;
            _currentReconnectInterval = 0;
            _reconnectInterval = DEFAULT_RECONNECT_INTERVAL;
            _inactivityInterval = DEFAULT_INACTIVITY_INTERVAL;
            _formatterContext = new FormatterContext(
                FormatterContext.DefaultBufferSize);
            _parserContext = new ParserContext(
                ParserContext.DefaultBufferSize);
            _expectingDataLength = false;
            _expectedDataLength = 0;
            _messageProcessor = null;
            _timer = new Timer(OnTimerTick, null,
                Timeout.Infinite, Timeout.Infinite);
            _disposed = false;
            _name = null;
            _logger = null;
            _incomingFilters = null;
            _outgoingFilters = null;
            _sessionHeader = null;
            MaxPacketLength = DEFAULT_MAX_PACKET_LENGTH;
        }

        /// <summary>
        /// It initializes a new instance of <see cref="TcpChannel"/> class,
        /// and tries to connect to the indicated host and port.
        /// </summary>
        /// <param name="formatter">
        /// It's the messages formatter to use.
        /// </param>
        /// <param name="hostName">
        /// It's the host's name to which it tries to connect.
        /// </param>
        /// <param name="port">
        /// It's the port number in the host to which it tries to connect.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// hostName is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// port isn't in the defined range by <see cref="IPEndPoint.MinPort"/>
        /// and <see cref="IPEndPoint.MaxPort"/>.
        /// </exception>
        public TcpChannel(IMessageFormatter formatter, string hostName,
            int port)
            : this(formatter)
        {
            if (hostName == null)
                throw new ArgumentNullException("hostName");

            if (!NetUtilities.IsValidTcpPort(port))
                throw new ArgumentOutOfRangeException("port");

            _hostName = hostName;
            _port = port;
        }

        /// <summary>
        /// It destroys the channel.
        /// </summary>
        ~TcpChannel()
        {
            Dispose(false);
        }

        /// <summary>
        /// Get or set session header.
        /// </summary>
        public string SessionHeader
        {
            get
            {
                if (_sessionHeader == null)
                    return null;

                return FrameworkEncoding.GetInstance().Encoding.GetString(_sessionHeader);
            }

            set {
                _sessionHeader = value == null ? null : FrameworkEncoding.GetInstance().Encoding.GetBytes(value);
            }
        }

        /// <summary>
        /// It sets or returns the flag indicating if the channel attempts a reconnection
        /// if the connection is lost.
        /// </summary>
        public bool Reconnect
        {
            get { return _reconnect; }

            set { _reconnect = value; }
        }

        /// <summary>
        /// Set session header, but can be specified in hex (i.e. 840 = 383430).
        /// </summary>
        public string HexadecimalSessionHeader
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    _sessionHeader = null;
                else
                {
                    _sessionHeader = new byte[(value.Length + 1) >> 1];

                    // Initialize result bytes.
                    for (int i = _sessionHeader.Length - 1; i >= 0; i--)
                        _sessionHeader[i] = 0;

                    // Format data.
                    for (int i = 0; i < value.Length; i++)
                        if (value[i] < 0x40)
                            _sessionHeader[(i >> 1)] |=
                                (byte) (((value[i]) - 0x30) << ((i & 1) == 1 ? 0 : 4));
                        else
                            _sessionHeader[(i >> 1)] |=
                                (byte) (((value[i]) - 0x37) << ((i & 1) == 1 ? 0 : 4));
                }
            }
        }

        /// <summary>
        /// It returns or sets the remote server name to which the channel
        /// tries to connect.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The channel is active, isn't possible to modify the property.
        /// </exception>
        public string HostName
        {
            get { return _hostName; }

            set
            {
                if (_enabled)
                    throw new InvalidOperationException("This channel is active.");

                _hostName = value;
            }
        }


        /// <summary>
        /// It returns or sets the local interface to which the channel
        /// is bound.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The channel is active, isn't possible to modify the property.
        /// </exception>
        public string LocalInterface
        {
            get { return _localInterface; }

            set
            {
                if (_enabled)
                    throw new InvalidOperationException("This channel is active.");

                _localInterface = value;
                _localEndPoint = null;
            }
        }

        /// <summary>
        /// It returns or sets the remote server port to which the channel
        /// tries to connect.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The channel is active, isn't possible to modify the property.
        /// </exception>
        public int Port
        {
            get { return _port; }

            set
            {
                if (_enabled)
                    throw new InvalidOperationException("This channel is active.");

                _port = value;
            }
        }

        /// <summary>
        /// It indicates whether the channel is enabled, I mean, whether it's
        /// connected or is trying to reconnect.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
        }

        /// <summary>
        /// It returns or sets the kind of family of the IP protocol the
        /// channel uses.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The channel is active, it's not possible to modify the property.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// It's attempted to assing a family other than
        /// <see cref="AddressFamily.InterNetwork"/> or
        /// <see cref="AddressFamily.InterNetworkV6"/>.
        /// </exception>
        public AddressFamily Family
        {
            get { return _family; }

            set
            {
                if (_enabled)
                    throw new InvalidOperationException("This channel is active.");

                if ((value != AddressFamily.InterNetwork) &&
                    (value != AddressFamily.InterNetworkV6))
                    throw new ArgumentException("Invalid family.", "value");

                _family = value;
            }
        }

        /// <summary>
        /// It returns or sets the interval waited by the channel
        /// before trying a reconnection to the remote host.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// It's attempted to assign an invalid value to the property.
        /// </exception>
        public int ReconnectInterval
        {
            get { return _reconnectInterval; }

            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "Must be equal or greater than zero.", "value");

                _reconnectInterval = value;
            }
        }

        /// <summary>
        /// It returns or sets the interval the channel waits to
        /// shutdown the connection with the remote host, if data
        /// hasn't been received in this period.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// It's attempted to assign an invalid value to the property.
        /// </exception>
        public int InactivityInterval
        {
            get { return _inactivityInterval; }

            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "Must be equal or greater than zero.", "value");

                _inactivityInterval = value;
            }
        }

        /*
         * Largo máximo del paquete que se puede procesar.
         */
        public int MaxPacketLength { get; set; }

        /// <summary>
        /// It returns the messages formatter used each time a
        /// message is sent.
        /// </summary>
        /// <remarks>
        /// If a message has a formatter, will be used at the moment
        /// to send it.
        /// </remarks>
        public IMessageFormatter Formatter
        {
            get { return _formatter; }
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
        /// It returns the name of the logger used by the class.
        /// </summary>
        public string LoggerName
        {
            set {
                Logger = string.IsNullOrEmpty(value) ? null : LogManager.GetLogger(value);
            }

            get { return Logger.Logger.Name; }
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
        /// It returns or sets the received messages processor.
        /// </summary>
        public IMessageProcessor MessageProcessor
        {
            get { return _messageProcessor; }

            set { _messageProcessor = value; }
        }

        /// <summary>
        /// It return the incoming filters of the messages.
        /// </summary>
        public IMessageFilter IncomingFilters
        {
            get { return _incomingFilters; }

            set { _incomingFilters = value; }
        }

        /// <summary>
        /// It return the outgoing filters of the messages.
        /// </summary>
        public IMessageFilter OutgoingFilters
        {
            get { return _outgoingFilters; }

            set { _outgoingFilters = value; }
        }

        /// <summary>
        /// It informs whether the channel is connected.
        /// </summary>
        public bool IsConnected
        {
            get { return _connected; }
        }

        /// <summary>
        /// It's fired when the channel has been connected.
        /// </summary>
        public event ChannelConnectedEventHandler Connected;

        /// <summary>
        /// It's fired when the channel has been disconnected.
        /// </summary>
        public event ChannelDisconnectedEventHandler Disconnected;

        /// <summary>
        /// It's fired when a message has been received.
        /// </summary>
        public event ChannelReceiveEventHandler Receive;

        /// <summary>
        /// It's fired when an error has been catched in the internal
        /// channel processing.
        /// </summary>
        /// <remarks>
        /// This event is received from the channel when a catched error
        /// causes its inhabilitation, it's necessary to call
        /// Connect again to use it.
        /// </remarks>
        public event ChannelErrorEventHandler Error;

        #region IDisposable Members
        /// <summary>
        /// It disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region IFilteredChannel Members
        /// <summary>
        /// It begins the association of a channel with a connection accepted
        /// by a listener.
        /// </summary>
        /// <param name="connectionData">
        /// It's the data of the connection accepted by
        /// the listener.
        /// </param>
        public virtual void BeginBind(object connectionData)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (connectionData == null)
                throw new ArgumentNullException("connectionData");

            if (!(connectionData is Socket))
                throw new ArgumentException("Must be a socket.", "connectionData");

            _socket = (Socket) connectionData;
            _remoteEndPoint = (IPEndPoint) (_socket.RemoteEndPoint);
            _reconnect = false;
            _enabled = true;
            _connected = true;
            _hostName = ((IPEndPoint) (_socket.RemoteEndPoint)).Address.ToString();
            _port = ((IPEndPoint) (_socket.RemoteEndPoint)).Port;
            _family = _socket.RemoteEndPoint.AddressFamily;
            _currentReconnectInterval = 0;
            _formatterContext.Initialize();
            _parserContext.Initialize();
            _expectingDataLength = false;
            _expectedDataLength = 0;
        }

        /// <summary>
        /// It ends the association of a channel with a connection accepted
        /// by a listener.
        /// </summary>
        public void EndBind()
        {
            lock (this)
            {
                try
                {
                    NDC.Push(string.Format("{0}-{1}", GetNdcName(), "EndBind"));

                    _lastConnectionDateTime = DateTime.Now;

                    PrepareMessageRead();

                    _timer.Change(_inactivityInterval, Timeout.Infinite);

                    // Send the session header.
                    if ((_sessionHeader != null) && (_sessionHeader.Length > 0))
                    {
                        _socket.Send(_sessionHeader);

                        if (Logger.IsInfoEnabled)
                            Logger.Info(StringUtilities.GetPrintableBuffer(
                                string.Format("Session header sent ({0} {1}): ",
                                    _sessionHeader.Length,
                                    _sessionHeader.Length == 1 ? "byte" : "bytes"),
                                _sessionHeader, 0, _sessionHeader.Length));
                    }

                    // Begins to asynchronously receive data from a connected socket.
                    _socket.BeginReceive(_parserContext.GetBuffer(), 0,
                        _parserContext.FreeBufferSpace, SocketFlags.None,
                        AsyncReadRequestHandler, _socket);

                    ThreadPool.QueueUserWorkItem(OnConnected);
                }
                catch (Exception e)
                {
                    HandleExceptionAndTryReconnection(e);
                    throw new ApplicationException("Can't EndBind, see inner exception.", e);
                }
                finally
                {
                    NDC.Pop();
                }
            }
        }

        /// <summary>
        /// It starts the connection of the channel with the remote system.
        /// </summary>
        /// <returns>
        /// Returns true if connection has been started, otherwise
        /// returns false.
        /// </returns>
        public bool Connect()
        {
            if (_hostName == null)
                throw new MessagingException("Invalid host name.");

            if (!NetUtilities.IsValidTcpPort(_port))
                throw new MessagingException("Invalid port number.");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_enabled)
            {
                try
                {
                    var ipEndPoint = new IPEndPoint(IPAddress.Parse(_hostName), _port);
                    if (ipEndPoint.Equals(_remoteEndPoint))
                        return true;
                }
                catch (FormatException)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);

                    foreach (IPAddress t in hostEntry.AddressList)
                        if (t.AddressFamily == _family)
                            if (new IPEndPoint(t, _port).Equals(
                                _remoteEndPoint))
                                return true;
                }

                Close();
            }

            try
            {
                return Connect(new IPEndPoint(IPAddress.Parse(_hostName), _port));
            }
            catch (FormatException)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);

                foreach (IPAddress t in hostEntry.AddressList)
                    if (t.AddressFamily == _family)
                        return Connect(new IPEndPoint(
                            t, _port));
            }

            return false;
        }

        /// <summary>
        /// Close the connection, if exists, stablished with
        /// the remote system.
        /// </summary>
        public void Close()
        {
            lock (this)
            {
                try
                {
                    NDC.Push(string.Format("{0}", GetNdcName()));

                    Close("Disconnection requested.");
                }
                finally
                {
                    NDC.Pop();
                }
            }
        }

        /// <summary>
        /// It sends the specified message to the remote system.
        /// </summary>
        /// <param name="message">
        /// It's the message to be sent.
        /// </param>
        public virtual void Send(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (this)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (!_enabled)
                    throw new InvalidOperationException("The channel isn't active.");

                if (!_connected)
                    throw new InvalidOperationException("The channel isn't connected.");

                try
                {
                    NDC.Push(string.Format("{0}-{1}", GetNdcName(), "Send"));

                    if (ApplyOutgoingFilters(message) == MessageFilterDecision.Deny)
                    {
                        if (Logger.IsInfoEnabled)
                            Logger.Info(string.Format(
                                "Send cancelled, message denied by outgoing filters: {0}",
                                message));
                    }
                    else
                    {
                        int dl = DataLengthHeaderLength(true);
                        if (dl == 0)
                            _formatterContext.Clear();
                        else
                            _formatterContext.Clear(dl);

                        // The message must known its formatter for message.ToString()
                        message.Formatter = _formatter;

                        if (Logger.IsInfoEnabled)
                            Logger.Info(string.Format("Message to send: {0}", message));

                        message.Formatter.Format(message, ref _formatterContext);

                        UpdateDataLengthHeader(ref _formatterContext);

                        if (Logger.IsDebugEnabled)
                            Logger.Debug(StringUtilities.GetPrintableBuffer(
                                string.Format("Raw data to send ({0} {1}): ",
                                    _formatterContext.DataLength,
                                    _formatterContext.DataLength == 1 ? "byte" : "bytes"),
                                _formatterContext.GetBuffer(), 0, _formatterContext.DataLength));

                        _socket.Send(_formatterContext.GetBuffer(), 0,
                            _formatterContext.DataLength, SocketFlags.None);
                    }
                }
                catch (Exception e)
                {
                    HandleExceptionAndTryReconnection(e);
                    throw;
                }
                finally
                {
                    NDC.Pop();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public void AddFilter(IMessageFilter filter)
        {
            IMessageFilter cursor;

            if (_incomingFilters != null)
            {
                cursor = _incomingFilters;

                while (filter.Next != null)
                    cursor = cursor.Next;

                cursor.Next = filter;
            }
            else
                _incomingFilters = filter;

            if (_incomingFilters != null)
            {
                cursor = _outgoingFilters;

                while (filter.Next != null)
                    cursor = cursor.Next;

                cursor.Next = filter;
            }
            else
                _outgoingFilters = filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public void RemoveFilter(IMessageFilter filter)
        {
            IMessageFilter cursor = _incomingFilters;
            IMessageFilter prev = null;
            while (cursor != null)
            {
                if (cursor == filter)
                {
                    if (prev == null)
                        _incomingFilters = null;
                    else
                        prev.Next = cursor.Next;

                    break;
                }

                prev = cursor;
                cursor = cursor.Next;
            }

            cursor = _outgoingFilters;
            prev = null;
            while (cursor != null)
            {
                if (cursor == filter)
                {
                    if (prev == null)
                        _outgoingFilters = null;
                    else
                        prev.Next = cursor.Next;

                    break;
                }

                prev = cursor;
                cursor = cursor.Next;
            }
        }

        /// <summary>
        /// Clones the channel.
        /// </summary>
        /// <returns>
        /// The clone.
        /// </returns>
        public virtual object Clone()
        {
            var clone = new TcpChannel(_formatter);
            SetCommonProperties(clone);

            return clone;
        }
        #endregion

        /// <summary>
        /// It fires the <see cref="Error"/> event.
        /// </summary>
        /// <param name="exception">
        /// It's the exception the error made.
        /// </param>
        private void OnError(Exception exception)
        {
            try
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(exception);

                if (Error != null)
                    Error(this, new ErrorEventArgs(exception));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// It invokes the message processors and fires the
        /// <see cref="Receive"/> event.
        /// </summary>
        /// <param name="state">
        /// It's the received message.
        /// </param>
        private void OnReceive(object state)
        {
            try
            {
                NDC.Push(string.Format("{0}-{1}", GetNdcName(), "Receive"));

                var message = state as Message;

                if (message == null)
                    return;

                if (Logger.IsInfoEnabled)
                    Logger.Info(string.Format("Received message: {0}", message));

                if (ApplyIncomingFilters(message) == MessageFilterDecision.Deny)
                {
                    if (Logger.IsInfoEnabled)
                        Logger.Info(string.Format(
                            "Message denied by incoming filters: {0}", message));
                }
                else
                {
                    IMessageProcessor processor = _messageProcessor;

                    if (Logger.IsWarnEnabled)
                        if (processor == null)
                            Logger.Warn("A message was received, but not processors exist to handle it.");

                    while ((processor != null) && !processor.Process(this, message))
                        processor = processor.NextMessageProcessor;

                    if (Receive != null)
                        Receive(this, new ReceiveEventArgs(message));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                NDC.Pop();
            }
        }

        /// <summary>
        /// It fires the <see cref="Connected"/> event.
        /// </summary>
        /// <param name="state">
        /// Not used.
        /// </param>
        private void OnConnected(object state)
        {
            try
            {
                NDC.Push(GetNdcName());

                if (Logger.IsInfoEnabled)
                    Logger.Info("Connection stablished.");

                if (Connected != null)
                    Connected(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                NDC.Pop();
            }
        }

        /// <summary>
        /// It fires the <see cref="Disconnected"/> event.
        /// </summary>
        private void OnDisconnected()
        {
            try
            {
                if (Disconnected != null)
                    Disconnected(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private string GetNdcName()
        {
            string ndcName = string.Empty;

            if (_name == null)
                if (_remoteEndPoint == null)
                {
                    if (_socket != null)
                        ndcName = _socket.LocalEndPoint.ToString();
                }
                else if (_socket == null)
                    ndcName = string.Format("local-{0}", _remoteEndPoint);
                else
                    ndcName = string.Format("{0}-{1}", _socket.LocalEndPoint,
                        _remoteEndPoint);
            else if (_remoteEndPoint == null)
                ndcName = _name;
            else
                ndcName = string.Format("{0}-{1}", _name, _remoteEndPoint);

            return ndcName;
        }

        /// <summary>
        /// It disposes the object.
        /// </summary>
        /// <param name="disposing">
        /// It indicates with a true logical value the object is
        /// disposing explicitly throught the Dispose
        /// method invocation.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            lock (this)
            {
                Close("Disconnection required because channel is disposing.");
            }

            _disposed = true;
        }

        /// <summary>
        /// It connects the channel to the remote system  with the
        /// objective of interchanging messages
        /// </summary>
        /// <param name="hostName">
        /// It's the name or IP address of the remote system.
        /// </param>
        /// <param name="port">
        /// It's the port in the remote system to which you wish to connect.
        /// </param>
        /// <returns>
        /// Returns true if connection has been started, otherwise
        /// returns false.
        /// </returns>
        public bool Connect(string hostName, int port)
        {
            if (hostName == null)
                throw new ArgumentNullException("hostName");

            if (!NetUtilities.IsValidTcpPort(port))
                throw new ArgumentOutOfRangeException("port");

            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_enabled)
                if ((port == _port) && (hostName == _hostName))
                    return true;
                else
                    Close();

            _hostName = hostName;
            _port = port;

            return Connect();
        }

        /// <summary>
        /// This method receives the events from the channel timer.
        /// </summary>
        /// <param name="state">
        /// Not used.
        /// </param>
        /// <remarks>
        /// The timer's event is received when reconnection is required, or when the 
        /// given interval of time has been fulfilled and no messages has been received
        ///  from the remote system. 
        /// </remarks>
        private void OnTimerTick(object state)
        {
            lock (this)
            {
                // Some called Close, nothing to do.
                if (!_enabled)
                    return;

                try
                {
                    NDC.Push(GetNdcName());

                    if (_connected)
                        // We're connected, the timer has been fired because we have
                        // reception inactivity. Close socket and create a new one.
                        Close(string.Format(
                            "Disconnection required caused by reception inactivity (reconexión = {0}).",
                            _reconnect));

                    if (_reconnect)
                        // Try to reconnect.
                        CreateSocketAndStartConnection();
                    else
                    {
                        _socket = new Socket(_family, SocketType.Stream, ProtocolType.Tcp);

                        // Socket will linger for 10 seconds after close is called.
                        var lingerOption = new LingerOption(true, 10);
                        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

                        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);

                        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, -1);

                        _enabled = true;
                    }
                }
                catch (Exception e)
                {
                    Close("Disconnection required caused by  exception in reception inactivity handling.");
                    _enabled = true;
                    OnError(e);

                    try
                    {
                        // Schedule a connection request.
                        _timer.Change(5000, Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        _enabled = false;
                        OnError(ex);
                        return;
                    }
                }
                finally
                {
                    NDC.Pop();
                }
            }
        }

        /// <summary>
        /// It informs the length of the packet length indicator.
        /// </summary>
        /// <param name="formatting">
        /// true if we're formatting, false if we're receiving a message.
        /// </param>
        /// <returns>
        /// The length of the packet length indicator.
        /// </returns>
        protected virtual int DataLengthHeaderLength(bool formatting)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual MessageFilterDecision ApplyIncomingFilters(Message message)
        {
            MessageFilterDecision decision = MessageFilterDecision.Accept;

            if (_incomingFilters != null)
            {
                IMessageFilter filter = _incomingFilters;

                do
                {
                    switch (filter.Decide(this, message))
                    {
                        case MessageFilterDecision.Deny:
                            decision = MessageFilterDecision.Deny;
                            filter = null;
                            break;

                        case MessageFilterDecision.Accept:
                            filter = null;
                            break;

                        case MessageFilterDecision.Neutral:
                            filter = filter.Next;
                            break;
                    }
                } while (filter != null);
            }

            return decision;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual MessageFilterDecision ApplyOutgoingFilters(Message message)
        {
            MessageFilterDecision decision = MessageFilterDecision.Accept;

            if (_outgoingFilters != null)
            {
                IMessageFilter filter = _outgoingFilters;

                do
                {
                    switch (filter.Decide(this, message))
                    {
                        case MessageFilterDecision.Deny:
                            decision = MessageFilterDecision.Deny;
                            filter = null;
                            break;

                        case MessageFilterDecision.Accept:
                            filter = null;
                            break;

                        case MessageFilterDecision.Neutral:
                            filter = filter.Next;
                            break;
                    }
                } while (filter != null);
            }

            return decision;
        }

        /// <summary>
        /// Analyse from the parser context the length of the data packet
        /// to be processed.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context which holds the information of the
        /// packet length.
        /// </param>
        /// <param name="lengthConsumed">
        /// Used to indicate data length was consumed.
        /// </param>
        /// <returns>
        /// It's the data packet length which can be turned into a
        /// message.
        /// </returns>
        /// <remarks>
        /// This method consumes data from the parser context.
        /// </remarks>
        protected virtual int GetDataLength(ref ParserContext parserContext,
            ref bool lengthConsumed)
        {
            return 0;
        }

        /// <summary>
        /// It's a method called by the reception routine, when an equal or
        /// less than zero packet length indicator has been received.
        /// </summary>
        /// <param name="socket">
        /// It's the socket holded whith the remote system.
        /// </param>
        protected virtual void NullDataLengthReceived(Socket socket)
        {
        }

        /// <summary>
        /// Initializes the reading of a new message.
        /// </summary>
        private void PrepareMessageRead()
        {
            _expectedDataLength = DataLengthHeaderLength(false);
            _expectingDataLength = _expectedDataLength > 0;
        }

        /// <summary>
        /// It starts a connection request.
        /// </summary>
        private void CreateSocketAndStartConnection()
        {
            if (Logger.IsInfoEnabled)
                Logger.Info("Trying to connect.");

            try
            {
                _socket = new Socket(_family, SocketType.Stream, ProtocolType.Tcp);

                // Socket will linger for 10 seconds after close is called.
                var lingerOption = new LingerOption(true, 10);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, -1);

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, -1);

                if ((_localEndPoint == null) && !string.IsNullOrEmpty(_localInterface))
                {
                    IPAddress addr;
                    if (IPAddress.TryParse(_localInterface, out addr))
                        _localEndPoint = new IPEndPoint(addr, 0);
                    else
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(_localInterface);

                        if (hostEntry.AddressList.Length == 0)
                            throw new MessagingException("Can't resolve local interface name.");

                        // Create server socket, over the first located address.
                        _localEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                    }
                }

                if (_localEndPoint != null)
                    _socket.Bind(_localEndPoint);

                _enabled = true;
                _socket.BeginConnect(_remoteEndPoint,
                    AsyncConnectionRequestHandler, _socket);
            }
            catch (Exception e)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error(e);

                try
                {
                    _socket.Close();
                }
                catch
                {
                }
                _socket = null;
                _enabled = true;

                try
                {
                    // Schedule a connection request.
                    _timer.Change(5000, Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    _enabled = false;
                    OnError(ex);
                    return;
                }
            }
        }

        /// <summary>
        /// It handles an error according to the kind of exception which was
        /// generated, starting a reconnection if necessary.
        /// </summary>
        /// <param name="e">
        /// It's the generated exception because of the catched error.
        /// </param>
        private void HandleExceptionAndTryReconnection(Exception e)
        {
            int currentReconnectInterval = _currentReconnectInterval;

            _parserContext.Initialize();

            if (e.GetType() == typeof (SocketException))
            {
                LogSocketException((SocketException) e);
                Close(string.Empty);
            }
            else
            {
                Close("The connection is closed (HandleExceptionAndTryReconnection).");

                // Notify error.
                OnError(e);
            }

            if (((e is ThreadAbortException) ||
                (e is StackOverflowException)) ||
                    (e is OutOfMemoryException))
                return;

            if (_reconnect)
            {
                TimeSpan connectionDuration = DateTime.Now - _lastConnectionDateTime;
                if (connectionDuration.Seconds < 1)
                {
                    // If the connection duration was less than 1 second,
                    // recompute reconnection interval to prevent reconnection
                    // starvation (a sequence of infinite connect - disconnect,
                    // very dangerous because can consume server resources)
                    _currentReconnectInterval = currentReconnectInterval;

                    ComputeNextReconnectionInterval();

                    if (Logger.IsWarnEnabled)
                        Logger.Warn(string.Format(
                            "The last connection lived less than one second ({0} ms), next reconnection attempt in {1} s (HandleExceptionAndTryReconnection).",
                            connectionDuration.Milliseconds, _currentReconnectInterval));

                    try
                    {
                        // Schedule a connection request.
                        _enabled = true;
                        _timer.Change(_currentReconnectInterval, Timeout.Infinite);
                    }
                    catch (Exception ex)
                    {
                        Close("The connection is closed (exception in AsyncConnectionRequestHandler in _timer.Change).");
                        OnError(ex);
                        return;
                    }
                }
                else
                    // At this point, perhaps we can recover from the
                    // received exception, we try a reconnection.
                    try
                    {
                        CreateSocketAndStartConnection();
                    }
                    catch (Exception ex)
                    {
                        Close("The connection is closed (exception in HandleExceptionAndTryReconnection).");
                        OnError(ex);
                    }
            }
        }

        private void LogSocketException(SocketException e)
        {
            switch (e.ErrorCode)
            {
                case 10054:
                    // An existing connection was forcibly closed by the remote host.
                    if (Logger.IsInfoEnabled)
                        Logger.Info(string.Format(
                            "Connection closed: {0}.", e.Message));
                    break;
                case 10060:
                    // Connection timed out. A connection attempt failed because the
                    // connected party did not properly respond after a period of time,
                    // or the established connection failed because the connected host
                    // has failed to respond.
                    if (Logger.IsWarnEnabled)
                        Logger.Warn(string.Format(
                            "Connection timed out: {0}. Next connection attempt in {1}ms.",
                            e.Message, _currentReconnectInterval));
                    break;
                case 10061:
                    // Connection refused. No connection could be made because the target
                    // machine actively refused it. This usually results from trying to
                    // connect to a service that is inactive on the foreign host — that is,
                    // one with no server application running. 
                    if (Logger.IsWarnEnabled)
                        Logger.Warn(string.Format(
                            "Connection refused: {0}. Next connection attempt in {1}ms.",
                            e.Message, _currentReconnectInterval));
                    break;
                default:
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Socket exception (error " + e.ErrorCode.ToString(
                            CultureInfo.InvariantCulture) + ")", e);
                    break;
            }
        }

        /// <summary>
        /// It's the asynchronous reading handler.
        /// </summary>
        /// <param name="asyncResult">
        /// It's the result of the asynchronous reading.
        /// </param>
        private void AsyncReadRequestHandler(IAsyncResult asyncResult)
        {
            if (asyncResult.AsyncState == _socket)
                lock (this)
                {
                    if (asyncResult.AsyncState == _socket)
                    {
                        if (!_enabled)
                        {
                            // Someone called Close. EndReceive and return.
                            try
                            {
                                _socket.EndReceive(asyncResult);
                            }
                            catch
                            {
                            }
                            Monitor.Pulse(this);
                            return;
                        }

                        try
                        {
                            NDC.Push(GetNdcName());

                            int receivedBytes = _socket.EndReceive(asyncResult);

                            if (receivedBytes == 0)
                            {
                                int currentReconnectInterval = _currentReconnectInterval;

                                // Connection has been closed.
                                Close("The connection was lost, the remote peer has terminated the communication.");

                                if (_reconnect)
                                {
                                    TimeSpan connectionDuration = DateTime.Now - _lastConnectionDateTime;

                                    if (connectionDuration.Seconds < 1)
                                    {
                                        // If the connection duration was less than 1 second,
                                        // recompute reconnection interval to prevent reconnection
                                        // starvation (a sequence of infinite connect - disconnect,
                                        // very dangerous because can consume server resources)
                                        _currentReconnectInterval = currentReconnectInterval;
                                        ComputeNextReconnectionInterval();

                                        if (Logger.IsWarnEnabled)
                                            Logger.Warn(string.Format(
                                                "The last connection lived less than one second ({0} ms), next reconnection attempt in {1} s (AsyncReadRequestHandler).",
                                                connectionDuration.Milliseconds, _currentReconnectInterval));

                                        try
                                        {
                                            // Schedule a connection request.
                                            _enabled = true;
                                            _timer.Change(_currentReconnectInterval, Timeout.Infinite);
                                        }
                                        catch (Exception ex)
                                        {
                                            Close(
                                                "The connection is closed (exception in AsyncConnectionRequestHandler in _timer.Change).");
                                            OnError(ex);
                                            return;
                                        }
                                    }
                                    else
                                        CreateSocketAndStartConnection();
                                }
                                return;
                            }

                            if (Logger.IsDebugEnabled)
                                Logger.Debug(StringUtilities.GetPrintableBuffer(
                                    string.Format("Raw received data ({0} {1}): ", receivedBytes,
                                        receivedBytes == 1 ? "byte" : "bytes"), _parserContext.GetBuffer(),
                                    _parserContext.UpperDataBound, receivedBytes));

                            _parserContext.UpperDataBound += receivedBytes;

                            // Reschedule a reconnection due reception innactivity.
                            _timer.Change(_inactivityInterval, Timeout.Infinite);

                            // Parse available messages in parser context buffer.
                            while (true)
                            {
                                if (_expectingDataLength)
                                    if (_parserContext.DataLength >= _expectedDataLength)
                                    {
                                        bool lengthConsumed = true;
                                        _expectedDataLength = GetDataLength(ref _parserContext,
                                            ref lengthConsumed);

                                        if (_expectedDataLength > 0)
                                        {
                                            if (lengthConsumed)
                                            {
                                                _expectingDataLength = false;
                                                _parserContext.FrontierUpperBound =
                                                    _parserContext.LowerDataBound + _expectedDataLength;
                                            }

                                            if (_expectedDataLength > MaxPacketLength)
                                            {
                                                /* Error severo, cortamos la conexión */
                                                Logger.Error(
                                                    string.Format(
                                                        "Se recibió un indicador de largo de paquete igual a {0} " +
                                                            "que supera el máximo permitido de {1}", _expectedDataLength,
                                                        MaxPacketLength));
                                                /*
                                                 * 10053 - WSAECONNABORTED: Software caused connection abort.
                                                 * An established connection was aborted by the software in your host computer,
                                                 * possibly due to a data transmission time-out or protocol error.
                                                 */
                                                HandleExceptionAndTryReconnection(new SocketException(10053));
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            // Zero or lower than zero, data length received.
                                            NullDataLengthReceived(_socket);
                                            PrepareMessageRead();
                                            continue;
                                        }
                                    }

                                if (_parserContext.DataLength < _expectedDataLength)
                                {
                                    // More data is expected. Read again.
                                    // Check parser context free buffer space.
                                    if (_parserContext.FreeBufferSpace <
                                        (_expectedDataLength - _parserContext.DataLength))
                                        // More buffer space required to read data, resize it.
                                        _parserContext.ResizeBuffer(_expectedDataLength -
                                            _parserContext.DataLength);

                                    if (_socket == null)
                                        // Perhaps we closed the socket in message processing.
                                        return;

                                    // Begins to asynchronously receive data from a connected socket.
                                    _socket.BeginReceive(_parserContext.GetBuffer(),
                                        _parserContext.UpperDataBound, _parserContext.FreeBufferSpace,
                                        SocketFlags.None, AsyncReadRequestHandler,
                                        _socket);
                                    break;
                                }
                                else
                                {
                                    if (_parserContext.CurrentMessage == null)
                                        // This is the first time we enter here.
                                        RemovePreamble(ref _parserContext);

                                    // Enough data received, parse it.
                                    Message receivedMessage = null;

                                    if (_parserContext.DataLength > 0)
                                        /*
                                        if ( Logger.IsDebugEnabled ) {
                                            Logger.Debug( string.Format( "Ready to parse message: {0}",
                                                _parserContext ) );
                                        }
                                         */

                                        receivedMessage = _formatter.Parse(ref _parserContext);

                                    if (receivedMessage == null)
                                    {
                                        // More data required to parse the message.
                                        // Check parser context free buffer space.
                                        if (_parserContext.FreeBufferSpace == 0)
                                            // More buffer space required to read data, resize it.
                                            _parserContext.ResizeBuffer(ParserContext.DefaultBufferSize);

                                        // Begins to asynchronously receive data from a connected socket.
                                        _socket.BeginReceive(_parserContext.GetBuffer(),
                                            _parserContext.UpperDataBound,
                                            _parserContext.FreeBufferSpace,
                                            SocketFlags.None,
                                            AsyncReadRequestHandler,
                                            _socket);
                                        break;
                                    }

                                    PrepareMessageRead();

                                    // The thread pool has a built-in delay (half a second in the
                                    // .NET Framework version 2.0) before starting new idle threads.

                                    // A message was received.
                                    ThreadPool.QueueUserWorkItem(OnReceive, receivedMessage);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            HandleExceptionAndTryReconnection(e);
                            return;
                        }
                        finally
                        {
                            NDC.Pop();
                        }
                    }
                }
        }

        /// <summary>
        /// Compute next reconnection attempt interval.
        /// </summary>
        private void ComputeNextReconnectionInterval()
        {
            // Compute next connection request interval.
            if (_currentReconnectInterval == 0)
                _currentReconnectInterval = 1000;
            else
            {
                _currentReconnectInterval *= 2;
                if (_currentReconnectInterval > _reconnectInterval)
                    _currentReconnectInterval = _reconnectInterval;
            }
        }

        /// <summary>
        /// It's the handler of the asynchronous connection request.
        /// </summary>
        /// <param name="asyncResult">
        /// It's the result of the asynchronous connection request.
        /// </param>
        private void AsyncConnectionRequestHandler(IAsyncResult asyncResult)
        {
            if (asyncResult.AsyncState == _socket)
                lock (this)
                {
                    if (asyncResult.AsyncState == _socket)
                    {
                        if (!_enabled)
                        {
                            // Someone called Close. EndConnect and return.
                            try
                            {
                                _socket.EndConnect(asyncResult);
                                Close("Disconnection required.");
                            }
                            catch
                            {
                            }
                            Monitor.Pulse(this);
                            return;
                        }

                        bool popNdc = false;
                        try
                        {
                            NDC.Push(GetNdcName());
                            popNdc = true;

                            _socket.EndConnect(asyncResult);

                            _lastConnectionDateTime = DateTime.Now;

                            _connected = true;

                            // Schedule a reconnection due reception innactivity.
                            _timer.Change(_inactivityInterval, Timeout.Infinite);

                            PrepareMessageRead();

                            // Send the session header.
                            if ((_sessionHeader != null) && (_sessionHeader.Length > 0))
                            {
                                _socket.Send(_sessionHeader);

                                if (Logger.IsInfoEnabled)
                                    Logger.Info(StringUtilities.GetPrintableBuffer(
                                        string.Format("Session header sent ({0} {1}): ",
                                            _sessionHeader.Length,
                                            _sessionHeader.Length == 1 ? "byte" : "bytes"),
                                        _sessionHeader, 0, _sessionHeader.Length));
                            }

                            // Begins to asynchronously receive data from a connected socket.
                            _socket.BeginReceive(_parserContext.GetBuffer(), 0,
                                _parserContext.FreeBufferSpace, SocketFlags.None,
                                AsyncReadRequestHandler, _socket);

                            ThreadPool.QueueUserWorkItem(OnConnected);
                        }
                        catch (Exception e)
                        {
                            int currentReconnectInterval = _currentReconnectInterval;
                            Close("The connection is closed (exception in AsyncConnectionRequestHandler).");
                            _enabled = true;

                            if (((e is ThreadAbortException) ||
                                (e is StackOverflowException)) ||
                                    (e is OutOfMemoryException))
                            {
                                OnError(e);
                                return;
                            }

                            _currentReconnectInterval = currentReconnectInterval;

                            ComputeNextReconnectionInterval();

                            if (e.GetType() == typeof (SocketException))
                                LogSocketException((SocketException) e);
                            else if (Logger.IsErrorEnabled)
                                Logger.Error(e);

                            try
                            {
                                // Schedule a connection request.
                                _timer.Change(_currentReconnectInterval, Timeout.Infinite);
                            }
                            catch (Exception ex)
                            {
                                Close(
                                    "The connection is closed (exception in AsyncConnectionRequestHandler in _timer.Change).");
                                OnError(ex);
                                return;
                            }
                        }
                        finally
                        {
                            if (popNdc)
                                NDC.Pop();
                        }
                    }
                }
            else if (Logger.IsDebugEnabled)
                Logger.Debug("Async connection received for an unknown request.");
        }

        /// <summary>
        /// Connects the channel to the remote system
        /// </summary>
        /// <param name="remoteEndPoint">
        /// It represents the connection point to the remote system.
        /// </param>
        /// <returns>
        /// Returns true if connection has been started, otherwise
        /// returns false.
        /// </returns>
        public bool Connect(IPEndPoint remoteEndPoint)
        {
            lock (this)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (remoteEndPoint == null)
                    throw new ArgumentNullException("remoteEndPoint");

                if (_enabled)
                    if (remoteEndPoint.Equals(_remoteEndPoint))
                        return true;
                    else
                        Close(string.Empty);

                try
                {
                    NDC.Push(string.Format("{0}-{1}", GetNdcName(), "Connect"));

                    _remoteEndPoint = remoteEndPoint;
                    CreateSocketAndStartConnection();
                }
                catch
                {
                    Close("Disconnect required handling exception in Connect.");
                    throw;
                }
                finally
                {
                    NDC.Pop();
                }
            }

            return true;
        }

        /// <summary>
        /// Close the connection, if exists, stablished with
        /// the remote system.
        /// </summary>
        private void Close(string message)
        {
            bool raiseOnDisconnected = false;

            _enabled = false;

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }

                try
                {
                    _socket.Close();
                }
                catch
                {
                }

                _socket = null;

                if (_connected)
                {
                    raiseOnDisconnected = true;
                    _connected = false;

                    if (Logger.IsInfoEnabled)
                        if (message == null)
                            Logger.Info("The connection is closed.");
                        else if (!string.IsNullOrEmpty(message))
                            Logger.Info(message);
                }
            }

            _currentReconnectInterval = 0;
            _parserContext.Clear();
            _formatterContext.Clear();

            if (raiseOnDisconnected)
                OnDisconnected();
        }

        /// <summary>
        /// Updates the data length to be sent to the remote system.
        /// </summary>
        /// <param name="formatterContext">
        /// It's the message formatter context.
        /// </param>
        protected virtual void UpdateDataLengthHeader(ref FormatterContext formatterContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parserContext"></param>
        protected virtual void RemovePreamble(ref ParserContext parserContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clone"></param>
        protected virtual void SetCommonProperties(TcpChannel clone)
        {
            clone.Logger = _logger;
            clone.Name = _name;
            clone.HostName = _hostName;
            clone.Port = _port;
            clone.MessageProcessor = _messageProcessor;
            clone.IncomingFilters = _incomingFilters;
            clone.OutgoingFilters = _outgoingFilters;
        }
    }
}