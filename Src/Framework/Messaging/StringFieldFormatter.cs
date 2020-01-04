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
    /// <summary>
    /// It represents a string field formatter.
    /// </summary>
    public class StringFieldFormatter : FieldFormatter
    {
        private readonly IDataEncoder _encoder;
        private readonly LengthManager _lengthManager;
        private readonly IStringPadding _padding;
        private readonly IStringValidator _validator;
        private readonly IStringFieldValueFormatter _valueFormatter;

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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder)
            : this(fieldNumber, lengthManager, encoder,
                null, null, null, string.Empty)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, string description)
            : this(fieldNumber,
                lengthManager, encoder, null, null, null, description)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding) :
                this(fieldNumber, lengthManager, encoder, padding, null, null,
                    string.Empty)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding, string description) :
                this(fieldNumber, lengthManager, encoder, padding, null, null,
                    description)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringValidator validator) :
                this(fieldNumber, lengthManager, encoder, null, validator, null,
                    string.Empty)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringValidator validator, string description) :
                this(fieldNumber, lengthManager, encoder, null, validator, null,
                    description)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator) :
                this(fieldNumber, lengthManager, encoder, padding, validator, null,
                    string.Empty)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator,
            IStringFieldValueFormatter valueFormatter) : this(fieldNumber,
                lengthManager, encoder, padding, validator, valueFormatter, string.Empty)
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
        public StringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator,
            IStringFieldValueFormatter valueFormatter, string description) :
                base(fieldNumber, description)
        {
            if (lengthManager == null)
                throw new ArgumentNullException("lengthManager");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lengthManager = lengthManager;
            _encoder = encoder;
            _validator = validator;
            _valueFormatter = valueFormatter;

            if ((padding == null) && (lengthManager is FixedLengthManager))
                _padding = SpacePaddingRight.GetInstance(false);
            else
                _padding = padding;
        }

        /// <summary>
        /// It returns the field data length manager.
        /// </summary>
        public LengthManager LengthManager
        {
            get { return _lengthManager; }
        }

        /// <summary>
        /// It returns the field data encoder.
        /// </summary>
        /// <remarks>
        /// <see cref="IDataEncoder"/> replaces legacy IStringEncoder.
        /// </remarks>
        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }

        /// <summary>
        /// It returns the field data padder.
        /// </summary>
        public IStringPadding Padding
        {
            get { return _padding; }
        }

        /// <summary>
        /// It returns the field data validator.
        /// </summary>
        public IStringValidator Validator
        {
            get { return _validator; }
        }

        /// <summary>
        /// It returns the field data value formatter.
        /// </summary>
        public IStringFieldValueFormatter ValueFormatter
        {
            get { return _valueFormatter; }
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
            if (!(field is StringField))
                throw new ArgumentException("Field must be a string message field.", "field");

            string fieldValue = ((StringField) field).FieldValue;

            // Pad if padding available.
            if (_padding != null)
                fieldValue = _padding.Pad(fieldValue, _lengthManager.MaximumLength);

            if (_validator != null)
                _validator.Validate(fieldValue);

            if (fieldValue == null)
            {
                _lengthManager.WriteLength(field, 0, 0, ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, 0, 0, ref formatterContext);
            }
            else
                InternalFormat(field, ref formatterContext, fieldValue);
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
        protected virtual void InternalFormat(Field field, ref FormatterContext formatterContext, string fieldValue)
        {
            _lengthManager.WriteLength(field, fieldValue.Length, _encoder.GetEncodedLength(fieldValue.Length),
                ref formatterContext);
            _encoder.Encode(fieldValue, ref formatterContext);
            _lengthManager.WriteLengthTrailer(field, fieldValue.Length,
                _encoder.GetEncodedLength(fieldValue.Length), ref formatterContext);
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
                parserContext.DecodedLength =
                    _lengthManager.ReadLength(ref parserContext);
            }

            if (parserContext.DataLength < _encoder.GetEncodedLength(
                parserContext.DecodedLength)) // Insufficient data to parse field value, return null.
                return null;

            string fieldValue = InternalParse(ref parserContext);

            if (_padding != null)
                fieldValue = _padding.RemovePad(fieldValue);

            if (_validator != null)
                _validator.Validate(fieldValue);

            var field = new StringField(FieldNumber, fieldValue);

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
        protected virtual string InternalParse(ref ParserContext parserContext)
        {
            return _encoder.DecodeString(ref parserContext, parserContext.DecodedLength);
        }
    }
}