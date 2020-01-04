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
    /// Raise by a channel server when a new connection is available. It allows the sinks
    /// in the pipeline to decide if connection can be stablished or not.
    /// </summary>
    public class ConnectionRequestChannelEvent : ChannelEvent
    {
        #region Constructors
        internal ConnectionRequestChannelEvent()
            : base(ChannelEventType.ConnectionRequested)
        {
            Accept = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// True when the connection can be accepted, otherwise false.
        /// </summary>
        public bool Accept
        {
            get;
            set;
        }
        #endregion
    }
}
