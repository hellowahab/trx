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
    [TestFixture(Description = "Local server channel tests.")]
    public class LocalServerChannelTest
    {
        [Test(Description = "Constructor and properties test.")]
        public void ConstructorAndPropertiesTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address123";

            var ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            var channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.IsNull(channel.TupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory, tupleSpace));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory, tupleSpace);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var messagesIdentifier = new MessagesIdentifierMock();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory,
                tupleSpace, null));
            Assert.That(ex.ParamName, Is.EqualTo("messagesIdentifier"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChannel(null, pipeline, pipelineFactory,
                tupleSpace, messagesIdentifier));
            Assert.That(ex.ParamName, Is.EqualTo("address"));

            channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory, tupleSpace,
                messagesIdentifier);

            Assert.AreSame(channel.Address, someAddress);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.AreSame(channel.MessagesIdentifier, messagesIdentifier);

            Assert.IsNotNull(channel.Childs);
            Assert.IsFalse(channel.IsConnected);
            Assert.IsFalse(channel.IsListening);
        }

        [Test(Description = "Disconnect test.")]
        public void DisconnectTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address124";

            new LocalServerChannel(someAddress, pipeline, pipelineFactory).Disconnect();
        }

        [Test(Description = "Start listening test.")]
        public void StartListeningTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address125";

            var channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            var ctrl = channel.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            var ctrl2 = channel.StartListening();

            Assert.AreSame(ctrl, ctrl2);

            var channel2 = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            ctrl = channel2.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);

            channel.StopListening();
        }

        [Test(Description = "Stop listening test.")]
        public void StopListeningTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address126";

            var channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            var ctrl = channel.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            channel.StopListening();

            Assert.IsFalse(channel.IsListening);
        }

        [Test(Description = "Close test.")]
        public void CloseTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address127";

            var channel = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            var ctrl = channel.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            channel.Close();

            Assert.IsFalse(channel.IsListening);

            channel.StopListening();
        }

        private LocalServerChildChannel GetChild(LocalServerChannel server)
        {
            foreach (var child in server.Childs.Values)
                return child as LocalServerChildChannel;

            return null;
        }

        [Test(Description = "Accept test.")]
        public void AcceptTest()
        {
            var pipeline = new Pipeline();
            var clientPipeline = new Pipeline();
            clientPipeline.Push(new SinkMock());
            var pipelineFactory = new ClonePipelineFactory(clientPipeline);
            const string someAddress = "address128";

            var server = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            var client = new LocalClientChannel(someAddress, pipeline);
            Assert.IsNull(server.Accept(client));

            server.StartListening();

            client = new LocalClientChannel(someAddress, pipeline);
            client.Connect();
            Assert.AreEqual(1, server.Childs.Count);
            var child = GetChild(server);
            Assert.IsNotNull(child);

            server.StopListening();

            Assert.AreSame(child.Peer, client);
            Assert.IsNotNull(child.Pipeline);
            Assert.AreNotSame(child.Pipeline, pipeline);
            Assert.AreEqual(1, child.Pipeline.Sinks.Count);
            var sink = child.Pipeline.Sinks.First.Value as SinkMock;
            Assert.NotNull(sink);
            Assert.AreSame(child.ParentChannel, server);
            Assert.IsNull(child.TupleSpace);
            Assert.IsNull(child.MessagesIdentifier);

            Assert.AreEqual(ChannelEventType.Connected, sink.ChannelEvent.EventType);
            Assert.IsNull(sink.PreviousChannelEvent);

            client.Disconnect();

            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.IsNotNull(sink.PreviousChannelEvent);
            Assert.AreEqual(ChannelEventType.Connected, sink.PreviousChannelEvent.EventType);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            server = new LocalServerChannel(someAddress, pipeline, pipelineFactory, tupleSpace);

            server.StartListening();

            client = new LocalClientChannel(someAddress, pipeline);
            client.Connect();
            Assert.AreEqual(1, server.Childs.Count);
            child = GetChild(server);
            Assert.IsNotNull(child);
            
            server.StopListening();

            Assert.AreSame(child.Peer, client);
            Assert.IsNotNull(child.Pipeline);
            Assert.AreNotSame(child.Pipeline, pipeline);
            Assert.AreEqual(1, child.Pipeline.Sinks.Count);
            sink = child.Pipeline.Sinks.First.Value as SinkMock;
            Assert.NotNull(sink);
            Assert.AreSame(child.ParentChannel, server);
            Assert.AreSame(child.TupleSpace, tupleSpace);
            Assert.IsNull(child.MessagesIdentifier);

            Assert.AreEqual(ChannelEventType.Connected, sink.ChannelEvent.EventType);
            Assert.IsNull(sink.PreviousChannelEvent);

            child.Disconnect();

            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.IsNotNull(sink.PreviousChannelEvent);
            Assert.AreEqual(ChannelEventType.DisconnectionRequested, sink.PreviousChannelEvent.EventType);

            var messagesIdentifier = new MessagesIdentifierMock();

            server = new LocalServerChannel(someAddress, pipeline, pipelineFactory, tupleSpace,
                messagesIdentifier);

            server.StartListening();

            client = new LocalClientChannel(someAddress, pipeline);
            child = server.Accept(client);
            Assert.IsNotNull(child);
            Assert.IsInstanceOf<LocalServerChildChannel>(child);
            server.StopListening();

            Assert.AreSame(child.Peer, client);
            Assert.IsNotNull(child.Pipeline);
            Assert.AreNotSame(child.Pipeline, pipeline);
            Assert.AreSame(child.ParentChannel, server);
            Assert.AreSame(child.TupleSpace, tupleSpace);
            Assert.AreSame(child.MessagesIdentifier, messagesIdentifier);
        }

        [Test(Description = "Test connection request event firing and handling.")]
        public void ConnectionRequestChannelEventTest()
        {
            var pipeline = new Pipeline();
            var sink = new ConnectionRequestSinkMock { Accept = true };
            pipeline.Push(sink);
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());
            const string someAddress = "address128";

            var server = new LocalServerChannel(someAddress, pipeline, pipelineFactory);

            server.StartListening();

            var client = new LocalClientChannel(someAddress, pipeline);
            var ctrl = client.Connect();

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            var evt = sink.ChannelEvent as ConnectionRequestChannelEvent;
            Assert.IsNotNull(evt);

            Assert.IsTrue(server.Childs.Count == 1);

            client.Disconnect();

            Assert.IsTrue(server.Childs.Count == 0);

            server.StopListening();

            // Test not accepting the connection

            sink.Reset();

            sink.Accept = false;

            server.StartListening();

            client = new LocalClientChannel(someAddress, pipeline);
            ctrl = client.Connect();

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);

            evt = sink.ChannelEvent as ConnectionRequestChannelEvent;
            Assert.IsNotNull(evt);

            Assert.IsTrue(server.Childs.Count == 0);

            server.StopListening();
        }
    }
}
