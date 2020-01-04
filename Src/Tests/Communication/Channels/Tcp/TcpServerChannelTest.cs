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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;

namespace Tests.Trx.Communication.Channels.Tcp
{
    [TestFixture(Description = "Tcp server channel tests.")]
    public class TcpServerChannelTest
    {
        [Test(Description = "Constructor and properties test.")]
        public void ConstructorAndPropertiesTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());

            var ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            var channel = new TcpServerChannel(pipeline, pipelineFactory);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.IsNull(channel.TupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, pipelineFactory, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            channel = new TcpServerChannel(pipeline, pipelineFactory, tupleSpace);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var messagesIdentifier = new MessagesIdentifierMock();

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipelineFactory"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, pipelineFactory, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpServerChannel(pipeline, pipelineFactory, tupleSpace, null));
            Assert.That(ex.ParamName, Is.EqualTo("messagesIdentifier"));

            channel = new TcpServerChannel(pipeline, pipelineFactory, tupleSpace,
                messagesIdentifier);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.PipelineFactory, pipelineFactory);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.AreSame(channel.MessagesIdentifier, messagesIdentifier);

            Assert.IsNotNull(channel.Childs);
            Assert.IsFalse(channel.IsConnected);
            Assert.IsFalse(channel.IsListening);

            Assert.AreEqual( "0.0.0.0", channel.LocalInterface );
            channel.LocalInterface = "127.0.0.1";
            channel.LocalInterface = "127.0.0.1";
            Assert.AreEqual("127.0.0.1", channel.LocalInterface);
            Assert.IsNull( channel.LocalEndPoint );
            
            Assert.AreEqual(AddressFamily.InterNetwork, channel.AddressFamily);
            channel.AddressFamily = AddressFamily.InterNetworkV6;
            Assert.AreEqual(AddressFamily.InterNetworkV6, channel.AddressFamily);

            Assert.AreEqual(0, channel.Port);
            channel.Port = 9999;
            channel.Port = 9999;
            Assert.AreEqual(9999, channel.Port);

            Assert.AreEqual(TcpBaseSenderReceiverChannel.DefaultSendMaxRequestSize, channel.SendMaxRequestSize);
            channel.SendMaxRequestSize = 1024;
            channel.SendMaxRequestSize = 1024;
            Assert.AreEqual(1024, channel.SendMaxRequestSize);
        }

        [Test(Description = "Disconnect test.")]
        public void DisconnectTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());

            new TcpServerChannel(pipeline, pipelineFactory).Disconnect();
        }

        internal static bool IsActiveListener(IPEndPoint ep)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] localEndPoints = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var localEndPoint in localEndPoints)
                if (localEndPoint.Address.Equals( ep.Address ) && localEndPoint.Port == ep.Port)
                    return true;

            return false;
        }

        [Test(Description = "Start/stop listening test.")]
        public void StartStopListeningTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory( new Pipeline() );

            const int testPort = 9999;

            var targetEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), testPort);
            Assert.IsFalse( IsActiveListener( targetEndPoint ) );


            // Invalid local interface
            var server = new TcpServerChannel(pipeline, pipelineFactory) { LocalInterface = null };
            var ctrl = server.StartListening();
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.AreEqual("Invalid local interface: null.", ctrl.Error.Message);

            // Invalid local port
            server = new TcpServerChannel(pipeline, pipelineFactory) { Port = -1 };
            ctrl = server.StartListening();
            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.WaitCompletion(5000, true));
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Error);
            Assert.AreEqual("Invalid port number -1.", ctrl.Error.Message);

            server = new TcpServerChannel(pipeline, pipelineFactory) { Port = testPort };
            ctrl = server.StartListening();
            Assert.IsNotNull( ctrl );
            Assert.IsTrue( ctrl.IsCompleted );
            Assert.IsTrue( ctrl.Successful );
            Assert.IsTrue(server.IsListening);

            var ctrl2 = server.StartListening();

            Assert.AreSame( ctrl, ctrl2 );

            Assert.IsTrue( IsActiveListener( targetEndPoint ) );

            server.StopListening();

            Assert.IsFalse(server.IsListening);

            Assert.IsFalse( IsActiveListener( targetEndPoint ) );
        }

        [Test(Description = "Listen on a given interface.")]
        public void ListenOnAGivenInterfaceTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());

            const int testPort = 7777;

            var targetEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), testPort);
            Assert.IsFalse(IsActiveListener(targetEndPoint));

            var channel = new TcpServerChannel(pipeline, pipelineFactory)
                              {Port = testPort, LocalInterface = "127.0.0.1"};

            var ctrl = channel.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);
            Assert.IsTrue(channel.IsListening);

            var ctrl2 = channel.StartListening();

            Assert.AreSame(ctrl, ctrl2);

            Assert.IsTrue(IsActiveListener(targetEndPoint));

            channel.StopListening();

            Assert.IsFalse(channel.IsListening);

            Assert.IsFalse(IsActiveListener(targetEndPoint));
        }

        [Test(Description = "Close test.")]
        public void CloseTest()
        {
            var pipeline = new Pipeline();
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());

            var channel = new TcpServerChannel(pipeline, pipelineFactory);

            var ctrl = channel.StartListening();

            Assert.IsNotNull(ctrl);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            channel.Close();

            Assert.IsFalse(channel.IsListening);
        }

        private TcpServerChildChannel GetChild(TcpServerChannel server)
        {
            foreach (var child in server.Childs.Values)
                return child as TcpServerChildChannel;

            return null;
        }

        [Test(Description = "Accept test.")]
        public void AcceptTest()
        {
            var pipeline = new Pipeline();
            var serverPipeline = new Pipeline();
            serverPipeline.Push(new SinkMock());
            var pipelineFactory = new ClonePipelineFactory(serverPipeline);

            const int testPort = 8888;

            var server = new TcpServerChannel(pipeline, pipelineFactory) { Port = testPort };

            var svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            Assert.IsTrue(server.Childs.Count == 0);

            var client = new TcpClientChannel(pipeline) { RemotePort = testPort, RemoteInterface = "127.0.0.1" };

            var cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion( 5000, true );
            Assert.IsTrue( cltCtrl.IsCompleted );
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            server.StopListening();

            Assert.IsTrue( server.Childs.Count == 1 );

            TcpServerChildChannel child = GetChild(server);

            Assert.IsNotNull(child);

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

            Thread.Sleep(100); // Give some time to server to complete connection

            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.IsNotNull(sink.PreviousChannelEvent);
            Assert.AreEqual(ChannelEventType.Connected, sink.PreviousChannelEvent.EventType);

            Assert.IsTrue(server.Childs.Count == 0);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            server = new TcpServerChannel(pipeline, pipelineFactory, tupleSpace) { Port = testPort };

            svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 1);

            child = GetChild(server);

            Assert.IsNotNull(child);
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

            Thread.Sleep(100); // Give some time to server to complete disconnection

            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.IsNotNull(sink.PreviousChannelEvent);
            Assert.AreEqual(ChannelEventType.DisconnectionRequested, sink.PreviousChannelEvent.EventType);

            Assert.IsTrue(server.Childs.Count == 0);

            var messagesIdentifier = new MessagesIdentifierMock();

            server = new TcpServerChannel(pipeline, pipelineFactory, tupleSpace, messagesIdentifier) { Port = testPort };

            svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 1);

            child = GetChild(server);

            Assert.IsNotNull(child);
            Assert.IsNotNull(child.Pipeline);
            Assert.AreNotSame(child.Pipeline, pipeline);
            Assert.AreEqual(1, child.Pipeline.Sinks.Count);
            sink = child.Pipeline.Sinks.First.Value as SinkMock;
            Assert.NotNull(sink);
            Assert.AreSame(child.ParentChannel, server);
            Assert.AreSame(child.TupleSpace, tupleSpace);
            Assert.AreSame(child.MessagesIdentifier, messagesIdentifier);

            client.Disconnect();

            Thread.Sleep(100); // Give some time to server to complete connection

            Assert.IsTrue(server.Childs.Count == 0);
        }

        [Test(Description = "Test connection request event firing and handling.")]
        public void ConnectionRequestChannelEventTest()
        {
            var pipeline = new Pipeline();
            var sink = new ConnectionRequestSinkMock {Accept = true};
            pipeline.Push(sink);
            var pipelineFactory = new ClonePipelineFactory(new Pipeline());

            const int testPort = 8899;

            var server = new TcpServerChannel(pipeline, pipelineFactory) { Port = testPort };

            var svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            Assert.IsTrue(server.Childs.Count == 0);

            var client = new TcpClientChannel(pipeline) { RemotePort = testPort, RemoteInterface = "127.0.0.1" };

            var cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            var evt = sink.ChannelEvent as SocketConnectionRequestChannelEvent;
            Assert.IsNotNull(evt);

            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 1);

            var child = GetChild(server);

            Assert.IsNotNull(child);

            Assert.IsNotNull(child.Pipeline);
            Assert.AreNotSame(child.Pipeline, pipeline);
            Assert.AreSame(child.ParentChannel, server);
            Assert.IsNull(child.TupleSpace);
            Assert.IsNull(child.MessagesIdentifier);

            client.Disconnect();

            Thread.Sleep(100); // Give some time to server to complete connection

            Assert.IsTrue(server.Childs.Count == 0);

            // Test not accepting the connection

            sink.Reset();

            sink.Accept = false;

            svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            evt = sink.ChannelEvent as SocketConnectionRequestChannelEvent;
            Assert.IsNotNull(evt);

            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 0);

            Assert.IsFalse(client.IsConnected);

            // Test raising an exception in the accept pipeline

            sink.Reset();

            sink.ExceptionMessage = "Exception as requested";

            svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);

            Thread.Sleep(100); // Give some time to server to complete connection

            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 0);

            Assert.IsFalse(client.IsConnected);
        }
    }
}
