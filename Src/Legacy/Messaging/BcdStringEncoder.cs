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
    /// This class allows to format and parse messaging components,
    /// producing and consuming data in BCD (Binary Coded Decimal).
    /// BCD uses 4 bits (a nibble) in a byte to represent a decimal
    /// digit, i.e., if we have the number 4531, it is stored in two
    /// bytes, the first will contain 0x45 and the second 0x31.
    /// When the number of digits to encode or decode in BCD is odd,
    /// this class allows to choose if the nibble not used is stored
    /// in the left or the right; likewise the value of those 4 bits
    /// can be selected (by default is 0).
    /// </summary>
    /// <remarks>
    /// This class implements the Singleton pattern, you must use
    /// <see cref="GetInstance"/> to acquire the instance.
    ///
    /// Replaced by class HexDataEncoder.
    /// </remarks>
    [Obsolete("Replaced by class BcdDataEncoder")]
    public class BcdStringEncoder : IStringEncoder
    {
        private static volatile BcdStringEncoder[] _leftPaddedInstance = {
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null
        };

        private static volatile BcdStringEncoder[] _rightPaddedInstance = {
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null
        };

        private readonly bool _leftPadded;
        private readonly byte _pad;

        /// <summary>
        /// It initializes a new instance of the encoder.
        /// </summary>
        /// <param name="leftPadded">
        /// If <see langref="true"/> and the number of digits to encode
        /// or decode is odd, the 4 bits not used are placed at the left,
        /// if <see langref="false"/> the 4 bits not used are placed at
        /// the right.
        /// </param>
        /// <param name="pad">
        /// It's the value for the 4 bits not used.
        /// </param>
        private BcdStringEncoder(bool leftPadded, byte pad)
        {
            _leftPadded = leftPadded;
            _pad = pad;
        }

        /// If <see langref="true"/> and the number of digits to encode
        /// or decode is odd, the 4 bits not used are placed at the left,
        /// if <see langref="false"/> the 4 bits not used are placed at
        /// the right.
        public bool LeftPadded
        {
            get { return _leftPadded; }
        }

        /// It's the value for the 4 bits not used.
        public byte Pad
        {
            get { return _pad; }
        }

        /// <summary>
        /// It initializes a new instace of the class.
        /// </summary>
        /// <param name="leftPadded">
        /// If <see langref="true"/> and the number of digits to encode
        /// or decode is odd, the 4 bits not used are placed at the left,
        /// if <see langref="false"/> the 4 bits not used are placed at
        /// the right.
        /// </param>
        /// <param name="pad">
        /// It's the value for the 4 bits not used.
        /// </param>
        /// <returns>
        /// An instance of <see cref="BcdStringEncoder"/>.
        /// </returns>
        public static BcdStringEncoder GetInstance(bool leftPadded, byte pad)
        {
            if ((pad < 0) || (pad > 15))
                throw new ArgumentOutOfRangeException("pad", pad, "Pad value must be between 0 and 15.");

            BcdStringEncoder[] instances = leftPadded ? _leftPaddedInstance : _rightPaddedInstance;
            if (instances[pad] == null)
                lock (typeof (BcdStringEncoder))
                {
                    if (instances[pad] == null)
                        instances[pad] = new BcdStringEncoder(leftPadded, pad);
                }

            return instances[pad];
        }

        #region IStringEncoder Members
        /// <summary>
        /// It computes the encoded data length for the given data length.
        /// </summary>
        /// <param name="dataLength">
        /// It's the length of the data to be encoded.
        /// </param>
        /// <returns>
        /// The length of the encoded data for the given data length.
        /// </returns>
        public int GetEncodedLength(int dataLength)
        {
            return (dataLength + 1) >> 1;
        }

        /// <summary>
        /// It encodes the given data.
        /// </summary>
        /// <param name="data">
        /// It's the data to encode.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to store the encoded data.
        /// </param>
        public void Encode(string data, ref FormatterContext formatterContext)
        {
            int length = (data.Length + 1) >> 1;

            // Check if we must resize formatter context buffer.
            if (formatterContext.FreeBufferSpace < length)
                formatterContext.ResizeBuffer(length);

            byte[] buffer = formatterContext.GetBuffer();
            int offset = formatterContext.UpperDataBound;

            // Initialize result bytes.
            for (int i = offset + length - 1; i >= offset; i--)
                buffer[i] = 0;

            // Format data.
            int start = (((data.Length & 1) == 1) && _leftPadded) ? 1 : 0;
            for (int i = start; i < (start + data.Length); i++)
                if (data[i - start] < 0x40)
                    buffer[offset + (i >> 1)] |= (byte) (((data[i - start]) - 0x30) <<
                        ((i & 1) == 1 ? 0 : 4));
                else
                    buffer[offset + (i >> 1)] |= (byte) (((data[i - start]) - 0x37) <<
                        ((i & 1) == 1 ? 0 : 4));

            // Pad if required.
            if ((_pad >= 0) && (data.Length & 1) == 1)
                if (_leftPadded)
                    buffer[offset] |= (byte) (_pad << 4);
                else
                    buffer[offset + length - 1] |= (byte) (0x0F & _pad);

            formatterContext.UpperDataBound += length;
        }

        /// <summary>
        /// It decodes the data.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context holding the data to be parsed.
        /// </param>
        /// <param name="length">
        /// It's the length of the data to get from the parser context.
        /// </param>
        /// <returns>
        /// A string with the decoded data.
        /// </returns>
        public string Decode(ref ParserContext parserContext, int length)
        {
            if (parserContext.DataLength < ((length + 1) >> 1))
                throw new ArgumentException("Insufficient data.", "length");

            var result = new char[length];
            byte[] buffer = parserContext.GetBuffer();
            int offset = parserContext.LowerDataBound;
            int start = (((length & 1) == 1) && _leftPadded) ? 1 : 0;

            for (int i = start; i < length + start; i++)
            {
                int shift = ((i & 1) == 1 ? 0 : 4);

                int c = ((buffer[offset + (i >> 1)] >> shift) & 0x0F);

                if (c < 10)
                    c += 0x30;
                else
                    c += 0x37;

                result[i - start] = (char) c;
            }

            parserContext.Consumed((length + 1) >> 1);

            return new string(result);
        }
        #endregion
    }
}