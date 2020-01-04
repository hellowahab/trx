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

namespace Trx.Server.Services
{
    public enum ChannelServiceServingPolicy
    {
        /// <summary>
        /// Discard the request if can't be served.
        /// </summary>
        Discard = 0,

        /// <summary>
        /// Wait for the channel to complete service.
        /// </summary>
        Wait = 1,

        /// <summary>
        /// Return the message to the tuple space.
        /// </summary>
        Return = 2
    }
}
