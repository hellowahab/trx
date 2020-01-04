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
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Local
{
    public abstract class LocalBaseSenderReceiverChannel : BaseSenderReceiverChannel
    {
        private bool _isConnected;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        protected LocalBaseSenderReceiverChannel(Pipeline pipeline)
            : base(pipeline)
        {
        }

        /// <summary>
        /// Buils a channel to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        protected LocalBaseSenderReceiverChannel(Pipeline pipeline,
            ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
        }

        /// <summary>
        /// Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier to compute keys to match requests with responses.
        /// </param>
        protected LocalBaseSenderReceiverChannel(Pipeline pipeline,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        /// Tells if the channel is connected.
        /// </summary>
        public override sealed bool IsConnected
        {
            get { return _isConnected; }
            protected set { _isConnected = value; }
        }

        /// <summary>
        /// Remote peer address.
        /// </summary>
        public string Address { get; internal set; }

        /// <summary>
        /// Get or sets the peer channel.
        /// </summary>
        internal LocalBaseSenderReceiverChannel Peer { get; set; }

        /// <summary>
        /// Called from the peer.
        /// </summary>
        /// <param name="message">
        /// The received message.
        /// </param>
        internal void ReceiveCallFromPeer(object message)
        {
            PipelineContext.ReceivedMessage = message;
            Pipeline.Receive(PipelineContext);
            BaseReceive(PipelineContext.ReceivedMessage);
        }

        private void SendToPeer(object message)
        {
            Peer.ReceiveCallFromPeer(message);
        }

        protected virtual void OnDisconnection()
        {
            if (!IsConnected)
                return;

            PipelineContext.Reset();
            IsConnected = false;
            Peer = null;
            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Disconnected), true, Logger);
        }

        /// <summary>
        /// Called from client to inform disconnection.
        /// </summary>
        internal void DisconnectCallFromPeer()
        {
            OnDisconnection();
        }

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        public override void Disconnect()
        {
            lock (SyncRoot)
            {
                Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.DisconnectionRequested),
                    true, Logger);

                if (!_isConnected)
                    return;

                Logger.Info(string.Format("{0}: disconnecting from {1}.", GetChannelTitle(), Address));

                Peer.DisconnectCallFromPeer();

                OnDisconnection();
            }
        }

        /// <summary>
        /// Close channel and release all the allocated resources. In most cases the channel cannot be used again.
        /// </summary>
        public override void Close()
        {
            Disconnect();
        }

        /// <summary>
        /// It sends the specified message asynchronously.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        public override ChannelRequestCtrl Send(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (SyncRoot)
            {
                if (!_isConnected)
                    return null;

                try
                {
                    PipelineContext.MessageToSend = message;
                    Pipeline.Send(PipelineContext);
                    if (PipelineContext.MessageToSend == null)
                    {
                        Logger.Info(string.Format("{0}: the message has been consumed by the pipeline",
                            GetChannelTitle()));
                        return new ChannelRequestCtrl(false)
                                   {
                                       Message = "The message has been consumed by the pipeline"
                                   };
                    }

                    SendToPeer(PipelineContext.MessageToSend);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }

                return new ChannelRequestCtrl(true);
            }
        }

        /// <summary>
        /// Send a request message expecting a response from remote peer.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds for a response.
        /// </param>
        /// <param name="sendToTupleSpace">
        /// If true, the request is sent to the channel tuple space on completion or time out.
        /// </param>
        /// <param name="key">
        /// Request key, can be null.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        /// <remarks>
        /// Completed or timed out requests are stored in the receive tuple space if <paramref name="sendToTupleSpace"/>
        /// was set to true. If false synchronous wait of the response from the calling thread is assumed via the 
        /// <see cref="Request.WaitResponse"/> method.
        /// </remarks>
        public override SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout, bool sendToTupleSpace,
            object key)
        {
            SendRequestParametersChecks(message, timeout);

            lock (SyncRoot)
            {
                if (!_isConnected)
                    return null;

                Request request;
                SendRequestHandlerCtrl ctrl;
                try
                {
                    PipelineContext.MessageToSend = message;
                    Pipeline.Send(PipelineContext);
                    Logger.Info(string.Format("{0} message to send: {1}", GetChannelTitle(), message));
                    if (PipelineContext.MessageToSend == null)
                    {
                        Logger.Info(string.Format("{0}: the message has been consumed by the pipeline",
                            GetChannelTitle()));
                        return new SendRequestHandlerCtrl(false, null)
                                   {
                                       Message = "The message has been consumed by the pipeline"
                                   };
                    }

                    if (!BaseSendRequest(message, timeout, sendToTupleSpace, out request, out ctrl))
                        return ctrl;

                    SendToPeer(PipelineContext.MessageToSend);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new SendRequestHandlerCtrl(false, null)
                               {
                                   Error = ex
                               };
                }

                ctrl.MarkAsCompleted(true);

                request.Key = key;
                request.StartTimer();

                return ctrl;
            }
        }
    }
}