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

namespace Trx.Messaging
{
    /// <summary>
    /// It defines a messages formatter.
    /// </summary>
    public interface IMessageFormatter : ICloneable
    {
        /// <summary>
        /// It returns the collection of field formatters known by this instance of messages formatter.
        /// </summary>
        FieldFormatterCollection FieldFormatters { get; }

        MessageSecuritySchema MessageSecuritySchema { get; set; }

        /// <summary>
        /// It formats a message.
        /// </summary>
        /// <param name="message">
        /// It's the message to be formatted.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to be used in the format.
        /// </param>
        void Format(Message message, ref FormatterContext formatterContext);

        /// <summary>
        /// It parses the data contained in the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// It's the context holding the information to produce a new message instance.
        /// </param>
        /// <returns>
        /// The parsed message, or a null reference if the data contained in the context
        /// is insufficient to produce a new message.
        /// </returns>
        Message Parse(ref ParserContext parserContext);
    }
}