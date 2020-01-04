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
using System.Text;
using Trx.Utilities;

namespace Trx.Messaging
{
    [Serializable]
    public class BitMapField : Field
    {
        private readonly int _lowerFieldNumber;
        private readonly int _upperFieldNumber;
        private byte[] _value;

        public BitMapField(int fieldNumber, int lowerFieldNumber, int upperFieldNumber) : base(fieldNumber)
        {
            if (lowerFieldNumber < 0)
                throw new ArgumentOutOfRangeException("lowerFieldNumber", lowerFieldNumber,
                    "Can't be lower than zero.");

            if (lowerFieldNumber > upperFieldNumber)
                throw new ArgumentOutOfRangeException("lowerFieldNumber", lowerFieldNumber,
                    "Must be lower or equal to upperFieldNumber.");

            _lowerFieldNumber = lowerFieldNumber;
            _upperFieldNumber = upperFieldNumber;

            _value = new byte[((upperFieldNumber - lowerFieldNumber) + 8)/8];

            for (int i = 0; i < _value.Length; i++)
                _value[i] = 0;
        }

        public BitMapField(int fieldNumber, int lowerFieldNumber, int upperFieldNumber, byte[] value)
            : base(fieldNumber)
        {
            if (lowerFieldNumber > upperFieldNumber)
                throw new ArgumentOutOfRangeException("lowerFieldNumber", lowerFieldNumber,
                    "Must be lower or equal to upperFieldNumber.");

            if (value.Length != (((upperFieldNumber - lowerFieldNumber) + 8)/8))
                throw new ArgumentException("Unexpected length.", "value");

            _lowerFieldNumber = lowerFieldNumber;
            _upperFieldNumber = upperFieldNumber;

            _value = value;
        }

        public BitMapField(int fieldNumber, BitMapField bitmap) : base(fieldNumber)
        {
            _lowerFieldNumber = bitmap.LowerFieldNumber;
            _upperFieldNumber = bitmap.UpperFieldNumber;

            byte[] bitmapValue = bitmap.GetBytes();

            _value = new byte[bitmapValue.Length];

            for (int i = 0; i < _value.Length; i++)
                _value[i] = bitmapValue[i];
        }

        public int LowerFieldNumber
        {
            get { return _lowerFieldNumber; }
        }

        public int UpperFieldNumber
        {
            get { return _upperFieldNumber; }
        }

        public bool this[int fieldNumber]
        {
            get { return IsSet(fieldNumber); }

            set { Set(fieldNumber, value); }
        }

        public override object Value
        {
            get { return _value; }

            set
            {
                if (value is byte[])
                    SetFieldValue((byte[]) value);
                else if (value is string)
                {
                    if (((string) value).Length != _value.Length)
                        throw new ArgumentException("Invalid length.", "value");

                    _value = FrameworkEncoding.GetInstance().Encoding.GetBytes((string) value);
                }
                else
                    throw new ArgumentException("Can't handle parameter type.", "value");
            }
        }

        public void SetFieldValue(byte[] value)
        {
            if (value.Length != _value.Length)
                throw new ArgumentException("Invalid length.", "value");

            _value = value;
        }

        public void Set(int fieldNumber, bool value)
        {
            if ((fieldNumber < _lowerFieldNumber) || (fieldNumber > _upperFieldNumber))
                throw new ArgumentOutOfRangeException("fieldNumber", fieldNumber,
                    string.Format("Invalid fieldNumber number, must be between {0} and {1}.", _lowerFieldNumber,
                        _upperFieldNumber));

            fieldNumber -= _lowerFieldNumber;

            if (value)
                _value[fieldNumber/8] |= (byte) (1 << (7 - (fieldNumber%8)));
            else
                _value[fieldNumber/8] &= (byte) (~(1 << (7 - (fieldNumber%8))));
        }

        public void Clear()
        {
            for (int i = 0; i < _value.Length; i++)
                _value[i] = 0;
        }

        public bool IsSet(int fieldNumber)
        {
            if ((fieldNumber < _lowerFieldNumber) || (fieldNumber > _upperFieldNumber))
                throw new ArgumentOutOfRangeException("fieldNumber", fieldNumber,
                    string.Format("Invalid fieldNumber number, must be between {0} and {1}.", _lowerFieldNumber,
                        _upperFieldNumber));

            fieldNumber -= _lowerFieldNumber;

            return (_value[fieldNumber/8] & (1 << (7 - (fieldNumber%8)))) != 0;
        }

        public override byte[] GetBytes()
        {
            return _value;
        }

        public override object Clone()
        {
            return new BitMapField(FieldNumber, this);
        }

        public override string ToString()
        {
            var rendered = new StringBuilder();
            bool comma = false;

            rendered.Append("[");
            for (int i = _lowerFieldNumber; i <= _upperFieldNumber; i++)
                if (IsSet(i))
                    if (comma)
                        rendered.Append(string.Format(",{0}", i));
                    else
                    {
                        comma = true;
                        rendered.Append(i);
                    }
            rendered.Append("]");

            return rendered.ToString();
        }

        public override MessagingComponent NewComponent()
        {
            return new BitMapField(FieldNumber, _lowerFieldNumber, _upperFieldNumber);
        }
    }
}