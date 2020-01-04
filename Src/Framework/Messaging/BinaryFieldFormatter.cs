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
    /// Implements a binary fields formatter.
    /// </summary>
    public class BinaryFieldFormatter : FieldFormatter
    {
        private readonly IDataEncoder _encoder;
        private readonly LengthManager _lengthManager;

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
        public BinaryFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder)
            : this(fieldNumber, lengthManager, encoder, string.Empty)
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
        public BinaryFieldFormatter(int fieldNumber, LengthManager lengthManager, IDataEncoder encoder,
            string description) : base(fieldNumber, description)
        {
            if (lengthManager == null)
                throw new ArgumentNullException("lengthManager");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lengthManager = lengthManager;
            _encoder = encoder;
        }

        /// <summary>
        /// It returns the field length manager.
        /// </summary>
        public LengthManager LengthManager
        {
            get { return _lengthManager; }
        }

        /// <summary>
        /// It returns the field value encoder.
        /// </summary>
        /// <remarks>
        /// <see cref="IDataEncoder"/> replaces legacy IBinaryEncoder.
        /// </remarks>
        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }

        /// <summary>
        /// Formats the specified field.
        /// </summary>
        /// <param name="field">
        /// It's the field to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        public override void Format(Field field, ref FormatterContext formatterContext)
        {
            if (!(field is BinaryField))
                throw new ArgumentException("Field must be a binary message field.", "field");

            if ((field.GetBytes() == null))
            {
                _lengthManager.WriteLength(field, 0, 0, ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, 0, 0, ref formatterContext);
            }
            else
                InternalFormat(field, ref formatterContext, field.GetBytes());
        }

        /// <summary>
        /// Gives an opportunity to a derived class to process the value (i.e. compress).
        /// </summary>
        /// <param name="field">
        /// It's the field to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        /// <param name="fieldValue">
        /// The field value.
        /// </param>
        /// <returns>
        /// The processed value.
        /// </returns>
        protected virtual void InternalFormat(Field field, ref FormatterContext formatterContext, byte[] fieldValue)
        {
            _lengthManager.WriteLength(field, fieldValue.Length, _encoder.GetEncodedLength(fieldValue.Length),
                ref formatterContext);
            _encoder.Encode(field.GetBytes(), ref formatterContext);
            _lengthManager.WriteLengthTrailer(field, fieldValue.Length, _encoder.GetEncodedLength(fieldValue.Length),
                ref formatterContext);
        }

        /// <summary>
        /// It parses the information in the parser context and builds the field.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// The new field built with the information found in the parser context.
        /// </returns>
        public override Field Parse(ref ParserContext parserContext)
        {
            // If MinValue, at this moment the length hasn't been decoded.
            if (parserContext.DecodedLength == int.MinValue)
            {
                if (!_lengthManager.EnoughData(ref parserContext)) // Insufficient data to parse length, return null.
                    return null;

                // Save length in parser context just in case field value
                // can't be parsed at this time (more data needed).
                parserContext.DecodedLength = _lengthManager.ReadLength(ref parserContext);
            }

            if (parserContext.DataLength < _encoder.GetEncodedLength(parserContext.DecodedLength))
                // Insufficient data to parse field value, return null.
                return null;

            // Create the new messaging component with parsing context data.
            var field = new BinaryField(FieldNumber, InternalParse(ref parserContext));

            _lengthManager.ReadLengthTrailer(ref parserContext);

            return field;
        }

        /// <summary>
        /// Gives an opportunity to a derived class to process the value (i.e. decompress).
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// The processed value.
        /// </returns>
        protected virtual byte[] InternalParse(ref ParserContext parserContext)
        {
            return _encoder.DecodeBytes(ref parserContext, parserContext.DecodedLength);
        }
    }
}