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

namespace Trx.Buffer
{
    /// <summary>
    /// Defines a secure area within a buffer.
    /// </summary>
    public class BufferSecureArea
    {
        /// <summary>
        /// Starting index fo the secure area.
        /// </summary>
        public int From { get; internal set; }

        /// <summary>
        /// Ending index fo the secure area.
        /// </summary>
        public int To { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the secure area.
        /// </summary>
        /// <param name="from">
        /// Starting index fo the secure area.
        /// </param>
        /// <param name="to">
        /// Ending index fo the secure area.
        /// </param>
        public BufferSecureArea(int from, int to)
        {
            if (from > to)
                throw new ArgumentException("from parameter cannot be greater than to", "from");

            From = from;
            To = to;
        }
    }
}
