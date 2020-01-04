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

using System.Collections;


namespace Trx.Messaging.FlowControl
{
    public class ServerPeerCollection : IEnumerable
    {
        private readonly Hashtable _peers;

        public ServerPeerCollection()
        {
            _peers = new Hashtable(64);
        }

        public ServerPeer this[string name]
        {
            get { return (ServerPeer) _peers[name]; }

            set
            {
                if (value == null)
                    return;

                _peers[name] = value;
            }
        }

        public int Count
        {
            get { return _peers.Count; }
        }

        public void Add(ServerPeer peer)
        {
            if (peer == null)
                return;

            this[peer.Name] = peer;
        }

        public void Remove(string name)
        {
            _peers.Remove(name);
        }

        public void Clear()
        {
            if (_peers.Count == 0)
                return;

            _peers.Clear();
        }

        public bool Contains(string name)
        {
            return _peers.Contains(name);
        }

        #region Implementation of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new PeersEnumerator(_peers);
        }

        private class PeersEnumerator : IEnumerator
        {
            private readonly IEnumerator _peersEnumerator;

            public PeersEnumerator(Hashtable peers)
            {
                _peersEnumerator = peers.GetEnumerator();
            }

            #region Implementation of IEnumerator>
            public void Reset()
            {
                _peersEnumerator.Reset();
            }

            public bool MoveNext()
            {
                return _peersEnumerator.MoveNext();
            }

            public object Current
            {
                get { return ((DictionaryEntry) _peersEnumerator.Current).Value; }
            }
            #endregion
        }
        #endregion
    }
}