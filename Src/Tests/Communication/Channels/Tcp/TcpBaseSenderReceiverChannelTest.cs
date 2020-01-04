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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tests.Trx.Buffer;
using Trx.Buffer;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Sinks;
using Trx.Communication.Channels.Sinks.Framing;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;
using Trx.Logging;
using Trx.Messaging;

namespace Tests.Trx.Communication.Channels.Tcp
{
    [TestFixture(Description = "Tcp client/child channels base tests.")]
    public class TcpBaseSenderReceiverChannelTest
    {
        private IPEndPoint GetServerAvlEndPoint()
        {
            var random = new Random(DateTime.UtcNow.Millisecond*(DateTime.UtcNow.Second + 1));
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep;
            do
            {
                int port = random.Next(1024, 60000);
                ep = new IPEndPoint(ipAddress, port);
            } while (TcpServerChannelTest.IsActiveListener(ep));

            return ep;
        }

        private TcpServerChannel SetupServer(IPEndPoint ep, IPipelineFactory pipelineFactory)
        {
            var server = new TcpServerChannel(new Pipeline(), pipelineFactory,
                new TupleSpace<ReceiveDescriptor>(), new MessagesIdentifierMock())
                             {
                                 Port = ep.Port,
                                 LocalInterface = ep.Address.ToString()
                             };

            ChannelRequestCtrl ctrl = server.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(server.IsListening);

            return server;
        }

        private TcpServerChannel SetupServer(IPEndPoint ep)
        {
            return SetupServer(ep, new ClonePipelineFactory(new Pipeline()));
        }

        private TcpClientChannel SetupClient(TcpServerChannel server)
        {
            var client = new TcpClientChannel(new Pipeline(),
                new TupleSpace<ReceiveDescriptor>(), new MessagesIdentifierMock())
                             {
                                 RemotePort = server.LocalEndPoint.Port,
                                 RemoteInterface = server.LocalEndPoint.Address.ToString()
                             };

            int connCnt = server.Childs.Count;
            ChannelRequestCtrl ctrl = client.Connect();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsTrue(ctrl.Successful);

            for (int i = 0; i < 100; i++)
            {
                if (server.Childs.Count > connCnt)
                    return client;
                Thread.Sleep(50); // Give some time to server to setup child channel
            }

            throw new ApplicationException("Timeout waiting server to setup child channel.");
        }

        private TcpServerChildChannel GetChild(TcpServerChannel server)
        {
            foreach (var child in server.Childs.Values)
                return child as TcpServerChildChannel;

            return null;
        }

        [Test(Description = "Check disposed test.")]
        public void CheckDisposal()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);

            client.Close(); // Close and disposal expected
            client.Close();

            Assert.Throws<ObjectDisposedException>(() => client.Connect());
            Assert.Throws<ObjectDisposedException>(() => client.Send(string.Empty));
            Assert.Throws<ObjectDisposedException>(() => client.SendExpectingResponse(string.Empty, 0, false, null));
            Assert.Throws<ObjectDisposedException>(() => client.SendExpectingResponse(string.Empty, 0, true, null));
        }

        [Test(Description = "Disconnecting a channel with queued messages to send.")]
        public void DisconnectionWithQueuedSend()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);

            client.Sending = true; // Force queue

            const string firstMessage = "First queued message";
            var buffer = new SingleChunkBuffer();
            buffer.Write(false, firstMessage);
            ChannelRequestCtrl firstQueuedMsgctrl = client.Send(buffer);

            const string secondMessage = "Second queued message";
            buffer = new SingleChunkBuffer();
            buffer.Write(false, secondMessage);
            ChannelRequestCtrl secondQueuedMsgctrl = client.Send(buffer);

            client.Sending = false;

            Assert.IsFalse(firstQueuedMsgctrl.IsCompleted);
            Assert.IsFalse(secondQueuedMsgctrl.IsCompleted);

            client.Disconnect();

            Assert.IsTrue(firstQueuedMsgctrl.IsCompleted);
            Assert.IsTrue(secondQueuedMsgctrl.IsCompleted);
            const string msg = "Channel was disconnected";
            Assert.IsFalse(firstQueuedMsgctrl.Successful);
            Assert.AreEqual(msg, firstQueuedMsgctrl.Message);
            Assert.IsFalse(secondQueuedMsgctrl.Successful);
            Assert.AreEqual(msg, secondQueuedMsgctrl.Message);
        }

        [Test(Description = "Exception in receive pipeline.")]
        public void ExceptionInReceivePipeline()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);

            const string msg = "Hello world from client!";
            string msgLen = string.Format("{0:000}", msg.Length);
            const int headerLength = 3;
            child.Pipeline.Push(new StringFrameLengthSink(headerLength));
            var sink = new SinkMock();
            child.Pipeline.Push(sink);

            sink.ExceptionMessage = "Throwing a requested exception in SinkMock";

            var buffer = new SingleChunkBuffer();
            buffer.Write(false, msgLen);
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            int i;
            for (i = 0; i < 5000; i += 10)
                if (client.IsConnected)
                    Thread.Sleep(10);
                else
                    break;
            if (i == 5000)
                Assert.Fail("Client still connected!");
            Assert.IsFalse(client.IsConnected);
            for (i = 0; i < 5000; i += 10)
                if (child.IsConnected)
                    Thread.Sleep(10);
                else
                    break;
            if (i == 5000)
                Assert.Fail("Child still connected!");
            Assert.IsFalse(child.IsConnected);
        }

        [Test(Description = "Fragmented send test.")]
        public void FragmentedSend()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(3));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(3));

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer();
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            Assert.AreEqual(3, buffer.DiscardCallCnt);

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);
        }

        [Test(Description = "Framed receive test.")]
        public void FramedReceive()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);

            const string msg = "Hello world from client!";
            string msgLen = string.Format("{0:000}", msg.Length);
            const int headerLength = 3;
            child.Pipeline.Push(new StringFrameLengthSink(headerLength));

            var buffer = new NonDisposableSimpleBuffer();

            Assert.AreEqual(0, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(0, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            // Send whole packet.
            buffer.Write(false, msgLen);
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);

            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(0, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(0, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            // Now length first then the rest of the data.
            buffer.Clear();
            buffer.Write(false, msgLen);
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            int i;
            for (i = 0; i < 5000; i += 10)
                if (child.InputBuffer.DataLength > 0)
                    break;
                else
                    Thread.Sleep(10);
            if (i == 5000)
                Assert.Fail("Data has not received in the server!");
            Assert.IsNull(child.TupleSpace.Take(null, 0));
            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(headerLength, child.InputBuffer.DataLength);
            Assert.AreEqual(msg.Length + headerLength, child.PipelineContext.ExpectedBytes);
            buffer.Clear();
            buffer.Write(false, msg);
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);

            Assert.AreEqual(0, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(0, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            // Now length and data but fragmented.
            buffer.Clear();
            buffer.Write(false, msgLen.Substring(0, 1)); // One byte from length.
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            for (i = 0; i < 5000; i += 10)
                if (child.InputBuffer.DataLength > 0)
                    break;
                else
                    Thread.Sleep(10);
            if (i == 5000)
                Assert.Fail("Data has not received in the server!");
            Assert.IsNull(child.TupleSpace.Take(null, 0));
            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(headerLength, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(1, child.InputBuffer.DataLength);
            Assert.IsTrue(child.IsConnected);
            buffer.Clear();
            buffer.Write(false, msgLen.Substring(1)); // Now the rest of the header.
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            for (i = 0; i < 5000; i += 10)
                if (child.InputBuffer.DataLength >= headerLength)
                    break;
                else
                    Thread.Sleep(10);
            if (i == 5000)
                Assert.Fail("Data has not received in the server!");
            Assert.IsNull(child.TupleSpace.Take(null, 0));
            Assert.AreEqual(headerLength, child.InputBuffer.DataLength);
            Assert.IsTrue(child.IsConnected);
            buffer.Clear();
            buffer.Write(false, msg.Substring(0, 10)); // 10 bytes of the data.
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            for (i = 0; i < 5000; i += 10)
                if (child.InputBuffer.DataLength >= headerLength + 10)
                    break;
                else
                    Thread.Sleep(10);
            if (i == 5000)
                Assert.Fail("Data has not received in the server!");
            Assert.IsNull(child.TupleSpace.Take(null, 0));
            Assert.AreEqual(msg.Length + headerLength, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(headerLength + 10, child.InputBuffer.DataLength);
            Assert.IsTrue(child.IsConnected);
            buffer.Clear();
            buffer.Write(false, msg.Substring(10)); // Now the rest of the data.
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);

            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(0, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(0, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            // Now a full packet and one byte of the header
            buffer.Clear();
            buffer.Write(false, msgLen);
            buffer.Write(false, msg);
            buffer.Write(false, msgLen.Substring(0, 1)); // One byte from length.
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);

            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(headerLength, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(1, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            buffer.Clear();
            buffer.Write(false, msgLen.Substring(1)); // Now the rest of the header.
            buffer.Write(false, msg);
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);

            Assert.IsTrue(child.IsConnected);
            Assert.AreEqual(0, child.PipelineContext.ExpectedBytes);
            Assert.AreEqual(0, child.InputBuffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, child.InputBuffer.Capacity);

            // Close socket.
            client.Close();
            Assert.IsFalse(client.IsConnected);
            for (i = 0; i < 5000; i += 10)
                if (child.IsConnected)
                    Thread.Sleep(10);
                else
                    break;
            if (i == 5000)
                Assert.Fail("Child still connected!");
        }

        [Test(Description = "Test a message consumed in the send pipeline.")]
        public void MessageConsumedInSendPipeline()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            var sink = new SinkMock {ConsumeMessageOnSend = true};
            client.Pipeline.Push(sink);
            TcpServerChildChannel child = GetChild(server);

            Assert.IsTrue(client.IsConnected);
            Assert.IsTrue(child.IsConnected);

            const string msg = "The message has been consumed by the pipeline";

            var ctrl = client.Send(string.Empty);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Message);
            Assert.AreSame(msg, ctrl.Message);
            var requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Message);
            Assert.AreSame(msg, requestCtrl.Message);
            requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Message);
            Assert.AreSame(msg, requestCtrl.Message);
        }

        [Test(Description = "Sending a message wich is not an IBuffer.")]
        public void SendAMessageOtherThanIBuffer()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);

            const string msg = "This channel implementation only support to send messages of type IBuffer.";

            var ctrl = client.Send(string.Empty);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.IsNotNull(ctrl.Error as NotSupportedException);
            Assert.AreEqual(msg, ctrl.Error.Message);
            var requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Error);
            Assert.IsNotNull(requestCtrl.Error as NotSupportedException);
            Assert.AreEqual(msg, requestCtrl.Error.Message);
            requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Error);
            Assert.IsNotNull(requestCtrl.Error as NotSupportedException);
            Assert.AreEqual(msg, requestCtrl.Error.Message);
        }

        [Test(Description = "Exception in the send pipeline.")]
        public void ExceptionInSendPipeline()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            var sink = new SinkMock { ExceptionMessage = "Exception as requested" };
            client.Pipeline.Push(sink);
            TcpServerChildChannel child = GetChild(server);

            Assert.IsTrue(client.IsConnected);
            Assert.IsTrue(child.IsConnected);

            var ctrl = client.Send(string.Empty);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.AreEqual(sink.ExceptionMessage, ctrl.Error.Message);
            var requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Error);
            Assert.AreEqual(sink.ExceptionMessage, requestCtrl.Error.Message);
            requestCtrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(requestCtrl.IsCompleted);
            Assert.IsFalse(requestCtrl.Successful);
            Assert.IsNotNull(requestCtrl.Error);
            Assert.AreEqual(sink.ExceptionMessage, requestCtrl.Error.Message);
        }

        [Test(Description = "Queued send test.")]
        public void QueuedSend()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            client.Sending = true; // Force queue

            const string firstMessage = "First queued message";
            var buffer = new SingleChunkBuffer();
            buffer.Write(false, firstMessage);
            ChannelRequestCtrl firstQueuedMsgctrl = client.Send(buffer);

            const string secondMessage = "Second queued message";
            buffer = new SingleChunkBuffer();
            buffer.Write(false, secondMessage);
            ChannelRequestCtrl secondQueuedMsgctrl = client.Send(buffer);

            client.Sending = false;

            const string notQueuedMessage = "Not queued message";
            buffer = new SingleChunkBuffer();
            buffer.Write(false, notQueuedMessage);
            ChannelRequestCtrl notQueuedMsgctrl = client.Send(buffer);

            Assert.IsTrue(notQueuedMsgctrl.WaitCompletion(5000, true));
            Assert.IsTrue(firstQueuedMsgctrl.WaitCompletion(5000, true));
            Assert.IsTrue(secondQueuedMsgctrl.WaitCompletion(5000, true));

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(notQueuedMessage, rcvDesc.ReceivedMessage);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(firstMessage, rcvDesc.ReceivedMessage);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(secondMessage, rcvDesc.ReceivedMessage);
        }

        [Test(Description = "Raise an exception in StartAsyncSend - stream sending.")]
        public void RaiseExceptionStartAsyncSend1()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer { RaiseExceptionInGetArray = true, RaiseExceptionInGetDataSegments = true };
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.Successful);

            Assert.IsFalse(client.IsConnected);
            child.Close();

            // No partial message must be put in the warehouse.
            Assert.IsNull(child.TupleSpace.Take(null, 0));
        }

        [Test(Description = "Raise an exception in StartAsyncSend - framed sending.")]
        public void RaiseExceptionStartAsyncSend2()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer { RaiseExceptionInGetArray = true, RaiseExceptionInGetDataSegments = true };
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.Successful);

            Assert.IsFalse(client.IsConnected);
            child.Close();

            // No partial message must be put in the warehouse.
            Assert.IsNull(child.TupleSpace.Take(null, 0));
        }

        [Test(Description = "Raise an exception in AsyncSendRequestHandler - stream sending.")]
        public void RaiseExceptionInAsyncSendRequestHandler1()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer {RaiseExceptionInDiscard = true};
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.Successful);

            Assert.IsFalse(client.IsConnected);

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg.Substring(0, client.SendMaxRequestSize), rcvDesc.ReceivedMessage);
        }

        [Test(Description = "Raise an exception in AsyncSendRequestHandler - framed sending.")]
        public void RaiseExceptionInAsyncSendRequestHandler2()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer {RaiseExceptionInDiscard = true};
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.Successful);

            Assert.IsFalse(client.IsConnected);
            child.Close();

            // No partial message must be put in the warehouse.
            Assert.IsNull(child.TupleSpace.Take(null, 0));
        }

        [Test(Description = "Raise an exception in AsyncSendRequestHandler - buffer dispose.")]
        public void RaiseExceptionInAsyncSendRequestHandler3()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            client.SendMaxRequestSize = 7;

            const string msg = "A fragmented message";
            var buffer = new TestingSingleChunkBuffer { RaiseExceptionInDispose = true };
            buffer.Write(false, msg);
            ChannelRequestCtrl ctrl = client.Send(buffer);

            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(client.IsConnected);

            // Message must be delivered.
            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msg, rcvDesc.ReceivedMessage);
        }

        [Test(Description = "Simple send/receive test.")]
        public void SimpleSendReceive()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);

            var ex1 = Assert.Throws<ArgumentNullException>(() => client.Send(null));
            Assert.That(ex1.ParamName, Is.EqualTo("message"));

            const string msgFromClient = "Hello world from client!";

            var buffer = new SingleChunkBuffer();
            buffer.Write(false, msgFromClient);

            ChannelRequestCtrl ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(buffer.IsDisposed);

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msgFromClient, rcvDesc.ReceivedMessage);

            // Burst send.

            for (int i = 0; i < 100; i++)
            {
                buffer = new SingleChunkBuffer();
                string msg = string.Format("{0} {1}", msgFromClient, i);
                buffer.Write(false, msg);

                ctrl = client.Send(buffer);
                Assert.IsTrue(ctrl.WaitCompletion(5000, true));
                Assert.IsTrue(ctrl.Successful);
                Assert.IsTrue(buffer.IsDisposed);

                rcvDesc = child.TupleSpace.Take(null, 5000);
                Assert.IsNotNull(rcvDesc);
                Assert.AreEqual(msg, rcvDesc.ReceivedMessage);
            }

            // Now send from server to client.

            const string msgFromServer = "Hello world from server!";

            buffer = new SingleChunkBuffer();
            buffer.Write(false, msgFromServer);

            child.SendMaxRequestSize = 0;

            ctrl = child.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(buffer.IsDisposed);

            rcvDesc = client.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.AreEqual(msgFromServer, rcvDesc.ReceivedMessage);

            // Now both peers sending and receiving in burst mode.

            for (int i = 0; i < 1000; i++)
            {
                int index = i;
                Parallel.Invoke(() =>
                                    {
                                        var pBuffer = new SingleChunkBuffer();
                                        string msg = string.Format("{0} {1}", msgFromClient, index);
                                        pBuffer.Write(false, msg);

                                        ChannelRequestCtrl pCtrl = client.Send(pBuffer);
                                        Assert.IsTrue(pCtrl.WaitCompletion(5000, true));
                                        Assert.IsTrue(pCtrl.Successful);
                                        Assert.IsTrue(pBuffer.IsDisposed);

                                        ReceiveDescriptor pRcvDesc = child.TupleSpace.Take(null, 5000);
                                        Assert.IsNotNull(pRcvDesc);
                                        Assert.AreEqual(msg, pRcvDesc.ReceivedMessage);
                                    },
                    () =>
                        {
                            var pBuffer = new SingleChunkBuffer();
                            string msg = string.Format("{0} {1}", msgFromServer, index);
                            pBuffer.Write(false, msg);

                            ChannelRequestCtrl pCtrl = child.Send(pBuffer);
                            Assert.IsTrue(pCtrl.WaitCompletion(5000, true));
                            Assert.IsTrue(pCtrl.Successful);
                            Assert.IsTrue(pBuffer.IsDisposed);

                            ReceiveDescriptor pRcvDesc = client.TupleSpace.Take(null, 5000);
                            Assert.IsNotNull(pRcvDesc);
                            Assert.AreEqual(msg, pRcvDesc.ReceivedMessage);
                        });
            }

            // Now send a message.
            var formatter = new BasicMessageFormatter();
            formatter.FieldFormatters.Add(new StringFieldFormatter(1, new FixedLengthManager(12),
                DataEncoder.GetInstance(), "Sample field"));

            client.Pipeline.Push(new StringFrameLengthSink(2));
            client.Pipeline.Push(new MessageFormatterSink(formatter));
            child.Pipeline.Push(new StringFrameLengthSink(2));
            child.Pipeline.Push(new MessageFormatterSink(formatter));

            var message = new Message();
            message.Fields.Add(1, "TEST");

            ctrl = client.Send(message);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(buffer.IsDisposed);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            Assert.IsNotNull(rcvDesc.ReceivedMessage as Message);
        }

        private ISenderChannel GetSenderChannel(IChannelAddress channelAddress)
        {
            var reference = channelAddress as ReferenceChannelAddress;
            if (reference != null)
            {
                var channel = reference.Channel as ISenderChannel;
                if (channel != null)
                    return channel;
            }

            return null;
        }

        [Test(Description = "Send expecting response from peer")]
        public void SendExpectingResponseToTupleSpace()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            var cltTupleSpace = client.TupleSpace;
            var svrTupleSpace = child.TupleSpace;
            var messagesIdentifier = client.MessagesIdentifier as MessagesIdentifierMock;
            Assert.IsNotNull(messagesIdentifier);

            var reqMessage = new NonDisposableSimpleBuffer();
            const string reqMessageStr = "Request from client!";

            var rspMessage = new NonDisposableSimpleBuffer();
            const string rspMessageStr = "Response from server!";

            var ex1 = Assert.Throws<ArgumentNullException>(() => client.SendExpectingResponse(null, 0, true, null));
            Assert.That(ex1.ParamName, Is.EqualTo("message"));

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendExpectingResponse(string.Empty, 0, true, null));
            Assert.That(ex2.ParamName, Is.EqualTo("timeout"));

            var ex3 = Assert.Throws<ChannelException>(() => new TcpClientChannel(new Pipeline()).SendExpectingResponse(string.Empty, 100, true, null));
            Assert.That(ex3.Message,
                Is.EqualTo(
                    "Channel isn't configured with a messages identifier, unable to match requests with responses."));

            // Send a request to child, but don't respond it.
            reqMessage.Write(false, reqMessageStr);
            var ctrl = client.SendExpectingResponse(reqMessage, 100, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);
            ReceiveDescriptor receivedMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(receivedMessage);
            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreEqual(reqMessageStr, receivedMessage.ReceivedMessage);

            // Get timed out request.
            ReceiveDescriptor request = cltTupleSpace.Take(null, 5000);

            Assert.IsNotNull(request as Request);
            Assert.IsTrue(((Request)request).IsExpired);

            // Send two requests with same key.
            reqMessage.Clear();
            reqMessage.Write(false, reqMessageStr);
            ctrl = client.SendExpectingResponse(reqMessage, 5000, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            var ctrl2 = client.SendExpectingResponse(reqMessage, 100, true, null);
            Assert.IsNotNull(ctrl2);
            Assert.IsTrue(ctrl2.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl2.Successful);

            Assert.IsNotNull(ctrl2.Message);
            Assert.IsTrue(ctrl2.Message == "There's already a pending request with the same request message key.");

            // Respond request.
            ReceiveDescriptor requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            var sourceChannel = GetSenderChannel(requestMessage.ChannelAddress);
            Assert.IsNotNull(sourceChannel);
            rspMessage.Write(false, rspMessageStr);
            var ctrl3 = sourceChannel.Send(rspMessage);
            Assert.IsNotNull(ctrl3);
            Assert.IsTrue(ctrl3.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl3.Successful);

            ReceiveDescriptor response = cltTupleSpace.Take(null, 5000);

            Assert.IsNotNull(request as Request);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ReceivedMessage);
            Assert.AreEqual(rspMessageStr, response.ReceivedMessage);

            // Change response message key to prevent matching, return null.
            reqMessage.Clear();
            reqMessage.Write(false, reqMessageStr);
            ctrl = client.SendExpectingResponse(reqMessage, 1000, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            sourceChannel = GetSenderChannel(requestMessage.ChannelAddress);
            Assert.IsNotNull(sourceChannel);
            messagesIdentifier.ReturnValue = null;
            rspMessage.Clear();
            rspMessage.Write(false, rspMessageStr);
            ctrl3 = sourceChannel.Send(rspMessage);
            Assert.IsNotNull(ctrl3);
            Assert.IsTrue(ctrl3.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl3.Successful);

            // Receive unmatched response
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNull(response as Request);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ReceivedMessage);
            Assert.AreEqual(rspMessageStr, response.ReceivedMessage);

            // Receive timed out request
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNotNull(response as Request);
            Assert.IsTrue(((Request)response).IsExpired);

            // Try to send a null message key request
            ctrl = client.SendExpectingResponse(reqMessage, 100, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsTrue(ctrl.Message ==
                "The message key generated by the messages identifier is null, can't send.");

            // Do the same but with an invalid key
            messagesIdentifier.ReturnValue = "validKey";
            reqMessage.Clear();
            reqMessage.Write(false, reqMessageStr);
            ctrl = client.SendExpectingResponse(reqMessage, 1000, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            var address = requestMessage.ChannelAddress as ReferenceChannelAddress;
            Assert.IsNotNull(address);
            sourceChannel = address.Channel as ISenderChannel;
            Assert.IsNotNull(sourceChannel);
            messagesIdentifier.ReturnValue = "invalidKey";
            rspMessage.Clear();
            rspMessage.Write(false, rspMessageStr);
            ctrl3 = sourceChannel.Send(rspMessage);
            Assert.IsNotNull(ctrl3);
            Assert.IsTrue(ctrl3.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl3.Successful);

            // Receive unmatched response
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNull(response as Request);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ReceivedMessage);
            Assert.AreEqual(rspMessageStr, response.ReceivedMessage);

            // Receive timed out request
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNotNull(response as Request);
            Assert.IsTrue(((Request)response).IsExpired);
        }


        [Test(Description = "Send expecting response from peer and getting the request handler")]
        public void SendExpectingResponseNotToTupleSpace()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            var cltTupleSpace = client.TupleSpace;
            var svrTupleSpace = child.TupleSpace;
            var messagesIdentifier = client.MessagesIdentifier as MessagesIdentifierMock;
            Assert.IsNotNull(messagesIdentifier);

            var reqMessage = new NonDisposableSimpleBuffer();
            const string reqMessageStr = "Request from client!";

            var ex1 = Assert.Throws<ArgumentNullException>(() => client.SendExpectingResponse(null, 0, false, null));
            Assert.That(ex1.ParamName, Is.EqualTo("message"));

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => client.SendExpectingResponse(string.Empty, 0, false, null));
            Assert.That(ex2.ParamName, Is.EqualTo("timeout"));

            var ex3 = Assert.Throws<ChannelException>(() => new TcpClientChannel(new Pipeline()).SendExpectingResponse(string.Empty, 100, false, null));
            Assert.That(ex3.Message,
                Is.EqualTo(
                    "Channel isn't configured with a messages identifier, unable to match requests with responses."));

            // SendAndReturnRequestHandler test.
            reqMessage.Write(false, reqMessageStr);
            var handlerCtrl = client.SendExpectingResponse(reqMessage, 1000, false, null);
            Assert.IsNotNull(handlerCtrl);
            Assert.IsTrue(handlerCtrl.WaitCompletion(5000, true));
            Assert.IsTrue(handlerCtrl.Successful);

            Assert.IsNotNull(handlerCtrl.Request);

            ReceiveDescriptor receivedMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(receivedMessage);
            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreEqual(reqMessageStr, receivedMessage.ReceivedMessage);

            Assert.IsFalse(handlerCtrl.Request.WaitResponse());
            Assert.IsTrue(handlerCtrl.Request.IsExpired);

            // Timed out synchronous request cannot go to the request tuple space.
            Assert.IsNull(cltTupleSpace.Read(null, 0));
        }

        [Test(Description = "Cancel child pending requests on disconnect")]
        public void CancelPendingRequestsOnDisconnection()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server1 = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server1);
            client.Pipeline.Push(new StringFrameLengthSink(2));
            TcpServerChildChannel child = GetChild(server1);
            child.Pipeline.Push(new StringFrameLengthSink(2));

            var reqMessage = new NonDisposableSimpleBuffer();
            const string reqMessageStr = "Request from child!";

            reqMessage.Write(false, reqMessageStr);
            var ctrl = child.SendExpectingResponse(reqMessage, 5000, false, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            Assert.IsNotNull(ctrl.Request);

            ReceiveDescriptor receivedMessage = client.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(receivedMessage);
            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreEqual(reqMessageStr, receivedMessage.ReceivedMessage);

            child.Disconnect();
            client.Disconnect();

            Assert.IsFalse(child.IsConnected);
            Assert.IsFalse(client.IsConnected);

            Assert.IsTrue(ctrl.Request.IsCancelled);
        }

        [Test(Description = "Cancel child pending requests on disconnect")]
        public void ResolveHostEntry()
        {
            Assert.IsNotNull(TcpBaseSenderReceiverChannel.ResolveHostEntry( "Test", "localhost", AddressFamily.InterNetwork ));
            Assert.Throws<SocketException>(() => TcpBaseSenderReceiverChannel.ResolveHostEntry("Test", "invalid.reallyInvalid", AddressFamily.InterNetwork));
            Assert.Throws<ChannelException>(() => TcpBaseSenderReceiverChannel.ResolveHostEntry("Test", "localhost", AddressFamily.AppleTalk));
        }

        [Test(Description = "Big send/receive test.")]
        public void BigSendReceive()
        {
            const int headerLength = 7;
            const int dataLen = 1048583;   // 1 MB + header :)

            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            client.Pipeline.Push(new StringFrameLengthSink(headerLength));
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(headerLength));

            client.Logger = new DummyLogger("noname");
            child.Logger = client.Logger;

            var buffer = new SingleChunkBuffer(dataLen) { LowerDataBound = headerLength, UpperDataBound = dataLen };
            client.SendMaxRequestSize = 0;

            ChannelRequestCtrl ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(60000, true));
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(buffer.IsDisposed);

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 60000);
            Assert.IsNotNull(rcvDesc);
            var rcvMessage = rcvDesc.ReceivedMessage as string;
            Assert.IsNotNull(rcvMessage);
            Assert.AreEqual(dataLen - headerLength, rcvMessage.Length);
        }

        [Test(Description = "Big send/receive test.")]
        public void InputBufferResize()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new StringFrameLengthSink(4));

            var buffer = new SingleChunkBuffer(65536);
            buffer.Write( false, "2000" );
            buffer.UpperDataBound = 2004;
            buffer.Write( false, "3000"  );
            buffer.UpperDataBound = 5008;
            buffer.Write(false, "5000");
            buffer.UpperDataBound = 10012;

            ChannelRequestCtrl ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            ReceiveDescriptor rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            var rcvMessage = rcvDesc.ReceivedMessage as string;
            Assert.IsNotNull(rcvMessage);
            Assert.AreEqual(2000, rcvMessage.Length);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            rcvMessage = rcvDesc.ReceivedMessage as string;
            Assert.IsNotNull(rcvMessage);
            Assert.AreEqual(3000, rcvMessage.Length);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            rcvMessage = rcvDesc.ReceivedMessage as string;
            Assert.IsNotNull(rcvMessage);
            Assert.AreEqual(5000, rcvMessage.Length);

            client.Close();
            child.Close();

            // Now test expand of the buffer

            buffer = new SingleChunkBuffer();
            buffer.UpperDataBound = buffer.Capacity;

            client = SetupClient(server);
            child = GetChild(server);
            var sink = new SinkMock { ReceiveReturnValue = false };
            child.Pipeline.Push(sink);

            buffer.LowerDataBound = 0;
            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            // Wait sink receive is called
            int i;
            for (i = 0; i < 5000; i += 10)
                if (sink.ReceiveMessage == null)
                    Thread.Sleep(10);
                else
                    break;
            if (i == 5000)
                Assert.Fail("Sink not called!");
            var inputBuffer = sink.ReceiveMessage as IBuffer;
            Assert.IsNotNull( inputBuffer );
            // Now wait all data is received
            for (i = 0; i < 5000; i += 10)
                if (inputBuffer.DataLength < buffer.Capacity)
                    Thread.Sleep( 10 );
                else
                    break;
            if (i == 5000)
                Assert.Fail("Whole data not received!");

            // Give time to start read again.
            Thread.Sleep( 300 );

            // Now accept the whole packet.
            sink.ReceiveReturnValue = true;

            buffer = new SingleChunkBuffer {UpperDataBound = 1};

            ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            rcvDesc = child.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(rcvDesc);
            rcvMessage = rcvDesc.ReceivedMessage as string;
            Assert.IsNotNull(rcvMessage);
            Assert.AreEqual(buffer.Capacity + 1, rcvMessage.Length);

            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize * 2, child.InputBuffer.Capacity);
        }


        [Test(Description = "Expected bytes < 0.")]
        public void NegativeExpectedBytes()
        {
            IPEndPoint serverEndPoint = GetServerAvlEndPoint();
            TcpServerChannel server = SetupServer(serverEndPoint);
            TcpClientChannel client = SetupClient(server);
            TcpServerChildChannel child = GetChild(server);
            child.Pipeline.Push(new SinkMock { ExpectedBytes = -1, ReceiveReturnValue = false});

            Assert.IsTrue(client.IsConnected);
            Assert.IsTrue(child.IsConnected);

            const string msg = "Some data";
            var buffer = new SingleChunkBuffer();
            buffer.Write(false, msg);
            var ctrl = client.Send(buffer);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsTrue(ctrl.Successful);

            int i;
            for (i = 0; i < 5000; i += 10)
                if (child.IsConnected)
                    Thread.Sleep(10);
                else
                    break;
            if (i == 5000)
                Assert.Fail("Child still connected!");

        }
    }
}