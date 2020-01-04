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
using Trx.Exceptions;

namespace Trx.Logging
{
    public abstract class BaseLogger : ILogger
    {
        private readonly string _name;

        protected BaseLogger(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _name = name;
        }

        #region ILogger Members
        public string Name
        {
            get { return _name; }
        }

        public void Log(LogLevel level, Object message)
        {
            var renderer = Renderer ?? LogManager.Renderer;
            switch (level)
            {
                case LogLevel.Debug:
                    if (IsDebugEnabled())
                        InternalDebug(renderer == null ? message + Environment.NewLine : renderer.Render(DateTime.Now, level, message));
                    break;
                case LogLevel.Info:
                    if (IsInfoEnabled())
                        InternalInfo(renderer == null ? message + Environment.NewLine : renderer.Render(DateTime.Now, level, message));
                    break;
                case LogLevel.Warn:
                    if (IsWarnEnabled())
                        InternalWarn(renderer == null ? message + Environment.NewLine : renderer.Render(DateTime.Now, level, message));
                    break;
                case LogLevel.Error:
                    if (IsErrorEnabled())
                        InternalError(renderer == null ? message + Environment.NewLine : renderer.Render(DateTime.Now, level, message));
                    break;
                default:
                    throw new BugException();
            }
        }

        public void Log(LogLevel level, Object message, Exception cause)
        {
            var renderer = Renderer ?? LogManager.Renderer;
            switch (level)
            {
                case LogLevel.Debug:
                    if (IsDebugEnabled())
                        if (renderer == null)
                            InternalDebug(message + Environment.NewLine, cause);
                        else
                            InternalDebug(renderer.Render(DateTime.Now, level, message, cause));
                    break;
                case LogLevel.Info:
                    if (IsInfoEnabled())
                        if (renderer == null)
                            InternalInfo(message + Environment.NewLine, cause);
                        else
                            InternalInfo(renderer.Render(DateTime.Now, level, message, cause));
                    break;
                case LogLevel.Warn:
                    if (IsWarnEnabled())
                        if (renderer == null)
                            InternalWarn(message + Environment.NewLine, cause);
                        else
                            InternalWarn(renderer.Render(DateTime.Now, level, message, cause));
                    break;
                case LogLevel.Error:
                    if (IsErrorEnabled())
                        // An error event should not raise an exception logging it, so ...
                        try
                        {
                            if (renderer == null)
                                InternalError(message + Environment.NewLine, cause);
                            else
                                InternalError(renderer.Render(DateTime.Now, level, message, cause));
                        }
                        catch
                        {
                            // ... just ignore it.
                        }
                    break;
                default:
                    throw new BugException();
            }
        }

        public bool IsEnabledFor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return IsDebugEnabled();
                case LogLevel.Info:
                    return IsInfoEnabled();
                case LogLevel.Warn:
                    return IsWarnEnabled();
                case LogLevel.Error:
                    return IsErrorEnabled();
                default:
                    throw new BugException();
            }
        }

        public abstract bool IsDebugEnabled();

        public void Info(Object message)
        {
            Log(LogLevel.Info, message);
        }

        public void Info(Object message, Exception cause)
        {
            Log(LogLevel.Info, message, cause);
        }

        public abstract bool IsInfoEnabled();

        public void Debug(Object message)
        {
            Log(LogLevel.Debug, message);
        }

        public void Debug(Object message, Exception cause)
        {
            Log(LogLevel.Debug, message, cause);
        }

        public void Warn(Object message)
        {
            Log(LogLevel.Warn, message);
        }

        public void Warn(Object message, Exception cause)
        {
            Log(LogLevel.Warn, message, cause);
        }

        public abstract bool IsWarnEnabled();

        public void Error(Object message)
        {
            Log(LogLevel.Error, message);
        }

        public void Error(Object message, Exception cause)
        {
            Log(LogLevel.Error, message, cause);
        }

        public abstract bool IsErrorEnabled();
        #endregion

        public IRenderer Renderer { get; set; }

        protected abstract void InternalDebug(Object message);

        protected abstract void InternalDebug(Object message, Exception cause);

        protected abstract void InternalInfo(Object message);

        protected abstract void InternalInfo(Object message, Exception cause);

        protected abstract void InternalWarn(Object message);

        protected abstract void InternalWarn(Object message, Exception cause);

        protected abstract void InternalError(Object message);

        protected abstract void InternalError(Object message, Exception cause);
    }
}