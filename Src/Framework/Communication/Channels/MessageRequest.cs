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
    /// Represents a message request for a given channel.
    /// </summary>
    [Serializable]
    public class MessageRequest
    {
        public MessageRequest(object key, object message, int timeout)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (timeout < 1)
                throw new ArgumentOutOfRangeException("timeout", timeout, "Must be greater than zero.");

            Key = key;
            Message = message;
            Timeout = timeout;
        }

        /// <summary>
        /// Returned in the message response.
        /// </summary>
        public object Key { get; private set; }

        public object Message { get; private set; }

        /// <summary>
        /// Returns request timeout in milliseconds.
        /// </summary>
        public int Timeout { get; private set; }
    }
}
