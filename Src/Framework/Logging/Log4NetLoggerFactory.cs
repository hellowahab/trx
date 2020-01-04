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
using Trx.Server;
using log4net.Config;

namespace Trx.Logging
{
    public class Log4NetLoggerFactory : LoggerFactory
    {
        private bool _configured;

        public Log4NetLoggerFactory()
        {
            Watch = true;
        }

        public string ConfigFile { get; set; }

        public bool Watch { get; set; }

        public override ILogger GetInstance(string name)
        {
            if (!_configured)
            {
                var configFile = ConfigFile;
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".log4net");
                    if (!File.Exists(configFile))
                        configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                            Path.GetFileName(Assembly.GetCallingAssembly().Location) + ".log4net");
                }

                if (!Path.IsPathRooted(configFile))
                {
                    var configDirectory = AppDomain.CurrentDomain.GetData(TrxServerHost.ConfigDirectoryKey) as string;
                    if (!string.IsNullOrEmpty(configDirectory))
                        configFile = Path.Combine(configDirectory, configFile);
                }

                if (!string.IsNullOrEmpty(configFile))
                    if (Watch)
                        XmlConfigurator.ConfigureAndWatch(new FileInfo(configFile));
                    else
                        XmlConfigurator.Configure(new FileInfo(configFile));

                _configured = true;
            }

            return new Log4NetLogger(name, log4net.LogManager.GetLogger(name));
        }
    }
}