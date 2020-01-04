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
    /// It's a message to be sent to a specific channel referenced by it's address.
    /// </summary>
    [Serializable]
    public class MessageToAddress
    {
        public MessageToAddress(object message, IChannelAddress channelAddress)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (channelAddress == null)
                throw new ArgumentNullException("channelAddress");

            Message = message;
            ChannelAddress = channelAddress;
        }

        public IChannelAddress ChannelAddress { get; private set; }

        public object Message { get; private set; }
    }
}
