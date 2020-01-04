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
using System.Text;
using NUnit.Framework;
using Trx.Messaging;

namespace Tests.Trx.Messaging
{
    /// <summary>
    /// Test fixture for BcdDataEncoder.
    /// </summary>
    [TestFixture(Description = "BCD encoder tests.")]
    public class BcdDataEncoderTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
        }
        #endregion

        /// <summary>
        /// Decode bytes.
        /// </summary>
        [Test(Description = "Decode bytes.")]
        public void DecodeBytes()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            BcdDataEncoder encoder = BcdDataEncoder.GetInstance(true, 0xF);
            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45}, 0, 3);
            Assert.IsTrue(Encoding.UTF7.GetString(encoder.DecodeBytes(ref parserContext, 5)).Equals("12D45"));
            Assert.IsTrue(parserContext.DataLength == 0);

            encoder = BcdDataEncoder.GetInstance(false, 0);
            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45, 0x20, 0x20}, 0, 5);
            string data = Encoding.UTF7.GetString(encoder.DecodeBytes(ref parserContext, 5));
            Assert.IsTrue(data.Equals("F12D4"));
            Assert.IsTrue(parserContext.DataLength == 2);
            parserContext.Clear();

            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45}, 0, 3);
            Assert.IsTrue(Encoding.UTF7.GetString(encoder.DecodeBytes(ref parserContext, 6)).Equals("F12D45"));
            Assert.IsTrue(parserContext.DataLength == 0);

            try
            {
                encoder.DecodeBytes(ref parserContext, 6);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.ParamName.Equals("length"));
            }
        }

        /// <summary>
        /// Decode string.
        /// </summary>
        [Test(Description = "Decode string.")]
        public void DecodeString()
        {
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            BcdDataEncoder encoder = BcdDataEncoder.GetInstance(true, 0xF);
            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45}, 0, 3);
            Assert.IsTrue(encoder.DecodeString(ref parserContext, 5).Equals("12D45"));
            Assert.IsTrue(parserContext.DataLength == 0);

            encoder = BcdDataEncoder.GetInstance(false, 0);
            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45, 0x20, 0x20}, 0, 5);
            string data = encoder.DecodeString(ref parserContext, 5);
            Assert.IsTrue(data.Equals("F12D4"));
            Assert.IsTrue(parserContext.DataLength == 2);
            parserContext.Clear();

            parserContext.Write(new byte[] {0xF1, 0x2D, 0x45}, 0, 3);
            Assert.IsTrue(encoder.DecodeString(ref parserContext, 6).Equals("F12D45"));
            Assert.IsTrue(parserContext.DataLength == 0);

            try
            {
                encoder.DecodeString(ref parserContext, 6);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.ParamName.Equals("length"));
            }
        }

        /// <summary>
        /// Encode bytes.
        /// </summary>
        [Test(Description = "Encode bytes.")]
        public void EncodeBytes()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            BcdDataEncoder encoder = BcdDataEncoder.GetInstance(true, 0xF);
            encoder.Encode(Encoding.UTF7.GetBytes("12D45"), ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0xF1, 0x2D, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(false, 0xE);
            encoder.Encode(Encoding.UTF7.GetBytes("12D45"), ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0xD4, 0x5E})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 4);
            encoder.Encode(Encoding.UTF7.GetBytes("12a45"), ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x41, 0x2a, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 0);
            encoder.Encode(Encoding.UTF7.GetBytes("1245"), ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 0xf);
            encoder.Encode(Encoding.UTF7.GetBytes("1245"), ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0x45})));
        }

        /// <summary>
        /// Encode string.
        /// </summary>
        [Test(Description = "Encode string.")]
        public void EncodeString()
        {
            var formatterContext = new FormatterContext(FormatterContext.DefaultBufferSize);

            BcdDataEncoder encoder = BcdDataEncoder.GetInstance(true, 0xF);
            encoder.Encode("12D45", ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0xF1, 0x2D, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(false, 0xE);
            encoder.Encode("12D45", ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0xD4, 0x5E})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 4);
            encoder.Encode("12a45", ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x41, 0x2a, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 0);
            encoder.Encode("1245", ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0x45})));

            formatterContext.Clear();

            encoder = BcdDataEncoder.GetInstance(true, 0xf);
            encoder.Encode("1245", ref formatterContext);
            Assert.IsTrue(formatterContext.GetDataAsString().Equals(
                Encoding.UTF7.GetString(new byte[] {0x12, 0x45})));
        }

        /// <summary>
        /// Test GetEncodedLength method.
        /// </summary>
        [Test(Description = "Test GetEncodedLength method.")]
        public void GetEncodedLength()
        {
            for (int i = 0; i < 16; i++)
            {
                BcdDataEncoder encoder = BcdDataEncoder.GetInstance(false, (byte) i);

                Assert.IsTrue(encoder.GetEncodedLength(0) == 0);
                Assert.IsTrue(encoder.GetEncodedLength(1) == 1);
                Assert.IsTrue(encoder.GetEncodedLength(2) == 1);
                Assert.IsTrue(encoder.GetEncodedLength(10) == 5);
                Assert.IsTrue(encoder.GetEncodedLength(11) == 6);
                Assert.IsFalse(encoder.GetEncodedLength(8) == 5);

                encoder = BcdDataEncoder.GetInstance(true, (byte) i);

                Assert.IsTrue(encoder.GetEncodedLength(0) == 0);
                Assert.IsTrue(encoder.GetEncodedLength(1) == 1);
                Assert.IsTrue(encoder.GetEncodedLength(2) == 1);
                Assert.IsTrue(encoder.GetEncodedLength(10) == 5);
                Assert.IsTrue(encoder.GetEncodedLength(11) == 6);
                Assert.IsFalse(encoder.GetEncodedLength(8) == 5);
            }
        }

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            for (int i = 0; i < 16; i++)
            {
                BcdDataEncoder encoder = BcdDataEncoder.GetInstance(false, (byte) i);
                Assert.IsNotNull(encoder);
                Assert.IsFalse(encoder.LeftPadded);
                Assert.IsTrue(encoder.Pad == (byte) i);
                Assert.IsTrue(encoder == BcdDataEncoder.GetInstance(false, (byte) i));

                encoder = BcdDataEncoder.GetInstance(true, (byte) i);
                Assert.IsNotNull(encoder);
                Assert.IsTrue(encoder.LeftPadded);
                Assert.IsTrue(encoder.Pad == (byte) i);
                Assert.IsTrue(encoder == BcdDataEncoder.GetInstance(true, (byte) i));
            }
        }
    }
}