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

using Trx.Messaging.Channels;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// This interface defines what a class must implements to be
    /// a server peers manager.
    /// </summary>
    public interface IServerPeerManager
    {
        /// <summary>
        /// It returns the collection of known peers by the server peer
        /// manager.
        /// </summary>
        ServerPeerCollection Peers { get; }

        /// <summary>
        /// It returns or sets the objets which process the received messages
        /// by the connections points.
        /// </summary>
        /// <remarks>
        /// The server peer manager sends every received message from the
        /// peers to the messages processor set here.
        /// </remarks>
        IMessageProcessor MessageProcessor { get; set; }

        /// <summary>
        /// This function is used to know if the peer manager accepts the connection
        /// request.
        /// </summary>
        /// <param name="connectionInfo">
        /// It's the connection request information. The server peer manager
        /// can use this information in order to take the decision to accept
        /// or not the connection request.
        /// </param>
        /// <returns>
        /// A logical value equal to true if the connection request is accepted,
        /// otherwise false .
        /// </returns>
        /// <remarks>
        /// The connection requests arrives from objects implementing
        /// <see cref="IListener"/> interface.
        /// </remarks>
        bool AcceptConnectionRequest(object connectionInfo);

        /// <summary>
        /// Through the invocation of this method <see cref="Server"/> informs to
        /// the peer manager of the connection of the indicated channel.
        /// </summary>
        /// <param name="channel">
        /// It's the connected channel.
        /// </param>
        /// <returns>
        /// The peer associated to the channel.
        /// </returns>
        /// <remarks>
        /// Normally at this time the peers manager associates the channel to the peer.
        /// </remarks>
        ServerPeer Connected(IChannel channel);
    }
}