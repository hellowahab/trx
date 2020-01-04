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

namespace Trx.Messaging
{
    public class CardDataObfuscator : IFieldObfuscator
    {
        #region IFieldObfuscator Members
        /// <summary>
        /// It obfuscates card data (ISO 8583 fields 2, 14, 35 and 45)
        /// </summary>
        /// <param name="field">
        /// The field card data.
        /// </param>
        /// <returns>
        /// The obfuscated data.
        /// </returns>
        /// <remarks>
        /// ObfuscateCardData( 4000000000000002 ) = ************0002
        /// ObfuscateCardData( 0805 ) = ****
        /// ObfuscateCardData( 4000000000000002=0805123456 ) = ************0002=**********
        /// ObfuscateCardData( B4000000000000002^JOHN DOE^0805123456 ) = B************0002^JOHN DOE^**********
        /// </remarks>
        public string Obfuscate(Field field)
        {
            string data = field.ToString();
            var b = new StringBuilder(data.Length);

            int i = data.IndexOf('^');
            int j = -1;
            if (i == -1)
            {
                // Try track 2, determine the correct field separator (valids are 'D' o '=').
                i = data.IndexOf('=');
                if (i == -1)
                    i = data.IndexOf('D');

                if ((i == -1) && (data.Length > 11))
                    i = data.Length;
            }
            else // It's track 1
                j = data.IndexOf('^', i + 1);

            for (int k = 0; k < data.Length; k++)
                if (((k <= i) && (k > (i - 5))) ||
                    ((k <= j) && (k > i)))
                    b.Append(data[k]);
                else
                    b.Append(char.IsDigit(data[k]) ? '*' : data[k]);

            return b.ToString();
        }
        #endregion
    }
}