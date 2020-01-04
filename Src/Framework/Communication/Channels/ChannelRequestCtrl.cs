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
using System.Threading;

namespace Trx.Communication.Channels
{
    public class ChannelRequestCtrl
    {
        private readonly object _lockObj = new object();
        private readonly DateTime _utcRequestDateTime;
        private bool _isCompleted;
        private DateTime _utcCompletionDateTime;
        private bool _isCancelled;
        private DateTime _utcCancellationDateTime;
        private bool _successful;

        #region Constructors
        internal ChannelRequestCtrl()
        {
            _utcRequestDateTime = DateTime.UtcNow;
            _utcCompletionDateTime = DateTime.MinValue;
            _utcCancellationDateTime = DateTime.MinValue;
        }

        internal ChannelRequestCtrl(bool successful)
        {
            _utcRequestDateTime = DateTime.UtcNow;
            _successful = successful;
            _isCompleted = true;
            _utcCompletionDateTime = _utcRequestDateTime;
            _utcCancellationDateTime = DateTime.MinValue;
        }
        #endregion

        #region Properties
        /// <summary>
        /// UTC Date and time of the request.
        /// </summary>
        public DateTime UtcRequestDateTime
        {
            get
            {
                return _utcRequestDateTime;
            }
        }

        /// <summary>
        /// True if the request is completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        /// <summary>
        /// UTC Completion date and time of the request.
        /// </summary>
        /// <remarks>
        /// Only has a valid value if <see cref="IsCompleted"/> is true, otherwise it value is <see cref="DateTime.MinValue"/>
        /// </remarks>
        public DateTime UtcCompletionDateTime
        {
            get
            {
                return _utcCompletionDateTime;
            }
        }

        /// <summary>
        /// True if the request has been cancelled.
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                return _isCancelled;
            }
        }

        /// <summary>
        /// UTC Cancellation date and time of the request.
        /// </summary>
        /// <remarks>
        /// Only has a valid value if <see cref="IsCancelled"/> is true, otherwise it value is <see cref="DateTime.MinValue"/>
        /// </remarks>
        public DateTime UtcCancellationDateTime
        {
            get
            {
                return _utcCancellationDateTime;
            }
        }

        /// <summary>
        /// True if the request is succesfully completed.
        /// </summary>
        public bool Successful
        {
            get
            {
                return _successful;
            }
        }

        /// <summary>
        /// Optional message. Usually set when the request is not successful.
        /// </summary>
        public string Message
        {
            get;
            internal set;
        }

        /// <summary>
        /// Optional error. Usually set when the request is not successful.
        /// </summary>
        public Exception Error
        {
            get;
            internal set;
        }
        #endregion

        #region Methods
        internal void MarkAsCompleted(bool successful)
        {
            if (!_isCompleted && !_isCancelled)
                lock (_lockObj)
                {
                    if (_isCompleted || _isCancelled)
                        return;

                    _isCompleted = true;
                    _utcCompletionDateTime = DateTime.UtcNow;
                    _successful = successful;

                    Monitor.PulseAll(_lockObj);
                }
        }

        /// <summary>
        /// Wait until the request is completed.
        /// </summary>
        /// <param name="timeout">
        /// Milliseconds to wait before timeout the completion of the request. <see cref="Timeout.Infinite"/> to wait until completion.
        /// </param>
        /// <param name="cancelOnTimeout">
        /// If true is given, the request is automatically cancelled on timeout.
        /// </param>
        /// <returns>
        /// True if the request is succesfully completed.
        /// </returns>
        public bool WaitCompletion(int timeout, bool cancelOnTimeout)
        {
            if (_isCompleted || _isCancelled)
                return _isCompleted;

            lock (_lockObj)
            {
                if (_isCompleted || _isCancelled)
                    return _isCompleted;

                if (!Monitor.Wait(_lockObj, timeout) && cancelOnTimeout)
                    CancelImpl();

                return _isCompleted;
            }
        }

        /// <summary>
        /// Wait until the request is completed.
        /// </summary>
        /// <returns>
        /// True if the request is succesfully completed.
        /// </returns>
        public bool WaitCompletion()
        {
            return WaitCompletion(Timeout.Infinite, false);
        }

        private void CancelImpl()
        {
            if (_isCompleted || _isCancelled)
                return;

            _isCancelled = true;
            _utcCancellationDateTime = DateTime.UtcNow;

            Monitor.PulseAll(_lockObj);
        }

        /// <summary>
        /// Cancel the request if is not done.
        /// </summary>
        public void Cancel()
        {
            if (!_isCompleted && !_isCancelled)
                lock (_lockObj)
                    CancelImpl();
        }
        #endregion
    }
}
