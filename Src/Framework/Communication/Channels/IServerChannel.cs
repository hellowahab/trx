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

using System.Collections.Generic;

namespace Trx.Communication.Channels
{
    /// <summary>
    /// <see cref="IServerChannel.ChildConnected"/> event delegate.
    /// </summary>
    public delegate void ServerChannelChildConnectedEventHandler(object sender, ServerChannelChildEventArgs e);

    /// <summary>
    /// <see cref="IServerChannel.ChildDisconnected"/> event delegate.
    /// </summary>
    public delegate void ServerChannelChildDisconnectedEventHandler(object sender, ServerChannelChildEventArgs e);

    /// <summary>
    /// <see cref="IServerChannel.ChildAddressChanged"/> event delegate.
    /// </summary>
    public delegate void ServerChannelChildAddressChangedEventHandler(
        object sender, ServerChannelChildAddressChangedEventArgs e);

    public interface IServerChannel : IChannel
    {
        /// <summary>
        /// It returns the factory of new pipelines for accepted connections.
        /// </summary>
        IPipelineFactory PipelineFactory { get; }

        /// <summary>
        /// True if the server channel is listening for new connections. Otherwise false.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// It returns the child list of accepted and connected channels.
        /// </summary>
        Dictionary<string, IServerChildChannel> Childs { get; }

        /// <summary>
        /// Ask the current channel to start listening connections.
        /// </summary>
        /// <returns>
        /// The operation request control.
        /// </returns>
        ChannelRequestCtrl StartListening();

        /// <summary>
        /// Ask the current channel to stop listening new connections.
        /// </summary>
        void StopListening();

        /// <summary>
        /// Raised when a child channel is connected and accepted.
        /// </summary>
        event ServerChannelChildConnectedEventHandler ChildConnected;

        /// <summary>
        /// Raised when a child disconnects.
        /// </summary>
        event ServerChannelChildDisconnectedEventHandler ChildDisconnected;

        /// <summary>
        /// Raised when a child changes its address.
        /// </summary>
        event ServerChannelChildAddressChangedEventHandler ChildAddressChanged;
    }
}