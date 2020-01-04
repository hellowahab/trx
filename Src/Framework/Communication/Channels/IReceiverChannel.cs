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

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Defines a channel cappable to send messages.
    /// </summary>
    public interface IReceiverChannel : IChannel
    {
        /// <summary>
        /// Space where the received messages (and completed/timedout requests).
        /// </summary>
        ITupleSpace<ReceiveDescriptor> TupleSpace
        {
            get;
        }

        /// <summary>
        /// Time in milliseconds used by the channel implementation when writing a received
        /// message in the receive tuple space. Default value is Timeout.Infinite.
        /// </summary>
        int TupleSpaceTtl
        {
            get;
            set;
        }

        /// <summary>
        /// Object used to get keys from messages to match requests with responses.
        /// </summary>
        IMessagesIdentifier MessagesIdentifier
        {
            get;
        }
    }
}
