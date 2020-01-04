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

using Trx.Messaging;
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for StringMessageHeaderFormatter.
	/// </summary>
	[TestFixture( Description="String message header formatter tests.")]
	public class StringMessageHeaderFormatterTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="StringMessageHeaderFormatterTest"/>.
		/// </summary>
		public StringMessageHeaderFormatterTest() {

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
		/// Test constructors.
		/// </summary>
		[Test( Description="Test constructors.")]
		public void Constructors() {

			// Test fixed length properties.
			StringMessageHeaderFormatter formatter = new StringMessageHeaderFormatter(
				new FixedLengthManager( 12), DataEncoder.GetInstance(),
				SpacePaddingRight.GetInstance( false));

			Assert.IsTrue( formatter.LengthManager.MaximumLength == 12);
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false));

			// Test variable length properties without padding.
			formatter = new StringMessageHeaderFormatter( new VariableLengthManager( 1, 800,
				StringLengthEncoder.GetInstance( 5)), DataEncoder.GetInstance());

			Assert.IsTrue( formatter.LengthManager.MaximumLength == 800);
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == null);

			// Test variable length properties with padding.
			formatter = new StringMessageHeaderFormatter( new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 3)), DataEncoder.GetInstance(),
				SpacePaddingRight.GetInstance( false));

			Assert.IsTrue( formatter.LengthManager.MaximumLength == 999);
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false));
		}

		/// <summary>
		/// Test Format method.
		/// </summary>
		[Test( Description="Test Format method.")]
		public void Format() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			StringMessageHeader header = new StringMessageHeader( "DATA");
			StringMessageHeaderFormatter formatter;
			string formattedData;

			// Test fixed length formatting.
			formatter = new StringMessageHeaderFormatter(
				new FixedLengthManager( 12), DataEncoder.GetInstance());
			formatter.Format( header, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "DATA        "));
			formatterContext.Clear();
			formatter.Format( null, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "            "));

			// Test variable length formatting without padding.
			formatterContext.Clear();
			formatter = new StringMessageHeaderFormatter( new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)),
				DataEncoder.GetInstance());
			formatter.Format( header, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "004DATA"));
			formatterContext.Clear();
			formatter.Format( null, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "000"));

			// Test variable length formatting with padding.
			formatterContext.Clear();
			formatter = new StringMessageHeaderFormatter( new VariableLengthManager( 10,
				10, StringLengthEncoder.GetInstance( 10)),
				DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( false));
			formatter.Format( header, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "10      DATA"));
		}

		/// <summary>
		/// Test Format method.
		/// </summary>
		[Test( Description="Test Parse method.")]
		public void Parse() {

			ParserContext parseContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			StringMessageHeader header;
			StringMessageHeaderFormatter formatter;

			// Setup data for three complete fields an one with partial data.
			parseContext.Write( "DATA        20   DATA TO BE PARSED009SOME DATA00");

			// Test fixed length parse.
			formatter = new StringMessageHeaderFormatter( new FixedLengthManager( 12),
				DataEncoder.GetInstance());
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( header.Value.Equals( "DATA"));

			// Test variable length parse with padding.
			formatter  = new StringMessageHeaderFormatter( new VariableLengthManager( 1, 20,
				StringLengthEncoder.GetInstance( 99)), DataEncoder.GetInstance(),
				SpacePaddingLeft.GetInstance( false));
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( header.Value.Equals( "DATA TO BE PARSED"));

			// Test variable length parse without padding.
			formatter  = new StringMessageHeaderFormatter( new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( header.Value.Equals( "SOME DATA"));

			// Test partial variable length parse without padding.
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNull( header);
			parseContext.Write( "9MORE D");
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNull( header);
			parseContext.Write( "ATA");
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( header.Value.Equals( "MORE DATA"));

			// Test partial fixed parse with padding.
			formatter = new StringMessageHeaderFormatter( new FixedLengthManager( 12),
				DataEncoder.GetInstance());
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNull( header);
			parseContext.Write( "ONE MORE");
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNull( header);
			parseContext.Write( "    ");
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( header.Value.Equals( "ONE MORE"));

			// Test variable length header with zero length.
			formatter  = new StringMessageHeaderFormatter( new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			parseContext.Write( "000");
			header = ( StringMessageHeader)formatter.Parse( ref parseContext);
			Assert.IsNotNull( header);
			parseContext.ResetDecodedLength();
			Assert.IsNull( header.Value);
		}
		#endregion
	}
}
