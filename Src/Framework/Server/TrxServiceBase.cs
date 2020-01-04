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
using System.Reflection;
using System.Text;
using Trx.Exceptions;
using Trx.Logging;

namespace Trx.Server
{
    public abstract class TrxServiceBase : ITrxService
    {
        public const string DefaultInputContext = "input";
        public const string DefaultOutputContext = "output";

        private readonly object _lockObj = new object();

        private readonly TrxServiceState[] _stateHistory = new[]
                                                               {
                                                                   TrxServiceState.Created,
                                                                   TrxServiceState.Created,
                                                                   TrxServiceState.Created,
                                                                   TrxServiceState.Created
                                                               };

        private readonly DateTime[] _stateHistoryDate = new[]
                                                            {
                                                                DateTime.MinValue,
                                                                DateTime.MinValue,
                                                                DateTime.MinValue,
                                                                DateTime.MinValue
                                                            };

        private string _inputContext = DefaultInputContext;

        private ILogger _logger;
        private string _outputContext = DefaultOutputContext;
        private TrxServiceState _state = TrxServiceState.Created;
        private DateTime _stateDate = DateTime.Now;
        private TrxServer _trxServer;

        /// <summary>
        /// Initialized on service init if <see ref="TrxServerTupleSpaceName"/> is not null.
        /// </summary>
        private ITrxServerTupleSpace _trxServerTupleSpace;

        /// <summary>
        /// Input context is where the messages to be consumed are read.
        /// </summary>
        public string InputContext
        {
            get { return _inputContext; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ConfigurationException("Invalid null or empty input context name.");

                _inputContext = value;
            }
        }

        /// <summary>
        /// Output context is where the messages to send to another service are written.
        /// </summary>
        public string OutputContext
        {
            get { return _outputContext; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ConfigurationException("Invalid null or empty output context name.");

                _outputContext = value;
            }
        }

        /// <summary>
        /// Trx Server tuple space to take and write messages.
        /// </summary>
        public string TrxServerTupleSpaceName { get; set; }

        public ITrxServerTupleSpace TrxServerTupleSpace
        {
            get { return _trxServerTupleSpace; }
        }

        #region ITrxService Members
        public string LoggerName { get; set; }

        public ILogger Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    LoggerName ?? MethodBase.GetCurrentMethod().DeclaringType.ToString()));
            }
            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        LoggerName ?? MethodBase.GetCurrentMethod().DeclaringType.ToString());
                else
                    _logger = value;
            }
        }

        public string Name { get; set; }

        public int Id { get; set; }

        public TrxServiceState State
        {
            get { return _state; }
        }

        public TrxServer TrxServer
        {
            get { return _trxServer; }
        }

        public void Init(TrxServer trxServer)
        {
            if (!_state.Equals(TrxServiceState.Created))
                UnexpectedState("initializing");

            SetState(TrxServiceState.Initializing);

            _trxServer = trxServer;

            if (_trxServer != null && !string.IsNullOrEmpty(TrxServerTupleSpaceName))
            {
                _trxServerTupleSpace = _trxServer.GetTupleSpaceByName(TrxServerTupleSpaceName);
                if (_trxServerTupleSpace == null)
                    Logger.Warn(string.Format("'{0}' service configured to use Trx Server tuple space: {1}, but it was not found.", Name,
                        TrxServerTupleSpaceName));
                else
                    Logger.Info(string.Format("'{0}' service configured to use Trx Server tuple space: {1}.", Name,
                        TrxServerTupleSpaceName));
            }

            try
            {
                ProtectedInit();
            }
            catch
            {
                SetState(TrxServiceState.Failed);
                throw;
            }

            if (!_state.Equals(TrxServiceState.Initializing))
                UnexpectedState("initialized");

            SetState(TrxServiceState.Initialized);
        }

        public void Start()
        {
            if (_state.Equals(TrxServiceState.Starting) || _state.Equals(TrxServiceState.Started))
                return;

            lock (_lockObj)
            {
                if (_state.Equals(TrxServiceState.Created))
                    Init(TrxServer);
                else if (!_state.Equals(TrxServiceState.Initialized) && !_state.Equals(TrxServiceState.Stopped))
                    UnexpectedState("starting");

                SetState(TrxServiceState.Starting);

                try
                {
                    ProtectedStart();
                }
                catch
                {
                    SetState(TrxServiceState.Failed);
                    throw;
                }

                if (!_state.Equals(TrxServiceState.Starting))
                    UnexpectedState("started");

                SetState(TrxServiceState.Started);
            }
        }

        public void Stop()
        {
            if (_state.Equals(TrxServiceState.Stopping) || _state.Equals(TrxServiceState.Stopped))
                return;

            lock (_lockObj)
            {
                if (!_state.Equals(TrxServiceState.Started) && !_state.Equals(TrxServiceState.Failed))
                    UnexpectedState("stopping");

                SetState(TrxServiceState.Stopping);

                try
                {
                    ProtectedStop();
                }
                catch
                {
                    SetState(TrxServiceState.Failed);
                    throw;
                }

                if (!_state.Equals(TrxServiceState.Stopping))
                    UnexpectedState("started");

                SetState(TrxServiceState.Stopped);
            }
        }

        public void Dispose()
        {
            if (_state.Equals(TrxServiceState.Destroying) || _state.Equals(TrxServiceState.Destroyed))
                return;

            if (!_state.Equals(TrxServiceState.Created) && !_state.Equals(TrxServiceState.Stopped) &&
                !_state.Equals(TrxServiceState.Failed))
                UnexpectedState("disposing");

            SetState(TrxServiceState.Destroying);

            try
            {
                ProtectedDispose();
            }
            catch
            {
                SetState(TrxServiceState.Failed);
                throw;
            }

            SetState(TrxServiceState.Destroyed);
        }
        #endregion

        public void SetLoggerByName(String loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        public string GetCurrentStateAndDate()
        {
            return string.Format("{0} on {1} (server time)", _state, _stateDate);
        }

        public string GetCurrentStateTimeSpan()
        {
            TimeSpan ts = DateTime.Now - _stateDate;
            return string.Format("{0} days, {1} hours, {2} min, {3} sec", ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        }

        public String GetPreviousStates()
        {
            if (_stateHistoryDate[0] == DateTime.MinValue)
                return string.Empty; // No history yet

            var sb = new StringBuilder();

            for (int i = 0; i < _stateHistory.Length && _stateHistoryDate[i] != DateTime.MinValue; i++)
            {
                if (i > 0)
                    sb.Append(" <- ");
                sb.Append(_stateHistory[i]);
                sb.Append(" on ");
                sb.Append(_stateHistoryDate[i]);
            }

            return sb.ToString();
        }

        protected void FireEvent(String eventName)
        {
        }

        protected void SetState(TrxServiceState state)
        {
            lock (_lockObj)
            {
                for (int i = _stateHistory.Length - 1; i > 0; i--)
                {
                    _stateHistory[i] = _stateHistory[i - 1];
                    _stateHistoryDate[i] = _stateHistoryDate[i - 1];
                }
                _stateHistory[0] = _state;
                _stateHistoryDate[0] = _stateDate;
                _state = state;
                _stateDate = DateTime.Now;
                string eventToFire = state.EventToFire;
                if (eventToFire != null)
                    FireEvent(eventToFire);
            }
        }

        private void UnexpectedState(string context)
        {
            throw new TrxServiceException(string.Format("Unexpected state {0} in context '{1}'", _state, context));
        }

        /// <summary>
        /// Gives the opportunity to derived classes to customize the initialization stage.
        /// </summary>
        protected virtual void ProtectedInit()
        {
        }

        /// <summary>
        /// Gives the opportunity to derived classes to customize the start stage.
        /// </summary>
        protected virtual void ProtectedStart()
        {
        }

        /// <summary>
        /// Gives the opportunity to derived classes to customize the stop stage.
        /// </summary>
        protected virtual void ProtectedStop()
        {
        }

        /// <summary>
        /// Gives the opportunity to derived classes to customize the dispose stage.
        /// </summary>
        protected virtual void ProtectedDispose()
        {
        }
    }
}