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

namespace Trx.Messaging.ConditionalFormatting
{
    /// <summary>
    /// This class implements the message expression.
    /// </summary>
    [Serializable]
    public class MessageExpression : IMessageExpression
    {
        private int _fieldNumber;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public MessageExpression()
        {
            _fieldNumber = -1;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// The leaf field number.
        /// </param>
        public MessageExpression(int fieldNumber)
        {
            _fieldNumber = fieldNumber;
        }

        /// <summary>
        /// It returns or sets the leaf field number.
        /// </summary>
        public int FieldNumber
        {
            get { return _fieldNumber; }

            set { _fieldNumber = value; }
        }

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
        public Message GetLeafMessage(ref ParserContext parserContext, Message message)
        {
            if (message == null)
                message = parserContext.CurrentMessage;

            if (message == null)
                throw new ExpressionEvaluationException("The message is null.");

            return message;
        }

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
        public Message GetLeafMessage(ref FormatterContext formatterContext, Message message)
        {
            if (message == null)
                message = formatterContext.CurrentMessage;

            if (message == null)
                throw new ExpressionEvaluationException("The message is null.");

            return message;
        }

        /// <summary>
        /// It returns the leaf field number of a message hierarchy.
        /// </summary>
        /// <returns>
        /// The leaf field number.
        /// </returns>
        public int GetLeafFieldNumber()
        {
            return _fieldNumber;
        }

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
        public string GetLeafFieldValueString(ref ParserContext parserContext, Message message)
        {
            if (message == null)
                message = parserContext.CurrentMessage;

            return GetString(message);
        }

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
        public string GetLeafFieldValueString(ref FormatterContext formatterContext, Message message)
        {
            if (message == null)
                message = formatterContext.CurrentMessage;

            return GetString(message);
        }

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
        public byte[] GetLeafFieldValueBytes(ref ParserContext parserContext, Message message)
        {
            if (message == null)
                message = parserContext.CurrentMessage;

            return GetBytes(message);
        }

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
        public byte[] GetLeafFieldValueBytes(ref FormatterContext formatterContext, Message message)
        {
            if (message == null)
                message = formatterContext.CurrentMessage;

            return GetBytes(message);
        }

        /// <summary>
        /// It returns the field value as string.
        /// </summary>
        /// <returns>
        /// The field value.
        /// </returns>
        private string GetString(Message message)
        {
            if (message == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get value of field {0} because the message is null.",
                    _fieldNumber));

            if (!message.Fields.Contains(_fieldNumber))
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get value of field {0} because isn't present in the message.",
                    _fieldNumber));

            return message[_fieldNumber].ToString();
        }

        /// <summary>
        /// It returns the field value as a byte array.
        /// </summary>
        /// <returns>
        /// The field value.
        /// </returns>
        private byte[] GetBytes(Message message)
        {
            if (message == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get value of field {0} because the message is null.",
                    _fieldNumber));

            if (!message.Fields.Contains(_fieldNumber))
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get value of field {0} because isn't present in the message.",
                    _fieldNumber));

            return message[_fieldNumber].GetBytes();
        }
    }
}