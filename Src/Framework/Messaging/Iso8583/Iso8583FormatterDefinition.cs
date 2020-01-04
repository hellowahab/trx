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

namespace Trx.Messaging.Iso8583
{
    /// <summary>
    /// Used to create message field formatter definition. Consumed by <see cref="Iso8583MessageFormatter"/>.
    /// </summary>
    public class Iso8583FormatterDefinition : FormatterDefinition
    {
        /// <summary>
        /// Clear message type identifier formatter (set to null).
        /// </summary>
        public bool ClearMessageTypeIdentifierFormatter { get; set; }

        /// <summary>
        /// Get or set the message type identifier formatter.
        /// </summary>
        /// <remarks>
        /// To clear a previous message type identifier formatter set <see ref="ClearMessageTypeIdentifierFormatter"/> to true.
        /// </remarks>
        public StringFieldFormatterFactory MessageTypeIdentifierFormatter { get; set; }
    }
}
