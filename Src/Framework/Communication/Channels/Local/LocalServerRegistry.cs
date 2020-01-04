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

namespace Trx.Communication.Channels.Local
{
    internal sealed class LocalServerRegistry
    {
        private readonly object _lockObj = new object();
        private static volatile LocalServerRegistry _instance;
        private readonly Dictionary<string, LocalServerChannel> _servers;

        #region Constructors
        private LocalServerRegistry()
        {
            _servers = new Dictionary<string, LocalServerChannel>(4);
        }
        #endregion

        #region Methods
        public static LocalServerRegistry GetInstance()
        {
            if (_instance == null)
                lock (typeof(LocalServerRegistry))
                    if (_instance == null)
                        _instance = new LocalServerRegistry();

            return _instance;
        }

        public bool Register(string address, LocalServerChannel channel)
        {
            lock (_lockObj)
            {
                if (_servers.ContainsKey(address))
                    return false;

                _servers.Add(address, channel);

                return true;
            }
        }

        public void Unregister(string address)
        {
            lock (_lockObj)
                _servers.Remove(address);
        }

        public LocalServerChildChannel Connect(string address, LocalClientChannel client)
        {
            lock (_lockObj)
                return _servers.ContainsKey(address) ? _servers[address].Accept(client) : null;
        }
        #endregion
    }
}
