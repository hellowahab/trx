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
using Trx.Utilities;

namespace Trx.Messaging
{
    /// <remarks>
    /// Replaces classes HexadecimalBinaryEncoder and HexadecimalStringEncoder.
    /// </remarks>
    public class HexDataEncoder : IDataEncoder
    {
        private static volatile HexDataEncoder _instance;

        private HexDataEncoder()
        {
        }

        #region IDataEncoder Members
        /// <summary>
        /// It returns the length of the encoded data for the given length of data.
        /// </summary>
        /// <param name="dataLength">
        /// The length of the data to encode.
        /// </param>
        /// <returns>
        /// The length of the enconded data.
        /// </returns>
        public int GetEncodedLength(int dataLength)
        {
            return dataLength << 1;
        }

        /// <summary>
        /// Encode the given data.
        /// </summary>
        /// <param name="data">
        /// The data to encode.
        /// </param>
        /// <param name="formatterContext">
        /// The formatter context.
        /// </param>
        public void Encode(string data, ref FormatterContext formatterContext)
        {
            // Check if we must resize formatter context buffer.
            if (formatterContext.FreeBufferSpace < (data.Length << 1))
                formatterContext.ResizeBuffer(data.Length << 1);

            byte[] buffer = formatterContext.Buffer.GetArray();
            int offset = formatterContext.UpperDataBound;

            // Format data.
            for (int i = 0; i < data.Length; i++)
            {
                buffer[offset + (i << 1)] = StringUtilities.HexadecimalAsciiDigits[(((byte) data[i]) & 0xF0) >> 4];
                buffer[offset + (i << 1) + 1] = StringUtilities.HexadecimalAsciiDigits[((byte) data[i]) & 0x0F];
            }

            formatterContext.UpperDataBound += data.Length << 1;
        }

        /// <summary>
        /// Encode the given data.
        /// </summary>
        /// <param name="data">
        /// The data to encode.
        /// </param>
        /// <param name="formatterContext">
        /// The formatter context.
        /// </param>
        public void Encode(byte[] data, ref FormatterContext formatterContext)
        {
            // Check if we must resize formatter context buffer.
            if (formatterContext.FreeBufferSpace < (data.Length << 1))
                formatterContext.ResizeBuffer(data.Length << 1);

            byte[] buffer = formatterContext.Buffer.GetArray();
            int offset = formatterContext.UpperDataBound;

            // Format data.
            for (int i = 0; i < data.Length; i++)
            {
                buffer[offset + (i << 1)] = StringUtilities.HexadecimalAsciiDigits[(data[i] & 0xF0) >> 4];
                buffer[offset + (i << 1) + 1] = StringUtilities.HexadecimalAsciiDigits[data[i] & 0x0F];
            }

            formatterContext.UpperDataBound += data.Length << 1;
        }

        /// <summary>
        /// Decode the given amount of data in the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// The parser context.
        /// </param>
        /// <param name="length">
        /// The length of the data to decode.
        /// </param>
        /// <returns>
        /// Decoded data.
        /// </returns>
        public string DecodeString(ref ParserContext parserContext, int length)
        {
            return parserContext.Buffer.Encoding.GetString(DecodeBytes(ref parserContext, length));
        }

        /// <summary>
        /// Decode the given amount of data in the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// The parser context.
        /// </param>
        /// <param name="length">
        /// The length of the data to decode.
        /// </param>
        /// <returns>
        /// Decoded data.
        /// </returns>
        public byte[] DecodeBytes(ref ParserContext parserContext, int length)
        {
            if (parserContext.DataLength < (length << 1))
                throw new ArgumentException("Insufficient data.", "length");

            var result = new byte[length];
            byte[] buffer = parserContext.Buffer.GetArray();
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

        public static HexDataEncoder GetInstance()
        {
            if (_instance == null)
                lock (typeof (HexDataEncoder))
                {
                    if (_instance == null)
                        _instance = new HexDataEncoder();
                }

            return _instance;
        }
    }
}