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
    /// This class implements the sub field message expression, suitable to access
    /// inner messages.
    /// </summary>
    [Serializable]
    public class SubMessageExpression : IMessageExpression
    {
        private int _fieldNumber;
        private IMessageExpression _messageExpression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public SubMessageExpression()
        {
            _fieldNumber = -1;
            _messageExpression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// The field number to get the value.
        /// </param>
        /// <param name="messageExpression">
        /// The sub field message expression.
        /// </param>
        public SubMessageExpression(int fieldNumber, IMessageExpression messageExpression)
        {
            _fieldNumber = fieldNumber;
            _messageExpression = messageExpression;
        }

        /// <summary>
        /// It returns or sets the field number.
        /// </summary>
        public int FieldNumber
        {
            get { return _fieldNumber; }

            set { _fieldNumber = value; }
        }

        /// <summary>
        /// It returns or sets the sub field message expression.
        /// </summary>
        public IMessageExpression MessageExpression
        {
            get { return _messageExpression; }

            set { _messageExpression = value; }
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

            return _messageExpression.GetLeafMessage(ref parserContext, GetInnerMessage(message));
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

            return _messageExpression.GetLeafMessage(ref formatterContext, GetInnerMessage(message));
        }

        /// <summary>
        /// It returns the leaf field number of a message hierarchy.
        /// </summary>
        /// <returns>
        /// The leaf field number.
        /// </returns>
        public int GetLeafFieldNumber()
        {
            return _messageExpression.GetLeafFieldNumber();
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

            return _messageExpression.GetLeafFieldValueString(
                ref parserContext, GetInnerMessage(message));
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

            return _messageExpression.GetLeafFieldValueString(
                ref formatterContext, GetInnerMessage(message));
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

            return _messageExpression.GetLeafFieldValueBytes(
                ref parserContext, GetInnerMessage(message));
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

            return _messageExpression.GetLeafFieldValueBytes(
                ref formatterContext, GetInnerMessage(message));
        }

        /// <summary>
        /// It returns the inner message located in a field (pointed by
        /// _fieldNumber) of a given message.
        /// </summary>
        /// <param name="message">
        /// The given message.
        /// </param>
        /// <returns>
        /// The inner message located in a field (pointed by _fieldNumber)
        /// of a given message.
        /// </returns>
        private Message GetInnerMessage(Message message)
        {
            if (message == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get inner message in field {0} because the message is null.",
                    _fieldNumber));

            if (!message.Fields.Contains(_fieldNumber))
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get inner message in {0} because isn't present in the message.",
                    _fieldNumber));

            var innerMessageField = message[_fieldNumber] as InnerMessageField;

            if (innerMessageField == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get inner message in {0} because isn't an inner message.",
                    _fieldNumber));

            var innerMessage = innerMessageField.Value as Message;

            if (innerMessage == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get inner message in {0} because it doesn't contain a message.",
                    _fieldNumber));

            return innerMessage;
        }
    }
}