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

using System.Net.Sockets;
using Trx.Coordination.TupleSpace;
using Trx.Exceptions;

namespace Trx.Communication.Channels.Tcp
{
    public class TcpServerChildChannel : TcpBaseSenderReceiverChannel, IServerChildChannel
    {
        private TcpServerChannel _parentChannel;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="fireOnConnected">
        /// If true, the constructor fires OnConnected event and start to receive data.
        /// </param>
        internal TcpServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel, Socket socket,
            string name, bool fireOnConnected)
            : base(pipeline)
        {
            ConstructorHelper(parentChannel, socket, name, fireOnConnected);
        }

        /// <summary>
        /// Buils a channel to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="fireOnConnected">
        /// If true, the constructor fires OnConnected event and start to receive data.
        /// </param>
        internal TcpServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel, Socket socket,
            string name, ITupleSpace<ReceiveDescriptor> tupleSpace, bool fireOnConnected)
            : base(pipeline, tupleSpace)
        {
            ConstructorHelper(parentChannel, socket, name, fireOnConnected);
        }

        /// <summary>
        /// Buils a channel to send and receive messages and request responses.
        /// </summary>
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
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="fireOnConnected">
        /// If true, the constructor fires OnConnected event and start to receive data.
        /// </param>
        internal TcpServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel, Socket socket,
            string name, ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier,
            bool fireOnConnected)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
            ConstructorHelper(parentChannel, socket, name, fireOnConnected);
        }

        private void ConstructorHelper(TcpServerChannel parentChannel, Socket socket, string name, bool fireOnConnected)
        {
            _parentChannel = parentChannel;
            Name = name;
            Socket = socket;
            IsConnected = true;

            if (fireOnConnected)
            {
                Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true, Logger);
                StartAsyncReceive();
            }
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

        protected override void OnDisconnection()
        {
            if (!IsConnected)
                return;

            base.OnDisconnection();

            // Cancel pending requests because a child channel doesn't reconnect.
            CancelPendingRequests();

            _parentChannel.ChildDisconnection(this);
            Dispose();
        }
    }
}