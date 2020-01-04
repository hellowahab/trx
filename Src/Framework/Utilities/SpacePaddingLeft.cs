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
    /// It performs the work adding or removing spaces at the
    /// beginning of the given value.
    /// </summary>
    /// <remarks>
    /// In addition, this filler verifies that the length of the data
    /// to fill up, does not exceed the expected length.
    /// </remarks>
    public sealed class SpacePaddingLeft : StringPaddingLeft
    {
        private static volatile SpacePaddingLeft _instanceWithTruncate;
        private static volatile SpacePaddingLeft _instanceWithoutTruncate;

        /// <summary>
        /// It constructs a new instance of the filler. It's private,
        /// in order to force the user to use <see cref="GetInstance"/>.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        private SpacePaddingLeft(bool truncate) : base(truncate, ' ')
        {
        }

        /// <summary>
        /// It returns an instance of class <see cref="SpacePaddingLeft"/>.
        /// </summary>
        /// <param name="truncate">
        /// <see langref="true"/> to discard data over the supported length,
        /// otherwise <see langref="false"/> to receive an exception if
        /// data doesn't fit in field.
        /// </param>
        /// <returns>
        /// An instance of class <see cref="SpacePaddingLeft"/>.
        /// </returns>
        public static SpacePaddingLeft GetInstance(bool truncate)
        {
            SpacePaddingLeft instance = truncate ? _instanceWithTruncate : _instanceWithoutTruncate;

            if (instance == null)
                lock (typeof (SpacePaddingLeft))
                {
                    instance = truncate ? _instanceWithTruncate : _instanceWithoutTruncate;
                    if (instance == null)
                    {
                        instance = new SpacePaddingLeft(truncate);

                        if (truncate)
                            _instanceWithTruncate = instance;
                        else
                            _instanceWithoutTruncate = instance;
                    }
                }

            return instance;
        }
    }
}