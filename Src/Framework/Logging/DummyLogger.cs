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

namespace Trx.Logging
{
    /// <summary>
    /// Implements a logger wich basically do nothing.
    /// </summary>
    public class DummyLogger : BaseLogger
    {
        private readonly bool _enabled = false;
        private readonly LogLevel _logLevel = LogLevel.Debug;

        public DummyLogger(string name) : base(name)
        {
        }

        public DummyLogger(string name, bool enabled, LogLevel level)
            : base(name)
        {
            _enabled = enabled;
            _logLevel = level;
        }

        public bool Enabled { get { return _enabled; } }

        public LogLevel LogLevel { get { return _logLevel; } }

        public override bool IsDebugEnabled()
        {
            return _enabled && _logLevel >= LogLevel.Debug;
        }

        public override bool IsInfoEnabled()
        {
            return _enabled && _logLevel >= LogLevel.Info;
        }

        public override bool IsWarnEnabled()
        {
            return _enabled && _logLevel >= LogLevel.Warn;
        }

        public override bool IsErrorEnabled()
        {
            return _enabled && _logLevel == LogLevel.Error;
        }

        protected override void InternalDebug(object message)
        {
        }

        protected override void InternalDebug(object message, Exception cause)
        {
        }

        protected override void InternalInfo(object message)
        {
        }

        protected override void InternalInfo(object message, Exception cause)
        {
        }

        protected override void InternalWarn(object message)
        {
        }

        protected override void InternalWarn(object message, Exception cause)
        {
        }

        protected override void InternalError(object message)
        {
        }

        protected override void InternalError(object message, Exception cause)
        {
        }
    }
}
