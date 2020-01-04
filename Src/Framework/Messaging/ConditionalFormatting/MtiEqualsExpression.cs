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
using Trx.Messaging.Iso8583;

namespace Trx.Messaging.ConditionalFormatting
{
    /// <summary>
    /// This class implements equality comparison between a
    /// given MTI and the message MTI of the formatting/parsing operation.
    /// </summary>
    [Serializable]
    public class MtiEqualsExpression : IBooleanExpression
    {
        private IMessageExpression _messageExpression;
        private int _mti;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public MtiEqualsExpression()
        {
            _mti = -1;
            _messageExpression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="mti">
        /// MTI to compare with the message MTI.
        /// </param>
        /// <param name="messageExpression">
        /// The expression which supply the message to compare the MTI.
        /// </param>
        public MtiEqualsExpression(int mti, IMessageExpression messageExpression)
        {
            _mti = mti;
            _messageExpression = messageExpression;
        }

        /// <summary>
        /// It returns or sets the MTI to compare with the message MTI.
        /// </summary>
        public int Mti
        {
            get { return _mti; }

            set { _mti = value; }
        }

        /// <summary>
        /// It returns or sets the message expression.
        /// </summary>
        public IMessageExpression MessageExpression
        {
            get { return _messageExpression; }

            set { _messageExpression = value; }
        }

        /// <summary>
        /// Evaluates the expression when parsing a message.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// A boolean value.
        /// </returns>
        public bool EvaluateParse(ref ParserContext parserContext)
        {
            return GetMessage(_messageExpression.GetLeafMessage(
                ref parserContext, null)).MessageTypeIdentifier == _mti;
        }

        /// <summary>
        /// Evaluates the expression when formatting a message.
        /// </summary>
        /// <param name="field">
        /// It's the field to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        /// <returns>
        /// A boolean value.
        /// </returns>
        public bool EvaluateFormat(Field field, ref FormatterContext formatterContext)
        {
            return GetMessage(_messageExpression.GetLeafMessage(
                ref formatterContext, null)).MessageTypeIdentifier == _mti;
        }

        /// <summary>
        /// It returns the field value as string.
        /// </summary>
        /// <returns>
        /// The field value.
        /// </returns>
        private Iso8583Message GetMessage(Message message)
        {
            var isoMsg = message as Iso8583Message;

            if (isoMsg == null)
                throw new ExpressionEvaluationException(
                    "Can't compare MTI against a non ISO 8583 message.");

            return isoMsg;
        }
    }
}