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

using System.Text;

namespace Trx.Utilities
{
    /// <summary>
    /// Provides the way to configure the encoding to be used in the framework.
    /// </summary>
    public class FrameworkEncoding
    {
        private static volatile FrameworkEncoding _instance;

        private Encoding _encoding = Encoding.Default;

        /// <summary>
        /// Initializes a new instance of <see cref="FrameworkEncoding"/>.
        /// </summary>
        private FrameworkEncoding()
        {
        }

        /// <summary>
        /// It sets or returns the Encoding used by the framework.
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }

            set { _encoding = value; }
        }

        /// <summary>
        /// It returns an instance of <see cref="FrameworkEncoding"/> class.
        /// </summary>
        /// <returns>
        /// An <see cref="FrameworkEncoding"/> instance.
        /// </returns>
        public static FrameworkEncoding GetInstance()
        {
            if (_instance == null)
                lock (typeof (FrameworkEncoding))
                {
                    if (_instance == null)
                        _instance = new FrameworkEncoding();
                }

            return _instance;
        }
    }
}