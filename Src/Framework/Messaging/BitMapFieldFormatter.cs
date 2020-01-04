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
    public class BitMapFieldFormatter : FieldFormatter
    {
        private readonly int _bitmapLength;
        private readonly IDataEncoder _encoder;
        private readonly int _lowerFieldNumber;
        private readonly int _upperFieldNumber;

        #region Constructors
        public BitMapFieldFormatter(int fieldNumber, int lowerFieldNumber,
            int upperFieldNumber, IDataEncoder encoder)
            : this(fieldNumber,
                lowerFieldNumber, upperFieldNumber, encoder, string.Empty)
        {
        }

        public BitMapFieldFormatter(int fieldNumber, int lowerFieldNumber,
            int upperFieldNumber, IDataEncoder encoder, string description) :
                base(fieldNumber, description)
        {
            if (lowerFieldNumber < 0)
                throw new ArgumentOutOfRangeException("lowerFieldNumber", lowerFieldNumber,
                    "Can't be lower than zero.");

            if (lowerFieldNumber > upperFieldNumber)
                throw new ArgumentOutOfRangeException("lowerFieldNumber", lowerFieldNumber,
                    "Must be lower or equal to upperFieldNumber.");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lowerFieldNumber = lowerFieldNumber;
            _upperFieldNumber = upperFieldNumber;
            _encoder = encoder;
            _bitmapLength = ((_upperFieldNumber - _lowerFieldNumber) + 8)/8;
        }
        #endregion

        #region Properties
        public int LowerFieldNumber
        {
            get { return _lowerFieldNumber; }
        }

        public int UpperFieldNumber
        {
            get { return _upperFieldNumber; }
        }

        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }
        #endregion

        #region Methods
        public override void Format(Field field, ref FormatterContext formatterContext)
        {
            if (!(field is BitMapField))
                throw new ArgumentException("Field must be a bitmap.", "field");

            _encoder.Encode((field).GetBytes(), ref formatterContext);
        }

        public override Field Parse(ref ParserContext parserContext)
        {
            if (parserContext.DataLength < _encoder.GetEncodedLength(_bitmapLength))
                // Insufficient data to parse bitmap, return null.
                return null;

            return new BitMapField(FieldNumber, _lowerFieldNumber, _upperFieldNumber,
                _encoder.DecodeBytes(ref parserContext, _bitmapLength));
        }
        #endregion
    }
}