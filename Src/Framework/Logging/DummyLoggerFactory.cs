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

namespace Trx.Logging
{
    public class DummyLoggerFactory : LoggerFactory
    {
        private readonly bool _enabled;
        private readonly LogLevel _logLevel = LogLevel.Debug;

        public DummyLoggerFactory()
        {
        }

        public DummyLoggerFactory(bool enabled, LogLevel level)
        {
            _enabled = enabled;
            _logLevel = level;
        }

        public bool Enabled
        {
            get { return _enabled; }
        }

        public LogLevel LogLevel
        {
            get { return _logLevel; }
        }

        public override ILogger GetInstance(string name)
        {
            return new DummyLogger(name, _enabled, _logLevel);
        }
    }
}