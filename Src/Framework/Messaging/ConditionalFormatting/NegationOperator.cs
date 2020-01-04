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
    /// This class implements the not operator.
    /// </summary>
    [Serializable]
    public class NegationOperator : IBooleanExpression
    {
        private IBooleanExpression _expression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public NegationOperator()
        {
            _expression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="expression">
        /// The expression to negate.
        /// </param>
        public NegationOperator(IBooleanExpression expression)
        {
            _expression = expression;
        }

        /// <summary>
        /// It returns or sets the expression to negate.
        /// </summary>
        public IBooleanExpression Expression
        {
            get { return _expression; }

            set { _expression = value; }
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
            return !_expression.EvaluateParse(ref parserContext);
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
            return !_expression.EvaluateFormat(field, ref formatterContext);
        }
    }
}