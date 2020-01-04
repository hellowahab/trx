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

namespace Trx.Utilities
{
    /// <summary>
    /// This class implements a filler of values of type string.
    /// It performs the work adding or removing a character at the
    /// end of the given value.
    /// </summary>
    /// <remarks>
    /// In addition, this filler verifies that the length of the data
    /// to fill up, does not exceed the expected length.
    /// </remarks>
    public abstract class StringPaddingRight : IStringPadding
    {
        private readonly bool _canRemovePad = true;
        private readonly char[] _padChars;
        private readonly bool _truncate;

        /// <summary>
        /// It constructs a new instance of the filler.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        /// <param name="pad">
        /// It is the character to use in the filling.
        /// </param>
        /// <param name="canRemovePad">
        /// It indicates in true if the filling is removed, in false it does not do it.
        /// </param>
        protected StringPaddingRight(bool truncate, char pad, bool canRemovePad)
        {
            _truncate = truncate;
            _padChars = new[] {pad};
            _canRemovePad = canRemovePad;
        }

        /// <summary>
        /// It constructs a new instance of the filler.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        /// <param name="pad">
        /// It is the character to use in the filling.
        /// </param>
        protected StringPaddingRight(bool truncate, char pad)
        {
            _truncate = truncate;
            _padChars = new[] {pad};
        }

        /// <summary>
        /// It informs if the class has been formed to truncate the
        /// data of the fields whose length is superior to the supported one.
        /// </summary>
        public bool Truncate
        {
            get { return _truncate; }
        }

        /// <summary>
        /// It informs if the filler is formed to remove the filling character.
        /// </summary>
        public bool CanRemovePad
        {
            get { return _canRemovePad; }
        }

        /// <summary>
        /// It returns the pad character.
        /// </summary>
        public char PadCharacter
        {
            get { return _padChars[0]; }
        }

        #region IStringPadding Members
        /// <summary>
        /// It carries out the filling of a value of type string.
        /// </summary>
        /// <param name="data">
        /// It is the value that must be filled up.
        /// </param>
        /// <param name="totalWidth">
        /// It is the maximum length of the resulting value.
        /// </param>
        /// <returns>
        /// A string with the filled up value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="totalWidth"/> less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Data length > totalWidth (the instance can't
        /// truncate value).
        /// </exception>
        public virtual string Pad(string data, int totalWidth)
        {
            if (totalWidth < 1)
                throw new ArgumentOutOfRangeException("totalWidth", totalWidth,
                    "Must be greater than zero.");

            if (string.IsNullOrEmpty(data))
                return new string(_padChars[0], totalWidth);

            if (!_truncate && (data.Length > totalWidth))
                // Check data length, if bigger than total width throw an exception.
                throw new ArgumentException(
                    "Unexpected bigger data length.", "data");

            if (data.Length == totalWidth) // No string padding necessary, reduce overhead returning
                // same value.
                return data;

            if (data.Length > totalWidth)
                return data.Substring(0, totalWidth);

            return data.PadRight(totalWidth, _padChars[0]);
        }

        /// <summary>
        /// It eliminates the filling used in the given value.
        /// </summary>
        /// <param name="data">
        /// It is the filled up value.
        /// </param>
        /// <returns>
        /// The value without its filling.
        /// </returns>
        public virtual string RemovePad(string data)
        {
            if (!_canRemovePad)
                return data;

            if (string.IsNullOrEmpty(data))
                return data;

            if (data[data.Length - 1] == _padChars[0]) // Last char is a space, remove string padding.
                return data.TrimEnd(_padChars);

            return data;
        }
        #endregion
    }
}