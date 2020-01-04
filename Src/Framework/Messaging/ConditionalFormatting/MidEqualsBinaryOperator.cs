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
    /// This class implements the equals operator of two expressions, the first it's a
    /// set of bytes of a field value, the second it's a binary constant.
    /// </summary>
    [Serializable]
    public class MidEqualsBinaryOperator : EqualityEqualsOperator
    {
        private int _length;
        private int _startIndex;
        private BinaryConstantExpression _valueExpression;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        public MidEqualsBinaryOperator()
        {
            _valueExpression = null;
            _startIndex = 0;
            _length = 0;
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="messageExpression">
        /// The message expression, source of the field value of the equality
        /// operator (left part of the operator).
        /// </param>
        /// <param name="valueExpression">
        /// The value expression of the equality operator (right part of the operator).
        /// </param>
        /// <param name="startIndex">
        /// The index of the start of the set of bytes.
        /// </param>
        /// <param name="length">
        /// The length of the set of bytes.
        /// </param>
        public MidEqualsBinaryOperator(IMessageExpression messageExpression,
            BinaryConstantExpression valueExpression, int startIndex, int length)
            : base(messageExpression)
        {
            if (startIndex < 0)
                throw new ArgumentException(
                    "The start index of the set of bytes can't be a negative value.", "startIndex");

            if (length < 0)
                throw new ArgumentException(
                    "The length of the set of bytes can't be a negative value.", "length");

            _startIndex = startIndex;
            _length = length;

            ValueExpression = valueExpression;
        }

        /// <summary>
        /// It returns or sets the value expression of the equality operator (right
        /// part of the operator).
        /// </summary>
        public override sealed IValueExpression ValueExpression
        {
            get { return _valueExpression; }

            set
            {
                if ((value != null) && !(value is BinaryConstantExpression))
                    throw new ArgumentException("A BinaryConstantExpression type was expected.");

                _valueExpression = value as BinaryConstantExpression;
            }
        }

        /// <summary>
        /// It returns or sets the index of the start of the set of bytes.
        /// </summary>
        public int StartIndex
        {
            get { return _startIndex; }

            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "The start index of the set of bytes can't be a negative value.");

                _startIndex = value;
            }
        }

        /// <summary>
        /// It returns or sets the length of the set of bytes.
        /// </summary>
        public int Length
        {
            get { return _length; }

            set
            {
                if (value < 0)
                    throw new ArgumentException(
                        "The length of the set of bytes can't be a negative value.");

                _length = value;
            }
        }

        private bool CompareByteArrays(byte[] data1, byte[] data2)
        {
            // If both are null, they're equal
            if (data1 == null && data2 == null)
                return true;

            // If either but not both are null, they're not equal
            if (data1 == null || data2 == null)
                return false;

            if (data1.Length != data2.Length)
                return false;

            for (int i = data1.Length - 1; i >= 0; i--)
                if (data1[i] != data2[i])
                    return false;

            return true;
        }

        /// <summary>
        /// It builds the set of bytes from the field value.
        /// </summary>
        /// <param name="fieldValue">
        /// The field value.
        /// </param>
        /// <returns>
        /// A byte array which is the set of bytes from the field value..
        /// </returns>
        private byte[] GetSet(byte[] fieldValue)
        {
            if (fieldValue == null)
                return null;

            if (_startIndex > fieldValue.Length)
                throw new ExpressionEvaluationException(
                    "The start index of the set of bytes is greater than the field value length.");

            if (_startIndex > (fieldValue.Length - _length))
                throw new ExpressionEvaluationException(
                    "There isn't enough data in the field value to get a subset of bytes.");

            byte[] data = null;

            if (_length > 0)
            {
                data = new byte[_length];

                int j = _length - 1;
                for (int i = _startIndex + _length - 1; i >= _startIndex; i--)
                    data[j--] = fieldValue[i];
            }

            return data;
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
        public override bool EvaluateParse(ref ParserContext parserContext)
        {
            return CompareByteArrays(GetSet(MessageExpression.GetLeafFieldValueBytes(
                ref parserContext, null)), _valueExpression.GetValue());
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
        public override bool EvaluateFormat(Field field, ref FormatterContext formatterContext)
        {
            return CompareByteArrays(GetSet(MessageExpression.GetLeafFieldValueBytes(
                ref formatterContext, null)), _valueExpression.GetValue());
        }
    }
}