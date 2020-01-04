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
    public enum BufferDumpFormat
    {
        /// <summary>
        /// Standard Trx Framework buffer dump: |.....| Hh Hh Hh Hh Hh
        /// </summary>
        TwoColumnsBufferAndHex = 0,

        /// <summary>
        /// Buffer dump and hexa bytes values in the next two lines:
        /// line n:      |.....|
        /// line n + 1:   HHHHH
        /// line n + 2:   hhhhh
        /// </summary>
        BufferAndHexInNextTwoLines = 1,

        /// <summary>
        /// Combination of the previous two formats.
        /// </summary>
        Both = 99
    }
    /// <summary>
    /// Allows to config buffer dump.
    /// </summary>
    public class BufferDumpConfig
    {
        private static volatile BufferDumpConfig _instance;

        /// <summary>
        /// Initializes a new instance of <see cref="BufferDumpConfig"/>.
        /// </summary>
        private BufferDumpConfig()
        {
            DumpFormat = BufferDumpFormat.TwoColumnsBufferAndHex;
            BytesPerLine = 20;
        }

        /// <summary>
        /// Sets or gets the dump format.
        /// </summary>
        /// <remarks>
        /// Default value is <see cref="BufferDumpFormat.BufferAndHexInNextTwoLines"/>.
        /// </remarks>
        public BufferDumpFormat DumpFormat { get; set; }

        /// <summary>
        /// Number of bytes per line.
        /// </summary>
        /// <remarks>
        /// Default value is 70.
        /// </remarks>
        public int BytesPerLine { get; set; }

        /// <summary>
        /// It returns an instance of <see cref="BufferDumpConfig"/> class.
        /// </summary>
        /// <returns>
        /// An <see cref="BufferDumpConfig"/> instance.
        /// </returns>
        public static BufferDumpConfig GetInstance()
        {
            if (_instance == null)
                lock (typeof(BufferDumpConfig))
                {
                    if (_instance == null)
                        _instance = new BufferDumpConfig();
                }

            return _instance;
        }
    }
}