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
	/// Test fixture for StringLengthEncoder.
	/// </summary>
	[TestFixture( Description="String length encoder tests.")]
	public class StringLengthEncoderTest {

		#region Constructors
		public StringLengthEncoderTest() {

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

			StringLengthEncoder encoder;
			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = StringLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder == StringLengthEncoder.GetInstance( sizes[i]));
			}

			try {
				encoder = StringLengthEncoder.GetInstance( 1000000);
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
			int[] expectedSizes = { 9, 99, 999, 9999, 99999, 999999};
			StringLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = StringLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder.MaximumLength == expectedSizes[i]);
			}
		}

		/// <summary>
		/// Test GetEncodedLength method.
		/// </summary>
		[Test( Description="Test GetEncodedLength method.")]
		public void GetEncodedLength() {

			int[] sizes = { 3, 15, 999, 4510, 16384, 681115};
			StringLengthEncoder encoder;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = StringLengthEncoder.GetInstance( sizes[i]);
				Assert.IsTrue( encoder.EncodedLength == ( i + 1));
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

			StringLengthEncoder encoder;
			string data;

			for ( int i = 0; i < sizes.Length; i++) {
				encoder = StringLengthEncoder.GetInstance( sizes[i]);
				encoder.Encode( sizes[i], ref formatterContext);
				data = formatterContext.GetDataAsString();
				Assert.IsTrue( data.Length == ( i + 1));
				Assert.IsTrue( Convert.ToInt32( data) == sizes[i]);
				formatterContext.Clear();
			}

			encoder = StringLengthEncoder.GetInstance( 999);
			try {
				encoder.Encode( 1000, ref formatterContext);
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
			StringLengthEncoder encoder;
			int[] sizes = { 9, 12, 115, 1984, 14215, 245121};
			int length;
			int i;

			for ( i = 0; i < sizes.Length; i++) {
				parserContext.Write( Convert.ToString( sizes[i]));
			}
			parserContext.Write( "Some data");

			for ( i = 0; i < sizes.Length; i++) {
				encoder = StringLengthEncoder.GetInstance( sizes[i]);
				length = encoder.Decode( ref parserContext);
				Assert.IsTrue( length == sizes[i]);
			}

			encoder = StringLengthEncoder.GetInstance( 999999);
			try {
				length = encoder.Decode( ref parserContext);
				Assert.Fail();
			} catch ( MessagingException) {
			}
		}
		#endregion
	}
}
