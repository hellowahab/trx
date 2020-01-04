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
using Trx.Messaging.Channels;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// This class defines the arguments of the event <see cref="IListener.Connected"/>.
    /// </summary>
    public class ListenerConnectedEventArgs : EventArgs
    {
        private readonly IChannel _channel;

        /// <summary>
        /// It creates and initializes a new instance of the
        /// type <see cref="ListenerConnectedEventArgs"/>.
        /// </summary>
        /// <param name="channel">
        /// It's the accepted channel.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// channel holds a null invalid reference.
        /// </exception>
        public ListenerConnectedEventArgs(IChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            _channel = channel;
        }

        /// <summary>
        /// It returns the accepted channel.
        /// </summary>
        public IChannel Channel
        {
            get { return _channel; }
        }
    }
}