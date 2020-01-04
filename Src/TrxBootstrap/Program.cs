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
using System.Configuration;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Trx.Logging;
using Trx.Server;

namespace TrxBootstrap
{
    internal class Program
    {
        private static readonly object LockObject = new object();
        public static bool KeepRunning = true;

        private static int Main(string[] args)
        {
            if (Environment.UserInteractive)
                Console.CancelKeyPress += OnCancelKeyPress;

            LogManager.Domain = ConfigurationManager.AppSettings["Domain"];
            if (string.IsNullOrEmpty(LogManager.Domain))
                LogManager.Domain = "TrxBootstrap";

            var loggerFactoryTypeName = ConfigurationManager.AppSettings["LoggerFactoryTypeName"];
            if (!string.IsNullOrEmpty(loggerFactoryTypeName))
                LogManager.LoggerFactory =
                    Activator.CreateInstance(Type.GetType(loggerFactoryTypeName)) as LoggerFactory;

            var loggerRendererTypeName = ConfigurationManager.AppSettings["LoggerRendererTypeName"];
            if (!string.IsNullOrEmpty(loggerRendererTypeName))
                LogManager.Renderer = Activator.CreateInstance(Type.GetType(loggerRendererTypeName)) as IRenderer;

            var loggerName = ConfigurationManager.AppSettings["LoggerName"];
            var logger = string.IsNullOrEmpty(loggerName)
                ? LogManager.GetLogger()
                : LogManager.GetLogger(loggerName);

            var version = Assembly.GetEntryAssembly().GetName().Version;
            logger.Info(string.Format("Starting Trx Bootstrap version {0}", version));

            var configDirectory = ConfigurationManager.AppSettings["ConfigDirectory"];
            if (args != null && args.Length > 0)
                configDirectory = args[0];

            if (string.IsNullOrEmpty(configDirectory))
            {
                logger.Error(
                    "Configuration directory was not given in application configuration file (ConfigDirectory key in appSettings) or command line");
                return -1;
            }

            bool reloadAppOnChanges;
            bool.TryParse(ConfigurationManager.AppSettings["ReloadAppOnChanges"], out reloadAppOnChanges);

            bool useSharedBaseDirectory;
            bool.TryParse(ConfigurationManager.AppSettings["UseSharedBaseDirectory"], out useSharedBaseDirectory);

            int pollInterval;
            if (!int.TryParse(ConfigurationManager.AppSettings["FileSystemChangesProcessingInMs"], out pollInterval))
                pollInterval = 5000;

            try
            {
                if (Environment.UserInteractive)
                {
                    var bootstrap = new Bootstrap(logger, configDirectory, reloadAppOnChanges, useSharedBaseDirectory,
                        pollInterval);

                    bootstrap.Start();

                    string msg = "Press Ctrl-C for a soft shutdown";
                    if (LogManager.Renderer != null)
                        msg = LogManager.Renderer.Render(DateTime.Now, LogLevel.Info, msg);
                     Console.WriteLine(msg);

                    lock (LockObject)
                        while (KeepRunning)
                            try
                            {
                                Monitor.Wait(LockObject);
                            }
                            catch (ThreadInterruptedException)
                            {
                            }

                    bootstrap.Shutdown();
                }
                else
                    ServiceBase.Run(new ServiceBase[]
                                        {
                                            new Service(new Bootstrap(logger, configDirectory, reloadAppOnChanges,
                                                useSharedBaseDirectory,
                                                pollInterval))
                                        });
            }
            catch (Exception e)
            {
                logger.Error("Exception caught on main Trx Bootstrap thread", e);
                return -1;
            }

            return 0;
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            LogManager.GetLogger().Info("Cancel key catched, shutting down ...");

            lock (LockObject)
            {
                KeepRunning = false;
                Monitor.Pulse(LockObject);
            }
            e.Cancel = true;
        }
    }
}