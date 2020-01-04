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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Trx.Communication.Channels;

namespace Tests.Trx.Communication.Channels
{
    [TestFixture(Description = "Channel exception tests.")]
    public class ChannelExceptionTest
    {
        #region Methods
        [Test(Description = "Constructors and properties test.")]
        public void ConstructorsAndPropertiesTest()
        {
            var ex = new ChannelException();

            Assert.IsNull(ex.InnerException);

            const string msg = "exception";
            ex = new ChannelException(msg);

            Assert.IsNotNull(ex.Message);
            Assert.IsTrue(ex.Message == msg);
            Assert.IsNull(ex.InnerException);

            const string msg2 = "exception2";
            var ie = new ApplicationException(msg2);
            ex = new ChannelException(ie);

            Assert.IsNotNull(ex.Message);
            Assert.IsTrue(ex.Message == msg2);
            Assert.IsNotNull(ex.InnerException);
            Assert.AreSame(ex.InnerException, ie);

            ex = new ChannelException(msg, ie);

            Assert.IsNotNull(ex.Message);
            Assert.IsTrue(ex.Message == msg);
            Assert.IsNotNull(ex.InnerException);
            Assert.AreSame(ex.InnerException, ie);

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, ex);

            ms.Seek(0, SeekOrigin.Begin);
            var obj = bf.Deserialize(ms);
            Assert.IsNotNull(obj);

            var ex2 = obj as ChannelException;
            Assert.IsNotNull(ex2);

            Assert.IsNotNull(ex2.Message);
            Assert.IsTrue(ex2.Message == msg);
            Assert.IsNotNull(ex2.InnerException);
            Assert.IsInstanceOf<ApplicationException>(ex.InnerException);
        }
        #endregion
    }
}
