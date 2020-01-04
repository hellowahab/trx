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
using System.IO;
using System.Net;
using System.Net.Sockets;
using Trx.Buffer;
using Trx.Coordination.TupleSpace;
using Trx.Utilities;

namespace Trx.Communication.Channels.Tcp
{
    public abstract class TcpBaseSenderReceiverChannel : BaseSenderReceiverChannel, IDisposable
    {
        public const int DefaultSendMaxRequestSize = 4096;

        private readonly IBuffer _inputBuffer;
        private bool _disposed;
        private bool _isConnected;
        private int _sendMaxRequestSize = DefaultSendMaxRequestSize;
        private Queue<AsyncSendInfo> _sendQueue;
        private bool _sending;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        protected TcpBaseSenderReceiverChannel(Pipeline pipeline)
            : base(pipeline)
        {
            _inputBuffer = pipeline.BufferFactory == null
                ? new SingleChunkBuffer()
                : pipeline.BufferFactory.GetInstance();
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
        protected TcpBaseSenderReceiverChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
            _inputBuffer = pipeline.BufferFactory == null
                ? new SingleChunkBuffer()
                : pipeline.BufferFactory.GetInstance();
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
        protected TcpBaseSenderReceiverChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
            _inputBuffer = pipeline.BufferFactory == null
                ? new SingleChunkBuffer()
                : pipeline.BufferFactory.GetInstance();
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
        /// Internal socket handle.
        /// </summary>
        internal Socket Socket { get; set; }

        /// <summary>
        /// Internal use only from unit tests to control channel behavior.
        /// </summary>
        internal bool Sending
        {
            set { _sending = value; }
        }

        /// <summary>
        /// Internal use only from unit tests to check channel behavior.
        /// </summary>
        internal IBuffer InputBuffer
        {
            get { return _inputBuffer; }
        }

        /// <summary>
        /// Send maximum request size, data greater than this is fragmented internally in batchs of subsequents sends.
        /// </summary>
        public int SendMaxRequestSize
        {
            get { return _sendMaxRequestSize; }
            set { _sendMaxRequestSize = value; }
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose current channel.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _inputBuffer.Dispose();

            _disposed = true;
        }
        #endregion

        protected virtual void OnDisconnection()
        {
            if (!IsConnected)
                return;

            IsConnected = false;
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            finally
            {
                Socket = null;
            }
            _sending = false;
            if (!_inputBuffer.IsDisposed)
                _inputBuffer.Clear();
            PipelineContext.Reset();
            Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Disconnected), true, Logger);
            if (_sendQueue != null)
            {
                foreach (AsyncSendInfo sendInfo in _sendQueue)
                {
                    sendInfo.Ctrl.Message = "Channel was disconnected";
                    sendInfo.Ctrl.MarkAsCompleted(false);
                }
                _sendQueue.Clear();
            }
        }

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        public override void Disconnect()
        {
            lock (SyncRoot)
            {
                // Fire event even if we are not connected to inform sinks about an explicit disconnection request.
                Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.DisconnectionRequested),
                    true, Logger);

                if (!_isConnected)
                    return;

                Logger.Info(string.Format("{0}: disconnecting from {1}, local end point {2}.", GetChannelTitle(),
                    Socket.RemoteEndPoint, Socket.LocalEndPoint));

                OnDisconnection();
            }
        }

        protected void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Channel is disposed");
        }

        /// <summary>
        /// Close channel and release all the allocated resources. In most cases the channel cannot be used again.
        /// </summary>
        public override void Close()
        {
            if (_disposed)
                return;

            Disconnect();
            Dispose();
        }

        private static IBuffer CastMessageToBuffer(object message)
        {
            var buffer = message as IBuffer;
            if (buffer == null)
                throw new NotSupportedException(
                    "This channel implementation only support to send messages of type IBuffer.");

            return buffer;
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

            CheckDisposed();

            lock (SyncRoot)
            {
                if (!_isConnected)
                    return new ChannelRequestCtrl(false)
                               {
                                   Message = "The channel is not connected"
                               };

                ChannelRequestCtrl ctrl;
                try
                {
                    PipelineContext.MessageToSend = message;
                    Pipeline.Send(PipelineContext);
                    Logger.Info(string.Format("{0} message to send: {1}", GetChannelTitle(), message));
                    if (PipelineContext.MessageToSend == null)
                    {
                        Logger.Info(string.Format("{0}: the message has been consumed by the pipeline",
                            GetChannelTitle()));
                        return new ChannelRequestCtrl(false)
                                   {
                                       Message = "The message has been consumed by the pipeline"
                                   };
                    }

                    IBuffer buffer = CastMessageToBuffer(PipelineContext.MessageToSend);

                    ctrl = new ChannelRequestCtrl();
                    StartAsyncSend(new AsyncSendInfo(Socket, buffer, ctrl));
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }

                return ctrl;
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
            CheckDisposed();

            SendRequestParametersChecks(message, timeout);

            lock (SyncRoot)
            {
                if (!_isConnected)
                    return new SendRequestHandlerCtrl(false, null)
                               {
                                   Message = "The channel is not connected"
                               };

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

                    IBuffer buffer = CastMessageToBuffer(PipelineContext.MessageToSend);

                    if (!BaseSendRequest(message, timeout, sendToTupleSpace, out request, out ctrl))
                        return ctrl;

                    StartAsyncSend(new AsyncSendInfo(Socket, buffer, ctrl));
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new SendRequestHandlerCtrl(false, null)
                               {
                                   Error = ex
                               };
                }

                request.Key = key;
                request.StartTimer();

                return ctrl;
            }
        }

        internal static IPAddress ResolveHostEntry(string channelTitle, string hostNameOrAddress,
            AddressFamily addressFamily)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);

            // Get the first address of the family we want to use.
            foreach (IPAddress address in hostEntry.AddressList)
                if (address.AddressFamily == addressFamily)
                    return address;

            throw new ChannelException(string.Format("{0}: can't resolve remote interface name {1}, family {2}.",
                channelTitle, hostNameOrAddress, addressFamily));
        }

        protected virtual int EndReceiveOrRead(IAsyncResult asyncResult)
        {
            return Socket.EndReceive(asyncResult);
        }

        protected void AsyncReceiveOrReadRequestHandler(IAsyncResult asyncResult)
        {
            lock (SyncRoot)
            {
                var socket = asyncResult.AsyncState as Socket;
                if (socket == null || !ReferenceEquals(socket, Socket))
                    // Someone called Close over socket.
                    return;

                try
                {
                    int receivedBytes = EndReceiveOrRead(asyncResult);
                    if (receivedBytes == 0)
                    {
                        Logger.Info(
                            string.Format(
                                "{0}: the connection was lost, the remote peer {1} has terminated the communication, local end point {2}.",
                                GetChannelTitle(), Socket.RemoteEndPoint, Socket.LocalEndPoint));
                        OnDisconnection();
                        return;
                    }

                    _inputBuffer.UpperDataBound += receivedBytes;

                    if (PipelineContext.ExpectedBytes > 0 && _inputBuffer.DataLength < PipelineContext.ExpectedBytes)
                    {
                        if (Logger.IsDebugEnabled())
                            Logger.Debug(
                                string.Format(
                                    "{0}: received {1} byte/s, more data is needed got {2} byte/s, need {3} byte/s.",
                                    GetChannelTitle(), receivedBytes, _inputBuffer.DataLength,
                                    PipelineContext.ExpectedBytes));
                        StartAsyncReceive();
                        return;
                    }

                    bool loop = true;
                    while (loop)
                    {
                        int lowerDataBound = _inputBuffer.LowerDataBound;
                        int dataLength = _inputBuffer.DataLength;
                        PipelineContext.ReceivedMessage = _inputBuffer;
                        try
                        {
                            if (!Pipeline.Receive(PipelineContext))
                            {
                                if (PipelineContext.ExpectedBytes < 0)
                                    throw new ChannelException(
                                        string.Format("Invalid expected data length set by the pipeline: {0}.",
                                            PipelineContext.ExpectedBytes));
                                break;
                            }
                        }
                        finally
                        {
                            if (dataLength != _inputBuffer.DataLength)
                            {
                                if (Logger.IsDebugEnabled())
                                {
                                    int consumed = _inputBuffer.LowerDataBound - lowerDataBound;
                                    if (consumed <= 0)
                                        consumed = dataLength;
                                    Logger.Debug(StringUtilities.DumpBufferData(string.Format(
                                        "{0} raw received data consumed in the pipeline {1} byte/s:",
                                        GetChannelTitle(), consumed), _inputBuffer, lowerDataBound, consumed));
                                }
                                _inputBuffer.ForgetSecureAreas();
                            }
                        }

                        if (ReferenceEquals(PipelineContext.ReceivedMessage, _inputBuffer))
                            // No pipeline transformation, we get the raw data
                            PipelineContext.ReceivedMessage = _inputBuffer.ReadAsString(true,
                                PipelineContext.ExpectedBytes > 0
                                    ? PipelineContext.ExpectedBytes
                                    : _inputBuffer.DataLength);

                        if (PipelineContext.ReceivedMessage != null)
                        {
                            BaseReceive(PipelineContext.ReceivedMessage);

                            // Reset length
                            PipelineContext.ExpectedBytes = 0;
                        }

                        if (_inputBuffer.DataLength == 0)
                            loop = false;
                    }

                    StartAsyncReceive();
                }
                catch (Exception ex)
                {
                    if (ex is SocketException)
                        LogSocketException((SocketException) ex);
                    else if (ex is IOException && ex.InnerException != null && ex.InnerException is SocketException)
                        LogSocketException((SocketException) ex.InnerException);
                    else
                        Logger.Error(string.Format("{0}: exception caught handling asynchronous receive/read.",
                            GetChannelTitle()), ex);
                    OnDisconnection();
                }
            }
        }

        protected virtual void OnStartAsyncReceive()
        {
            if (_inputBuffer.MultiChunk)
                Socket.BeginReceive(_inputBuffer.GetFreeSegments(), SocketFlags.None, AsyncReceiveOrReadRequestHandler,
                    Socket);
            else
                Socket.BeginReceive(_inputBuffer.GetArray(), _inputBuffer.UpperDataBound,
                    _inputBuffer.FreeSpaceAtTheEnd, SocketFlags.None, AsyncReceiveOrReadRequestHandler, Socket);
        }

        protected void StartAsyncReceive()
        {
            int free = _inputBuffer.FreeSpaceAtTheEnd;
            int need = PipelineContext.ExpectedBytes - _inputBuffer.DataLength;
            if (free == 0 || need > free)
                if (need > 0)
                    if (_inputBuffer.FreeSpaceAtTheBeginning + free >= need)
                        // Combined free space in the beginning and the end is enough to cover our needs,
                        // so we compact the buffer.
                        _inputBuffer.Compact();
                    else
                        _inputBuffer.Capacity += need;
                else
                    _inputBuffer.Expand();

            OnStartAsyncReceive();
        }

        private void DisposeBuffer(IBuffer buffer)
        {
            try
            {
                buffer.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format(
                    "{0}: critical exception caught disposing a buffer in asynchronous send.",
                    GetChannelTitle()), ex);
            }
        }

        protected virtual int EndAsyncSendOrWrite(IAsyncResult asyncResult, AsyncSendInfo sendInfo)
        {
            return Socket.EndSend(asyncResult);
        }

        protected void AsyncSendOrWriteRequestHandler(IAsyncResult asyncResult)
        {
            var sendInfo = asyncResult.AsyncState as AsyncSendInfo;
            if (sendInfo == null || !ReferenceEquals(sendInfo.Socket, Socket))
                // Someone called Close over socket.
                return;

            lock (SyncRoot)
            {
                try
                {
                    int sentBytes = EndAsyncSendOrWrite(asyncResult, sendInfo);

                    _sending = false;

                    if (sentBytes < sendInfo.Buffer.DataLength)
                    {
                        // Keep sending the rest of the data
                        sendInfo.Buffer.Discard(sentBytes);
                        StartAsyncSend(sendInfo);
                    }
                    else
                    {
                        DisposeBuffer(sendInfo.Buffer);
                        sendInfo.Ctrl.MarkAsCompleted(true);
                    }
                }
                catch (Exception ex)
                {
                    DisposeBuffer(sendInfo.Buffer);

                    OnDisconnection();

                    if (ex is SocketException)
                        LogSocketException((SocketException) ex);
                    else
                        Logger.Error(
                            string.Format("{0}: exception caught handling asynchronous send/write.", GetChannelTitle()),
                            ex);

                    sendInfo.Ctrl.Error = ex;
                    sendInfo.Ctrl.MarkAsCompleted(false);

                    return;
                }

                if (_sendQueue != null && _sendQueue.Count > 0)
                    StartAsyncSend(_sendQueue.Dequeue());
            }
        }

        protected virtual void OnStartAsyncSend(AsyncSendInfo sendInfo, int dataToSend)
        {
            IBuffer buffer = sendInfo.Buffer;

            if (buffer.MultiChunk)
                Socket.BeginSend(buffer.GetDataSegments(dataToSend), SocketFlags.None,
                    AsyncSendOrWriteRequestHandler, sendInfo);
            else
                Socket.BeginSend(buffer.GetArray(), buffer.LowerDataBound, dataToSend, SocketFlags.None,
                    AsyncSendOrWriteRequestHandler, sendInfo);
        }

        protected void StartAsyncSend(AsyncSendInfo sendInfo)
        {
            try
            {
                IBuffer buffer = sendInfo.Buffer;

                if (Logger.IsDebugEnabled())
                    Logger.Debug(StringUtilities.DumpBufferData(string.Format(
                        "{0} raw data to send ({1} {2}): ", GetChannelTitle(),
                        buffer.DataLength, buffer.DataLength == 1 ? "byte" : "bytes"),
                        buffer, buffer.LowerDataBound, buffer.DataLength));

                if (_sending)
                {
                    // Queue send request
                    if (_sendQueue == null)
                        _sendQueue = new Queue<AsyncSendInfo>();
                    _sendQueue.Enqueue(sendInfo);
                    return;
                }

                int dataToSend;
                if (SendMaxRequestSize <= 0)
                    // Try to send whole data in one call.
                    dataToSend = buffer.DataLength;
                else
                    dataToSend = buffer.DataLength > SendMaxRequestSize ? SendMaxRequestSize : buffer.DataLength;

                // It is very important to set _sending before BeginSend because AsyncSendRequestHandler can be called
                // in the current thread.
                _sending = true;

                OnStartAsyncSend(sendInfo, dataToSend);
            }
            catch (Exception ex)
            {
                OnDisconnection();

                if (ex is SocketException)
                    LogSocketException((SocketException) ex);
                else
                    Logger.Error(string.Format("{0}: exception caught handling asynchronous send.", GetChannelTitle()),
                        ex);

                sendInfo.Ctrl.Error = ex;
                sendInfo.Ctrl.MarkAsCompleted(false);
                return;
            }
        }

        protected void LogSocketException(SocketException ex)
        {
            switch (ex.ErrorCode)
            {
                case 10054:
                    // An existing connection was forcibly closed by the remote host.
                    Logger.Info(string.Format("{0}: connection closed, {1}.", GetChannelTitle(), ex.Message));
                    break;

                case 10060:
                    // Connection timed out. A connection attempt failed because the
                    // connected party did not properly respond after a period of time,
                    // or the established connection failed because the connected host
                    // has failed to respond.
                    Logger.Warn(string.Format("{0}: connection timed out, {1}.", GetChannelTitle(), ex.Message));
                    break;

                case 10061:
                    // Connection refused. No connection could be made because the target
                    // machine actively refused it. This usually results from trying to
                    // connect to a service that is inactive on the foreign host — that is,
                    // one with no server application running. 
                    Logger.Warn(string.Format("{0}: connection refused, {1}.", GetChannelTitle(), ex.Message));
                    break;

                default:
                    Logger.Error(string.Format("{0}: socket exception (error {1})", GetChannelTitle(), ex.ErrorCode), ex);
                    break;
            }
        }

        #region Nested type: AsyncSendInfo
        protected class AsyncSendInfo
        {
            private readonly IBuffer _buffer;
            private readonly ChannelRequestCtrl _ctrl;
            private readonly Socket _socket;

            public AsyncSendInfo(Socket socket, IBuffer buffer, ChannelRequestCtrl ctrl)
            {
                _socket = socket;
                _buffer = buffer;
                _ctrl = ctrl;
            }

            public ChannelRequestCtrl Ctrl
            {
                get { return _ctrl; }
            }

            public IBuffer Buffer
            {
                get { return _buffer; }
            }

            public Socket Socket
            {
                get { return _socket; }
            }

            public int RequestedBytes { get; set; }
        }
        #endregion
    }
}