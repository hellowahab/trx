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
    /// Test fixture for StringEncoder.
    /// </summary>
    [TestFixture(Description = "String encoder tests.")]
    public class StringEncoderTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _encoder = StringEncoder.GetInstance();
            Assert.IsNotNull(_encoder);
            _data = "Sample data";
            _binaryData = Encoding.Default.GetBytes(_data);
        }
        #endregion

        private StringEncoder _encoder;
        private string _data;
        private byte[] _binaryData;

        /// <summary>
        /// Test Decode method.
        /// </summary>
        [Test(Description = "Test Decode method.")]
        public void Decode()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_binaryData);

            string decodedData = _encoder.Decode(ref parserContext,
                parserContext.DataLength);

            Assert.IsTrue(!string.IsNullOrEmpty(decodedData));
            Assert.IsTrue(_data.Equals(decodedData));
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

            for (int i = _binaryData.Length - 1; i >= 0; i--)
                Assert.IsTrue(_binaryData[i] == encodedData[i]);
        }

        /// <summary>
        /// Test GetEncodedLength method.
        /// </summary>
        [Test(Description = "Test GetEncodedLength method.")]
        public void GetEncodedLength()
        {
            Assert.IsTrue(_encoder.GetEncodedLength(0) == 0);
            Assert.IsTrue(_encoder.GetEncodedLength(5) == 5);
            Assert.IsFalse(_encoder.GetEncodedLength(3) == 7);
        }

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            Assert.IsTrue(_encoder == StringEncoder.GetInstance());
        }
    }
}