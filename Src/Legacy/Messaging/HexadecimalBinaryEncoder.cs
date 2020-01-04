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
    /// <remarks>
    /// Replaced by class HexDataEncoder.
    /// </remarks>
    [Obsolete("Replaced by class HexDataEncoder")]
    public class HexadecimalBinaryEncoder : IBinaryEncoder
    {
        private static volatile HexadecimalBinaryEncoder _instance;

        private static readonly byte[] HexadecimalAsciiDigits = {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
            0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46
        };

        private HexadecimalBinaryEncoder()
        {
        }

        public static HexadecimalBinaryEncoder GetInstance()
        {
            if (_instance == null)
                lock (typeof (HexadecimalBinaryEncoder))
                {
                    if (_instance == null)
                        _instance = new HexadecimalBinaryEncoder();
                }

            return _instance;
        }

        #region IBinaryEncoder Members
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
            return dataLength << 1;
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
        public void Encode(byte[] data, ref FormatterContext formatterContext)
        {
            // Check if we must resize formatter context buffer.
            if (formatterContext.FreeBufferSpace < (data.Length << 1))
                formatterContext.ResizeBuffer(data.Length << 1);

            byte[] buffer = formatterContext.GetBuffer();
            int offset = formatterContext.UpperDataBound;

            // Format data.
            for (int i = 0; i < data.Length; i++)
            {
                buffer[offset + (i << 1)] = HexadecimalAsciiDigits[(data[i] & 0xF0) >> 4];
                buffer[offset + (i << 1) + 1] = HexadecimalAsciiDigits[data[i] & 0x0F];
            }

            formatterContext.UpperDataBound += data.Length << 1;
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
        /// A byte array with the decoded data.
        /// </returns>
        public byte[] Decode(ref ParserContext parserContext, int length)
        {
            if (parserContext.DataLength < (length << 1))
                throw new ArgumentException("Insufficient data.", "length");

            var result = new byte[length];
            byte[] buffer = parserContext.GetBuffer();
            int offset = parserContext.LowerDataBound;

            for (int i = offset + (length << 1) - 1; i >= offset; i--)
            {
                int right = buffer[i] > 0x40
                    ? 10 + (buffer[i] > 0x60
                        ? buffer[i] - 0x61
                        : buffer[i] - 0x41)
                    : buffer[i] - 0x30;
                int left = buffer[--i] > 0x40
                    ? 10 + (buffer[i] > 0x60
                        ? buffer[i] - 0x61
                        : buffer[i] - 0x41)
                    : buffer[i] - 0x30;

                result[(i - offset) >> 1] = (byte) ((left << 4) | right);
            }

            parserContext.Consumed(length << 1);

            return result;
        }
        #endregion
    }
}