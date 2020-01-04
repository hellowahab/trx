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
    /// Test fixture for DataEncoder.
    /// </summary>
    [TestFixture(Description = "Data encoder tests.")]
    public class DataEncoderTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _encoder = DataEncoder.GetInstance();
            Assert.IsNotNull(_encoder);
            _data = "Sample data";
            _binaryData = Encoding.Default.GetBytes(_data);
        }
        #endregion

        private DataEncoder _encoder;
        private string _data;
        private byte[] _binaryData;

        /// <summary>
        /// Decode string.
        /// </summary>
        [Test(Description = "Decode string.")]
        public void DecodeString()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_binaryData);

            string decodedData = _encoder.DecodeString(ref parserContext,
                parserContext.DataLength);

            Assert.IsTrue(!string.IsNullOrEmpty(decodedData));
            Assert.IsTrue(_data.Equals(decodedData));
        }

        /// <summary>
        /// Encode string.
        /// </summary>
        [Test(Description = "Encode string.")]
        public void EncodeString()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            _encoder.Encode(_data, ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _data.Length);

            byte[] encodedData = formatterContext.GetData();

            for (int i = _binaryData.Length - 1; i >= 0; i--)
                Assert.IsTrue(_binaryData[i] == encodedData[i]);
        }

        /// <summary>
        /// Decode bytes.
        /// </summary>
        [Test(Description = "Decode bytes.")]
        public void DecodeBytes()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_data);

            byte[] decodedData = _encoder.DecodeBytes(ref parserContext, parserContext.DataLength);

            Assert.IsNotNull(decodedData);
            Assert.IsTrue(decodedData.Length == _data.Length);
            for (int i = _data.Length - 1; i >= 0; i--)
                Assert.IsTrue(decodedData[i] == _data[i]);
        }

        /// <summary>
        /// Encode bytes.
        /// </summary>
        [Test(Description = "Encode bytes.")]
        public void EncodeBytes()
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
            Assert.IsTrue(_encoder.GetEncodedLength(5) == 5);
            Assert.IsFalse(_encoder.GetEncodedLength(3) == 7);
        }

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            Assert.IsTrue(_encoder == DataEncoder.GetInstance());
        }
    }
}