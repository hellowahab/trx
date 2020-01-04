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
using Trx.Utilities;

namespace Tests.Trx.Messaging
{
    /// <summary>
    /// Test fixture for HexDataEncoder.
    /// </summary>
    [TestFixture(Description = "Hexadecimal encoder tests.")]
    public class HexDataEncoderTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _encoder = HexDataEncoder.GetInstance();
            _data1 = Encoding.Default.GetBytes("123456789");
            _encodedData1 = Encoding.Default.GetBytes("313233343536373839");
            _data2 = new byte[] {0xFE, 0xDC, 0xBA, 0x09, 0x87, 0x65, 0x43, 0x21, 0x0};
            _encodedData2 = Encoding.Default.GetBytes("FEDCBA098765432100");
            Assert.IsNotNull(_encoder);
        }
        #endregion

        private HexDataEncoder _encoder;
        private byte[] _data1;
        private byte[] _encodedData1;
        private byte[] _data2;
        private byte[] _encodedData2;

        /// <summary>
        /// Decode bytes.
        /// </summary>
        [Test(Description = "Decode bytes.")]
        public void DecodeBytes()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_encodedData1);

            byte[] decodedData = _encoder.DecodeBytes(ref parserContext, _data1.Length);

            Assert.IsNotNull(decodedData);
            Assert.IsTrue(decodedData.Length == _data1.Length);
            for (int i = _data1.Length - 1; i >= 0; i--)
                Assert.IsTrue(decodedData[i] == _data1[i]);

            parserContext.Write(_encodedData2);

            decodedData = _encoder.DecodeBytes(ref parserContext, _data2.Length);

            Assert.IsNotNull(decodedData);
            Assert.IsTrue(decodedData.Length == _data2.Length);
            for (int i = _data2.Length - 1; i >= 0; i--)
                Assert.IsTrue(decodedData[i] == _data2[i]);
        }

        /// <summary>
        /// Decode string.
        /// </summary>
        [Test(Description = "Decode string.")]
        public void DecodeString()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(_encodedData1);

            string decodedData = _encoder.DecodeString(ref parserContext, _data1.Length);

            Assert.IsNotNull(decodedData);
            Assert.IsTrue(decodedData.Length == _data1.Length);
            for (int i = _data1.Length - 1; i >= 0; i--)
                Assert.IsTrue(decodedData[i] == _data1[i]);

            parserContext.Write(_encodedData2);

            decodedData = _encoder.DecodeString(ref parserContext, _data2.Length);
            Assert.IsNotNull(decodedData);
            var data = parserContext.Buffer.Encoding.GetBytes(decodedData);

            Assert.IsTrue(decodedData.Length == _data2.Length);
            for (int i = _data2.Length - 1; i >= 0; i--)
                Assert.IsTrue(data[i] == _data2[i]);
        }

        /// <summary>
        /// Encode bytes.
        /// </summary>
        [Test(Description = "Encode bytes.")]
        public void EncodeBytes()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            _encoder.Encode(_data1, ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _encodedData1.Length);

            byte[] encodedData = formatterContext.GetData();

            for (int i = _encodedData1.Length - 1; i >= 0; i--)
                Assert.IsTrue(_encodedData1[i] == encodedData[i]);

            formatterContext.Clear();

            _encoder.Encode(_data2, ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _encodedData2.Length);

            encodedData = formatterContext.GetData();

            for (int i = _encodedData2.Length - 1; i >= 0; i--)
                Assert.IsTrue(_encodedData2[i] == encodedData[i]);
        }

        /// <summary>
        /// Encode String.
        /// </summary>
        [Test(Description = "Encode string.")]
        public void EncodeString()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            _encoder.Encode(Encoding.Default.GetString(_data1), ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _encodedData1.Length);

            byte[] encodedData = formatterContext.GetData();

            for (int i = _encodedData1.Length - 1; i >= 0; i--)
                Assert.IsTrue(_encodedData1[i] == encodedData[i]);

            formatterContext.Clear();

            _encoder.Encode(Encoding.UTF7.GetString(_data2), ref formatterContext);

            Assert.IsTrue(formatterContext.DataLength == _encodedData2.Length);

            encodedData = formatterContext.GetData();

            for (int i = _encodedData2.Length - 1; i >= 0; i--)
                Assert.IsTrue(_encodedData2[i] == encodedData[i]);
        }

        /// <summary>
        /// Test GetEncodedLength method.
        /// </summary>
        [Test(Description = "Test GetEncodedLength method.")]
        public void GetEncodedLength()
        {
            Assert.IsTrue(_encoder.GetEncodedLength(0) == 0);
            Assert.IsTrue(_encoder.GetEncodedLength(10) == 20);
            Assert.IsFalse(_encoder.GetEncodedLength(8) == 4);
        }

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            Assert.IsTrue(_encoder == HexDataEncoder.GetInstance());
        }
    }
}