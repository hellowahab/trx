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
    /// It defines the self announce field marker manager.
    /// </summary>
    public abstract class SelfAnnouncedMarkerManager {

        /// <summary>
        /// It computes the encoded length of the announcement.
        /// </summary>
        /// <param name="field">
        /// It's the field to be announced.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context.
        /// </param>
        /// <returns>
        /// The encoded length of the announcement.
        /// </returns>
        public abstract int GetEncodedLength( Field field,
            ref FormatterContext formatterContext );

        /// <summary>
        /// It writes the announce into the formatter context.
        /// </summary>
        /// <param name="field">
        /// It's the field to be announced.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context.
        /// </param>
        /// <returns>
        /// The number of bytes written into the formatter context.
        /// </returns>
        public abstract int WriteAnnouncement( Field field,
            ref FormatterContext formatterContext );

        /// <summary>
        /// It computes the encoded length of the announcement.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// The encoded length of the announcement.
        /// </returns>
        public abstract int GetEncodedLength( ref ParserContext parserContext );

        /// <summary>
        /// It reads the announcement from the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// The announced field number.
        /// </returns>
        public abstract int ReadAnnouncement( ref ParserContext parserContext );
    }
}
