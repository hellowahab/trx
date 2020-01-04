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

using Trx.Messaging;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for BcdStringEncoder.
	/// </summary>
	[TestFixture( Description="BCD encoder tests.")]
	public class BcdStringEncoderTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="BcdStringEncoderTest"/>.
		/// </summary>
		public BcdStringEncoderTest() {

		}
		#endregion

		#region Methods
		/// <summary>
		/// This method will be called by NUnit for test setup.
		/// </summary>
		[SetUp]
		public void SetUp() {

		}

		/// <summary>
		/// Test GetInstance method.
		/// </summary>
		[Test( Description="Test GetInstance method.")]
		public void GetInstance() {

			BcdStringEncoder encoder;

			for ( int i = 0; i < 16; i++) {
				encoder = BcdStringEncoder.GetInstance( false, ( byte)i);
				Assert.IsNotNull( encoder);
				Assert.IsFalse( encoder.LeftPadded);
				Assert.IsTrue( encoder.Pad == ( byte)i);
				Assert.IsTrue( encoder == BcdStringEncoder.GetInstance( false, ( byte)i));

				encoder = BcdStringEncoder.GetInstance( true, ( byte)i);
				Assert.IsNotNull( encoder);
				Assert.IsTrue( encoder.LeftPadded);
				Assert.IsTrue( encoder.Pad == ( byte)i);
				Assert.IsTrue( encoder == BcdStringEncoder.GetInstance( true, ( byte)i));
			}
		}

		/// <summary>
		/// Test GetEncodedLength method.
		/// </summary>
		[Test( Description="Test GetEncodedLength method.")]
		public void GetEncodedLength() {

			BcdStringEncoder encoder;

			for ( int i = 0; i < 16; i++) {
				encoder = BcdStringEncoder.GetInstance( false, ( byte)i);

				Assert.IsTrue( encoder.GetEncodedLength( 0) == 0);
				Assert.IsTrue( encoder.GetEncodedLength( 1) == 1);
				Assert.IsTrue( encoder.GetEncodedLength( 2) == 1);
				Assert.IsTrue( encoder.GetEncodedLength( 10) == 5);
				Assert.IsTrue( encoder.GetEncodedLength( 11) == 6);
				Assert.IsFalse( encoder.GetEncodedLength( 8) == 5);

				encoder = BcdStringEncoder.GetInstance( true, ( byte)i);

				Assert.IsTrue( encoder.GetEncodedLength( 0) == 0);
				Assert.IsTrue( encoder.GetEncodedLength( 1) == 1);
				Assert.IsTrue( encoder.GetEncodedLength( 2) == 1);
				Assert.IsTrue( encoder.GetEncodedLength( 10) == 5);
				Assert.IsTrue( encoder.GetEncodedLength( 11) == 6);
				Assert.IsFalse( encoder.GetEncodedLength( 8) == 5);
			}
		}

		/// <summary>
		/// Test Encode method.
		/// </summary>
		[Test( Description="Test Encode method.")]
		public void Encode() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			BcdStringEncoder encoder;

			encoder = BcdStringEncoder.GetInstance( true, 0xF);
			encoder.Encode( "12D45", ref formatterContext);
			Assert.IsTrue( formatterContext.GetDataAsString().Equals(
				Encoding.UTF7.GetString( new byte[] { 0xF1, 0x2D, 0x45})));

			formatterContext.Clear();

			encoder = BcdStringEncoder.GetInstance( false, 0xE);
			encoder.Encode( "12D45", ref formatterContext);
			Assert.IsTrue( formatterContext.GetDataAsString().Equals(
				Encoding.UTF7.GetString( new byte[] { 0x12, 0xD4, 0x5E})));

			formatterContext.Clear();

			encoder = BcdStringEncoder.GetInstance( true, 4);
			encoder.Encode( "12a45", ref formatterContext);
			Assert.IsTrue( formatterContext.GetDataAsString().Equals(
				Encoding.UTF7.GetString( new byte[] { 0x41, 0x2a, 0x45})));

			formatterContext.Clear();

			encoder = BcdStringEncoder.GetInstance( true, 0);
			encoder.Encode( "1245", ref formatterContext);
			Assert.IsTrue( formatterContext.GetDataAsString().Equals(
				Encoding.UTF7.GetString( new byte[] { 0x12, 0x45})));

			formatterContext.Clear();

			encoder = BcdStringEncoder.GetInstance( true, 0xf);
			encoder.Encode( "1245", ref formatterContext);
			Assert.IsTrue( formatterContext.GetDataAsString().Equals(
				Encoding.UTF7.GetString( new byte[] { 0x12, 0x45})));
		}

		/// <summary>
		/// Test Decode method.
		/// </summary>
		[Test( Description="Test Decode method.")]
		public void Decode() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			BcdStringEncoder encoder;

			encoder = BcdStringEncoder.GetInstance( true, 0xF);
			parserContext.Write( new byte[] { 0xF1, 0x2D, 0x45}, 0, 3);
			Assert.IsTrue( encoder.Decode( ref parserContext, 5).Equals( "12D45"));
			Assert.IsTrue( parserContext.DataLength == 0);

			encoder = BcdStringEncoder.GetInstance( false, 0);
			parserContext.Write( new byte[] { 0xF1, 0x2D, 0x45, 0x20, 0x20}, 0, 5);
			string data = encoder.Decode( ref parserContext, 5);
			Assert.IsTrue( data.Equals( "F12D4"));
			Assert.IsTrue( parserContext.DataLength == 2);
			parserContext.Clear();

			parserContext.Write( new byte[] { 0xF1, 0x2D, 0x45}, 0, 3);
			Assert.IsTrue( encoder.Decode( ref parserContext, 6).Equals( "F12D45"));
			Assert.IsTrue( parserContext.DataLength == 0);

			try {
				encoder.Decode( ref parserContext, 6);
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName.Equals( "length"));
			}
		}
		#endregion
	}
}
