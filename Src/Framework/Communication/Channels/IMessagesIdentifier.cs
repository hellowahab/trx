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

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Used by <see cref="ISenderChannel"/> to match send and received messages by key.
    /// </summary>
    public interface IMessagesIdentifier
    {
        /// <summary>
        /// Compute message key.
        /// </summary>
        /// <param name="message">
        /// The message whose key is computed.
        /// </param>
        /// <returns>
        /// The message key.
        /// </returns>
        object ComputeIdentifier(object message);
    }
}
