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

using Trx.Messaging;
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for HexadecimalBinaryEncoder.
	/// </summary>
	[TestFixture( Description="Hexadecimal binary encoder tests.")]
	public class HexadecimalBinaryEncoderTest {

		private HexadecimalBinaryEncoder _encoder;
		private byte[] _data1;
		private byte[] _encodedData1;
		private byte[] _data2;
		private byte[] _encodedData2;

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="HexadecimalBinaryEncoderTest"/>.
		/// </summary>
		public HexadecimalBinaryEncoderTest() {

		}
		#endregion

		#region Methods
		/// <summary>
		/// This method will be called by NUnit for test setup.
		/// </summary>
		[SetUp]
		public void SetUp() {

			_encoder = HexadecimalBinaryEncoder.GetInstance();
			_data1 = Encoding.UTF7.GetBytes( "123456789");
			_encodedData1 = Encoding.UTF7.GetBytes( "313233343536373839");
			_data2 = new byte[] { 0xFE, 0xDC, 0xBA, 0x09, 0x87, 0x65, 0x43, 0x21, 0x0};
			_encodedData2 = Encoding.UTF7.GetBytes( "FEDCBA098765432100");
			Assert.IsNotNull( _encoder);
		}

		/// <summary>
		/// Test GetInstance method.
		/// </summary>
		[Test( Description="Test GetInstance method.")]
		public void GetInstance() {

			Assert.IsTrue( _encoder == HexadecimalBinaryEncoder.GetInstance());
		}

		/// <summary>
		/// Test GetEncodedLength method.
		/// </summary>
		[Test( Description="Test GetEncodedLength method.")]
		public void GetEncodedLength() {

			Assert.IsTrue( _encoder.GetEncodedLength( 0) == 0);
			Assert.IsTrue( _encoder.GetEncodedLength( 10) == 20);
			Assert.IsFalse( _encoder.GetEncodedLength( 8) == 4);
		}

		/// <summary>
		/// Test Encode method.
		/// </summary>
		[Test( Description="Test Encode method.")]
		public void Encode() {

			FormatterContext formatterContext =
				new FormatterContext( FormatterContext.DefaultBufferSize);

			_encoder.Encode( _data1, ref formatterContext);

			Assert.IsTrue( formatterContext.DataLength == _encodedData1.Length);

			byte[] encodedData = formatterContext.GetData();

			for ( int i = _encodedData1.Length - 1; i >= 0; i--) {
				Assert.IsTrue( _encodedData1[i] == encodedData[i]);
			}

			formatterContext.Clear();

			_encoder.Encode( _data2, ref formatterContext);

			Assert.IsTrue( formatterContext.DataLength == _encodedData2.Length);

			encodedData = formatterContext.GetData();

			for ( int i = _encodedData2.Length - 1; i >= 0; i--) {
				Assert.IsTrue( _encodedData2[i] == encodedData[i]);
			}
		}

		/// <summary>
		/// Test Decode method.
		/// </summary>
		[Test( Description="Test Decode method.")]
		public void Decode() {

			ParserContext parserContext =
				new ParserContext( ParserContext.DefaultBufferSize);

			parserContext.Write( _encodedData1);

			byte[] decodedData = _encoder.Decode( ref parserContext,
				_data1.Length);

			Assert.IsNotNull( decodedData);
			Assert.IsTrue( decodedData.Length == _data1.Length);
			for ( int i = _data1.Length - 1; i >= 0; i--) {
				Assert.IsTrue( decodedData[i] == _data1[i]);
			}

			parserContext.Write( _encodedData2);

			decodedData = _encoder.Decode( ref parserContext,
				_data2.Length);

			Assert.IsNotNull( decodedData);
			Assert.IsTrue( decodedData.Length == _data2.Length);
			for ( int i = _data2.Length - 1; i >= 0; i--) {
				Assert.IsTrue( decodedData[i] == _data2[i]);
			}
		}
		#endregion
	}
}
