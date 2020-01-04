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
using log4net;

namespace Trx.Logging
{
    public class Log4NetLogger : BaseLogger
    {
        private readonly ILog _logger;

        public Log4NetLogger(string name, ILog logger) : base(name)
        {
            _logger = logger;
        }

        public override bool IsDebugEnabled()
        {
            return _logger.IsDebugEnabled;
        }

        public override bool IsInfoEnabled()
        {
            return _logger.IsInfoEnabled;
        }

        public override bool IsWarnEnabled()
        {
            return _logger.IsWarnEnabled;
        }

        public override bool IsErrorEnabled()
        {
            return _logger.IsErrorEnabled;
        }

        protected override void InternalDebug(object message)
        {
            _logger.Debug(message);
        }

        protected override void InternalDebug(object message, Exception cause)
        {
            _logger.Debug(message, cause);
        }

        protected override void InternalInfo(object message)
        {
            _logger.Info(message);
        }

        protected override void InternalInfo(object message, Exception cause)
        {
            _logger.Info(message, cause);
        }

        protected override void InternalWarn(object message)
        {
            _logger.Warn(message);
        }

        protected override void InternalWarn(object message, Exception cause)
        {
            _logger.Warn(message, cause);
        }

        protected override void InternalError(object message)
        {
            _logger.Error(message);
        }

        protected override void InternalError(object message, Exception cause)
        {
            _logger.Error(message, cause);
        }
    }
}