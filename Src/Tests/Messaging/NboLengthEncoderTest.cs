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
	/// Test fixture for NboLengthEncoder.
	/// </summary>
	[TestFixture( Description="NBO length encoder tests.")]
	public class NboLengthEncoderTest {

		#region Constructors
		public NboLengthEncoderTest() {

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

			NboLengthEncoder encoder;
			int[] sizes = { 0, 115, 1999, 14510, 116384, 1681115};

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = NboLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder == NboLengthEncoder.GetInstance( sizes[i]));
			}
		}

		/// <summary>
		/// Test MaximumLength property.
		/// </summary>
		[Test( Description="Test MaximumLength property.")]
		public void MaximumLength() {

			int[] sizes = { 0, 115, 1999, 14510, 116384, 1681115};
			int[] expectedSizes = { 255, 255, 65535, 65535, 16777215, 16777215};
			NboLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = NboLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder.MaximumLength == expectedSizes[i]);
			}
		}

		/// <summary>
		/// Test GetEncodedLength method.
		/// </summary>
		[Test( Description="Test GetEncodedLength method.")]
		public void GetEncodedLength() {

			int[] sizes = { 0, 115, 1999, 14510, 116384, 1681115};
			NboLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = NboLengthEncoder.GetInstance( sizes[i]);
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
			int[] sizes = { 0, 115, 1999, 14510, 116384, 1681115};
			byte[][] expectedData = {
										new byte[] { 0},
										new byte[] { 0x73},
										new byte[] { 7, 0xCF},
										new byte[] { 0x38, 0xAE}, 
										new byte[] { 1, 0xC6, 0xA0}, 
										new byte[] { 0x19, 0xA6, 0xDB}
									};

			NboLengthEncoder encoder;
			byte[] data;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = NboLengthEncoder.GetInstance( sizes[i]);
				encoder.Encode( sizes[i], ref formatterContext);
				data = formatterContext.GetData();
				Assert.IsTrue( data.Length == expectedData[i].Length);
				for ( int j = expectedData[i].Length - 1; j >= 0; j--) {
					Assert.IsTrue( data[j] == expectedData[i][j]);
				}
				formatterContext.Clear();
			}

			encoder = NboLengthEncoder.GetInstance( 65535);
			try {
				encoder.Encode( 65536, ref formatterContext);
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
			NboLengthEncoder encoder;
			int[] sizes = { 0, 115, 1999, 14510, 116384, 1681115};
			int length;
			int i;

			parserContext.Write( new byte[] { 0, 0x73, 7, 0xCF, 0x38, 0xAE, 1,
												0xC6, 0xA0, 0x19, 0xA6, 0xDB});
			parserContext.Write( "Some data");

			for ( i = 0; i < sizes.Length; i++) {
				encoder = NboLengthEncoder.GetInstance( sizes[i]);
				length = encoder.Decode( ref parserContext);
				Assert.IsTrue( length == sizes[i]);
			}
		}
		#endregion
	}
}
