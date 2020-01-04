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
    /// <remarks>
    /// Replaced by class DataEncoder.
    /// </remarks>
    [Obsolete("Replaced by class DataEncoder")]
    public class BinaryEncoder : IBinaryEncoder
    {
        private static volatile BinaryEncoder _instance;

        private BinaryEncoder()
        {
        }

        public static BinaryEncoder GetInstance()
        {
            if (_instance == null)
                lock (typeof (BinaryEncoder))
                {
                    if (_instance == null)
                        _instance = new BinaryEncoder();
                }

            return _instance;
        }

        #region IBinaryEncoder Members
        public int GetEncodedLength(int dataLength)
        {
            return dataLength;
        }

        public void Encode(byte[] data, ref FormatterContext formatterContext)
        {
            formatterContext.Write(data);
        }

        public byte[] Decode(ref ParserContext parserContext, int length)
        {
            return parserContext.GetData(true, length);
        }
        #endregion
    }
}