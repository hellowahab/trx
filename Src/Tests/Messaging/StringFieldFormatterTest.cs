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
	/// Test fixture for StringFieldFormatter.
	/// </summary>
	[TestFixture( Description="String field formatter tests.")]
	public class StringFieldFormatterTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="StringFieldFormatterTest"/>.
		/// </summary>
		public StringFieldFormatterTest() {

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
			StringFieldFormatter formatter = new StringFieldFormatter( 37,
				new FixedLengthManager( 12), DataEncoder.GetInstance(),
				SpacePaddingRight.GetInstance( false), NumericValidator.GetInstance(),
				null, "My formatter");

			Assert.IsTrue( formatter.FieldNumber == 37);
			Assert.IsTrue( formatter.LengthManager.MaximumLength == 12);
			Assert.IsTrue( formatter.Description.Equals( "My formatter"));
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false));
			Assert.IsTrue( formatter.Validator == NumericValidator.GetInstance());
			Assert.IsNull( formatter.ValueFormatter);

			// Test variable length properties without padding.
			formatter = new StringFieldFormatter( 63, new VariableLengthManager( 1, 800,
				StringLengthEncoder.GetInstance( 5)), DataEncoder.GetInstance());

			Assert.IsTrue( formatter.FieldNumber == 63);
			Assert.IsTrue( formatter.LengthManager.MaximumLength == 800);
			Assert.IsTrue( formatter.Description.Equals( string.Empty));
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == null);
			Assert.IsTrue( formatter.Validator == null);

			// Test variable length properties with padding.
			formatter = new StringFieldFormatter( 48, new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 3)), DataEncoder.GetInstance(),
				SpacePaddingRight.GetInstance( false), "My formatter");

			Assert.IsTrue( formatter.FieldNumber == 48);
			Assert.IsTrue( formatter.LengthManager.MaximumLength == 999);
			Assert.IsTrue( formatter.Description.Equals( "My formatter"));
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
			Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false));
			Assert.IsTrue( formatter.Validator == null);
		}

		/// <summary>
		/// Test Format method.
		/// </summary>
		[Test( Description="Test Format method.")]
		public void Format() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			StringField field = new StringField( 1, "DATA");
			StringFieldFormatter formatter;
			string formattedData;

			// Test fixed length formatting.
			formatter = new StringFieldFormatter( 37,
				new FixedLengthManager( 12), DataEncoder.GetInstance());
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "DATA        "));

			// Test variable length formatting without padding.
			formatterContext.Clear();
			formatter = new StringFieldFormatter( 48, new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)),
				DataEncoder.GetInstance());
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "004DATA"));
			formatterContext.Clear();
			formatter.Format( new StringField( 5, null), ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "000"));

			// Test variable length formatting with padding.
			formatterContext.Clear();
			formatter = new StringFieldFormatter( 48, new VariableLengthManager( 10,
				10, StringLengthEncoder.GetInstance( 10)),
				DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( false),
				string.Empty);
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "10      DATA"));

			// Test validator with fixed length formatting.
			formatterContext.Clear();
			formatter = new StringFieldFormatter( 37,
				new FixedLengthManager( 12), DataEncoder.GetInstance(),
				NumericValidator.GetInstance(), string.Empty);
			field.FieldValue = "000000001500";
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "000000001500"));

			// Try with an invalid value.
			formatterContext.Clear();
			field.FieldValue = "D1500";
			try {
				formatter.Format( field, ref formatterContext);
				Assert.Fail();
			} catch ( StringValidationException) {
			}

			// Test validator with fixed length formatting and numeric padding.
			formatterContext.Clear();
			formatter = new StringFieldFormatter( 37,
				new FixedLengthManager( 12), DataEncoder.GetInstance(),
				ZeroPaddingLeft.GetInstance( false, true), NumericValidator.GetInstance());
			field.FieldValue = "56600";
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "000000056600"));

			// Try with an invalid value.
			formatterContext.Clear();
			field.FieldValue = "D1500";
			try {
				formatter.Format( field, ref formatterContext);
				Assert.Fail();
			} catch ( StringValidationException) {
			}
		}

		/// <summary>
		/// Test Parse method.
		/// </summary>
		[Test( Description="Test Parse method.")]
		public void Parse() {

			ParserContext parseContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			StringField field;
			StringFieldFormatter formatter;

			// Setup data for three complete fields an one with partial data.
			parseContext.Write( "DATA        20   DATA TO BE PARSED009SOME DATA00");

			// Test fixed length parse.
			formatter = new StringFieldFormatter( 37, new FixedLengthManager( 12),
				DataEncoder.GetInstance());
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "DATA"));

			// Test variable length parse with padding.
			formatter  = new StringFieldFormatter( 48, new VariableLengthManager( 1, 20,
				StringLengthEncoder.GetInstance( 99)), DataEncoder.GetInstance(),
				SpacePaddingLeft.GetInstance( false), string.Empty);
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "DATA TO BE PARSED"));

			// Test variable length parse without padding.
			formatter  = new StringFieldFormatter( 48, new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "SOME DATA"));

			// Test partial variable length parse without padding.
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "9MORE D");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "ATA");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "MORE DATA"));

			// Test partial fixed parse with padding.
			formatter = new StringFieldFormatter( 37, new FixedLengthManager( 12),
				DataEncoder.GetInstance());
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "ONE MORE");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "    ");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "ONE MORE"));

			// Test variable length header with zero length.
			formatter  = new StringFieldFormatter( 48, new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			parseContext.Write( "000");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsNull( field.FieldValue);

			// Test fixed length parse with validation.
			formatter = new StringFieldFormatter( 37, new FixedLengthManager( 12),
				DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance( false, true),
				NumericValidator.GetInstance());
			parseContext.Write( "000000145000");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "145000"));

			// Try with an invalid value.
			parseContext.Write( "0000001450F0");
			try {
				field = ( StringField)formatter.Parse( ref parseContext);
				Assert.Fail();
			} catch ( StringValidationException) {
			}

			// Test variable length parse with validation.
			parseContext.Clear();
			parseContext.ResetDecodedLength();
			formatter  = new StringFieldFormatter( 48, new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance(),
				NumericValidator.GetInstance());
			parseContext.Write( "00532000");
			field = ( StringField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.FieldValue.Equals( "32000"));

			// Try with an invalid value.
			parseContext.Write( "005350F0");
			try {
				field = ( StringField)formatter.Parse( ref parseContext);
				Assert.Fail();
			} catch ( StringValidationException) {
			}
		}
		#endregion
	}
}
