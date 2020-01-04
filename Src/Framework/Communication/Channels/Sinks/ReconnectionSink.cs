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

namespace Trx.Communication.Channels.Sinks
{
    /// <summary>
    /// Tryes to reconnect a client channel if the connection is lost.
    /// </summary>
    /// <remarks>
    /// This sink is state aware, cannot be used in two pipelines.
    /// </remarks>
    public class ReconnectionSink : ISink
    {
        public const int DefaultReconnectInterval = 60000; // Default value: one minute.
        public const int DefaultInactivityInterval = 300000; // Default value: five minutes.
        private int _connectionDurationThreshold = 2;

        private int _currentReconnectInterval;
        private bool _disconnectionFromTimer;
        private int _inactivityInterval = DefaultInactivityInterval;
        private DateTime _lastConnectionDateTime = DateTime.MinValue;
        private bool _reconnectEnabled;
        private int _reconnectInterval = DefaultReconnectInterval;
        private Timer _timer;
        private bool _timerRescheduled;


        public ReconnectionSink()
        {
            Reconnect = true;
            DisconnectOnReceptionInactivity = true;
        }

        /// <summary>
        /// If true, the sink call <see ref="IChannel.Connect"/> if the channel isn't connected.
        /// and a previous call to <see ref="IChannel.Connect"/> was made by the user.
        /// </summary>
        /// <remarks>
        /// The sink stop reconnection attempts if the user calls <see ref="IChannel.Disconnect"/>.
        /// </remarks>
        public bool Reconnect { get; set; }

        /// <summary>
        /// It returns or sets the interval in milliseconds the sink waits before trying a reconnection.
        /// </summary>
        public int ReconnectInterval
        {
            get { return _reconnectInterval; }
            set { _reconnectInterval = value < 0 ? 0 : value; }
        }

        /// <summary>
        /// Minimum time in seconds a connection is considered a good one, less than this
        /// value will trigger a sleep before next reconnection attempt.
        /// 
        /// Default value is two seconds.
        /// </summary>
        public int ConnectionDurationThreshold
        {
            get { return _connectionDurationThreshold; }
            set { _connectionDurationThreshold = value; }
        }

        /// <summary>
        /// If true, the sink disconnects the channel if no data was received in the time
        /// specified in <see cref="InactivityInterval"/>.
        /// </summary>
        public bool DisconnectOnReceptionInactivity { get; set; }

        /// <summary>
        /// It returns or sets the interval in milliseconds the sink waits to shutdown the
        /// connection with the remote host if data hasn't been received in this period.
        /// </summary>
        public int InactivityInterval
        {
            get { return _inactivityInterval; }
            set { _inactivityInterval = value <= 0 ? Timeout.Infinite : value; }
        }

        #region ISink Members
        /// <summary>
        /// Called when a significant event (other than send or receive) was caught in the channel.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <param name="channelEvent">
        /// The event.
        /// </param>
        /// <returns>
        /// True if the event can be informed to the next sink in the pipeline, otherwise false.
        /// </returns>
        public bool OnEvent(PipelineContext context, ChannelEvent channelEvent)
        {
            switch (channelEvent.EventType)
            {
                case ChannelEventType.ConnectionRequested:
                    _reconnectEnabled = Reconnect && context.Channel is IClientChannel;
                    break;

                case ChannelEventType.DisconnectionRequested:
                    _reconnectEnabled = _reconnectEnabled && _disconnectionFromTimer;
                    if (_timer != null)
                        SetTimer(context.Channel, Timeout.Infinite, Timeout.Infinite);
                    break;

                case ChannelEventType.Connected:
                    _lastConnectionDateTime = DateTime.UtcNow;
                    _currentReconnectInterval = 0;
                    if (DisconnectOnReceptionInactivity)
                        SetTimer(context.Channel, InactivityInterval, Timeout.Infinite);
                    break;

                case ChannelEventType.ConnectionFailed:
                case ChannelEventType.Disconnected:
                    if (_reconnectEnabled)
                    {
                        if ((DateTime.UtcNow - _lastConnectionDateTime).TotalSeconds < _connectionDurationThreshold)
                            // If the connection duration was less than X seconds, recompute reconnection interval
                            // to prevent reconnection starvation (a sequence of infinite connect - disconnect,
                            // very dangerous because can consume server resources)
                            ComputeNextReconnectionInterval();
                        if (_currentReconnectInterval > 0)
                            context.Channel.Logger.Info(string.Format("{0}: next reconnection attempt in {1} second/s.",
                                context.Channel.Name ?? "Channel", _currentReconnectInterval/1000));
                        SetTimer(context.Channel, _currentReconnectInterval, Timeout.Infinite);
                    }
                    else if (DisconnectOnReceptionInactivity)
                        SetTimer(context.Channel, Timeout.Infinite, Timeout.Infinite);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Process the message to be sent.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <remarks>
        /// The message to be sent is stored in the <see cref="PipelineContext.MessageToSend"/>. If
        /// null is set by the sink the message is consumed and it stop going through the pipeline,
        /// whereby the channel doesn't send it.
        /// </remarks>
        public void Send(PipelineContext context)
        {
            // Neutral to send operations
        }

        /// <summary>
        /// Process the received message.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <returns>
        /// True if the pipeline can continue with the next sink, otherwise false.
        /// </returns>
        /// <remarks>
        /// The received message (if the sink creates one) is be stored in the
        /// <see cref="PipelineContext.ReceivedMessage"/> property.  If null is set by the sink the
        /// message is consumed and it stop going through the pipeline, whereby the channel doesn't 
        /// put in the receive tuple space.
        /// </remarks>
        public bool Receive(PipelineContext context)
        {
            if (DisconnectOnReceptionInactivity)
                SetTimer(context.Channel, InactivityInterval, Timeout.Infinite);

            return true;
        }

        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>
        /// A copy of the instance.
        /// </returns>
        public object Clone()
        {
            return new ReconnectionSink
                       {
                           Reconnect = Reconnect,
                           ReconnectInterval = ReconnectInterval,
                           DisconnectOnReceptionInactivity = DisconnectOnReceptionInactivity,
                           InactivityInterval = InactivityInterval
                       };
        }
        #endregion

        /// <summary>
        /// Compute next reconnection attempt interval.
        /// </summary>
        private void ComputeNextReconnectionInterval()
        {
            // Compute next connection request interval.
            if (_currentReconnectInterval == 0)
                _currentReconnectInterval = 1000;
            else
            {
                _currentReconnectInterval *= 2;
                if (_currentReconnectInterval > _reconnectInterval)
                    _currentReconnectInterval = _reconnectInterval;
            }
        }

        private void SetTimer(IChannel channel, int dueTime, int period)
        {
            if (_timer == null)
                _timer = new Timer(OnTimerTick, channel, dueTime, period);
            else
                _timer.Change(dueTime, period);

            _timerRescheduled = true;
        }

        private void OnTimerTick(object state)
        {
            var channel = state as IChannel;
            if (channel == null)
                return;

            _timerRescheduled = false; // Just in case a reschedule is made while waiting for the lock.
            lock (channel.SyncRoot)
            {
                if (_timerRescheduled)
                    return;

                var clientChannel = channel as IClientChannel;
                if (channel.IsConnected)
                {
                    if (DisconnectOnReceptionInactivity)
                    {
                        channel.Logger.Info(string.Format("{0}: disconnecting due reception inactivity.",
                            channel.Name ?? "Channel"));
                        _disconnectionFromTimer = true;
                        channel.Disconnect();
                        _disconnectionFromTimer = false;
                    }
                }
                else if (clientChannel != null && _reconnectEnabled)
                {
                    ComputeNextReconnectionInterval();
                    clientChannel.Connect();
                }
            }
        }
    }
}