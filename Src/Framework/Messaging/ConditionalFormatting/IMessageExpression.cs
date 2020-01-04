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

namespace Trx.Messaging.ConditionalFormatting
{
    /// <summary>
    /// This interface defines an expression which handles a messages hierarchy.
    /// </summary>
    /// <remarks>
    /// Our primary interest with the hierarchy is to access the leaf message
    /// node and a given field value of it (by it's number which is also stored
    /// in the leaf node).
    /// </remarks>
    public interface IMessageExpression {

        /// <summary>
        /// It returns the message.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="message">
        /// The message to get the message field.
        /// </param>
        /// <returns>
        /// The message.
        /// </returns>
        Message GetLeafMessage( ref ParserContext parserContext, Message message );

        /// <summary>
        /// It returns the message.
        /// </summary>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        /// <param name="message">
        /// The message to get the message field.
        /// </param>
        /// <returns>
        /// The message.
        /// </returns>
        Message GetLeafMessage( ref FormatterContext formatterContext, Message message );

        /// <summary>
        /// It returns the leaf field number of a message hierarchy.
        /// </summary>
        /// <returns>
        /// The leaf field number.
        /// </returns>
        int GetLeafFieldNumber();

        /// <summary>
        /// It returns the field value as string.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="message">
        /// The message to get the field.
        /// </param>
        /// <returns>
        /// The field value.
        /// </returns>
        string GetLeafFieldValueString( ref ParserContext parserContext, Message message );

        /// <summary>
        /// It returns the field value as string.
        /// </summary>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        /// <param name="message">
        /// The message to get the field.
        /// </param>
        /// <returns>
        /// The field value.
        /// </returns>
        string GetLeafFieldValueString( ref FormatterContext formatterContext, Message message );

        /// <summary>
        /// It returns the field value as a byte array.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="message">
        /// The message to get the field.
        /// </param>
        /// <returns>
        /// The field value.
        /// </returns>
        byte[] GetLeafFieldValueBytes( ref ParserContext parserContext, Message message );

        /// <summary>
        /// It returns the field value as a byte array.
        /// </summary>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        /// <param name="message">
        /// The message to get the field.
        /// </param>
        /// <returns>
        /// The field value.
        /// </returns>
        byte[] GetLeafFieldValueBytes( ref FormatterContext formatterContext, Message message );
    }
}