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

namespace Tests.Trx.Communication.Channels
{
    [TestFixture(Description = "Receive descriptor tests.")]
    public class ReceiveDescriptorTest
    {
        #region Methods
        [Test(Description = "Properties test.")]
        public void PropertiesTest()
        {
            var channel = new LocalClientChannel("test", new Pipeline());
            const string message = "message";

            var utcNow = DateTime.UtcNow;
            var rd = new ReceiveDescriptor(channel.ChannelAddress, message);

            Assert.IsNotNull(rd.UtcReceiveDateTime);
            Assert.IsNotNull(rd.ChannelAddress);
            Assert.IsNotNull(rd.ChannelAddress);

            Assert.LessOrEqual(utcNow, rd.UtcReceiveDateTime);
            Assert.LessOrEqual(rd.UtcReceiveDateTime, DateTime.UtcNow);

            Assert.AreSame(rd.ChannelAddress, channel.ChannelAddress);
            Assert.AreSame(rd.ReceivedMessage, message);
        }
        #endregion
    }
}
