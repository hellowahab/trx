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
using System.IO;
using System.Reflection;
using Trx.Logging;
using Trx.Utilities.Dom2Obj;

namespace Trx.Server
{
    public class TrxServerProxy : MarshalByRefObject
    {
        private string _binDirectory;
        private TrxServer _trxServer;

        public string CreateTrxServer(string instanceName, string configDirectory, string configFileName, string binDirectory,
            TrxServerTupleSpaceProvider tupleSpaceProvider)
        {
            _binDirectory = binDirectory;

            if (AppDomain.CurrentDomain.BaseDirectory != _binDirectory)
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            _trxServer = Digester.DigestFile(configFileName) as TrxServer;
            if (_trxServer == null)
                throw new TrxServiceException(string.Format("Invalid root object in config file '{0}', " +
                    "an object of type Trx.Bootstrap.TrxServer was expected", configFileName));

            _trxServer.InstanceName = instanceName;
            _trxServer.ConfigDirectory = configDirectory;
            _trxServer.ConfigFileName = configFileName;

            // Configure logging in this application domain
            if (_trxServer.LoggerFactory != null)
                LogManager.LoggerFactory = _trxServer.LoggerFactory;

            if (_trxServer.LoggerRenderer != null)
                LogManager.Renderer = _trxServer.LoggerRenderer;

            _trxServer.TupleSpaceProvider = tupleSpaceProvider;

            return _trxServer.Name;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_binDirectory == null)
                return null;

            string[] assemblyDetail = args.Name.Split(',');
            string fileName = string.Format("{0}\\{1}.dll", _binDirectory, assemblyDetail[0]);
            if (!File.Exists(fileName))
                return null;

            return Assembly.LoadFrom(fileName);
        }

        public bool IsAssemblyLoaded(string lowCaseAssemblyName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var file = Path.GetFileName(asm.Location);
                if (file != null && file.ToLower() == lowCaseAssemblyName)
                    return true;
            }

            return false;
        }

        public ITrxServerTupleSpace GetTupleSpaceByName(string name)
        {
            return _trxServer.GetTupleSpaceByName(name);
        }

        /// <summary>
        /// Called from remote tuple space provider to notify a Trx Server is unloading.
        /// </summary>
        /// <param name="instanceName">
        /// The unloading Trx Server.
        /// </param>
        public void TrxServerIsUnloading(string instanceName)
        {
            _trxServer.TrxServerIsUnloading(instanceName);
        }

        public void Start()
        {
            if (_trxServer == null)
                return;

            _trxServer.Start();
        }

        public void Stop()
        {
            if (_trxServer == null)
                return;

            _trxServer.Stop();
        }

        /// <summary>
        /// Proxy lease control to prevent automatic reference release.
        /// </summary>
        /// <returns>
        /// Null, long live to the object :)
        /// </returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}