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
using System.Text;
using Trx.Communication.Channels;

namespace Trx.Messaging
{
    public class FieldsMessagesIdentifier : IMessagesIdentifier
    {
        private readonly int[] _fields;

        public FieldsMessagesIdentifier(int[] fields)
        {
            _fields = fields;
        }

        public FieldsMessagesIdentifier(int firstFieldNumber,
            int secondFieldNumber)
        {
            _fields = new[] {firstFieldNumber, secondFieldNumber};
        }

        public FieldsMessagesIdentifier(int fieldNumber)
        {
            _fields = new[] {fieldNumber};
        }

        #region IMessagesIdentifier Members
        /// <summary>
        /// Compute message key.
        /// </summary>
        /// <param name="message">
        /// The message whose key is computed.
        /// </param>
        /// <returns>
        /// The message key.
        /// </returns>
        public object ComputeIdentifier(object message)
        {
            var msg = message as Message;
            if (msg == null)
                throw new InvalidOperationException("FieldsMessagesIdentifier only support messages of type Trx.Messaging.Message.");

            if (!msg.Fields.Contains(_fields))
                return null;

            if (_fields.Length > 1)
            {
                var identifier = new StringBuilder();
                foreach (int t in _fields)
                    identifier.Append(msg.Fields[t].ToString());
                return identifier.ToString();
            }

            return msg.Fields[_fields[0]].ToString();
        }
        #endregion
    }
}