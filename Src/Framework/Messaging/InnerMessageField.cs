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
    /// Implements a field component which their values are messages.
    /// </summary>
    [Serializable]
    public class InnerMessageField : Field
    {
        private Message _value;

        /// <summary>
        /// It initializes a new inner message field component.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the field number of the new field.
        /// </param>
        public InnerMessageField(int fieldNumber) : base(fieldNumber)
        {
            _value = null;
        }

        /// <summary>
        /// It initializes a new inner message field component.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the field number of the new field.
        /// </param>
        /// <param name="value">
        /// It's the value of the new field.
        /// </param>
        public InnerMessageField(int fieldNumber, Message value) : base(fieldNumber)
        {
            _value = value;
        }

        /// <summary>
        /// It returns or sets the value of the field.
        /// </summary>
        public override object Value
        {
            get { return _value; }

            set
            {
                if (value is Message)
                    _value = (Message) value;
                else
                    throw new ArgumentException("Can't handle parameter type.", "value");
            }
        }

        /// <summary>
        /// It returns a string representation of the field value.
        /// </summary>
        /// <returns>
        /// A string representing the field value.
        /// </returns>
        /// <remarks>
        /// If the value is null, this function returns an empty string.
        /// </remarks>
        public override string ToString()
        {
            if (_value == null)
                return string.Empty;

            return _value.ToString();
        }

        /// <summary>
        /// It returns the field value.
        /// </summary>
        /// <returns>
        /// An array of bytes, or null if the field value is null.
        /// </returns>
        public override byte[] GetBytes()
        {
            if (_value == null)
                return null;

            return _value.GetBytes();
        }

        /// <summary>
        /// Clones the field.
        /// </summary>
        /// <returns>
        /// A clone of the field instance.
        /// </returns>
        public override object Clone()
        {
            return new InnerMessageField(FieldNumber,
                (_value == null) ? null : (Message) _value.Clone());
        }

        /// <summary>
        /// It creates a new binary field.
        /// </summary>
        /// <returns>
        /// A new binary field.
        /// </returns>
        public override MessagingComponent NewComponent()
        {
            return new InnerMessageField(FieldNumber);
        }
    }
}