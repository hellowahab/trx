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

using Trx.Messaging;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for BcdLengthEncoder.
	/// </summary>
	[TestFixture( Description="BCD length encoder tests.")]
	public class BcdLengthEncoderTest {

		#region Constructors
		public BcdLengthEncoderTest() {

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

			BcdLengthEncoder encoder;
			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = BcdLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder == BcdLengthEncoder.GetInstance( sizes[i]));
			}

			try {
				encoder = BcdLengthEncoder.GetInstance( 1000000);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "maximumLength");
			}
		}

		/// <summary>
		/// Test MaximumLength property.
		/// </summary>
		[Test( Description="Test MaximumLength property.")]
		public void MaximumLength() {

			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};
			int[] expectedSizes = { 99, 99, 9999, 9999, 999999, 999999};
			BcdLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = BcdLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder.MaximumLength == expectedSizes[i]);
			}
		}

		/// <summary>
		/// Test GetEncodedLength method.
		/// </summary>
		[Test( Description="Test GetEncodedLength method.")]
		public void GetEncodedLength() {

			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};
			BcdLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = BcdLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder.EncodedLength == ( ( i / 2) + 1));
			}
		}

		/// <summary>
		/// Test Encode method.
		/// </summary>
		[Test( Description="Test Encode method.")]
		public void Encode() {

			FormatterContext formatterContext =
				new FormatterContext( FormatterContext.DefaultBufferSize);
			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};
			byte[][] expectedData = {
				new byte[] { 3},
				new byte[] { 0x15},
				new byte[] { 9, 0x99},
				new byte[] { 0x45, 0x10}, 
				new byte[] { 1, 0x63, 0x84}, 
				new byte[] { 0x68, 0x11, 0x15}
			};

			BcdLengthEncoder encoder;
			byte[] data;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = BcdLengthEncoder.GetInstance( sizes[i]);
				encoder.Encode( sizes[i], ref formatterContext);
				data = formatterContext.GetData();
				Assert.IsTrue( data.Length == expectedData[i].Length);
				for ( int j = expectedData[i].Length - 1; j >= 0; j--) {
					Assert.IsTrue( data[j] == expectedData[i][j]);
				}
				formatterContext.Clear();
			}

			encoder = BcdLengthEncoder.GetInstance( 99);
			try {
				encoder.Encode( 100, ref formatterContext);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "length");
			}
		}

		/// <summary>
		/// Test Decode method.
		/// </summary>
		[Test( Description="Test Decode method.")]
		public void Decode() {

			ParserContext parserContext =
				new ParserContext( ParserContext.DefaultBufferSize);
			BcdLengthEncoder encoder;
			int[] sizes = { 9, 12, 115, 1984, 14215, 245121};
			int length;
			int i;

			parserContext.Write( new byte[] { 9, 0x12, 1, 0x15, 0x19, 0x84, 1,
					0x42, 0x15, 0x24, 0x51, 0x21, 0, 0xA4});
			parserContext.Write( "Some data");

			for ( i = 0; i < sizes.Length; i++) {
				encoder = BcdLengthEncoder.GetInstance( sizes[i]);
				length = encoder.Decode( ref parserContext);
				Assert.IsTrue( length == sizes[i]);
			}

			encoder = BcdLengthEncoder.GetInstance( 999);
			try {
				length = encoder.Decode( ref parserContext);
				Assert.Fail();
			} catch ( MessagingException) {
			}
		}
		#endregion
	}
}
