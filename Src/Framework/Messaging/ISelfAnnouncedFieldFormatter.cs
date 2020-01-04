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

namespace Trx.Messaging
{

    /// <summary>
    /// It defines the interface for a self announced fields.
    /// </summary>
    /// <remarks>
    /// Self announced fields are those ones which includes it
    /// field number in the formatted data. This is another way
    /// to have conditionally present fields.
    /// </remarks>
    public interface ISelfAnnouncedFieldFormatter {

        /// <summary>
        /// It informs the field number of the next field to be parsed.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="fieldNumber">
        /// The field number of the field to be parsed.
        /// </param>
        /// <returns>
        /// A boolean value indicating if the field number was extracted from
        /// the parser context. If it returns true, the field number it's
        /// stored in <paramref name="fieldNumber"/>.
        /// </returns>
        bool GetFieldNumber( ref ParserContext parserContext, out int fieldNumber );
    }
}
