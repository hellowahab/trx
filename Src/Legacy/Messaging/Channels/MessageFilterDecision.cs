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

namespace Trx.Messaging.Channels
{
    /// <summary>
    /// The return result from <see cref="IMessageFilter.Decide"/>
    /// </summary>
    /// <remarks>
    /// The return result from <see cref="IMessageFilter.Decide"/>
    /// </remarks>
    public enum MessageFilterDecision
    {
        /// <summary>
        /// The message must be dropped immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// </summary>
        Deny = -1,

        /// <summary>
        /// This filter is neutral with respect to the message. 
        /// The remaining filters, if any, should be consulted for a final decision.
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// The message must processed immediately without 
        /// consulting with the remaining filters, if any, in the chain.
        /// </summary>
        Accept = 1,
    }
}