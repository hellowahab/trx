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

namespace Trx.Logging
{
    public static class LogManager
    {
        public const string DefaultLogger = "Default";
        private static LoggerFactory _loggerFactory = new DummyLoggerFactory();
        private static readonly Dictionary<string, WeakReference> Loggers = new Dictionary<string, WeakReference>();

        static LogManager()
        {
            LoggerFactory = new DummyLoggerFactory();
        }

        public static string Domain { get; set; }

        public static LoggerFactory LoggerFactory
        {
            get { return _loggerFactory; }
            set
            {
                lock (Loggers)
                    _loggerFactory = value ?? new DummyLoggerFactory();
            }
        }

        public static IRenderer Renderer { get; set; }

        public static ILogger GetLogger(string name)
        {
            lock (Loggers)
            {
                WeakReference wr;
                ILogger logger;
                if (Loggers.ContainsKey(name))
                {
                    wr = Loggers[name];
                    logger = wr.Target as ILogger;
                    if (!wr.IsAlive)
                    {
                        logger = LoggerFactory.GetInstance(name);
                        wr = new WeakReference(logger);
                        Loggers[name] = wr;
                    }
                }
                else
                {
                    logger = LoggerFactory.GetInstance(name);
                    wr = new WeakReference(logger);
                    Loggers.Add(name, wr);
                }

                return logger;
            }
        }

        public static ILogger GetLogger()
        {
            return GetLogger(DefaultLogger);
        }

        public static void SetLogger(ILogger logger)
        {
            lock (Loggers)
                Loggers.Add(logger.Name, new WeakReference(logger));
        }
    }
}