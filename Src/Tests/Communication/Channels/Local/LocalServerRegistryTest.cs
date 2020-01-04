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

using NUnit.Framework;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Local;

namespace Tests.Trx.Communication.Channels.Local
{
    [TestFixture(Description = "Local server registry tests.")]
    public class LocalServerRegistryTest
    {
        #region Methods
        [Test(Description = "GetInstance test.")]
        public void GetInstanceTest()
        {
            var registry = LocalServerRegistry.GetInstance();

            Assert.IsNotNull(registry);
            Assert.AreSame(registry, LocalServerRegistry.GetInstance());
        }

        [Test(Description = "Register test.")]
        public void RegisterTest()
        {
            var registry = LocalServerRegistry.GetInstance();

            registry.Unregister("address");

            Assert.IsTrue(registry.Register("address", null));
            Assert.IsFalse(registry.Register("address", null));
        }

        [Test(Description = "Unregister test.")]
        public void UnregisterTest()
        {
            var registry = LocalServerRegistry.GetInstance();

            registry.Unregister("address");

            Assert.IsTrue(registry.Register("address", null));
            registry.Unregister("address");
            Assert.IsTrue(registry.Register("address", null));
        }

        [Test(Description = "Connect test.")]
        public void ConnectTest()
        {
            var registry = LocalServerRegistry.GetInstance();

            registry.Unregister("address");

            // Local server self register in the registry.
            var server = new LocalServerChannel("address", new Pipeline(), new ClonePipelineFactory(new Pipeline()));
            server.StartListening();

            // It's registered?
            Assert.IsFalse(registry.Register("address", server));

            var child = registry.Connect("address", new LocalClientChannel("address", new Pipeline()));

            Assert.IsNotNull(child);

            Assert.IsNull(registry.Connect("invalidAddress", new LocalClientChannel("address", new Pipeline())));
        }
        #endregion
    }
}
