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
    /// This class implements the parent message expression, suitable to access
    /// the parent message of an inner message.
    /// </summary>
    [Serializable]
    public class ParentMessageExpression : IMessageExpression
    {
        private IMessageExpression _messageExpression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public ParentMessageExpression()
        {
            _messageExpression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="messageExpression">
        /// The inner message expression.
        /// </param>
        public ParentMessageExpression(IMessageExpression messageExpression)
        {
            _messageExpression = messageExpression;
        }

        /// <summary>
        /// It returns or sets the inner message expression.
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

            return _messageExpression.GetLeafMessage(ref parserContext, GetParentMessage(message));
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

            return _messageExpression.GetLeafMessage(ref formatterContext, GetParentMessage(message));
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
                ref parserContext, GetParentMessage(message));
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
                ref formatterContext, GetParentMessage(message));
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
                ref parserContext, GetParentMessage(message));
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
                ref formatterContext, GetParentMessage(message));
        }

        /// <summary>
        /// It returns the parent message located of a given message.
        /// </summary>
        /// <param name="message">
        /// The given message.
        /// </param>
        /// <returns>
        /// The parent message located of a given message.
        /// </returns>
        private Message GetParentMessage(Message message)
        {
            if (message == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get parent because the child message is null."));

            if (message.Parent == null)
                throw new ExpressionEvaluationException(string.Format(
                    "Can't get parent because the message hasn't one."));

            return message.Parent;
        }
    }
}