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
    public interface ILogger
    {
        string Name { get; }

        void Log(LogLevel level, Object message);

        void Log(LogLevel level, Object message, Exception e);

        bool IsEnabledFor(LogLevel level);

        void Debug(Object message);

        void Debug(Object message, Exception e);

        bool IsDebugEnabled();

        void Info(Object message);

        void Info(Object message, Exception e);

        bool IsInfoEnabled();

        void Warn(Object message);

        void Warn(Object message, Exception e);

        bool IsWarnEnabled();

        void Error(Object message);

        void Error(Object message, Exception e);

        bool IsErrorEnabled();
    }
}