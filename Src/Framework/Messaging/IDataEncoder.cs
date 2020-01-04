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

namespace Trx.Messaging
{
    public interface IDataEncoder
    {
        /// <summary>
        /// It returns the length of the encoded data for the given length of data.
        /// </summary>
        /// <param name="dataLength">
        /// The length of the data to encode.
        /// </param>
        /// <returns>
        /// The length of the enconded data.
        /// </returns>
        int GetEncodedLength(int dataLength);

        /// <summary>
        /// Encode the given data.
        /// </summary>
        /// <param name="data">
        /// The data to encode.
        /// </param>
        /// <param name="formatterContext">
        /// The formatter context.
        /// </param>
        void Encode(string data, ref FormatterContext formatterContext);

        /// <summary>
        /// Encode the given data.
        /// </summary>
        /// <param name="data">
        /// The data to encode.
        /// </param>
        /// <param name="formatterContext">
        /// The formatter context.
        /// </param>
        void Encode(byte[] data, ref FormatterContext formatterContext);

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
        string DecodeString(ref ParserContext parserContext, int length);

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
        byte[] DecodeBytes(ref ParserContext parserContext, int length);
    }
}
