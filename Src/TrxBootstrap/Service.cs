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

using System.ServiceProcess;
using Trx.Server;

namespace TrxBootstrap
{
    partial class Service : ServiceBase
    {
        private static Bootstrap _bootstrap;

        public Service(Bootstrap bootstrap)
        {
            InitializeComponent();

            _bootstrap = bootstrap;
        }

        protected override void OnStart(string[] args)
        {
            _bootstrap.Start();
        }

        protected override void OnStop()
        {
            _bootstrap.Shutdown();
        }
    }
}
