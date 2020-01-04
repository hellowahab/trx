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

using System.Threading;
using NUnit.Framework;
using Trx.Coordination.TupleSpace;
using Trx.Utilities;

namespace Tests.Trx.Coordination.TupleSpace
{
    [TestFixture(Description = "Heap queue context tests.")]
    public class HeapQueueContextTest
    {
        private const string SyncObj = "_sync_";

        [Test(Description = "Simple write/take test.")]
        public void SimpleWriteReadTest()
        {
            var hqc = new HeapQueueContext<string>();

            // Check if empty
            string obj = hqc.Read(null, 0);
            Assert.IsNull(obj);

            hqc.Write("data", Timeout.Infinite);
            obj = hqc.Read(null, 0);
            Assert.IsNotNull(obj);
            Assert.True(obj == "data");

            // Check if empty
            obj = hqc.Read(null, 0);
            Assert.IsNotNull(obj);
        }

        [Test(Description = "Simple write/take test.")]
        public void SimpleWriteTakeTest()
        {
            var hqc = new HeapQueueContext<string>();

            // Check if empty
            string obj = hqc.Read(null, 0);
            Assert.IsNull(obj);

            hqc.Write("data", Timeout.Infinite);
            obj = hqc.Take(null, 0);
            Assert.IsNotNull(obj);
            Assert.True(obj == "data");

            // Check if empty
            obj = hqc.Read(null, 0);
            Assert.IsNull(obj);
        }

        [Test(Description = "Test queued item expiration.")]
        public void ItemExpirationTest()
        {
            var hqc = new HeapQueueContext<string>();

            hqc.Write("data", 5);
            Thread.Sleep(20);
            string obj = hqc.Read(null, 0);
            Assert.IsNull(obj);

            hqc.Write("data", 100);
            Thread.Sleep(20);
            obj = hqc.Take(null, 0);
            Assert.IsNotNull(obj);
        }

        [Test(Description = "Read/take timeout.")]
        public void ReadTakeTimeoutTest()
        {
            var hqc = new HeapQueueContext<string>();

            var pt = new PerformanceTimer();
            pt.Start();
            const int timeout = 20;
            string obj = hqc.Read(null, timeout);
            double elapsed = pt.IntervalInMilliseconds();
            Assert.IsTrue(elapsed > timeout);
            Assert.IsNull(obj);

            pt.Start();
            obj = hqc.Take(null, timeout);
            elapsed = pt.IntervalInMilliseconds();
            Assert.IsTrue(elapsed > timeout);
            Assert.IsNull(obj);
        }

        private static void SimpleProducerThread(object state)
        {
            var hqc = state as HeapQueueContext<string>;
            Assert.IsNotNull(hqc);

            lock (SyncObj)
            {
                Monitor.PulseAll(SyncObj);
            }

            Thread.Sleep(20);

            hqc.Write("data1", Timeout.Infinite);

            hqc.Write("data2", Timeout.Infinite);
        }

        [Test(Description = "Simple producer/consumer test.")]
        public void SimpleProducerConsumerTest()
        {
            var hqc = new HeapQueueContext<string>();
            var producer = new Thread(SimpleProducerThread);

            lock (SyncObj)
            {
                producer.Start(hqc);
                Monitor.Wait(SyncObj);
            }
            
            var pt = new PerformanceTimer();
            const int timeout = 1000;

            pt.Start();
            string obj = hqc.Take(null, timeout);
            double elapsed = pt.IntervalInMilliseconds();
            
            Assert.IsTrue(elapsed > 20);
            Assert.IsTrue(elapsed < timeout);
            Assert.IsNotNull(obj);
            Assert.True(obj == "data1");

            obj = hqc.Read(null, 5000);

            Assert.IsNotNull(obj);
            Assert.True(obj == "data2");

            // Check if not empty
            obj = hqc.Read(null, 0);
            Assert.IsNotNull(obj);
            Assert.True(obj == "data2");
        }
    }
}
