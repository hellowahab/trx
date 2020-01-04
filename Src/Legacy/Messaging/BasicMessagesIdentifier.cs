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

using System.Text;

namespace Trx.Messaging
{
    public class BasicMessagesIdentifier : IMessagesIdentifier
    {
        private readonly int[] _fields;

        public BasicMessagesIdentifier(int[] fields)
        {
            _fields = fields;
        }

        public BasicMessagesIdentifier(int firstFieldNumber,
            int secondFieldNumber)
        {
            _fields = new[] {firstFieldNumber, secondFieldNumber};
        }

        public BasicMessagesIdentifier(int fieldNumber)
        {
            _fields = new[] {fieldNumber};
        }

        public object ComputeIdentifier(Message message)
        {
            if (!message.Fields.Contains(_fields))
                return null;

            if (_fields.Length > 1)
            {
                var identifier = new StringBuilder();

                foreach (int t in _fields)
                    identifier.Append(message.Fields[t].ToString());

                return identifier.ToString();
            }

            return message.Fields[_fields[0]].ToString();
        }
    }
}