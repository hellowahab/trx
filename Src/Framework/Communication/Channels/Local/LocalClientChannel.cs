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
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Local
{
    public class LocalClientChannel : LocalBaseSenderReceiverChannel, IClientChannel
    {
        private ChannelRequestCtrl _lastSuccessfulConnect;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="address">
        /// Remote peer address.
        /// </param>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        public LocalClientChannel(string address, Pipeline pipeline)
            : base(pipeline)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            Address = address;
        }

        /// <summary>
        /// Buils a channel to send and receive messages.
        /// </summary>
        /// <param name="address">
        /// Remote peer address.
        /// </param>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        public LocalClientChannel(string address, Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            Address = address;
        }

        /// <summary>
        /// Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name="address">
        /// Remote peer address.
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
        public LocalClientChannel(string address, Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            Address = address;
        }

        /// <summary>
        /// Starts the connection of the channel.
        /// </summary>
        /// <returns>
        /// A connection request handler.
        /// </returns>
        public ChannelRequestCtrl Connect()
        {
            lock (SyncRoot)
            {
                if (IsConnected)
                    return _lastSuccessfulConnect;

                try
                {
                    Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.ConnectionRequested),
                        true, Logger);

                    Logger.Info(string.Format("{0}: connecting to {1}.", GetChannelTitle(),Address));

                    Peer = LocalServerRegistry.GetInstance().Connect(Address, this);
                    if (Peer == null)
                    {
                        Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.ConnectionFailed),
                            true, Logger);
                        Logger.Warn(string.Format("{0}: connection refused.", GetChannelTitle()));
                        return new ChannelRequestCtrl(false)
                                   {
                                       Message = "Connection refused."
                                   };
                    }

                    IsConnected = true;
                    _lastSuccessfulConnect = new ChannelRequestCtrl(true);
                    Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true, Logger);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: error caught on connection.", GetChannelTitle()), ex);
                    IsConnected = false;
                    _lastSuccessfulConnect = null;

                    Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.ConnectionFailed),
                        true, Logger);
                    
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }
                return _lastSuccessfulConnect;
            }
        }
    }
}
