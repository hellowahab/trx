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
using NUnit.Framework;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;

namespace Tests.Trx.Communication.Channels.Tcp
{
    [TestFixture(Description = "Tcp client channel tests.")]
    public class TcpClientChannelTest
    {
        [Test(Description = "Constructor and properties test.")]
        public void ConstructorAndPropertiesTest()
        {
            var pipeline = new Pipeline();

            var ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            var channel = new TcpClientChannel(pipeline);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.IsNull(channel.TupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(pipeline, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            channel = new TcpClientChannel(pipeline, tupleSpace);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var messagesIdentifier = new MessagesIdentifierMock();

            ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(pipeline, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(() => new TcpClientChannel(pipeline, tupleSpace, null));
            Assert.That(ex.ParamName, Is.EqualTo("messagesIdentifier"));

            channel = new TcpClientChannel(pipeline, tupleSpace, messagesIdentifier);

            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.AreSame(channel.MessagesIdentifier, messagesIdentifier);

            Assert.IsFalse(channel.IsConnected);

            Assert.AreEqual("0.0.0.0", channel.LocalInterface);
            channel.LocalInterface = "127.0.0.1";
            channel.LocalInterface = "127.0.0.1";
            Assert.AreEqual("127.0.0.1", channel.LocalInterface);
            Assert.IsNull(channel.LocalEndPoint);

            Assert.AreEqual(0, channel.LocalPort);
            channel.LocalPort = 8834;
            channel.LocalPort = 8834;
            Assert.AreEqual(8834, channel.LocalPort);

            Assert.IsNull(channel.RemoteInterface);
            channel.RemoteInterface = "127.0.0.1";
            channel.RemoteInterface = "127.0.0.1";
            Assert.AreEqual("127.0.0.1", channel.RemoteInterface);
            Assert.IsNull(channel.RemoteEndPoint);

            Assert.AreEqual(0, channel.RemotePort);
            channel.RemotePort = 9944;
            channel.RemotePort = 9944;
            Assert.AreEqual(9944, channel.RemotePort);

            Assert.AreEqual(AddressFamily.InterNetwork, channel.AddressFamily);
            channel.AddressFamily = AddressFamily.InterNetworkV6;
            Assert.AreEqual(AddressFamily.InterNetworkV6, channel.AddressFamily);
        }

        [Test(Description = "Connect/disconnect test.")]
        public void ConnectDisconnectTest()
        {
            var pipeline = new Pipeline();
            var sink = new SinkMock();
            pipeline.Push(sink);

            int testPort = 8833;

            var server = new TcpServerChannel(new Pipeline(), new ClonePipelineFactory(new Pipeline())) { Port = testPort };

            var svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            Assert.IsTrue(server.Childs.Count == 0);

            // Invalid local interface
            var client = new TcpClientChannel(pipeline) { LocalInterface = null };
            var cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Error);
            Assert.AreEqual( "Invalid local interface: null.", cltCtrl.Error.Message );

            // Invalid local port
            client = new TcpClientChannel(pipeline) { LocalPort = -1};
            cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Error);
            Assert.AreEqual("Invalid local port number -1.", cltCtrl.Error.Message);

            // Invalid remote interface
            client = new TcpClientChannel(pipeline) { RemoteInterface = null };
            cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Error);
            Assert.AreEqual("Invalid remote interface: null.", cltCtrl.Error.Message);

            // Invalid remote port
            client = new TcpClientChannel(pipeline) { RemotePort = -1, RemoteInterface = "127.0.0.1" };
            cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Error);
            Assert.AreEqual("Invalid remote port number -1.", cltCtrl.Error.Message);

            // Address family mismatch
            client.LocalEndPoint = new IPEndPoint( IPAddress.Any, 20000 );
            client.RemoteEndPoint = new IPEndPoint( IPAddress.IPv6Any, 20001 );
            cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Error);
            Assert.AreEqual("Channel: incompatible local address family type InterNetwork with " +
                "remote address family type InterNetworkV6, please specify the same type " +
                "in both end points.", cltCtrl.Error.Message);

            client = new TcpClientChannel(pipeline) { RemotePort = testPort, RemoteInterface = "127.0.0.1" };
            cltCtrl = client.Connect();
            Assert.IsNotNull(cltCtrl);
            Assert.AreSame( cltCtrl, client.Connect() );
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);
            Assert.IsTrue(client.IsConnected);
            Assert.AreSame(cltCtrl, client.Connect());

            Assert.IsNotNull(sink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.Connected, sink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.ConnectionRequested, sink.PreviousChannelEvent.EventType);

            Assert.IsNotNull(client.LocalEndPoint);
            Assert.IsNotNull(client.RemoteEndPoint);

            Assert.AreEqual(testPort, client.RemoteEndPoint.Port);
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), client.RemoteEndPoint.Address);

            var cltCtrl2 = client.Connect();

            Assert.AreSame(cltCtrl, cltCtrl2);

            sink.Reset();

            client.Disconnect();

            Assert.IsNotNull(sink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.Disconnected, sink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.DisconnectionRequested, sink.PreviousChannelEvent.EventType);

            Assert.IsFalse(client.IsConnected);

            server.StopListening();

            testPort = 8855;

            client.RemotePort = testPort;
            server.Port = testPort;

            svrCtrl = server.StartListening();

            Assert.IsNotNull(svrCtrl);
            Assert.IsTrue(svrCtrl.IsCompleted);
            Assert.IsTrue(svrCtrl.Successful);

            var random = new Random(DateTime.UtcNow.Millisecond*(DateTime.UtcNow.Second + 1));
            var clientLocalPort = random.Next(1024, 8000);
            client.LocalPort = clientLocalPort;

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            cltCtrl.WaitCompletion(5000, true);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsTrue(cltCtrl.Successful);
            Assert.IsTrue(client.IsConnected);

            Assert.IsNotNull(client.LocalEndPoint);
            Assert.IsNotNull(client.RemoteEndPoint);

            Assert.AreEqual(testPort, client.RemoteEndPoint.Port);
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), client.RemoteEndPoint.Address);
            Assert.AreEqual(clientLocalPort, client.LocalEndPoint.Port);

            client.Disconnect();

            server.StopListening();

            // Test connection failure

            client.LocalPort = 0;

            cltCtrl = client.Connect();

            Assert.IsNotNull(cltCtrl);
            Assert.IsTrue(cltCtrl.WaitCompletion(5000, true));
            Assert.IsFalse(cltCtrl.IsCancelled);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsFalse(client.IsConnected);

            Assert.IsNotNull(sink.ChannelEvent);
            Assert.AreEqual(ChannelEventType.ConnectionFailed, sink.ChannelEvent.EventType);
            Assert.AreEqual(ChannelEventType.ConnectionRequested, sink.PreviousChannelEvent.EventType);

            client.Disconnect();

            client = new TcpClientChannel(pipeline, new TupleSpace<ReceiveDescriptor>(), new MessagesIdentifierMock()) { RemotePort = testPort, RemoteInterface = "127.0.0.1" };

            const string msg = "The channel is not connected";

            cltCtrl = client.Send(string.Empty);
            Assert.IsTrue(cltCtrl.IsCompleted);
            Assert.IsFalse(cltCtrl.Successful);
            Assert.IsNotNull(cltCtrl.Message);
            Assert.AreSame(msg, cltCtrl.Message);
            var ctrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Message);
            Assert.AreSame(msg, ctrl.Message);
            ctrl = client.SendExpectingResponse(string.Empty, 100, false, null);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsNotNull(ctrl.Message);
            Assert.AreSame(msg, ctrl.Message);
        }
    }
}
