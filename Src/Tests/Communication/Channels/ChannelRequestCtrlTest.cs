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
using Trx.Utilities;

namespace Tests.Trx.Communication.Channels
{
    [TestFixture(Description = "Receive descriptor tests.")]
    public class ChannelRequestCtrlTest
    {
        #region Methods
        [Test(Description = "Parameterless constructor and properties test.")]
        public void ParameterlessConstructorTest()
        {
            var utcNow = DateTime.UtcNow;
            var ctrl = new ChannelRequestCtrl();

            Assert.LessOrEqual(utcNow, ctrl.UtcRequestDateTime);
            Assert.LessOrEqual(ctrl.UtcRequestDateTime, DateTime.UtcNow);

            Assert.AreEqual(ctrl.UtcCompletionDateTime, DateTime.MinValue);
            Assert.AreEqual(ctrl.UtcCancellationDateTime, DateTime.MinValue);

            Assert.IsFalse(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);

            Assert.IsNull(ctrl.Message);
            Assert.IsNull(ctrl.Error);
        }

        private void SuccessfulConstructor(bool successful)
        {
            var utcNow = DateTime.UtcNow;
            var ctrl = new ChannelRequestCtrl(successful);

            Assert.LessOrEqual(utcNow, ctrl.UtcRequestDateTime);
            Assert.LessOrEqual(ctrl.UtcRequestDateTime, DateTime.UtcNow);

            Assert.LessOrEqual(utcNow, ctrl.UtcCompletionDateTime);
            Assert.LessOrEqual(ctrl.UtcCompletionDateTime, DateTime.UtcNow);

            Assert.AreEqual(ctrl.UtcCancellationDateTime, DateTime.MinValue);

            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsTrue(ctrl.Successful == successful);

            Assert.IsNull(ctrl.Message);
            Assert.IsNull(ctrl.Error);
        }

        [Test(Description = "Successful constructor and properties test.")]
        public void SuccessfulConstructorTest()
        {
            SuccessfulConstructor(true);
            SuccessfulConstructor(false);
        }

        [Test(Description = "Properties test.")]
        public void PropertiesTest()
        {
            var ex = new ApplicationException();
            const string message = "message";
            var ctrl = new ChannelRequestCtrl(true)
                           {
                               Error = ex,
                               Message = message
                           };

            Assert.IsNotNull(ctrl.Error);
            Assert.AreSame(ctrl.Error, ex);

            Assert.IsNotNull(ctrl.Message);
            Assert.AreSame(ctrl.Message, message);
        }

        [Test(Description = "Successful constructor and properties test.")]
        public void CancelTest()
        {
            var ctrl = new ChannelRequestCtrl();

            var utcNow = DateTime.UtcNow;
            ctrl.Cancel();

            Assert.LessOrEqual(utcNow, ctrl.UtcCancellationDateTime);
            Assert.LessOrEqual(ctrl.UtcCancellationDateTime, DateTime.UtcNow);

            Assert.IsFalse(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);

            // Cancel again.
            var utcCancellationDateTime = ctrl.UtcCancellationDateTime;
            Thread.Sleep(20);
            ctrl.Cancel();
            Assert.IsTrue(ctrl.IsCancelled);
            Assert.AreEqual(utcCancellationDateTime, ctrl.UtcCancellationDateTime);

            // Try to cancel a successful request.
            ctrl = new ChannelRequestCtrl(true);
            ctrl.Cancel();
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsTrue(ctrl.Successful);
        }

        [Test(Description = "Wait completion test.")]
        public void WaitCompletionTest()
        {
            // Wait on a completed request.
            var ctrl = new ChannelRequestCtrl(true);
            Assert.IsTrue(ctrl.WaitCompletion(Timeout.Infinite, false));
            ctrl = new ChannelRequestCtrl(false);
            Assert.IsTrue(ctrl.WaitCompletion(Timeout.Infinite, false));

            // Wait on a cancelled request.
            ctrl = new ChannelRequestCtrl();
            ctrl.Cancel();
            Assert.IsFalse(ctrl.WaitCompletion(Timeout.Infinite, false));

            // Wait with a timeout not auto-cancelling.
            const int waitMs = 20;
            ctrl = new ChannelRequestCtrl();
            var pt = new PerformanceTimer();
            ctrl.WaitCompletion(waitMs, false);
            Assert.LessOrEqual(waitMs, pt.IntervalInMilliseconds());
            Assert.IsFalse(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);

            // Wait with a timeout auto-cancelling.
            ctrl = new ChannelRequestCtrl();
            pt.Start();
            ctrl.WaitCompletion(waitMs, true);
            Assert.LessOrEqual(waitMs, pt.IntervalInMilliseconds());
            Assert.IsFalse(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.IsCancelled);
            Assert.IsFalse(ctrl.Successful);
        }

        private readonly object _lockObj = new object();

        private void MarkAsCompletedThread(object state)
        {
            var ctrl = state as ChannelRequestCtrl;
            Assert.IsNotNull(ctrl);

            lock (_lockObj)
                Monitor.PulseAll(_lockObj);

            Thread.Sleep(20);
            ctrl.MarkAsCompleted(true);
        }

        [Test(Description = "Test two threads, one awaiting on completion and another setting it completed.")]
        public void TwoThreadsWaitCompletionTest()
        {
            var ctrl = new ChannelRequestCtrl();

            var t = new Thread(MarkAsCompletedThread);
            lock (_lockObj)
            {
                t.Start(ctrl);
                Monitor.Wait(_lockObj);
            }

            var pt = new PerformanceTimer();
            Assert.IsTrue(ctrl.WaitCompletion(1000, false));
            Assert.Less(pt.IntervalInMilliseconds(), 1000);
        }

        [Test(Description = "Mark as completed test.")]
        public void MarkAsCompletedTest()
        {
            // Try to complete an already completed request.
            var ctrl = new ChannelRequestCtrl(true);
            Thread.Sleep(20);
            var utcCompletionDateTime = ctrl.UtcCompletionDateTime;
            ctrl.MarkAsCompleted(false);
            Assert.AreEqual(utcCompletionDateTime, ctrl.UtcCompletionDateTime);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);

            // Try to complete a cancelled request.
            ctrl = new ChannelRequestCtrl();
            ctrl.Cancel();
            ctrl.MarkAsCompleted(true);
            Assert.IsFalse(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsTrue(ctrl.IsCancelled);

            // Complete a successful request.
            ctrl = new ChannelRequestCtrl();
            var utcNow = DateTime.UtcNow;
            ctrl.MarkAsCompleted(true);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsTrue(ctrl.Successful);
            Assert.IsFalse(ctrl.IsCancelled);
            Assert.LessOrEqual(utcNow, ctrl.UtcCompletionDateTime);
            Assert.LessOrEqual(ctrl.UtcCompletionDateTime, DateTime.UtcNow);

            // Complete an unsuccessful request.
            ctrl = new ChannelRequestCtrl();
            ctrl.MarkAsCompleted(false);
            Assert.IsTrue(ctrl.IsCompleted);
            Assert.IsFalse(ctrl.Successful);
            Assert.IsFalse(ctrl.IsCancelled);
        }
        #endregion
    }
}
