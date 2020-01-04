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
    public class StringMessageHeaderFormatter : IMessageHeaderFormatter
    {
        private readonly IDataEncoder _encoder;
        private readonly LengthManager _lengthManager;
        private readonly IStringPadding _padding;

        public StringMessageHeaderFormatter(LengthManager lengthManager,
            IDataEncoder encoder) :
                this(lengthManager, encoder, null)
        {
        }

        public StringMessageHeaderFormatter(LengthManager lengthManager,
            IDataEncoder encoder, IStringPadding padding)
        {
            if (lengthManager == null)
                throw new ArgumentNullException("lengthManager");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lengthManager = lengthManager;
            _encoder = encoder;

            if ((padding == null) && (lengthManager is FixedLengthManager))
                _padding = SpacePaddingRight.GetInstance(false);
            else
                _padding = padding;
        }

        public LengthManager LengthManager
        {
            get { return _lengthManager; }
        }

        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }

        public IStringPadding Padding
        {
            get { return _padding; }
        }

        #region IMessageHeaderFormatter Members
        public virtual void Format(MessageHeader header,
            ref FormatterContext formatterContext)
        {
            string headerValue = null;

            if (header != null)
            {
                if (!(header is StringMessageHeader))
                    throw new ArgumentException("Header must be a string message header.", "header");

                headerValue = ((StringMessageHeader) header).Value;
            }

            // Pad if padding available.
            if (_padding != null)
                headerValue = _padding.Pad(headerValue,
                    _lengthManager.MaximumLength);

            if (headerValue == null)
            {
                _lengthManager.WriteLength(header, 0, 0, ref formatterContext);
                _lengthManager.WriteLengthTrailer(header, 0, 0, ref formatterContext);
            }
            else
            {
                _lengthManager.WriteLength(header, headerValue.Length,
                    _encoder.GetEncodedLength(headerValue.Length),
                    ref formatterContext);
                _encoder.Encode(headerValue, ref formatterContext);
                _lengthManager.WriteLengthTrailer(header, headerValue.Length,
                    _encoder.GetEncodedLength(headerValue.Length),
                    ref formatterContext);
            }
        }

        public virtual MessageHeader Parse(ref ParserContext parserContext)
        {
            // If zero, at this moment the length hasn't been decoded.
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

            // Create the new messaging component with parsing context data.
            StringMessageHeader header;
            if (_padding == null)
                header = new StringMessageHeader(_encoder.DecodeString(
                    ref parserContext, parserContext.DecodedLength));
            else
                header = new StringMessageHeader(_padding.RemovePad(
                    _encoder.DecodeString(ref parserContext,
                        parserContext.DecodedLength)));

            _lengthManager.ReadLengthTrailer(ref parserContext);

            return header;
        }
        #endregion
    }
}