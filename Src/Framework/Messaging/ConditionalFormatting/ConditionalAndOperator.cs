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
    /// This class implements the and operator of two expressions.
    /// </summary>
    [Serializable]
    public class ConditionalAndOperator : IBooleanExpression
    {
        private IBooleanExpression _leftExpression;
        private IBooleanExpression _rightExpression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public ConditionalAndOperator()
        {
            _leftExpression = null;
            _rightExpression = null;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="leftExpression">
        /// The left expression of the and operator.
        /// </param>
        /// <param name="rightExpression">
        /// The right expression of the and operator.
        /// </param>
        public ConditionalAndOperator(IBooleanExpression leftExpression, IBooleanExpression rightExpression)
        {
            _leftExpression = leftExpression;
            _rightExpression = rightExpression;
        }

        /// <summary>
        /// It returns or sets the left expression of the and operator.
        /// </summary>
        public IBooleanExpression LeftExpression
        {
            get { return _leftExpression; }

            set { _leftExpression = value; }
        }

        /// <summary>
        /// It returns or sets the right expression of the and operator.
        /// </summary>
        public IBooleanExpression RightExpression
        {
            get { return _rightExpression; }

            set { _rightExpression = value; }
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
            return _leftExpression.EvaluateParse(ref parserContext) &&
                _rightExpression.EvaluateParse(ref parserContext);
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
            return _leftExpression.EvaluateFormat(field, ref formatterContext) &&
                _rightExpression.EvaluateFormat(field, ref formatterContext);
        }
    }
}