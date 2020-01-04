﻿#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
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
    [TestFixture(Description = "Channel event tests.")]
    public class ChannelEventTest
    {
        #region Methods
        [Test(Description = "Constructors and properties test.")]
        public void ConstructorsAndPropertiesTest()
        {
            var utcNow = DateTime.UtcNow;
            var evt = new ChannelEvent(ChannelEventType.Connected);

            Assert.IsTrue(evt.EventType == ChannelEventType.Connected);
            Assert.LessOrEqual(utcNow, evt.UtcEventDateTime);

            utcNow = DateTime.UtcNow;
            evt = new ChannelEvent(ChannelEventType.Other);

            Assert.IsTrue(evt.EventType == ChannelEventType.Other);
            Assert.LessOrEqual(utcNow, evt.UtcEventDateTime);
        }
        #endregion
    }
}
