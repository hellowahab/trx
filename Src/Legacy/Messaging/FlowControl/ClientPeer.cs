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
    /// This class encapsulates the client peer services.
    /// </summary>
    /// <remarks>
    /// The client peers are those which initiates the connection
    /// with the remote system.
    /// </remarks>
    public class ClientPeer : Peer
    {
        /// <summary>
        /// Initilizes a new instance of <see cref="ClientPeer"/> class.
        /// </summary>
        /// <param name="name">
        /// It's the name of the peer.
        /// </param>
        /// <param name="channel">
        /// It's the channel which the peer gets connection with the remote system.
        /// </param>
        public ClientPeer(string name, IChannel channel) : base(name)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            ProtectedChannel = channel;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ClientPeer"/>,
        /// configurating it to process requests.
        /// </summary>
        /// <param name="name">
        /// It's the name of the peer.
        /// </param>
        /// <param name="channel">
        /// It's the channel which the peer gets connection with the remote system.
        /// </param>
        /// <param name="messagesIdentifier">
        /// It's the messages identifier.
        /// </param>
        public ClientPeer(string name, IChannel channel,
            IMessagesIdentifier messagesIdentifier) : base(name, messagesIdentifier)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            ProtectedChannel = channel;
        }
    }
}