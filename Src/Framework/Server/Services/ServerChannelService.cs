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
using System.Threading;
using Trx.Communication.Channels;
using Trx.Coordination.TupleSpace;

namespace Trx.Server.Services
{
    /// <summary>
    /// Encapsulates an <see cref="IServerChannel"/> as a Trx Server service.
    /// 
    /// If a Trx Server tuple space is configured, read the outgoing messages and send through the
    /// underling client channel.
    /// 
    /// Received messages are written in <see ref="InputContext"/>. Messages read from <see ref="OutputContext"/>
    /// are sent through the client channel to the remote peer.
    /// </summary>
    public class ServerChannelService : ChannelService
    {
        private readonly BaseServerChannel _server;

        protected bool KeepRunning;
        private ChannelServiceServingPolicy _childNotConnectedPolicy = ChannelServiceServingPolicy.Discard;

        private TupleSpace<object> _localTupleSpace;
        private Thread _readingThread;

        /// <summary>
        /// Creates a new server channel service wich uses a local tuple space to store pending messages to be sent.
        /// </summary>
        /// <param name="serverChannel">
        /// Server channel.
        /// </param>
        public ServerChannelService(BaseServerChannel serverChannel)
        {
            if (serverChannel == null)
                throw new ArgumentNullException("serverChannel");

            _server = serverChannel;
            _server.ChildConnected += OnServerChildConnected;
            _server.ChildAddressChanged += OnServerChildAddressChanged;
        }

        /// <summary>
        /// Creates a new server channel service wich uses a local tuple space to store pending messages to be sent.
        /// </summary>
        /// <param name="serverChannel">
        /// Server channel.
        /// </param>
        /// <param name="localTupleSpace">
        /// A tuple space where to store messages to send whose channels are not connected yet. Only applies if 
        /// value of <see ref="ChildNotConnectedPolicy"/> is <see cref="ChannelServiceServingPolicy.Wait"/>.
        /// </param>
        public ServerChannelService(BaseServerChannel serverChannel, TupleSpace<object> localTupleSpace) : this(serverChannel)
        {
            if (localTupleSpace == null)
                throw new ArgumentNullException("localTupleSpace");

            _localTupleSpace = localTupleSpace;
        }

        public IServerChannel Server
        {
            get { return _server; }
        }

        /// <summary>
        /// Policy of messages wich destination channel isn't connected.
        /// </summary>
        public ChannelServiceServingPolicy ChildNotConnectedPolicy
        {
            get { return _childNotConnectedPolicy; }
            set { _childNotConnectedPolicy = value; }
        }

        protected override void ProtectedStart()
        {
            base.ProtectedStart();

            if (_server.TupleSpace == null && TrxServerTupleSpace != null)
                // If no tuple space is configured and we have a Trx Server tuple space
                // set it in the server to receive messages.
                _server.TupleSpace = this;

            _server.StartListening();

            if (TrxServerTupleSpace != null)
            {
                _readingThread = new Thread(StartReadingTupleSpace);
                _readingThread.Start();
            }
        }

        protected override void ProtectedStop()
        {
            base.ProtectedStop();

            if (_readingThread != null)
            {
                KeepRunning = false;
                _readingThread.Abort();
                _readingThread = null;
            }

            if (_localTupleSpace == null)
            {
                _server.ChildAddressChanged -= OnServerChildAddressChanged;
                _server.ChildConnected -= OnServerChildConnected;
                _localTupleSpace = new TupleSpace<object>();
            }

            _server.StopListening();
        }

        protected override void ProtectedDispose()
        {
            base.ProtectedDispose();
            _server.Close();
        }

        private void SendFromLocalContext(ISenderChannel childChannel, string context)
        {
            try
            {
                if (_localTupleSpace != null)
                {
                    object message;
                    while ((message = _localTupleSpace.Take(null, 0, context)) != null)
                        if (childChannel != null)
                            Send(childChannel, message as MessageToAddress, message as MessageRequest);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void OnServerChildConnected(object sender, ServerChannelChildEventArgs e)
        {
            SendFromLocalContext(e.Child as ISenderChannel, e.Child.ChannelAddress.ToString());
        }

        private void OnServerChildAddressChanged(object sender, ServerChannelChildAddressChangedEventArgs e)
        {
            SendFromLocalContext(e.Child as ISenderChannel, e.Child.ChannelAddress.ToString());
        }

        private void StoreExpectingChildConnection(MessageToAddress messageAddress, MessageRequest request, int ttl, string address)
        {
            if (_localTupleSpace == null)
            {
                _localTupleSpace = new TupleSpace<object>();
                _server.ChildConnected += OnServerChildConnected;
                _server.ChildAddressChanged += OnServerChildAddressChanged;
            }

            object message = request ?? (object)messageAddress;
            _localTupleSpace.Write(message, ttl, address);

            Logger.Info(string.Format("{0}: message queued waiting connection of address {1}: {2}.",
                Name, address, messageAddress.Message));
        }

        private void ApplyChildNotConnectedPolicy(MessageToAddress messageAddress, MessageRequest request,
            int timeSinceWrite, int ttl, string address)
        {
            switch (ChildNotConnectedPolicy)
            {
                case ChannelServiceServingPolicy.Discard:
                case ChannelServiceServingPolicy.Return: // Non sense for an message to address.
                    Logger.Info(string.Format("{0}: discarding message to address {1} (not connected): {2}.",
                        Name, address, messageAddress.Message));
                    break;

                case ChannelServiceServingPolicy.Wait:
                    if (ttl == Timeout.Infinite)
                        StoreExpectingChildConnection(messageAddress, request, Timeout.Infinite, address);
                    else
                    {
                        ttl = ttl - timeSinceWrite;
                        if (ttl > 0)
                            StoreExpectingChildConnection(messageAddress, request, ttl, address);
                    }
                    break;
            }
        }

        private void Send(ISenderChannel childChannel, MessageToAddress messageAddress, MessageRequest request)
        {
            if (messageAddress == null)
                return;

            if (request == null)
                childChannel.Send(messageAddress.Message);
            else
                childChannel.SendExpectingResponse(request.Message, request.Timeout, true, request.Key);
        }

        private void SendMessage(object message, int timeInTupleSpace, int ttl)
        {
            var request = message as MessageRequest;
            if (request != null)
                message = request.Message;

            var messageAddress = message as MessageToAddress;
            if (messageAddress == null)
                throw new InvalidOperationException(
                    "Messages with destination address is needed to send to a server channel.");

            string address = messageAddress.ChannelAddress.ToString();
            lock (_server.Childs)
                if (!_server.Childs.ContainsKey(address))
                    ApplyChildNotConnectedPolicy(messageAddress, request, timeInTupleSpace, ttl, address);
                else
                {
                    var childChannel = _server.Childs[address] as ISenderChannel;
                    if (childChannel == null)
                        throw new InvalidOperationException(
                            "Cannot send messages to a channel wich is not an ISenderChannel.");
                    Send(childChannel, messageAddress, request);
                }
        }

        private void StartReadingTupleSpace()
        {
            KeepRunning = true;
            while (KeepRunning)
                try
                {
                    int timeInTupleSpace, ttl;
                    var message = TrxServerTupleSpace.Take(null, Timeout.Infinite,
                        out timeInTupleSpace, out ttl, OutputContext);
                    SendMessage(message.Message, timeInTupleSpace, ttl);
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
        }
    }
}