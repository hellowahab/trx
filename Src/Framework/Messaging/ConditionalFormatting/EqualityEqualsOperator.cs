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
    /// This class implements the equals operator of two expressions.
    /// </summary>
    [Serializable]
    public abstract class EqualityEqualsOperator : IBooleanExpression
    {
        private IMessageExpression _messageExpression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        protected EqualityEqualsOperator()
        {
            _messageExpression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="messageExpression">
        /// The message expression, source of the field value of the equality
        /// operator (left part of the operator).
        /// </param>
        protected EqualityEqualsOperator(IMessageExpression messageExpression)
        {
            MessageExpression = messageExpression;
        }

        /// <summary>
        /// It returns or sets the message expression, source of the field value of
        /// the equality operator (left part of the operator).
        /// </summary>
        public IMessageExpression MessageExpression
        {
            get { return _messageExpression; }

            set { _messageExpression = value; }
        }

        /// <summary>
        /// It returns or sets the value expression of the equality operator (right
        /// part of the operator).
        /// </summary>
        public abstract IValueExpression ValueExpression { get; set; }

        #region IBooleanExpression Members
        /// <summary>
        /// Evaluates the expression when parsing a message.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// A boolean value.
        /// </returns>
        public abstract bool EvaluateParse(ref ParserContext parserContext);

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
        public abstract bool EvaluateFormat(Field field, ref FormatterContext formatterContext);
        #endregion
    }
}