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
using System.Collections;

namespace Trx.Messaging.FlowControl
{
    public class ServerCollection : ICollection
    {
        private readonly Hashtable _servers;

        public ServerCollection()
        {
            _servers = new Hashtable(8);
        }

        public Server this[string name]
        {
            get { return (Server) _servers[name]; }

            set
            {
                if (value == null)
                    return;

                _servers[name] = value;
            }
        }

        public int Count
        {
            get { return _servers.Count; }
        }

        public void Add(Server server)
        {
            if (server == null)
                return;

            this[server.Name] = server;
        }

        public void Remove(string name)
        {
            _servers.Remove(name);
        }

        public void Clear()
        {
            if (_servers.Count == 0)
                return;

            _servers.Clear();
        }

        public bool Contains(string name)
        {
            return _servers.Contains(name);
        }

        #region Implementation of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new ServersEnumerator(_servers);
        }

        private class ServersEnumerator : IEnumerator
        {
            private readonly IEnumerator _serversEnumerator;

            public ServersEnumerator(Hashtable servers)
            {
                _serversEnumerator = servers.GetEnumerator();
            }

            #region Implementation of IEnumerator
            public void Reset()
            {
                _serversEnumerator.Reset();
            }

            public bool MoveNext()
            {
                return _serversEnumerator.MoveNext();
            }

            public object Current
            {
                get { return ((DictionaryEntry) _serversEnumerator.Current).Value; }
            }
            #endregion
        }
        #endregion

        #region ICollection Members
        public bool IsSynchronized
        {
            get { return false; }
        }

        public void CopyTo(Array array, int index)
        {
            _servers.CopyTo(array, index);
        }

        public object SyncRoot
        {
            get { return this; }
        }
        #endregion
    }
}