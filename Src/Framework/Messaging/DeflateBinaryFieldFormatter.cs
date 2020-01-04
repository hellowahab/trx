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

using System.IO;
using System.IO.Compression;

namespace Trx.Messaging
{
    public class DeflateBinaryFieldFormatter : BinaryFieldFormatter
    {
        /// <summary>
        /// It initializes a new binary field formatter instance.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the field value encoder.
        /// </param>
        public DeflateBinaryFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder)
            : base(fieldNumber, lengthManager, encoder)
        {
        }

        /// <summary>
        /// It initializes a new binary field formatter instance.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the field value encoder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public DeflateBinaryFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            string description)
            : base(fieldNumber, lengthManager, encoder, description)
        {
        }

        protected override void InternalFormat(Field field, ref FormatterContext formatterContext, byte[] fieldValue)
        {
            // Compress the received value.
            using (var ms = new MemoryStream())
            using (var ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                ds.Write(fieldValue, 0, fieldValue.Length);
                ds.Close(); // Force flush.
                fieldValue = ms.ToArray();
                LengthManager.WriteLength(field, fieldValue.Length, Encoder.GetEncodedLength(fieldValue.Length),
                    ref formatterContext);
                Encoder.Encode(fieldValue, ref formatterContext);
                LengthManager.WriteLengthTrailer(field, fieldValue.Length, Encoder.GetEncodedLength(fieldValue.Length),
                    ref formatterContext);
            }
        }

        protected override byte[] InternalParse(ref ParserContext parserContext)
        {
            // Uncompress the received value.
            using (var ms = new MemoryStream(Encoder.DecodeBytes(ref parserContext, parserContext.DecodedLength)))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            using (var rs = new MemoryStream())
            {
                ds.CopyTo(rs);
                return rs.ToArray();
            }
        }
    }
}