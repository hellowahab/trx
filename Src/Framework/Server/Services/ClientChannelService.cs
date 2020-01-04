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
using Trx.Communication.Channels;
using Trx.Exceptions;

namespace Trx.Server.Services
{
    /// <summary>
    /// Encapsulates an <see cref="IClientChannel"/> as a Trx Server service.
    /// 
    /// If a Trx Server tuple space is configured, read the outgoing messages and send through the
    /// underling channel without checking the destination address (if given).
    /// 
    /// Received messages are written in <see ref="InputContext"/>. Messages read from <see ref="OutputContext"/>
    /// are sent through the channel to the remote peer.
    /// </summary>
    public class ClientChannelService : ChannelService
    {
        private const int SleepInterval = 50;
        
        private readonly IClientChannel _channel;
        private readonly BaseSenderReceiverChannel _senderChannel;

        private int _sendConnectTimeout = 5000;
        private ChannelServiceServingPolicy _sendConnectTimeoutPolicy = ChannelServiceServingPolicy.Wait;

        protected bool KeepRunning;
        private Thread _readingThread;

        public ClientChannelService(IClientChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");

            _channel = channel;
            _senderChannel = _channel as BaseSenderReceiverChannel;
            if (_senderChannel == null)
                throw new ConfigurationException("Client channel service only supports channels of type " +
                    "Trx.Communication.Channels.BaseSenderReceiverChannel");
        }

        public IClientChannel Channel
        {
            get { return _channel; }
        }

        /// <summary>
        /// In milliseconds, time to wait the underling channel in case it's not connected and
        /// a message was retrieved from the Trx Server tuple space. If time is
        /// reached, the <see ref="SendConnectTimeoutPolicy"/> policy will be applied.
        /// </summary>
        public int SendConnectTimeout
        {
            get { return _sendConnectTimeout; }
            set { _sendConnectTimeout = value; }
        }

        public ChannelServiceServingPolicy SendConnectTimeoutPolicy
        {
            get { return _sendConnectTimeoutPolicy; }
            set { _sendConnectTimeoutPolicy = value; }
        }

        protected override void ProtectedStart()
        {
            base.ProtectedStart();

            if ( _senderChannel.TupleSpace == null && TrxServerTupleSpace != null)
                // If no tuple space is configured and we have a Trx Server tuple space
                // set it in the channel to receive messages.
                _senderChannel.TupleSpace = this;

            _channel.Connect();

            if (TrxServerTupleSpace != null)
            {
                _readingThread = new Thread(StartReadingTupleSpace);
                _readingThread.Start();
            }
        }

        protected override void ProtectedStop()
        {
            base.ProtectedStop();

            if (_readingThread != null)
            {
                KeepRunning = false;
                _readingThread.Abort();
                _readingThread = null;
            }

            _channel.Disconnect();
        }

        protected override void ProtectedDispose()
        {
            base.ProtectedDispose();
            _channel.Close();
        }

        private int WaitUntilIsConnected(int timeout)
        {
            var start = DateTime.UtcNow;
            while (timeout > 0 && KeepRunning && !_channel.IsConnected)
            {
                Thread.Sleep(SleepInterval);
                timeout -= SleepInterval;
            }

            return ((int) (DateTime.UtcNow - start).TotalMilliseconds);
        }

        private void WaitUntilIsConnected()
        {
            while (KeepRunning && !_channel.IsConnected)
                Thread.Sleep(SleepInterval);
        }

        private void ReturnMessage(TrxServiceMessage message, int ttl)
        {
            TrxServerTupleSpace.Return(message, ttl, OutputContext);

            object msg = message.Message;
            if (msg is MessageToAddress)
                msg = ((MessageToAddress)msg).Message;
            Logger.Info(string.Format("{0}: message returned because channel is not connected: {1}.", Name, msg));
        }

        /// <summary>
        /// Apply a policy when a message was taken from the tuple space but the underling channel
        /// is not connected.
        /// </summary>
        /// <param name="message">
        /// Message to send.
        /// </param>
        /// <param name="timeSinceWrite">
        /// Time in milliseconds since the message was written in the tuple space.
        /// </param>
        /// <param name="ttl">
        /// Tuple space time to live.
        /// </param>
        private void ApplySendConnectTimeoutPolicy(TrxServiceMessage message, int timeSinceWrite, int ttl)
        {
            switch (SendConnectTimeoutPolicy)
            {
                case ChannelServiceServingPolicy.Discard:
                    object msg = message.Message;
                    if (msg is MessageToAddress)
                        msg = ((MessageToAddress)msg).Message;
                    Logger.Info(string.Format("{0}: discarding message channel not connected: {1}.", Name, msg));
                    break;

                case ChannelServiceServingPolicy.Wait:
                case ChannelServiceServingPolicy.Return:
                    if (ttl == Timeout.Infinite)
                        ReturnMessage(message, Timeout.Infinite);
                    else
                    {
                        ttl = ttl - timeSinceWrite;
                        if (ttl > 0)
                            ReturnMessage(message, ttl);
                    }
                    break;
            }
        }

        private void SendMessage(TrxServiceMessage message, int timeInTupleSpace, int ttl)
        {
            if (_senderChannel == null)
                throw new InvalidOperationException("Cannot send messages to a channel wich is not an ISenderChannel.");

            if (!_channel.IsConnected)
            {
                int elapsed = 0;
                if (SendConnectTimeoutPolicy == ChannelServiceServingPolicy.Wait)
                    elapsed = WaitUntilIsConnected(SendConnectTimeout);
                if (!_channel.IsConnected)
                {
                    ApplySendConnectTimeoutPolicy(message, timeInTupleSpace + elapsed, ttl);
                    return;
                }
            }

            object msg = message.Message;
            if (msg is MessageToAddress)
                // No address checking, assume all messages from the tuple
                // space must be sent to the channel.
                msg = ((MessageToAddress)msg).Message;

            if (msg is MessageRequest)
            {
                var request = (MessageRequest) msg;
                _senderChannel.SendExpectingResponse(request.Message, request.Timeout, true, request.Key);
            }
            else
                _senderChannel.Send(msg);
        }

        private void StartReadingTupleSpace()
        {
            KeepRunning = true;
            WaitUntilIsConnected();
            while (KeepRunning)
            {
                try
                {
                    int timeInTupleSpace, ttl;
                    var message = TrxServerTupleSpace.Take(null, 1000,
                        out timeInTupleSpace, out ttl, OutputContext);
                    if (message == null)
                    {
                        if (!_channel.IsConnected)
                            WaitUntilIsConnected();
                    }
                    else
                        SendMessage(message, timeInTupleSpace, ttl);
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
    }
}
