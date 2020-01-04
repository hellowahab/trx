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
using System.ComponentModel;
using System.Configuration.Install;

namespace TrxBootstrap
{
    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer
    {
        public ServiceInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            string serviceName = null;

            if (Context.Parameters.ContainsKey("servicename"))
                serviceName = Context.Parameters["servicename"];

            if (!string.IsNullOrEmpty(serviceName))
            {
                serviceInstaller.ServiceName = serviceName;
                serviceInstaller.DisplayName = string.Format("{0} ({1})", serviceInstaller.DisplayName, serviceName);
            }

            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            string serviceName = null;

            if (Context.Parameters.ContainsKey("servicename"))
                serviceName = Context.Parameters["servicename"];

            if (!string.IsNullOrEmpty(serviceName))
                serviceInstaller.ServiceName = serviceName;

            base.Uninstall(savedState);
        }
    }
}