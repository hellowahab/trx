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
    [TestFixture(Description = "Local server child channel tests.")]
    public class LocalServerChildChannelTest
    {
        #region Methods
        [Test(Description = "Constructor and properties test.")]
        public void ConstructorAndPropertiesTest()
        {
            const string someAddress = "address221";
            var client = new LocalClientChannel(someAddress, new Pipeline());
            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()));
            var pipeline = new Pipeline();

            var ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChildChannel(null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            var channel = new LocalServerChildChannel(client, pipeline, server);

            Assert.AreSame(channel.Peer, client);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.ParentChannel, server);
            Assert.IsNull(channel.TupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var tupleSpace = new TupleSpace<ReceiveDescriptor>();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChildChannel(null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChildChannel(null, pipeline, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            channel = new LocalServerChildChannel(client, pipeline, server, tupleSpace);

            Assert.AreSame(channel.Peer, client);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.ParentChannel, server);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.IsNull(channel.MessagesIdentifier);

            var messagesIdentifier = new MessagesIdentifierMock();

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChildChannel(null, null, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("pipeline"));

            ex = Assert.Throws<ArgumentNullException>(() => new LocalServerChildChannel(null, pipeline, null, null, null));
            Assert.That(ex.ParamName, Is.EqualTo("tupleSpace"));

            ex = Assert.Throws<ArgumentNullException>(
                () => new LocalServerChildChannel(null, pipeline, null, tupleSpace, null));
            Assert.That(ex.ParamName, Is.EqualTo("messagesIdentifier"));

            channel = new LocalServerChildChannel(client, pipeline, server, tupleSpace, messagesIdentifier);

            Assert.AreSame(channel.Peer, client);
            Assert.AreSame(channel.Pipeline, pipeline);
            Assert.AreSame(channel.ParentChannel, server);
            Assert.AreSame(channel.TupleSpace, tupleSpace);
            Assert.AreSame(channel.MessagesIdentifier, messagesIdentifier);
        }

        private LocalServerChildChannel GetChild(LocalServerChannel server)
        {
            foreach (var child in server.Childs.Values)
                return child as LocalServerChildChannel;

            return null;
        }

        [Test(Description = "Cancel child pending requests on disconnect")]
        public void CancelPendingRequestsOnDisconnectionTest()
        {
            var pipeline = new Pipeline();
            const string someAddress = "address222";
            const string message = "message";
            var cltTupleSpace = new TupleSpace<ReceiveDescriptor>();
            var svrTupleSpace = new TupleSpace<ReceiveDescriptor>();
            const int timeout = 1000;
            var messagesIdentifier = new MessagesIdentifierMock();

            var client = new LocalClientChannel(someAddress, pipeline, cltTupleSpace);
            var server = new LocalServerChannel(someAddress, new Pipeline(), new ClonePipelineFactory(new Pipeline()), svrTupleSpace, messagesIdentifier);
            server.StartListening();

            Assert.IsTrue(server.Childs.Count == 0);

            var ctrl = client.Connect();
            Assert.IsNotNull(ctrl);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            Assert.IsTrue(client.IsConnected);
            server.StopListening();

            Assert.IsTrue(server.Childs.Count == 1);

            var child = GetChild(server);
            Assert.IsNotNull(child);

            var ctrl2 = child.SendExpectingResponse(message, timeout, false, null);
            Assert.IsNotNull(ctrl2);

            Assert.IsTrue(ctrl2.IsCompleted);
            Assert.IsTrue(ctrl2.Successful);

            var receivedMessage = client.TupleSpace.Take(null, 0);
            Assert.IsNotNull(receivedMessage);

            Assert.IsNotNull(receivedMessage.ReceivedMessage);
            Assert.AreSame(receivedMessage.ReceivedMessage, message);

            child.Disconnect();

            Assert.IsFalse(child.IsConnected);
            Assert.IsFalse(client.IsConnected);

            Assert.IsTrue(ctrl2.Request.IsCancelled);
        }
        #endregion
    }
}
