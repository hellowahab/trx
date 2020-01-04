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
using System.Collections;
using Trx.Messaging.Channels;
using Trx.Utilities;

namespace Trx.Messaging.FlowControl
{
    public delegate void PeerConnectedEventHandler(object sender, EventArgs e);

    public delegate void PeerDisconnectedEventHandler(object sender, EventArgs e);

    public delegate void PeerReceiveEventHandler(object sender, ReceiveEventArgs e);

    public delegate void PeerRequestDoneEventHandler(
        object sender, PeerRequestDoneEventArgs e);

    public delegate void PeerRequestCancelledEventHandler(
        object sender, PeerRequestCancelledEventArgs e);

    public delegate void PeerErrorEventHandler(object sender, ErrorEventArgs e);

    public abstract class Peer : IMessageSource, IMessageProcessor, IDisposable
    {
        private readonly IMessagesIdentifier _messagesIdentifier;
        private readonly string _name;
        private readonly Hashtable _pendingRequests;
        private IChannel _channel;
        private IMessageProcessor _messageProcessor;

        protected Peer(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            _name = name;
            HostConnect = true;
            _messageProcessor = null;
            _messagesIdentifier = null;
            _pendingRequests = null;
            NextMessageProcessor = null;
        }

        protected Peer(string name, IMessagesIdentifier messagesIdentifier) : this(name)
        {
            if (messagesIdentifier == null)
                throw new ArgumentNullException("messagesIdentifier");

            _messagesIdentifier = messagesIdentifier;
            _pendingRequests = new Hashtable(64);
        }

        protected IChannel ProtectedChannel
        {
            set
            {
                if (_channel != null)
                {
                    _channel.MessageProcessor = null;
                    _channel.Disconnected -= OnChannelDisconnected;
                    _channel.Connected -= OnChannelConnected;
                    _channel.Error -= OnChannelError;
                }

                _channel = value;

                if (_channel != null)
                {
                    _channel.Error += OnChannelError;
                    _channel.Connected += OnChannelConnected;
                    _channel.Disconnected += OnChannelDisconnected;
                    _channel.MessageProcessor = this;
                }
            }
        }

        public IChannel Channel
        {
            get { return _channel; }
        }

        public bool HostConnect { get; set; }

        public string Name
        {
            get { return _name; }
        }

        public IMessageProcessor MessageProcessor
        {
            get { return _messageProcessor; }

            set { _messageProcessor = value; }
        }

        public IMessagesIdentifier MessagesIdentifier
        {
            get { return _messagesIdentifier; }
        }

        #region IMessageProcessor Members
        public IMessageProcessor NextMessageProcessor { get; set; }
        #endregion

        #region IMessageSource Members
        public bool IsConnected
        {
            get
            {
                if (_channel == null)
                    return false;

                return _channel.IsConnected;
            }
        }
        #endregion

        public event PeerConnectedEventHandler Connected;

        public event PeerDisconnectedEventHandler Disconnected;

        public event PeerReceiveEventHandler Receive;

        public event PeerRequestDoneEventHandler RequestDone;

        public event PeerRequestCancelledEventHandler RequestCancelled;

        public event PeerErrorEventHandler Error;

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            ProtectedChannel = null;
        }
        #endregion

        #region IMessageProcessor Members
        public virtual bool Process(IMessageSource source, Message message)
        {
            OnReceive(message);

            return true;
        }
        #endregion

        #region IMessageSource Members
        public virtual void Send(Message message)
        {
            if (Channel != null)
                Channel.Send(message);
        }
        #endregion

        protected virtual void OnError(ErrorEventArgs e)
        {
            if (Error != null)
                Error(this, e);
        }

        protected virtual void OnRequestDone(PeerRequest request)
        {
            if (RequestDone != null)
                RequestDone(this, new PeerRequestDoneEventArgs(request));
        }

        protected virtual void OnRequestCancelled(PeerRequest request)
        {
            if (RequestCancelled != null)
                RequestCancelled(this, new PeerRequestCancelledEventArgs(request));
        }

        protected virtual void OnReceive(Message message)
        {
            PeerRequest request = null;

            // Check if received message is response of a pending request.
            if ((_pendingRequests != null))
                lock (_pendingRequests)
                {
                    if (_pendingRequests.Count > 0)
                    {
                        object messageKey = _messagesIdentifier.ComputeIdentifier(message);
                        if (messageKey != null)
                        {
                            request = (PeerRequest) _pendingRequests[messageKey];
                            if (request != null)
                                _pendingRequests.Remove(messageKey);
                        }
                    }
                }

            if (request == null)
            {
                // The received message not matched a pending request,
                // notify clients via Receive event and peer associated
                // processors.

                if (Receive != null)
                    Receive(this, new ReceiveEventArgs(message));

                IMessageProcessor processor = _messageProcessor;

                while ((processor != null) &&
                    !processor.Process(this, message))
                    processor = processor.NextMessageProcessor;
            }
            else // The message is the response of a pending request.
                if (request.SetResponseMessage(message))
                    OnRequestDone(request);
                else // The peer request was signaled as expired by the request timer.
                    OnRequestCancelled(request);
        }

        protected virtual void OnConnected()
        {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }

        public virtual void Connect()
        {
            if (Channel != null)
                Channel.Connect();
        }

        public virtual void Close()
        {
            if (Channel != null)
                Channel.Close();
        }

        internal virtual void Send(PeerRequest request)
        {
            object messageKey = _messagesIdentifier.ComputeIdentifier(
                request.RequestMessage);

            lock (_pendingRequests)
            {
                if (_pendingRequests.Contains(messageKey))
                    _pendingRequests.Remove(messageKey);

                Send(request.RequestMessage);
                request.MarkAsTransmitted();
                _pendingRequests.Add(messageKey, request);
            }
        }

        internal virtual void Cancel(PeerRequest request)
        {
            object messageKey = _messagesIdentifier.ComputeIdentifier(
                request.RequestMessage);

            if (_pendingRequests.Contains(messageKey))
                lock (_pendingRequests)
                {
                    if (_pendingRequests.Contains(messageKey))
                    {
                        _pendingRequests.Remove(messageKey);
                        OnRequestCancelled(request);
                    }
                }
        }

        protected virtual void OnChannelError(object sender, ErrorEventArgs e)
        {
            OnError(e);
        }

        protected virtual void OnChannelConnected(object sender, EventArgs e)
        {
            OnConnected();
        }

        protected virtual void OnChannelDisconnected(object sender, EventArgs e)
        {
            OnDisconnected();
        }
    }
}