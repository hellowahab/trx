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
using System.Threading;
using NUnit.Framework;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Local;

namespace Tests.Trx.Communication.Channels
{
    [TestFixture(Description = "Request tests.")]
    public class RequestTest
    {
        #region Methods
        [Test(Description = "Constructor and properties test.")]
        public void ConstructorTest()
        {
            const string message = "message";
            const int timeout = 1000;
            const string key = "key";
            var channel = new LocalClientChannel("local", new Pipeline());

            var utcNow = DateTime.UtcNow;
            var request = new Request(message, timeout, true, key, channel.ChannelAddress);

            Assert.IsNotNull(request.SentMessage);
            Assert.AreSame(request.SentMessage, message);

            Assert.IsNotNull(request.RequestMessageKey);
            Assert.AreSame(request.RequestMessageKey, key);

            Assert.IsNotNull(request.ChannelAddress);
            Assert.AreSame(request.ChannelAddress, channel.ChannelAddress);

            Assert.LessOrEqual(utcNow, request.UtcRequestDateTime);

            Assert.IsTrue(request.SendToTupleSpace);

            Assert.AreEqual(request.Timeout, timeout);

            Assert.IsNull(request.ReceivedMessage);
            Assert.AreEqual(request.UtcReceiveDateTime, DateTime.MinValue);

            Assert.IsFalse(request.IsCancelled);
            Assert.AreEqual(request.UtcCancellationDateTime, DateTime.MinValue);

            Assert.IsFalse(request.IsExpired);
            Assert.AreEqual(request.UtcExpirationDateTime, DateTime.MinValue);

            try
            {
                new Request(message, 0, true, key, channel.ChannelAddress);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsTrue(ex.ParamName == "timeout");
                return;
            }

            Assert.Fail("Unreachable point :(");
        }

        private const string Response = "response";

        [Test(Description = "Timeout test.")]
        public void TimeoutTest()
        {
            const string message = "message";
            const int timeout = 30;
            const string key = "key";
            var channel = new LocalClientChannel("local", new Pipeline());

            var utcNow = DateTime.UtcNow;
            var request = new Request(message, timeout, true, key, channel.ChannelAddress);
            request.StartTimer();
            Assert.IsFalse(request.WaitResponse());

            Assert.IsNull(request.ReceivedMessage);
            Assert.AreEqual(request.UtcReceiveDateTime, DateTime.MinValue);

            Assert.IsTrue(request.IsExpired);
            Assert.LessOrEqual(utcNow, request.UtcExpirationDateTime);

            Assert.IsFalse(request.SetResponseMessage(Response));

            request.StartTimer();

            // Try again, not blocking call expected.
            Assert.IsFalse(request.WaitResponse());

            Assert.IsTrue(request.IsExpired);
        }

        private static void CancelRequest(object state)
        {
            var request = state as Request;
            Assert.IsNotNull(request);
            Assert.IsTrue(request.Cancel());
        }

        [Test(Description = "Cancellation test.")]
        public void CancellationTest()
        {
            const string message = "message";
            const int timeout = 1000;
            const string key = "key";
            var channel = new LocalClientChannel("local", new Pipeline());

            var utcNow = DateTime.UtcNow;
            var request = new Request(message, timeout, true, key, channel.ChannelAddress);
            request.StartTimer();
            ThreadPool.QueueUserWorkItem(CancelRequest, request);
            Assert.IsFalse(request.WaitResponse());

            Assert.IsNull(request.ReceivedMessage);
            Assert.AreEqual(request.UtcReceiveDateTime, DateTime.MinValue);

            Assert.IsFalse(request.IsExpired);

            Assert.IsTrue(request.IsCancelled);
            Assert.LessOrEqual(utcNow, request.UtcCancellationDateTime);

            Assert.IsTrue(request.Cancel());

            Assert.IsFalse(request.SetResponseMessage(Response));

            // Try again, not blocking call expected.
            Assert.IsFalse(request.WaitResponse());

            Assert.IsTrue(request.IsCancelled);
        }

        private static void SetResponse(object state)
        {
            var request = state as Request;
            Assert.IsNotNull(request);
            request.SetResponseMessage(Response);
        }

        [Test(Description = "Set response test.")]
        public void SetResponseTest()
        {
            const string message = "message";
            const int timeout = 1000;
            const string key = "key";
            var channel = new LocalClientChannel("local", new Pipeline());

            var utcNow = DateTime.UtcNow;
            var request = new Request(message, timeout, true, key, channel.ChannelAddress);
            request.StartTimer();
            ThreadPool.QueueUserWorkItem(SetResponse, request);
            Assert.IsTrue(request.WaitResponse());

            Assert.IsNotNull(request.ReceivedMessage);
            Assert.AreSame(request.ReceivedMessage, Response);
            Assert.LessOrEqual(utcNow, request.UtcReceiveDateTime);

            Assert.IsFalse(request.IsExpired);
            Assert.IsFalse(request.IsCancelled);

            // Try again, not blocking call expected.
            Assert.IsTrue(request.WaitResponse());
        }
        #endregion
    }
}
