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
using System.Collections.Generic;
using System.Threading;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels
{
    public abstract class BaseServerChannel : BaseChannel, IServerChannel
    {
        private readonly Dictionary<string, IServerChildChannel> _childs;
        private readonly IMessagesIdentifier _messagesIdentifier;
        private readonly IPipelineFactory _pipelineFactory;
        private int _nextChildId = 1;

        /// <summary>
        /// Builds a server channel with childs intended to send messages only.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        protected BaseServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory)
            : base(pipeline)
        {
            if (pipelineFactory == null)
                throw new ArgumentNullException("pipelineFactory");

            _pipelineFactory = pipelineFactory;
            _childs = new Dictionary<string, IServerChildChannel>();
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
        protected BaseServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline)
        {
            if (pipelineFactory == null)
                throw new ArgumentNullException("pipelineFactory");

            if (tupleSpace == null)
                throw new ArgumentNullException("tupleSpace");

            _pipelineFactory = pipelineFactory;
            _childs = new Dictionary<string, IServerChildChannel>();
            TupleSpace = tupleSpace;
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
        protected BaseServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier)
            : this(pipeline, pipelineFactory, tupleSpace)
        {
            if (messagesIdentifier == null)
                throw new ArgumentNullException("messagesIdentifier");

            _messagesIdentifier = messagesIdentifier;
        }

        public int NextChildId
        {
            get { return _nextChildId; }
        }

        /// <summary>
        /// Space where the received messages without a matching response are stored.
        /// </summary>
        public ITupleSpace<ReceiveDescriptor> TupleSpace { get; internal set; }

        /// <summary>
        /// Object used to get keys from messages to match requests with responses.
        /// </summary>
        public IMessagesIdentifier MessagesIdentifier
        {
            get { return _messagesIdentifier; }
        }

        #region IServerChannel Members
        /// <summary>
        /// Raised when a child channel is connected and accepted.
        /// </summary>
        public event ServerChannelChildConnectedEventHandler ChildConnected;

        /// <summary>
        /// Raised when a child disconnects.
        /// </summary>
        public event ServerChannelChildDisconnectedEventHandler ChildDisconnected;

        /// <summary>
        /// Raised when a child changes its address.
        /// </summary>
        public event ServerChannelChildAddressChangedEventHandler ChildAddressChanged;

        /// <summary>
        /// It returns the factory of new pipelines for accepted connections.
        /// </summary>
        public IPipelineFactory PipelineFactory
        {
            get { return _pipelineFactory; }
        }

        /// <summary>
        /// True if the server channel is listening for new connections. Otherwise false.
        /// </summary>
        public abstract bool IsListening { get; }

        /// <summary>
        /// It returns the child list of accepted and connected channels.
        /// </summary>
        public Dictionary<string, IServerChildChannel> Childs
        {
            get { return _childs; }
        }

        /// <summary>
        /// Tells if the channel is connected.
        /// </summary>
        public override bool IsConnected
        {
            get { return false; }
            protected set { }
        }

        /// <summary>
        /// Ask the current channel to start listening connections.
        /// </summary>
        /// <returns>
        /// The operation request control.
        /// </returns>
        public abstract ChannelRequestCtrl StartListening();

        /// <summary>
        /// Ask the current channel to stop listening new connections.
        /// </summary>
        public abstract void StopListening();

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        public override void Disconnect()
        {
        }

        /// <summary>
        /// Close channel and release all the allocated resources.
        /// </summary>
        public override void Close()
        {
            StopListening();

            List<IServerChildChannel> childs = null;
            lock (_childs)
                if (_childs.Count > 0)
                {
                    childs = new List<IServerChildChannel>(_childs.Count);
                    foreach (IServerChildChannel child in _childs.Values)
                        childs.Add(child);
                }
            if (childs != null)
                foreach (IServerChildChannel child in childs)
                    child.Close();
        }
        #endregion

        private void OnChildDisconnectedWorkItem(object state)
        {
            var args = state as ServerChannelChildEventArgs;
            if (ChildDisconnected != null && args != null)
                ChildDisconnected(this, args);
        }

        private void OnChildDisconnected(IServerChildChannel childChannel)
        {
            if (ChildDisconnected != null)
                ThreadPool.QueueUserWorkItem(OnChildDisconnectedWorkItem, new ServerChannelChildEventArgs(childChannel));
        }

        /// <summary>
        /// Called by a child to inform disconnection.
        /// </summary>
        /// <param name="childChannel">
        /// The disconnected child.
        /// </param>
        internal void ChildDisconnection(IServerChildChannel childChannel)
        {
            lock (_childs)
                _childs.Remove(childChannel.ChannelAddress.ToString());

            OnChildDisconnected(childChannel);
        }

        private void OnChildConnectionWorkItem(object state)
        {
            var args = state as ServerChannelChildEventArgs;
            if (ChildConnected != null && args != null)
                ChildConnected(this, args);
        }

        private void OnChildConnection(IServerChildChannel childChannel)
        {
            if (ChildConnected != null)
                ThreadPool.QueueUserWorkItem(OnChildConnectionWorkItem, new ServerChannelChildEventArgs(childChannel));
        }

        /// <summary>
        /// Called from a derived class to inform a new child connection (wich was accepted by the server pipeline).
        /// </summary>
        /// <param name="childChannel">
        /// The new child.
        /// </param>
        protected void ChildConnection(IServerChildChannel childChannel)
        {
            lock (_childs)
                _childs.Add(childChannel.ChannelAddress.ToString(), childChannel);

            _nextChildId++;

            OnChildConnection(childChannel);
        }

        private void OnChildAddressChangedWorkItem(object state)
        {
            var args = state as ServerChannelChildAddressChangedEventArgs;
            if (ChildAddressChanged != null && args != null)
                ChildAddressChanged(this, args);
        }

        private void OnChildAddressChanged(IServerChildChannel childChannel, IChannelAddress oldAddress)
        {
            if (ChildAddressChanged != null)
                ThreadPool.QueueUserWorkItem(OnChildAddressChangedWorkItem,
                    new ServerChannelChildAddressChangedEventArgs(childChannel, oldAddress));
        }

        /// <summary>
        /// Called by a child to inform about an address change.
        /// </summary>
        /// <param name="childChannel">
        /// The child changing his address.
        /// </param>
        /// <param name="newAddress">
        /// The child new address.
        /// </param>
        /// <returns>
        /// True when the address is successfully updated, false when the given child is unknown for the server.
        /// </returns>
        internal bool ChildAddressChange(IServerChildChannel childChannel, IChannelAddress newAddress)
        {
            lock (_childs)
            {
                var oldAddress = childChannel.ChannelAddress;
                string oldKey = oldAddress.ToString();
                if (_childs.ContainsKey(oldKey))
                {
                    IServerChildChannel knownChild = _childs[oldKey];
                    if (!ReferenceEquals(childChannel, knownChild))
                        return false;

                    _childs.Remove(oldKey);
                    _childs.Add(newAddress.ToString(), childChannel);

                    OnChildAddressChanged(childChannel, oldAddress);

                    return true;
                }
            }

            return false;
        }
    }
}