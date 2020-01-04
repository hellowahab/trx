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
using NUnit.Framework;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Local;
using Trx.Coordination.TupleSpace;

namespace Tests.Trx.Communication.Channels.Local
{
    [TestFixture(Description = "Local server registry tests.")]
    public class LocalClientChannelTest
    {
        [Test(Description = "Close test")]
        public void CloseTest()
        {
            var pipeline = new Pipeline();
            var sink = new SinkMock();
            pipeline.Push(sink);
            const string someAddress = "address115";

            var channel = new LocalClientChannel(someAddress, pipeline);

            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()));
            server.StartListening();

            ChannelRequestCtrl ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);

            DateTime utcNow = DateTime.UtcNow;
            channel.Close();

            Assert.IsNotNull(sink.ChannelEvent);
            Assert.IsTrue(sink.ChannelEvent.EventType == ChannelEventType.Disconnected);
            Assert.LessOrEqual(utcNow, sink.OnEventUtcDateTime);

            Assert.IsFalse(channel.IsConnected);

            server.StopListening();
        }

        [Test(Description = "Connect test")]
        public void ConnectTest()
        {
            var cltPipeline = new Pipeline();
            var cltSink = new SinkMock();
            cltPipeline.Push(cltSink);
            var svrPipeline = new Pipeline();
            var svrSink = new SinkMock();
            svrPipeline.Push(svrSink);
            const string someAddress = "address113";

            var channel = new LocalClientChannel(someAddress, cltPipeline);

            Assert.IsFalse(channel.IsConnected);

            ChannelRequestCtrl ctrl = channel.Connect();

            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsTrue(ctrl.Message == "Connection refused.");

            Assert.IsNotNull(cltSink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.ConnectionFailed, cltSink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.ConnectionRequested, cltSink.PreviousChannelEvent.EventType);

            var server = new LocalServerChannel(someAddress, svrPipeline, new ClonePipelineFactory(new Pipeline()));
            server.StartListening();

            DateTime utcNow = DateTime.UtcNow;
            ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);

            Assert.IsNotNull(cltSink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.Connected, cltSink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.ConnectionRequested, cltSink.PreviousChannelEvent.EventType);
            Assert.LessOrEqual(utcNow, cltSink.OnEventUtcDateTime);

            ChannelRequestCtrl ctrl2 = channel.Connect();

            Assert.AreSame(ctrl, ctrl2);

            channel.Disconnect();

            // Raise an exception in server pipeline
            const string svrExceptionMessage = "server exception";
            svrSink.ExceptionMessage = svrExceptionMessage;
            ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.IsInstanceOf<ChannelException>(ctrl.Error);
            Assert.AreEqual(ctrl.Error.Message, svrExceptionMessage);
            Assert.IsFalse(channel.IsConnected);
            svrSink.ExceptionMessage = null;

            // Raise an exception in client pipeline
            const string cltExceptionMessage = "client exception";
            cltSink.ExceptionMessage = cltExceptionMessage;
            ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);
            Assert.IsNull(ctrl.Error);
            Assert.IsTrue(channel.IsConnected);

            server.StopListening();
        }

        [Test(Description = "Constructor and properties test.")]
        public void ConstructorAndPropertiesTest()
        {
            var pipeline = new Pipeline();
            const string someAddress = "address112";

            var ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, pipeline));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            var channel = new LocalClientChannel(someAddress, pipeline);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.IsNull(channel.TupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, pipeline, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, pipeline, tupleSpace));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            channel = new LocalClientChannel(someAddress, pipeline, tupleSpace);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var messagesIdentifier = new MessagesIdentifierMock();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalClientChannel(null, pipeline, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(
                () => new LocalClientChannel(null, pipeline, tupleSpace, null));
            Assert.That(ex.ParamName, Is.EqualTo("messagesIdentifier"));

            ex = Assert.Throws<ArgumentNullException>(
                () => new LocalClientChannel(null, pipeline, tupleSpace, messagesIdentifier));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            channel = new LocalClientChannel(someAddress, pipeline, tupleSpace, messagesIdentifier);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.AreSame(channel.MessagesIdentifier, messagesIdentifier);
        }

        [Test(Description = "Disconnect test")]
        public void DisconnectTest()
        {
            var pipeline = new Pipeline();
            var sink = new SinkMock();
            pipeline.Push(sink);
            const string someAddress = "address114";

            var channel = new LocalClientChannel(someAddress, pipeline);

            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()));
            server.StartListening();

            ChannelRequestCtrl ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);

            DateTime utcNow = DateTime.UtcNow;
            channel.Disconnect();

            Assert.IsNotNull(sink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.DisconnectionRequested, sink.PreviousChannelEvent.EventType);
            Assert.LessOrEqual(utcNow, sink.OnEventUtcDateTime);

            Assert.IsFalse(channel.IsConnected);

            server.StopListening();
        }

        private ISenderChannel GetSenderChannel(IChannelAddress channelAddress)
        {
            var reference = channelAddress as ReferenceChannelAddress;
            if ( reference != null)
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
            var cltPipeline = new Pipeline();
            var cltSink = new SinkMock();
            cltPipeline.Push(cltSink);
            const string someAddress = "address118";
            const string message = "message";
            var cltTupleSpace = new TupleSpace<ReceiveDescriptor>();
            var svrTupleSpace = new TupleSpace<ReceiveDescriptor>();
            const int timeout = 300;
            var messagesIdentifier = new MessagesIdentifierMock();

            var channel = new LocalClientChannel(someAddress, cltPipeline);

            LocalClientChannel ch = channel;
            var ex = Assert.Throws<ArgumentNullException>(() => ch.SendExpectingResponse(null, 0, true, null));
            Assert.That(ex.ParamName, Is.EqualTo("message"));

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => ch.SendExpectingResponse(message, 0, true, null));
            Assert.That(ex2.ParamName, Is.EqualTo("timeout"));

            var ex3 = Assert.Throws<ChannelException>(() => ch.SendExpectingResponse(message, timeout, true, null));
            Assert.That(ex3.Message,
                Is.EqualTo(
                    "Channel isn't configured with a messages identifier, unable to match requests with responses."));

            channel = new LocalClientChannel(someAddress, cltPipeline, cltTupleSpace, messagesIdentifier);

            Assert.IsNull(channel.SendExpectingResponse(message, timeout, true, null));

            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()), svrTupleSpace);
            server.StartListening();

            ChannelRequestCtrl ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);
            server.StopListening();

            // Raise and exception in the client pipeline.
            const string exceptionMessage = "client exception";
            cltSink.ExceptionMessage = exceptionMessage;
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.IsInstanceOf<ChannelException>(ctrl.Error);
            Assert.AreEqual(ctrl.Error.Message, exceptionMessage);
            cltSink.ExceptionMessage = null;

            // SendExpectingResponse test.
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            ReceiveDescriptor receivedMessage = server.TupleSpace.Take(null, 5000);
            Assert.IsNotNull(receivedMessage);

            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreSame(receivedMessage.ReceivedMessage, message);

            // Get timed out request.
            ReceiveDescriptor request = cltTupleSpace.Take(null, 5000);

            Assert.IsNotNull(request as Request);
            Assert.IsTrue(((Request) request).IsExpired);

            // Send two requests with same key.
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            ChannelRequestCtrl ctrl2 = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl2);

            Assert.IsTrue(ctrl2.IsCompleted);
            Assert.IsFalse(ctrl2.Successful);

            Assert.IsNotNull(ctrl2.Message);
            Assert.IsTrue(ctrl2.Message == "There's already a pending request with the same request message key.");

            // Respond request.
            ReceiveDescriptor requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            var sourceChannel = GetSenderChannel(requestMessage.ChannelAddress);
            Assert.IsNotNull(sourceChannel);
            sourceChannel.Send(requestMessage.ReceivedMessage);

            ReceiveDescriptor response = cltTupleSpace.Take(null, 5000);

            Assert.IsNotNull(request as Request);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ReceivedMessage);
            Assert.AreSame(((Request) response).SentMessage, response.ReceivedMessage);

            // Change response message key to prevent matching, return null.
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.Successful);

            requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            sourceChannel = GetSenderChannel(requestMessage.ChannelAddress);
            Assert.IsNotNull(sourceChannel);

            messagesIdentifier.ReturnValue = null;
            sourceChannel.Send(requestMessage.ReceivedMessage);

            // Receive unmatched response
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNull(response as Request);

            // Receive timed out request
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNotNull(response as Request);
            Assert.IsTrue(((Request) response).IsExpired);

            // Try to send a null message key request
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsTrue(ctrl.Message ==
                "The message key generated by the messages identifier is null, can't send.");

            // Do the same but with an invalid key
            messagesIdentifier.ReturnValue = "validKey";
            ctrl = channel.SendExpectingResponse(message, timeout, true, null);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.Successful);

            requestMessage = svrTupleSpace.Take(null, 5000);
            Assert.IsNotNull(requestMessage);
            sourceChannel = GetSenderChannel(requestMessage.ChannelAddress);
            Assert.IsNotNull(sourceChannel);

            messagesIdentifier.ReturnValue = "invalidKey";
            sourceChannel.Send(requestMessage.ReceivedMessage);

            // Receive unmatched response
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNull(response as Request);

            // Receive timed out request
            response = cltTupleSpace.Take(null, 5000);
            Assert.IsNotNull(response as Request);

            Assert.IsNotNull(response);
            Assert.IsTrue(((Request) response).IsExpired);
        }

        [Test(Description = "Send expecting response from peer and getting the request handler")]
        public void SendExpectingResponseNotTupleSpace()
        {
            var cltPipeline = new Pipeline();
            var cltSink = new SinkMock();
            cltPipeline.Push(cltSink);
            const string someAddress = "address117";
            const string message = "message";
            var cltTupleSpace = new TupleSpace<ReceiveDescriptor>();
            var svrTupleSpace = new TupleSpace<ReceiveDescriptor>();
            const int timeout = 300;
            var messagesIdentifier = new MessagesIdentifierMock();

            var channel = new LocalClientChannel(someAddress, cltPipeline);

            LocalClientChannel ch = channel;
            var ex = Assert.Throws<ArgumentNullException>(() => ch.SendExpectingResponse(null, 0, false, null));
            Assert.That(ex.ParamName, Is.EqualTo("message"));

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => ch.SendExpectingResponse(message, 0, false, null));
            Assert.That(ex2.ParamName, Is.EqualTo("timeout"));

            var ex3 = Assert.Throws<ChannelException>(() => ch.SendExpectingResponse(message, timeout, false, null));
            Assert.That(ex3.Message,
                Is.EqualTo(
                    "Channel isn't configured with a messages identifier, unable to match requests with responses."));

            channel = new LocalClientChannel(someAddress, cltPipeline, cltTupleSpace, messagesIdentifier);

            Assert.IsNull(channel.SendExpectingResponse(message, timeout, false, null));

            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()), svrTupleSpace);
            server.StartListening();

            ChannelRequestCtrl ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);
            server.StopListening();

            // Raise and exception in the client pipeline.
            const string exceptionMessage = "client exception";
            cltSink.ExceptionMessage = exceptionMessage;
            SendRequestHandlerCtrl handlerCtrl = channel.SendExpectingResponse(message, timeout, false, null);
            Assert.IsNotNull(handlerCtrl);
            Assert.IsTrue(handlerCtrl.IsCompleted);
            Assert.IsFalse(handlerCtrl.Successful);
            Assert.IsNotNull(handlerCtrl.Error);
            Assert.IsInstanceOf<ChannelException>(handlerCtrl.Error);
            Assert.AreEqual(handlerCtrl.Error.Message, exceptionMessage);
            cltSink.ExceptionMessage = null;

            // SendAndReturnRequestHandler test.
            handlerCtrl = channel.SendExpectingResponse(message, timeout, false, null);
            Assert.IsNotNull(handlerCtrl);

            Assert.IsTrue(handlerCtrl.IsCompleted);
            Assert.IsTrue(handlerCtrl.Successful);

            Assert.IsNotNull(handlerCtrl.Request);

            ReceiveDescriptor receivedMessage = server.TupleSpace.Take(null, 0);
            Assert.IsNotNull(receivedMessage);

            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreSame(receivedMessage.ReceivedMessage, message);

            Assert.IsFalse(handlerCtrl.Request.WaitResponse());

            Assert.IsTrue(handlerCtrl.Request.IsExpired);

            // Timed out synchronous request cannot go to the request tuple space.
            Assert.IsNull(cltTupleSpace.Read(null, 0));
        }

        [Test(Description = "Send test")]
        public void SendTest()
        {
            var cltPipeline = new Pipeline();
            var cltSink = new SinkMock();
            cltPipeline.Push(cltSink);
            const string someAddress = "address116";
            const string message = "message";
            var receiveTupleSpace = new TupleSpace<ReceiveDescriptor>();

            var channel = new LocalClientChannel(someAddress, cltPipeline);

            var ex = Assert.Throws<ArgumentNullException>(() => channel.Send(null));
            Assert.That(ex.ParamName, Is.EqualTo("message"));

            Assert.IsNull(channel.Send(message));

            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()),
                receiveTupleSpace);
            server.StartListening();

            ChannelRequestCtrl ctrl = channel.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(channel.IsConnected);

            // Raise and exception in the client pipeline.
            const string exceptionMessage = "client exception";
            cltSink.ExceptionMessage = exceptionMessage;
            ctrl = channel.Send(message);
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.IsInstanceOf<ChannelException>(ctrl.Error);
            Assert.AreEqual(ctrl.Error.Message, exceptionMessage);
            cltSink.ExceptionMessage = null;

            // Send test.
            ctrl = channel.Send(message);
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsNotNull(cltSink.SendMessage);
            Assert.AreSame(cltSink.SendMessage, message);

            ReceiveDescriptor receivedMessage = server.TupleSpace.Take(null, 0);
            Assert.IsNotNull(receivedMessage);

            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreSame(receivedMessage.ReceivedMessage, message);

            server.StopListening();
        }
    }
}