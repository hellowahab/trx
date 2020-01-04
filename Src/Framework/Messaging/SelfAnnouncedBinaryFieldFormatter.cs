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
    /// Implements a self announced binary fields formatter.
    /// </summary>
    public class SelfAnnouncedBinaryFieldFormatter : FieldFormatter, ISelfAnnouncedFieldFormatter
    {
        private readonly IDataEncoder _encoder;
        private readonly bool _includeSelfAnnouncementInLength;
        private readonly LengthManager _lengthManager;
        private readonly SelfAnnouncedMarkerManager _selfAnnounceManager;

        /// <summary>
        /// It initializes a new binary field formatter instance.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// This flag tells the field formatter to add the field indicator length
        /// to the encoded field data length.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// It's the field announcement manager.
        /// </param>
        /// <param name="encoder">
        /// It's the field value encoder.
        /// </param>
        public SelfAnnouncedBinaryFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, string.Empty)
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
        /// <param name="includeSelfAnnouncementInLength">
        /// This flag tells the field formatter to add the field indicator length
        /// to the encoded field data length.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// It's the field announcement manager.
        /// </param>
        /// <param name="encoder">
        /// It's the field value encoder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public SelfAnnouncedBinaryFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, string description)
            :
                base(fieldNumber, description)
        {
            if (lengthManager == null)
                throw new ArgumentNullException("lengthManager");

            if (selfAnnounceManager == null)
                throw new ArgumentNullException("selfAnnounceManager");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lengthManager = lengthManager;
            _includeSelfAnnouncementInLength = includeSelfAnnouncementInLength;
            _selfAnnounceManager = selfAnnounceManager;
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
        /// It returns the flag which tells the field formatter to add the field
        /// indicator length to the encoded field data length.
        /// </summary>
        public bool IncludeSelfAnnouncementInLength
        {
            get { return _includeSelfAnnouncementInLength; }
        }

        /// <summary>
        /// It returns the field announcement manager.
        /// </summary>
        public SelfAnnouncedMarkerManager SelfAnnounceManager
        {
            get { return _selfAnnounceManager; }
        }

        /// <summary>
        /// It returns the field value encoder.
        /// </summary>
        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }

        #region ISelfAnnouncedFieldFormatter Members
        /// <summary>
        /// It informs the field number of the next field to be parsed.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="fieldNumber">
        /// The field number of the field to be parsed.
        /// </param>
        /// <returns>
        /// A boolean value indicating if the field number was extracted from
        /// the parser context. If it returns true, the field number it's
        /// stored in <paramref name="fieldNumber"/>.
        /// </returns>
        public bool GetFieldNumber(ref ParserContext parserContext, out int fieldNumber)
        {
            fieldNumber = -1;

            // If MinValue, at this moment the length hasn't been decoded.
            if (parserContext.DecodedLength == int.MinValue)
            {
                if (!_lengthManager.EnoughData(ref parserContext)) // Insufficient data to parse length, return null.
                    return false;

                // Save length in parser context just in case field value
                // can't be parsed at this time (more data needed).
                parserContext.DecodedLength = _lengthManager.ReadLength(ref parserContext);
            }

            if (parserContext.DataLength < _selfAnnounceManager.GetEncodedLength(ref parserContext))
                // Insufficient data to parse field announcement.
                return false;

            fieldNumber = _selfAnnounceManager.ReadAnnouncement(ref parserContext);
            return true;
        }
        #endregion

        /// <summary>
        /// Formats the specified field.
        /// </summary>
        /// <param name="field">
        /// It's the field to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        public override void Format(Field field,
            ref FormatterContext formatterContext)
        {
            if (!(field is BinaryField))
                throw new ArgumentException("Field must be a binary message field.", "field");

            int announcementLength = 0;
            if (_includeSelfAnnouncementInLength)
                announcementLength = _selfAnnounceManager.GetEncodedLength(field,
                    ref formatterContext);

            if ((field.GetBytes() == null))
            {
                _lengthManager.WriteLength(field, announcementLength, announcementLength,
                    ref formatterContext);
                _selfAnnounceManager.WriteAnnouncement(field, ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, 0, 0, ref formatterContext);
            }
            else
            {
                _lengthManager.WriteLength(field, field.GetBytes().Length + announcementLength,
                    _encoder.GetEncodedLength(field.GetBytes().Length) + announcementLength,
                    ref formatterContext);
                _selfAnnounceManager.WriteAnnouncement(field, ref formatterContext);
                _encoder.Encode(field.GetBytes(), ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, field.GetBytes().Length,
                    _encoder.GetEncodedLength(field.GetBytes().Length),
                    ref formatterContext);
            }
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
            int decodedLength = parserContext.DecodedLength;
            if (_includeSelfAnnouncementInLength)
                decodedLength -= _selfAnnounceManager.GetEncodedLength(ref parserContext);

            if (parserContext.DataLength < _encoder.GetEncodedLength(decodedLength))
                // Insufficient data to parse field value, return null.
                return null;

            // Create the new messaging component with parsing context data.
            var field = new BinaryField(FieldNumber,
                _encoder.DecodeBytes(ref parserContext, decodedLength));

            _lengthManager.ReadLengthTrailer(ref parserContext);

            return field;
        }
    }
}