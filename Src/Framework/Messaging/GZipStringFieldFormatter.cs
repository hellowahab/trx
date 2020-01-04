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
using Trx.Utilities;

namespace Trx.Messaging
{
    public class GZipStringFieldFormatter : StringFieldFormatter
    {
        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder)
            : base(fieldNumber, lengthManager, encoder)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            string description)
            : base(fieldNumber, lengthManager, encoder, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringPadding padding)
            : base(fieldNumber, lengthManager, encoder, padding)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringPadding padding, string description)
            : base(fieldNumber, lengthManager, encoder, padding, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringValidator validator)
            : base(fieldNumber, lengthManager, encoder, validator)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringValidator validator, string description)
            : base(fieldNumber, lengthManager, encoder, validator, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringPadding padding, IStringValidator validator)
            : base(fieldNumber, lengthManager, encoder, padding, validator)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="valueFormatter">
        /// It's the field value formatter.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringPadding padding, IStringValidator validator, IStringFieldValueFormatter valueFormatter)
            : base(fieldNumber, lengthManager, encoder, padding, validator, valueFormatter)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="valueFormatter">
        /// It's the field value formatter.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public GZipStringFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            IStringPadding padding, IStringValidator validator, IStringFieldValueFormatter valueFormatter,
            string description)
            : base(fieldNumber, lengthManager, encoder, padding, validator, valueFormatter, description)
        {
        }

        protected override void InternalFormat(Field field, ref FormatterContext formatterContext, string fieldValue)
        {
            // Compress the received value.
            using (var ms = new MemoryStream())
            using (var ds = new GZipStream(ms, CompressionMode.Compress))
            {
                byte[] data = formatterContext.Buffer.Encoding.GetBytes(fieldValue);
                ds.Write(data, 0, data.Length);
                ds.Close(); // Force flush.
                data = ms.ToArray();
                LengthManager.WriteLength(field, data.Length, Encoder.GetEncodedLength(data.Length),
                    ref formatterContext);
                Encoder.Encode(data, ref formatterContext);
                LengthManager.WriteLengthTrailer(field, data.Length, Encoder.GetEncodedLength(data.Length),
                    ref formatterContext);
            }
        }

        protected override string InternalParse(ref ParserContext parserContext)
        {
            // Uncompress the received value.
            using (var ms = new MemoryStream(Encoder.DecodeBytes(ref parserContext, parserContext.DecodedLength)))
            using (var ds = new GZipStream(ms, CompressionMode.Decompress))
            using (var rs = new MemoryStream())
            {
                ds.CopyTo(rs);
                return parserContext.Buffer.Encoding.GetString(rs.ToArray());
            }
        }
    }
}
