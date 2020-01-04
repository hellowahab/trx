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
    public class ServerPeer : Peer
    {
        public ServerPeer(string name) : base(name)
        {
        }

        public ServerPeer(string name, IMessagesIdentifier messagesIdentifier) :
            base(name, messagesIdentifier)
        {
        }

        public virtual void Bind(IChannel channel)
        {
            if (Channel != null)
                throw new InvalidOperationException("The peer is already binded.");

            if (channel == null)
                throw new ArgumentNullException("channel");

            ProtectedChannel = channel;
        }

        public override void Connect()
        {
            throw new InvalidOperationException("Invalid operation for this kind of peer.");
        }
    }
}