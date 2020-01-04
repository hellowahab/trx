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
    /// <summary>
    /// Represents a channel request expecting remote peer response.
    /// </summary>
    /// <remarks>
    /// Request messages are matched by the channel with help of an <see cref="IMessagesIdentifier"/>.
    /// </remarks>
    [Serializable]
    public sealed class Request : ReceiveDescriptor
    {
        [NonSerialized]
        private readonly object _lockObj = new object();
        private object _sentMessage;
        private readonly DateTime _utcRequestDateTime;

        [NonSerialized]
        private Timer _timer;

        #region Constructors
        internal Request(object sentMessage, int timeout, bool sendToTupleSpace, object requestMessageKey,
            IChannelAddress channelAddress)
            : base(channelAddress)
        {
            if (timeout < 1)
                throw new ArgumentOutOfRangeException("timeout", timeout, "Must be greater than zero.");

            _sentMessage = sentMessage;
            _utcRequestDateTime = DateTime.UtcNow;
            IsExpired = false;
            UtcExpirationDateTime = DateTime.MinValue;
            Timeout = timeout;
            SendToTupleSpace = sendToTupleSpace;
            RequestMessageKey = requestMessageKey;
            IsCancelled = false;
            UtcCancellationDateTime = DateTime.MinValue;
        }
        #endregion

        #region Class properties
        /// <summary>
        /// Request message (the one sent to the remote peer).
        /// </summary>
        public object SentMessage
        {
            get { return _sentMessage; }
            internal set { _sentMessage = value; }
        }

        /// <summary>
        /// UTC request date and time.
        /// </summary>
        public DateTime UtcRequestDateTime
        {
            get { return _utcRequestDateTime; }
        }

        /// <summary>
        /// The request message key computed by an <see cref="IMessagesIdentifier"/>.
        /// </summary>
        internal object RequestMessageKey { get; private set; }

        /// <summary>
        /// Returns request timeout in milliseconds.
        /// </summary>
        public int Timeout { get; private set; }

        /// <summary>
        /// If true the channel store the completed or timed out request in
        /// <see cref="IReceiverChannel.TupleSpace"/>.
        /// </summary>
        internal bool SendToTupleSpace { get; private set; }

        /// <summary>
        /// Returns true when the request has expired (response hasn't been received).
        /// </summary>
        public bool IsExpired { get; private set; }

        /// <summary>
        /// UTC expiration date and time.
        /// </summary>
        public DateTime UtcExpirationDateTime { get; private set; }

        /// <summary>
        /// Returns true when the request has been cancelled (by user request or server child channel
        /// disconnection).
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// UTC cancellation date and time.
        /// </summary>
        public DateTime UtcCancellationDateTime { get; private set; }

        /// <summary>
        /// Request key.
        /// </summary>
        public object Key { get; set; }
        #endregion

        #region Class methods
        internal void StartTimer()
        {
            if (IsExpired || IsCancelled || ReceivedMessage != null) // Panic check :)
                return;

            _timer = new Timer(OnTimerTick, null, Timeout, System.Threading.Timeout.Infinite);
        }

        private void DisposeTimer()
        {
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }

        internal bool Cancel(bool notifyChannel)
        {
            if (IsExpired || IsCancelled || ReceivedMessage != null)
                return IsCancelled;

            lock (_lockObj)
            {
                if (IsExpired || IsCancelled || ReceivedMessage != null)
                    return IsCancelled;

                DisposeTimer();

                IsCancelled = true;
                UtcCancellationDateTime = DateTime.UtcNow;
                var reference = ChannelAddress as ReferenceChannelAddress;
                if (notifyChannel && reference != null)
                {
                    var channel = reference.Channel as BaseSenderReceiverChannel;
                    if (channel != null)
                        channel.TimedOutOrCancelledRequest(this);
                }
                Monitor.PulseAll(_lockObj);
            }

            return true;
        }

        /// <summary>
        /// Cancel the request.
        /// </summary>
        /// <returns>
        /// True if the request has been cancelled, otherwise false.
        /// </returns>
        public bool Cancel()
        {
            return Cancel(!SendToTupleSpace);
        }

        private void OnTimerTick(object state)
        {
            if (IsCancelled || ReceivedMessage != null)
                return;

            lock (_lockObj)
            {
                if (IsCancelled || ReceivedMessage != null)
                    return;

                DisposeTimer();

                IsExpired = true;
                UtcExpirationDateTime = DateTime.UtcNow;

                var reference = ChannelAddress as ReferenceChannelAddress;
                if (reference != null)
                {
                    var channel = reference.Channel as BaseSenderReceiverChannel;
                    if (channel != null)
                        channel.TimedOutOrCancelledRequest(this);
                }

                Monitor.PulseAll(_lockObj);
            }
        }

        /// <summary>
        /// Sets the response to the request if it's not an expired request.
        /// </summary>
        /// <param name="message">
        /// It's the response message.
        /// </param>
        /// <returns>
        /// true if the response was set, false if the response wasn't set because
        /// the request is expired.
        /// </returns>
        internal bool SetResponseMessage(object message)
        {
            if (IsExpired || IsCancelled)
                return false;

            lock (_lockObj)
            {
                if (IsExpired || IsCancelled)
                    return false;

                DisposeTimer();

                ReceivedMessage = message;
                UtcReceiveDateTime = DateTime.UtcNow;
                Monitor.PulseAll(_lockObj);
            }

            return true;
        }

        /// <summary>
        /// Wait request response or timeout.
        /// </summary>
        /// <returns>
        /// True if we have a response, false otherwise.
        /// </returns>
        public bool WaitResponse()
        {
            if (IsExpired || IsCancelled || ReceivedMessage != null)
                return ReceivedMessage != null;

            lock (_lockObj)
            {
                if (IsExpired || IsCancelled || ReceivedMessage != null)
                    return ReceivedMessage != null;

                // Wait for response or timeout.
                Monitor.Wait(_lockObj, System.Threading.Timeout.Infinite);
            }

            return ReceivedMessage != null;
        }
        #endregion
    }
}