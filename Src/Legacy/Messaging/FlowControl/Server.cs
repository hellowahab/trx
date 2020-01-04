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
using System.Reflection;
using log4net;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// It's the <see cref="Server.Connected"/> event delegate.
    /// </summary>
    public delegate void ServerPeerConnectedEventHandler(
        object sender, ServerPeerConnectedEventArgs e);

    /// <summary>
    /// It's the <see cref="Server.Disconnected"/> event delegate.
    /// </summary>
    public delegate void ServerPeerDisconnectedEventHandler(
        object sender, ServerPeerDisconnectedEventArgs e);

    public class Server
    {
        private readonly IListener _listener;
        private readonly string _name;
        private readonly IServerPeerManager _serverPeerManager;
        private ILog _logger;

        public Server(string name, IListener listener, IServerPeerManager serverPeerManager)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (listener == null)
                throw new ArgumentNullException("listener");

            if (serverPeerManager == null)
                throw new ArgumentNullException("serverPeerManager");

            _name = name;
            HostStart = true;
            _serverPeerManager = serverPeerManager;
            _listener = listener;
            _listener.ConnectionRequest += OnListenerConnectionRequest;
            _listener.Connected += OnListenerConnected;
        }

        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HostStart { get; set; }

        public ILog Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType));
            }

            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        MethodBase.GetCurrentMethod().DeclaringType);
                else
                    _logger = value;
            }
        }

        public string LoggerName
        {
            set {
                Logger = string.IsNullOrEmpty(value) ? null : LogManager.GetLogger(value);
            }

            get { return Logger.Logger.Name; }
        }

        public IListener Listener
        {
            get { return _listener; }
        }

        public ServerPeerCollection Peers
        {
            get { return _serverPeerManager.Peers; }
        }

        public IServerPeerManager PeerManager
        {
            get { return _serverPeerManager; }
        }

        public event ServerPeerConnectedEventHandler Connected;

        public event ServerPeerDisconnectedEventHandler Disconnected;

        private void OnConnected(ServerPeer peer)
        {
            if (Connected != null)
                Connected(this, new ServerPeerConnectedEventArgs(peer));
        }

        private void OnDisconnected(ServerPeer peer)
        {
            if (Disconnected != null)
                Disconnected(this, new ServerPeerDisconnectedEventArgs(peer));
        }

        private void OnListenerConnectionRequest(object sender,
            ListenerConnectionRequestEventArgs e)
        {
            e.Accept = _serverPeerManager.AcceptConnectionRequest(e.ConnectionInfo);
        }

        private void OnPeerDisconnected(object sender, EventArgs e)
        {
            var peer = (ServerPeer) sender;

            peer.Disconnected -= OnPeerDisconnected;

            if (Logger.IsDebugEnabled)
                Logger.Debug(string.Format("Server '{0}' - OnPeerDisconnected '{1}'.",
                    _name, peer.Name));

            OnDisconnected(peer);
        }

        private void OnListenerConnected(object sender, ListenerConnectedEventArgs e)
        {
            ServerPeer peer = _serverPeerManager.Connected(e.Channel);

            peer.Disconnected += OnPeerDisconnected;

            if (Logger.IsDebugEnabled)
                Logger.Debug(string.Format("Server '{0}' - OnListenerConnected '{1}'.",
                    _name, peer.Name));

            OnConnected(peer);
        }
    }
}