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
using System.Collections.Generic;
using System.Threading;

namespace Trx.Server
{
    /// <summary>
    /// This class implements the object responsible to provide remote references between 
    /// Trx Server instances.
    /// </summary>
    public class TrxServerTupleSpaceProvider : MarshalByRefObject
    {
        private readonly Bootstrap _bootstrap;

        /// <summary>
        /// An indexed dictionary with the name server instances hosting one or more tuple spaces
        /// to a list of client servers.
        /// </summary>
        private readonly Dictionary<string, List<string>> _references = new Dictionary<string, List<string>>();

        // Keep manual reset events to make GetTupleSpaceByName to wait until a loading Trx Server is done.
        private readonly Dictionary<string, ManualResetEvent> _reloadingServers =
            new Dictionary<string, ManualResetEvent>();

        public TrxServerTupleSpaceProvider(Bootstrap bootstrap)
        {
            _bootstrap = bootstrap;
        }

        /// <summary>
        /// Called from Bootstrap to inform a Trx Server is loading, it allows us
        /// to implement a mechanism to sleep threads needing any tuple space hosted
        /// in that instance.
        /// </summary>
        /// <param name="instanceName">
        /// The name of the loading instance.
        /// </param>
        internal void LoadingInstance(string instanceName)
        {
            lock (_reloadingServers)
                _reloadingServers.Add(instanceName.ToLower(), new ManualResetEvent(false));
        }

        /// <summary>
        /// Works in conjunction with LoadingInstance to finish the containment mechanism.
        /// </summary>
        /// <param name="instanceName">
        /// The name of the loaded instance.
        /// </param>
        internal void LoadFinished(string instanceName)
        {
            lock (_reloadingServers)
            {
                var key = instanceName.ToLower();
                if (!_reloadingServers.ContainsKey(key))
                    return;

                _reloadingServers[key].Set();
                _reloadingServers.Remove(key);
            }
        }

        /// <summary>
        /// Notify every client using the unloading Trx Server.
        /// </summary>
        /// <param name="instanceName">
        /// The Trx Server instance wich is unloading.
        /// </param>
        internal void NotifyClientsInstanceIsUnloading(string instanceName)
        {
            lock (_references)
            {
                var key = instanceName.ToLower();
                if (!_references.ContainsKey(key))
                    return;

                List<string> list = _references[key];
                foreach (string client in list)
                {
                    var host = _bootstrap.GetTrxServerHost(client);
                    if (host != null)
                        host.TrxServerIsUnloading(instanceName);
                }

                _references.Remove(key);
            }
        }

        private void SaveReference(string clientInstanceName, string serverInstanceName)
        {
            var key = serverInstanceName.ToLower();
            if (_references.ContainsKey(key))
            {
                List<string> list = _references[key];
                foreach (string client in list)
                    if (client == clientInstanceName)
                        // Already registered.
                        return;
                list.Add(clientInstanceName);
            }
            else
                _references.Add(key, new List<string> {clientInstanceName});
        }

        public ITrxServerTupleSpace GetTupleSpaceByName(string clientInstanceName, string serverInstanceName,
            string name)
        {
            ManualResetEvent mre = null;
            lock (_reloadingServers)
            {
                var key = serverInstanceName.ToLower();
                if (_reloadingServers.ContainsKey(key))
                    mre = _reloadingServers[key];
            }

            if (mre != null)
                // Wait until the new instance is ready
                mre.WaitOne();

            lock (_references)
            {
                TrxServerHost host = _bootstrap.GetTrxServerHost(serverInstanceName);
                if (host != null)
                {
                    ITrxServerTupleSpace tupleSpace = host.GetTupleSpaceByName(name);
                    if (tupleSpace != null)
                        SaveReference(clientInstanceName, serverInstanceName);

                    return tupleSpace;
                }

                return null;
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}