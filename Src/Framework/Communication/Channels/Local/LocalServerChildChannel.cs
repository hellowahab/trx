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

using Trx.Coordination.TupleSpace;
using Trx.Exceptions;

namespace Trx.Communication.Channels.Local
{
    public class LocalServerChildChannel : LocalBaseSenderReceiverChannel, IServerChildChannel
    {
        private readonly LocalServerChannel _parentChannel;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="peer">
        /// Client peer.
        /// </param>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        internal LocalServerChildChannel(LocalClientChannel peer, Pipeline pipeline, LocalServerChannel parentChannel)
            : base(pipeline)
        {
            // Child channel must use the client channel SyncRoot to avoid nasty deadlocks.
            SyncRoot = peer.SyncRoot;

            _parentChannel = parentChannel;
            Address = parentChannel.Address;
            Peer = peer;
            IsConnected = true;
            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true, Logger);
        }

        /// <summary>
        /// Buils a channel to send and receive messages.
        /// </summary>
        /// <param name="peer">
        /// Client peer.
        /// </param>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        internal LocalServerChildChannel(LocalClientChannel peer, Pipeline pipeline, LocalServerChannel parentChannel,
            ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
            // Child channel must use the client channel SyncRoot to avoid nasty deadlocks.
            SyncRoot = peer.SyncRoot;

            _parentChannel = parentChannel;
            Address = parentChannel.Address;
            Peer = peer;
            IsConnected = true;
            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true, Logger);
        }

        /// <summary>
        /// Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name="peer">
        /// Client peer.
        /// </param>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier to compute keys to match requests with responses.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        internal LocalServerChildChannel(LocalClientChannel peer, Pipeline pipeline, LocalServerChannel parentChannel,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
            // Child channel must use the client channel SyncRoot to avoid nasty deadlocks.
            SyncRoot = peer.SyncRoot;

            _parentChannel = parentChannel;
            Address = parentChannel.Address;
            Peer = peer;
            IsConnected = true;
            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true, Logger);
        }

        /// <summary>
        /// It returns the parent channel.
        /// </summary>
        public IServerChannel ParentChannel
        {
            get { return _parentChannel; }
        }

        /// <summary>
        /// Called when the channel address will be changed (before the change occurs).
        /// </summary>
        /// <param name="newAddress">
        /// The channel new address.
        /// </param>
        /// <remarks>
        /// Used by child channels to notify parent of the change.
        /// </remarks>
        protected override void OnChannelAddressChange(IChannelAddress newAddress)
        {
            base.OnChannelAddressChange(ChannelAddress);

            if (!_parentChannel.ChildAddressChange(this, newAddress))
                throw new BugException("Parent server channel doesn't know the child, check configuration");
        }
        
        protected override sealed void OnDisconnection()
        {
            if (!IsConnected)
                return;

            base.OnDisconnection();

            CancelPendingRequests();
            _parentChannel.ChildDisconnection(this);
        }
    }
}