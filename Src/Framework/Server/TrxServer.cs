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
using Trx.Logging;

namespace Trx.Server
{
    [Serializable]
    public class TrxServer : TrxServiceBase
    {
        private readonly Dictionary<int, ITrxService> _services = new Dictionary<int, ITrxService>();

        private readonly List<ITrxServerTupleSpace> _tupleSpaces = new List<ITrxServerTupleSpace>();

        public IServicesProvider ServicesProvider { get; set; }

        public string InstanceName { get; set; }

        public string ConfigFileName { get; set; }

        public string ConfigDirectory { get; set; }

        public LoggerFactory LoggerFactory { get; set; }

        public IRenderer LoggerRenderer { get; set; }

        internal TrxServerTupleSpaceProvider TupleSpaceProvider { get; set; }

        public List<ITrxServerTupleSpace> TupleSpaces
        {
            get { return _tupleSpaces; }
        }

        public ITrxServerTupleSpace GetTupleSpaceByName(string name)
        {
            foreach (ITrxServerTupleSpace objectTupleSpace in _tupleSpaces)
                if (name == objectTupleSpace.Name)
                    return objectTupleSpace;

            return null;
        }

        /// <summary>
        /// Called from remote tuple space provider to notify a Trx Server is unloading.
        /// </summary>
        /// <param name="instanceName">
        /// The unloading Trx Server.
        /// </param>
        public void TrxServerIsUnloading(string instanceName)
        {
            foreach (ITrxServerTupleSpace trxServerTupleSpace in _tupleSpaces)
            {
                var ts = trxServerTupleSpace as TrxServerTupleSpaceProxy;
                if (ts != null)
                    ts.TrxServerIsUnloading(instanceName);
            }
        }

        /// <summary>
        /// Called from services provider when a new service must be deployed.
        /// </summary>
        /// <param name="service">
        /// The new service.
        /// </param>
        public void DeployService(ITrxService service)
        {
            try
            {
                Logger.Info(string.Format("Starting service '{0}'", service.Name));
                service.Init(this);
                service.Start();
                Logger.Info(string.Format("Service '{0}' was started", service.Name));

                _services.Add(service.Id, service);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception caught starting service '{0}', current state is: {1}.",
                    service.Name, service.State), e);
            }
        }

        private void PrivateUndeployService(ITrxService service, bool removeFromDictionary)
        {
            try
            {
                Logger.Info(string.Format("Stopping service '{0}'", service.Name));
                if (removeFromDictionary)
                    _services.Remove(service.Id);
                service.Stop();
                service.Dispose();
                Logger.Info(string.Format("Service '{0}' was stopped", service.Name));
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception caught stopping service '{0}', current state is: {1}.",
                    service.Name, service.State), e);
            }
        }

        /// <summary>
        /// Called from services provider when a service needs to be undeployed.
        /// </summary>
        /// <param name="service">
        /// The service to undeploy.
        /// </param>
        public void UndeployService(ITrxService service)
        {
            PrivateUndeployService(service, true);
        }

        protected override void ProtectedStart()
        {
            // Set reference to remote tuple space provider for remote tuple spaces
            foreach (ITrxServerTupleSpace trxServerTupleSpace in _tupleSpaces)
            {
                var ts = trxServerTupleSpace as TrxServerTupleSpaceProxy;
                if (ts != null)
                    ts.TupleSpaceProvider = TupleSpaceProvider;
            }

            if (ServicesProvider == null)
                return;

            ServicesProvider.Logger = Logger;
            ServicesProvider.Start(this);
        }

        protected override void ProtectedStop()
        {
            if (ServicesProvider != null)
                ServicesProvider.Stop();

            foreach (ITrxService trxService in _services.Values)
                PrivateUndeployService(trxService, false);

            _services.Clear();
        }
    }
}