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
    /// This class implements a listener peer. It accepts only one
    /// connection at a time.
    /// </summary>
    public class ListenerPeer : ClientPeer, IChannelPool
    {
        private const string SyncObject = "_so_";
        private bool _enabled;
        private IListener _listener;

        /// <summary>
        /// It initializes a new <see cref="ListenerPeer"/> class instance.
        /// </summary>
        /// <param name="name">
        /// It's the name of the listener peer.
        /// </param>
        /// <param name="listener">
        /// It's the listener accepting the remote connection.
        /// </param>
        /// <param name="channel">
        /// It's the channel to use.
        /// </param>
        /// <remarks>
        /// The listener and the channel must be inactive.
        /// </remarks>
        public ListenerPeer(string name, IChannel channel,
            IListener listener) : base(name, channel)
        {
            ConstructorHelper(listener);
        }

        /// <summary>
        /// It initializes a new <see cref="ListenerPeer"/> class instance,
        /// configurating it to process <see cref="PeerRequest"/>
        /// </summary>
        /// <param name="name">
        /// It's the name of the listener peer.
        /// </param>
        /// <param name="listener">
        /// It's the listener accepting the remote connection.
        /// </param>
        /// <param name="channel">
        /// It's the channel to use.
        /// </param>
        /// <param name="messagesIdentifier">
        /// It's the object in charge to match request messages with their
        /// responses.
        /// </param>
        /// <remarks>
        /// The listener and the channel must be inactive.
        /// </remarks>
        public ListenerPeer(string name, IChannel channel,
            IMessagesIdentifier messagesIdentifier,
            IListener listener) : base(name, channel, messagesIdentifier)
        {
            ConstructorHelper(listener);
        }

        /// <summary>
        /// It returns the listener accepting the remote connection.
        /// </summary>
        public IListener Listener
        {
            get { return _listener; }
        }

        #region IChannelPool Members
        /// <summary>
        /// It returns the number of free channels in the pool.
        /// </summary>
        public int Length
        {
            get
            {
                if (Channel.IsConnected)
                    return 0;
                return 1;
            }
        }

        /// <summary>
        /// It adds a channel to the pool.
        /// </summary>
        /// <param name="channel">
        /// It's the channel to be added.
        /// </param>
        /// <returns>
        /// True if the channel was added, otherwise false.
        /// </returns>
        /// <remarks>
        /// This peer can't handle more than one channel, the channel
        /// used by the ListenerPeer instance is the one received in their
        /// constructor.
        /// </remarks>
        public bool Add(IChannel channel)
        {
            return false;
        }

        /// <summary>
        /// It removes a channel from the pool.
        /// </summary>
        /// <returns>
        /// The removed channel, otherwise null if the pool was empty.
        /// </returns>
        /// <remarks>
        /// The channel used by the ListenerPeer is never removed. If it's
        /// connected, a reference of the channel is returned.
        /// </remarks>
        public IChannel Remove()
        {
            if (Channel.IsConnected)
                return null;
            return Channel;
        }
        #endregion

        /// <summary>
        /// Constructor helper.
        /// </summary>
        /// <param name="listener">
        /// It's the listener accepting the remote connection.
        /// </param>
        private void ConstructorHelper(IListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");

            _listener = listener;
            _listener.ChannelPool = this;
            _listener.Connected += OnListenerConnected;
            _enabled = false;
        }

        /// <summary>
        /// Traps the <see cref="IChannel.Disconnected"/> event.
        /// </summary>
        /// <param name="sender">
        /// It's the channel sending the event.
        /// </param>
        /// <param name="e">
        /// The event parameters.
        /// </param>
        protected override void OnChannelDisconnected(object sender, EventArgs e)
        {
            lock (SyncObject)
            {
                if (_enabled)
                    _listener.Start();
            }

            base.OnChannelDisconnected(sender, e);
        }

        /// <summary>
        /// Traps the <see cref="IListener.Connected"/> event.
        /// </summary>
        /// <param name="sender">
        /// It's the listener sending the event.
        /// </param>
        /// <param name="e">
        /// The event parameters.
        /// </param>
        private void OnListenerConnected(object sender, ListenerConnectedEventArgs e)
        {
            lock (SyncObject)
            {
                _listener.Stop();
            }
        }

        /// <summary>
        /// Start listening for the remote peer.
        /// </summary>
        public override void Connect()
        {
            lock (SyncObject)
            {
                if (!Channel.IsConnected)
                {
                    _enabled = true;
                    _listener.Start();
                }
            }
        }

        /// <summary>
        /// It closes the connection with the remote peer (if it's established).
        /// </summary>
        public override void Close()
        {
            lock (SyncObject)
            {
                _enabled = false;
                _listener.Stop();
                Channel.Close();
            }
        }
    }
}