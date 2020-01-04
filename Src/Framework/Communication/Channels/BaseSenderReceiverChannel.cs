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
using System.Collections.Generic;
using System.Threading;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels
{
    public abstract class BaseSenderReceiverChannel : BaseChannel, ISenderChannel, IReceiverChannel
    {
        private readonly object _lockObj = new object();

        private ITupleSpace<ReceiveDescriptor> _tupleSpace;
        private readonly IMessagesIdentifier _messagesIdentifier;
        private Dictionary<object, Request> _pendingRequests;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        protected BaseSenderReceiverChannel(Pipeline pipeline)
            : base(pipeline)
        {
            TupleSpaceTtl = Timeout.Infinite;
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
        protected BaseSenderReceiverChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline)
        {
            if (tupleSpace == null)
                throw new ArgumentNullException("tupleSpace");

            _tupleSpace = tupleSpace;
            TupleSpaceTtl = Timeout.Infinite;
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
        protected BaseSenderReceiverChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : this(pipeline, tupleSpace)
        {
            if (messagesIdentifier == null)
                throw new ArgumentNullException("messagesIdentifier");

            _messagesIdentifier = messagesIdentifier;
        }

        /// <summary>
        /// Space where the received messages (and completed/timedout requests).
        /// </summary>
        public ITupleSpace<ReceiveDescriptor> TupleSpace
        {
            get { return _tupleSpace; }
            internal set { _tupleSpace = value; }
        }

        /// <summary>
        /// Time in milliseconds used by the channel implementation when writing a received
        /// message in the receive tuple space. Default value is Timeout.Infinite.
        /// </summary>
        public int TupleSpaceTtl { get; set; }

        /// <summary>
        /// Object used to get keys from messages to match requests with responses.
        /// </summary>
        public IMessagesIdentifier MessagesIdentifier
        {
            get { return _messagesIdentifier; }
        }

        protected string GetChannelTitle()
        {
            return Name ?? "Channel";
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
        public abstract ChannelRequestCtrl Send(object message);

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
        public abstract SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout, bool sendToTupleSpace, object key);

        /// <summary>
        /// Send a request message expecting a response from remote peer.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds for a response.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        /// <remarks>
        /// Completed or timed out requests are stored in the receive tuple space.
        /// </remarks>
        public SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout)
        {
            return SendExpectingResponse( message, timeout, true, null );
        }

        protected void SendRequestParametersChecks(object message, int timeout)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (timeout < 1)
                throw new ArgumentOutOfRangeException("timeout", "Must be greater than zero.");

            if (_messagesIdentifier == null)
                throw new ChannelException(
                    "Channel isn't configured with a messages identifier, unable to match requests with responses.");
        }

        protected void CancelPendingRequests()
        {
            lock (_lockObj)
            {
                if (_pendingRequests == null)
                    return;

                foreach (Request request in _pendingRequests.Values)
                    request.Cancel(false);
                _pendingRequests.Clear();
                _pendingRequests = null;
            }
        }

        protected bool BaseSendRequest(object message, int timeout, bool sendToTupleSpace, out Request request, out SendRequestHandlerCtrl ctrl)
        {
            lock (_lockObj) {
                var requestMessageKey = PipelineContext.MessageToSendId ?? MessagesIdentifier.ComputeIdentifier( message );

                if (requestMessageKey == null)
                {
                    ctrl = new SendRequestHandlerCtrl(false, null)
                    {
                        Message = "The message key generated by the messages identifier is null, can't send."
                    };
                    request = null;
                    return false;
                }

                if (_pendingRequests == null)
                    _pendingRequests = new Dictionary<object, Request>();

                if (_pendingRequests.ContainsKey(requestMessageKey))
                {
                    ctrl = new SendRequestHandlerCtrl(false, null)
                    {
                        Message = "There's already a pending request with the same request message key."
                    };
                    request = null;
                    return false;
                }

                request = new Request(message, timeout, sendToTupleSpace, requestMessageKey, ChannelAddress);
                ctrl = new SendRequestHandlerCtrl(request);
                _pendingRequests.Add(requestMessageKey, request);
            }

            return true;
        }

        /// <summary>
        /// Called to notify a timeout or a cancellation.
        /// </summary>
        /// <param name="request">
        /// The timed out or cancelled request.
        /// </param>
        internal void TimedOutOrCancelledRequest(Request request)
        {
            lock (_lockObj)
                if ((_pendingRequests != null) && (_pendingRequests.ContainsKey(request.RequestMessageKey)))
                    if (_pendingRequests.Remove(request.RequestMessageKey) && request.SendToTupleSpace)
                        _tupleSpace.Write(request, TupleSpaceTtl);
        }

        /// <summary>
        /// Base receive processing.
        /// </summary>
        /// <param name="message">
        /// Received message.
        /// </param>
        protected void BaseReceive(object message)
        {
            if (message == null)
                // The message has been consumed by the pipeline.
                return;

            string dump;
            if (message is string)
                dump = message as string;
            else
                dump = message.ToString();
            Logger.Info(string.Format("{0} received message: {1}", GetChannelTitle(), dump));

            if (_messagesIdentifier != null)
                // Check if received message is response of a pending request.
                lock (_lockObj)
                    if ((_pendingRequests != null) && (_pendingRequests.Count > 0))
                    {
                        object messageKey = PipelineContext.ReceivedMessageId ?? _messagesIdentifier.ComputeIdentifier(message);
                        if (messageKey != null)
                        {
                            Request request = _pendingRequests.ContainsKey(messageKey)
                                ? _pendingRequests[messageKey]
                                : null;
                            if (request != null)
                            {
                                _pendingRequests.Remove(messageKey);
                                if (request.SetResponseMessage(message) && request.SendToTupleSpace)
                                    _tupleSpace.Write(request, TupleSpaceTtl);
                                return;
                            }
                        }
                    }

            if (_tupleSpace != null)
                _tupleSpace.Write(new ReceiveDescriptor(ChannelAddress, message), TupleSpaceTtl);
        }
    }
}