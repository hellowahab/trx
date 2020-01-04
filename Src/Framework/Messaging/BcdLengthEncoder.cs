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
    /// This class implements a length encoder in BCD (Binary
    /// Coded Decimal).
    /// </summary>
    /// <remarks>
    /// Length encoders are used when the messaging components data
    /// is variable.
    /// This class implements the Singleton pattern, you must use
    /// <see cref="GetInstance"/> to acquire the instance.
    /// </remarks>
    public class BcdLengthEncoder : ILengthEncoder
    {
        // One for each supported size, if more are required only
        // enlarge the instances array and set new lengths in _lengths.
        private static volatile BcdLengthEncoder[] _instances = {
            null, null, null
        };

        private static readonly int[] Lengths = {99, 9999, 999999};

        private readonly int _lengthsIndex;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="lengthsIndex">
        /// It's the index in _lengths array storing the maximum length
        /// this class instance can encode.
        /// </param>
        private BcdLengthEncoder(int lengthsIndex)
        {
            _lengthsIndex = lengthsIndex;
        }

        /// <summary>
        /// It's the maximum length to encode.
        /// </summary>
        public int MaximumLength
        {
            get { return Lengths[_lengthsIndex]; }
        }

        /// <summary>
        /// It returns an instance of <see cref="BcdLengthEncoder"/>
        /// class.
        /// </summary>
        /// <param name="maximumLength">
        /// It's the maximum length to encode.
        /// </param>
        /// <returns>
        /// An instance of <see cref="BcdLengthEncoder"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// It's thrown when <paramref name="maximumLength"/> holds an invalid value.
        /// </exception>
        public static BcdLengthEncoder GetInstance(int maximumLength)
        {
            if (maximumLength < 0)
                throw new ArgumentOutOfRangeException("maximumLength", maximumLength,
                    "Can't be lower than zero.");

            if (maximumLength > Lengths[Lengths.Length - 1])
                throw new ArgumentOutOfRangeException("maximumLength", maximumLength,
                    string.Format("Only 0 to {0} is allowed.", Lengths[Lengths.Length - 1]));

            int index = 0;
            for (; index < Lengths.Length; index++)
                if (maximumLength <= Lengths[index])
                    break;

            if (_instances[index] == null)
                lock (typeof (BcdLengthEncoder))
                {
                    if (_instances[index] == null)
                        _instances[index] = new BcdLengthEncoder(index);
                }

            return _instances[index];
        }

        #region ILengthEncoder Members
        /// <summary>
        /// It returns the length in bytes of the length indicator.
        /// </summary>
        public int EncodedLength
        {
            get { return _lengthsIndex + 1; }
        }

        /// <summary>
        /// It formats the length of the data of the messaging components.
        /// </summary>
        /// <param name="length">
        /// It's the length to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to store the formatted length.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// It's thrown when <paramref name="length"/> is greater than the
        /// maximum value supported by the instance.
        /// </exception>
        public void Encode(int length, ref FormatterContext formatterContext)
        {
            if (length > Lengths[_lengthsIndex])
                throw new ArgumentOutOfRangeException("length", length,
                    string.Format("Must be less or equal than {0}.", Lengths[_lengthsIndex]));

            // Check if we must resize our buffer.
            if (formatterContext.FreeBufferSpace < (_lengthsIndex + 1))
                formatterContext.ResizeBuffer(_lengthsIndex + 1);

            byte[] buffer = formatterContext.GetBuffer();

            // Write encoded length.
            for (int i = formatterContext.UpperDataBound + _lengthsIndex;
                i >= formatterContext.UpperDataBound;
                i--)
            {
                buffer[i] = (byte) ((((length%100)/10) << 4) + (length%10));
                length /= 100;
            }

            // Update formatter context upper data bound.
            formatterContext.UpperDataBound += _lengthsIndex + 1;
        }

        /// <summary>
        /// Gets the encoded length from the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context holding the data to be parsed.
        /// </param>
        /// <returns>
        /// The length parsed from the parser context.
        /// </returns>
        public int Decode(ref ParserContext parserContext)
        {
            // Check available data.
            if (parserContext.DataLength < (_lengthsIndex + 1))
                throw new ArgumentException("Insufficient data.", "parserContext");

            int length = 0;
            byte[] buffer = parserContext.GetBuffer();
            int offset = parserContext.LowerDataBound;

            // Decode length.
            for (int i = offset; i <= (offset + _lengthsIndex); i++)
            {
                int value = (buffer[i] & 0xF0) >> 4;
                if (value > 9)
                    throw new MessagingException(
                        string.Format("Invalid length detected, expecting a digit but {0} was found.", value));
                length = length*10 + value;

                value = buffer[i] & 0x0F;
                if (value > 9)
                    throw new MessagingException(
                        string.Format("Invalid data type detected, expecting a digit but {0} was found.", value));
                length = length*10 + value;
            }

            // Consume parser context data.
            parserContext.Consumed(_lengthsIndex + 1);

            return length;
        }
        #endregion
    }
}