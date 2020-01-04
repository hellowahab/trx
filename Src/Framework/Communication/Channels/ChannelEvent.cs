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

namespace Trx.Communication.Channels
{
    public class ChannelEvent
    {
        private readonly ChannelEventType _eventType;
        private readonly DateTime _eventDateTime;

        #region Constructors
        internal ChannelEvent(ChannelEventType eventType)
        {
            _eventType = eventType;
            _eventDateTime = DateTime.UtcNow;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Channel event type.
        /// </summary>
        public ChannelEventType EventType
        {
            get
            {
                return _eventType;
            }
        }

        /// <summary>
        /// UTC Date and time of the event.
        /// </summary>
        public DateTime UtcEventDateTime
        {
            get
            {
                return _eventDateTime;
            }
        }
        #endregion
    }
}
