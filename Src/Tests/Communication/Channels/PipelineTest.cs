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

namespace Tests.Trx.Communication.Channels
{
    [TestFixture(Description = "Receive descriptor tests.")]
    public class PipelineTest
    {
        #region Methods
        [Test(Description = "ProcessChannelEvent test.")]
        public void ProcessChannelEventTest()
        {
            var context = new PipelineContext(null);

            var pipeline = new Pipeline();

            var sink1 = new SinkMock();
            pipeline.Push(sink1);

            var sink2 = new SinkMock();
            pipeline.Push(sink2);

            var evt = new ChannelEvent(ChannelEventType.Other);
            pipeline.ProcessChannelEvent(context, evt, false, null);

            Assert.IsTrue(DateTime.MinValue == sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue == sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink2.OnEventUtcDateTime);

            Assert.Less(sink1.OnEventUtcDateTime, sink2.OnEventUtcDateTime);

            Assert.IsNotNull(sink1.ChannelEvent);
            Assert.AreSame(sink1.ChannelEvent, evt);

            Assert.IsNotNull(sink2.ChannelEvent);
            Assert.AreSame(sink2.ChannelEvent, evt);

            // First sink will stop event propagation.
            sink1.Reset();
            sink2.Reset();

            sink1.OnEventReturnValue = false;
            
            evt = new ChannelEvent(ChannelEventType.Other);
            pipeline.ProcessChannelEvent(context, evt, false, null);

            Assert.IsTrue(DateTime.MinValue == sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue == sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.OnEventUtcDateTime);

            Assert.IsNotNull(sink1.ChannelEvent);
            Assert.AreSame(sink1.ChannelEvent, evt);

            Assert.IsNull(sink2.ChannelEvent);
        }

        [Test(Description = "Send test.")]
        public void SendTest()
        {
            var context = new PipelineContext(null);

            var pipeline = new Pipeline();

            var sink1 = new SinkMock();
            pipeline.Push(sink1);

            var sink2 = new SinkMock();
            pipeline.Push(sink2);

            const string message = "message";
            context.MessageToSend = message;
            pipeline.Send(context);

            Assert.IsTrue(DateTime.MinValue != sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue != sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.OnEventUtcDateTime);

            Assert.Greater(sink1.SendUtcDateTime, sink2.SendUtcDateTime);

            Assert.IsNotNull(sink1.SendMessage);
            Assert.AreSame(sink1.SendMessage, message);

            Assert.IsNotNull(sink2.SendMessage);
            Assert.AreSame(sink2.SendMessage, message);

        }

        [Test(Description = "Receive test.")]
        public void ReceiveTest()
        {
            var context = new PipelineContext(null);

            var pipeline = new Pipeline();

            var sink1 = new SinkMock();
            pipeline.Push(sink1);

            var sink2 = new SinkMock();
            pipeline.Push(sink2);

            context.ReceivedMessage = "message";
            Assert.IsTrue(pipeline.Receive(context));

            Assert.IsTrue(DateTime.MinValue == sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue == sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.OnEventUtcDateTime);

            Assert.Less(sink1.ReceiveUtcDateTime, sink2.ReceiveUtcDateTime);

            Assert.AreSame(context.ReceivedMessage, sink1.ReceiveMessage);

            Assert.AreSame(context.ReceivedMessage, sink2.ReceiveMessage);

            // Now try partial receive in sinks.
            sink1.Reset();
            sink2.Reset();

            sink1.ReceiveReturnValue = false;
            sink2.ReceiveReturnValue = false;

            Assert.IsFalse(pipeline.Receive(context));
            sink1.ReceiveReturnValue = true;

            Assert.IsTrue(DateTime.MinValue == sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue == sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.OnEventUtcDateTime);

            Assert.IsFalse(pipeline.Receive(context));
            sink2.ReceiveReturnValue = true;

            Assert.IsTrue(DateTime.MinValue == sink1.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink1.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink1.OnEventUtcDateTime);

            Assert.IsTrue(DateTime.MinValue == sink2.SendUtcDateTime);
            Assert.IsTrue(DateTime.MinValue != sink2.ReceiveUtcDateTime);
            Assert.IsTrue(DateTime.MinValue == sink2.OnEventUtcDateTime);

            Assert.IsTrue(pipeline.Receive(context));

            Assert.Less(sink1.ReceiveUtcDateTime, sink2.ReceiveUtcDateTime);
        }
        #endregion
    }
}
