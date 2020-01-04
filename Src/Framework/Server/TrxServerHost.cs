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
using Trx.Logging;

namespace Trx.Server
{
    public class TrxServerHost
    {
        public static string ConfigDirectoryKey =  "TrxServerHost.ConfigDirectory";

        private readonly ILogger _logger;
        private readonly string _instanceName;
        private string _trxServerName;
        private readonly string _configDirectory;
        private readonly string _configFileName;
        private readonly string _binDirectory;
        private readonly bool _useSharedBaseDirectory;
        private readonly TrxServerTupleSpaceProvider _tupleSpaceProvider;

        private TrxServerProxy _proxy;

        public TrxServerHost(ILogger logger, string instanceName, string configDirectory, string configFileName,
            string binDirectory, bool useSharedBaseDirectory, TrxServerTupleSpaceProvider tupleSpaceProvider)
        {
            _logger = logger;
            _instanceName = instanceName;
            _configDirectory = configDirectory;
            _configFileName = configFileName;
            _binDirectory = binDirectory;
            _useSharedBaseDirectory = useSharedBaseDirectory;
            _tupleSpaceProvider = tupleSpaceProvider;

            CreateTrxServerProxy();
        }

        public bool Failed { get; private set; }

        public AppDomain Domain { get; private set; }

        public string ConfigDirectory
        {
            get { return _configDirectory; }
        }

        public string ConfigFileName
        {
            get { return _configFileName; }
        }

        public string BinDirectory
        {
            get { return _binDirectory; }
        }

        public int FileSystemChangeBatchId { get; set; }

        public bool UseSharedBaseDirectory
        {
            get { return _useSharedBaseDirectory; }
        }

        public string InstanceName
        {
            get { return _instanceName; }
        }

        private void CreateTrxServerProxy()
        {
            try
            {
                Domain = AppDomain.CreateDomain(InstanceName, AppDomain.CurrentDomain.Evidence,
                    UseSharedBaseDirectory ? AppDomain.CurrentDomain.BaseDirectory : _binDirectory, null, true);

                Domain.SetData(ConfigDirectoryKey, _configDirectory);

                var type = typeof (TrxServerProxy);
                _proxy = (TrxServerProxy) Domain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName, type.FullName);

                _logger.Info(string.Format("Creating Trx Server from config file: {0}", _configFileName));
                _trxServerName = _proxy.CreateTrxServer(InstanceName, _configDirectory, _configFileName, _binDirectory, _tupleSpaceProvider);
                _logger.Info(string.Format("Trx Server instance '{0}' was created{1}", InstanceName,
                    _trxServerName == null ? string.Empty : " (friendly name: " + _trxServerName + ")" ));
            } catch (Exception e)
            {
                Failed = true;
                try
                {
                    if (Domain != null)
                        AppDomain.Unload(Domain);
                } catch
                {
                }
                _logger.Error(string.Format("Failed to create instance '{0}'", InstanceName), e);
            }
        }

        public bool IsALoadedAssembly(string lowCaseAssemblyName)
        {
            return !Failed && _proxy.IsAssemblyLoaded(lowCaseAssemblyName);
        }

        public ITrxServerTupleSpace GetTupleSpaceByName(string name)
        {
            return Failed ? null : _proxy.GetTupleSpaceByName(name);
        }

        /// <summary>
        /// Called from remote tuple space provider to notify a Trx Server is unloading.
        /// </summary>
        /// <param name="instanceName">
        /// The unloading Trx Server.
        /// </param>
        public void TrxServerIsUnloading(string instanceName)
        {
            if (!Failed)
                _proxy.TrxServerIsUnloading(instanceName);
        }

        public void Start()
        {
            if (Failed)
                return;

            _logger.Info(string.Format("Starting Trx Server instance '{0}'", InstanceName));
            _proxy.Start();
            _logger.Info(string.Format("Trx Server instance '{0}' is started", InstanceName));
        }

        public void Stop()
        {
            if (Failed)
                return;

            _logger.Info(string.Format("Stopping Trx Server instance '{0}'", InstanceName));
            _proxy.Stop();
            _logger.Info(string.Format("Trx Server instance '{0}' is stopped", InstanceName));
        }

        public void Unload()
        {
            if (Failed)
                return;

            _logger.Info(string.Format("Unloading Trx Server instance '{0}'", InstanceName));
            _proxy.Stop();

            _tupleSpaceProvider.NotifyClientsInstanceIsUnloading(InstanceName);

            try
            {
                new Action<AppDomain>(AppDomain.Unload).BeginInvoke(Domain, null, null);
            } catch (CannotUnloadAppDomainException ex)
            {
                _logger.Warn("Recovered error caugth unloading application domain.", ex);
            }
            _logger.Info(string.Format("Trx Server instance '{0}' is unloaded", InstanceName));
        }
    }
}
