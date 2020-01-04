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

using System.Text;
using NUnit.Framework;
using Trx.Messaging;

namespace Tests.Trx.Messaging
{
    /// <summary>
    /// Test fixture for BinaryEncoder.
    /// </summary>
    [TestFixture(Description = "Binary encoder tests.")]
    public class BinaryEncoderTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _encoder = BinaryEncoder.GetInstance();
            _data = Encoding.UTF7.GetBytes("Sample data");
            Assert.IsNotNull(_encoder);
        }
        #endregion

        private BinaryEncoder _encoder;
        private byte[] _data;

        /// <summary>
        /// Test Decode method.
        /// </summary>
        [Test(Description = "Test Decode method.")]
        public void Decode()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_data);

            byte[] decodedData = _encoder.Decode(ref parserContext, parserContext.DataLength);

            Assert.IsNotNull(decodedData);
            Assert.IsTrue(decodedData.Length == _data.Length);
            for (int i = _data.Length - 1; i >= 0; i--)
                Assert.IsTrue(decodedData[i] == _data[i]);
        }

        /// <summary>
        /// Test Encode method.
        /// </summary>
        [Test(Description = "Test Encode method.")]
        public void Encode()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            _encoder.Encode(_data, ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _data.Length);

            byte[] encodedData = formatterContext.GetData();

            for (int i = _data.Length - 1; i >= 0; i--)
                Assert.IsTrue(_data[i] == encodedData[i]);
        }

        /// <summary>
        /// Test GetEncodedLength method.
        /// </summary>
        [Test(Description = "Test GetEncodedLength method.")]
        public void GetEncodedLength()
        {
            Assert.IsTrue(_encoder.GetEncodedLength(0) == 0);
            Assert.IsTrue(_encoder.GetEncodedLength(10) == 10);
            Assert.IsFalse(_encoder.GetEncodedLength(8) == 4);
        }

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            Assert.IsTrue(_encoder == BinaryEncoder.GetInstance());
        }
    }
}