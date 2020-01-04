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

using System.Collections.Generic;

namespace Trx.Messaging
{
    /// <summary>
    /// Used to create message field formatter definition. Consumed by <see cref="BasicMessageFormatter"/>.
    /// </summary>
    public class FormatterDefinition
    {
        private readonly List<FieldFormatterFactory> _fieldsFormattersFactories = new List<FieldFormatterFactory>();
        private readonly List<RestrictedLogField> _restrictedLogFields = new List<RestrictedLogField>();

        /// <summary>
        /// Clear description (set to null).
        /// </summary>
        public bool ClearDescription { get; set; }

        /// <summary>
        /// Get or set the description.
        /// </summary>
        /// <remarks>
        /// To clear a previous description set <see ref="ClearDescription"/> to true.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Clear the packet header (set to null).
        /// </summary>
        public bool ClearPacketHeader { get; set; }

        /// <summary>
        /// Get or set the packet header.
        /// </summary>
        /// <remarks>
        /// To clear a previous packet header set <see ref="ClearPacketHeader"/> to true.
        /// </remarks>
        public string PacketHeader { get; set; }

        /// <summary>
        /// Set packet header, but can be specified in hex (i.e. 840 = 383430).
        /// </summary>
        /// <remarks>
        /// To clear a previous packet header set <see ref="ClearPacketHeader"/> to true.
        /// </remarks>
        public string HexadecimalPacketHeader { get; set; }

        /// <summary>
        /// Clear message header formatter (set to null).
        /// </summary>
        public bool ClearMessageHeaderFormatter { get; set; }

        /// <summary>
        /// It returns or assigns the message header formatter. 
        /// </summary>
        /// <remarks>
        /// To clear a previous packet header set <see ref="ClearMessageHeaderFormatter"/> to true.
        /// </remarks>
        public IMessageHeaderFormatter MessageHeaderFormatter { get; set; }

        public bool ClearMessageSecuritySchema { get; set; }

        public MessageSecuritySchema MessageSecuritySchema { get; set; }

        /// <summary>
        /// Clear message fields formatters collection.
        /// </summary>
        public bool ClearFieldsFormatters { get; set; }

        /// <summary>
        /// It returns the collection of field formatters factories known by this formatter definition.
        /// </summary>
        /// <remarks>
        /// To clear a previous packet header set <see ref="ClearFieldsFormatters"/> to true.
        /// </remarks>
        public List<FieldFormatterFactory> FieldsFormattersFactories
        {
            get { return _fieldsFormattersFactories; }
        }
    }
}