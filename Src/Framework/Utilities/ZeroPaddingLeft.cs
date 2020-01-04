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

namespace Trx.Utilities
{
    /// <summary>
    /// This class implements a filler of values of type string.
    /// It performs the work adding or removing zeros at the
    /// beginning of the given value.
    /// </summary>
    /// <remarks>
    /// In addition, this filler verifies that the length of the data
    /// to fill up, does not exceed the expected length.
    /// </remarks>
    public sealed class ZeroPaddingLeft : StringPaddingLeft
    {
        private static volatile ZeroPaddingLeft _instanceWithTruncateAndRemovePad;
        private static volatile ZeroPaddingLeft _instanceWithoutTruncateAndRemovePad;
        private static volatile ZeroPaddingLeft _instanceWithTruncateAndWithoutRemovePad;
        private static volatile ZeroPaddingLeft _instanceWithoutTruncateAndWithoutRemovePad;

        /// <summary>
        /// It constructs a new instance of the filler. It's private,
        /// in order to force the user to use <see cref="GetInstance"/>.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        /// <param name="canRemovePad">
        /// <see langref="true"/> if pad must be removed, otherwise <see langref="false"/>.
        /// </param>
        private ZeroPaddingLeft(bool truncate, bool canRemovePad) :
            base(truncate, '0', canRemovePad)
        {
        }

        /// <summary>
        /// It returns an instance of class <see cref="ZeroPaddingLeft"/>.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        /// <param name="canRemovePad">
        /// <see langref="true"/> if pad must be removed, otherwise <see langref="false"/>.
        /// </param>
        /// <returns>
        /// An instance of class <see cref="ZeroPaddingLeft"/>.
        /// </returns>
        public static ZeroPaddingLeft GetInstance(bool truncate, bool canRemovePad)
        {
            ZeroPaddingLeft instance;

            if (truncate)
                instance = canRemovePad ? _instanceWithTruncateAndRemovePad : _instanceWithTruncateAndWithoutRemovePad;
            else
                instance = canRemovePad
                    ? _instanceWithoutTruncateAndRemovePad
                    : _instanceWithoutTruncateAndWithoutRemovePad;

            if (instance == null)
                lock (typeof (ZeroPaddingLeft))
                {
                    instance = new ZeroPaddingLeft(truncate, canRemovePad);
                    if (truncate)
                        if (canRemovePad)
                            _instanceWithTruncateAndRemovePad = instance;
                        else
                            _instanceWithTruncateAndWithoutRemovePad = instance;
                    else if (canRemovePad)
                        _instanceWithoutTruncateAndRemovePad = instance;
                    else
                        _instanceWithoutTruncateAndWithoutRemovePad = instance;
                }

            return instance;
        }

        /// <summary>
        /// Removes the pad from the string.
        /// </summary>
        /// <param name="data">
        /// It's the padded data.
        /// </param>
        /// <returns>
        /// The data without the pad.
        /// </returns>
        public override string RemovePad(string data)
        {
            data = base.RemovePad(data);

            if (data == string.Empty)
                data = "0";

            return data;
        }
    }
}