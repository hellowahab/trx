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

namespace Trx.Communication.Channels
{
    /// <summary>
    /// This class holds info about a received message.
    /// </summary>
    [Serializable]
    public class ReceiveDescriptor
    {
        private readonly IChannelAddress _channelAddress;

        internal ReceiveDescriptor(IChannelAddress channel, object receivedMessage)
            : this(channel)
        {
            ReceivedMessage = receivedMessage;
            UtcReceiveDateTime = DateTime.UtcNow;
        }

        internal ReceiveDescriptor(IChannelAddress channelAddress)
        {
            _channelAddress = channelAddress;
        }

        /// <summary>
        /// Receive UTC date and time.
        /// </summary>
        public DateTime UtcReceiveDateTime { get; protected set; }

        /// <summary>
        /// Source channel.
        /// </summary>
        public IChannelAddress ChannelAddress
        {
            get { return _channelAddress; }
        }

        /// <summary>
        /// Received message.
        /// </summary>
        public object ReceivedMessage { get; protected set; }
    }
}