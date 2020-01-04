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
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for BinaryFieldFormatter.
	/// </summary>
	[TestFixture( Description="Binary field formatter tests.")]
	public class BinaryFieldFormatterTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="BinaryFieldFormatterTest"/>.
		/// </summary>
		public BinaryFieldFormatterTest() {

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
			BinaryFieldFormatter formatter = new BinaryFieldFormatter( 37,
				new FixedLengthManager( 12), DataEncoder.GetInstance(),
				"My formatter");

			Assert.IsTrue( formatter.FieldNumber == 37);
			Assert.IsTrue( formatter.LengthManager.MaximumLength == 12);
			Assert.IsTrue( formatter.Description.Equals( "My formatter"));
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());

			// Test variable length properties.
			formatter = new BinaryFieldFormatter( 63, new VariableLengthManager( 1, 800,
				StringLengthEncoder.GetInstance( 5)), DataEncoder.GetInstance());

			Assert.IsTrue( formatter.FieldNumber == 63);
			Assert.IsTrue( formatter.LengthManager.MaximumLength == 800);
			Assert.IsTrue( formatter.Description.Equals( string.Empty));
			Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance());
		}

		/// <summary>
		/// Test Format method.
		/// </summary>
		[Test( Description="Test Format method.")]
		public void Format() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			BinaryField field = new BinaryField( 1);
			field.Value = "DATA";
			BinaryFieldFormatter formatter;
			string formattedData;

			// Test fixed length formatting.
			formatter = new BinaryFieldFormatter( 37,
				new FixedLengthManager( 4), DataEncoder.GetInstance());
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "DATA"));

			formatterContext.Clear();
			field.Value = "MORE DATA";
			try {
				formatter.Format( field, ref formatterContext);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "dataLength"));
			}

			// Test variable length formatting.
			formatterContext.Clear();
			formatter = new BinaryFieldFormatter( 48, new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)),
				DataEncoder.GetInstance());
			formatter.Format( field, ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "009MORE DATA"));
			formatterContext.Clear();
			formatter.Format( new BinaryField( 5, null), ref formatterContext);
			formattedData = formatterContext.GetDataAsString();
			Assert.IsTrue( formattedData.Equals( "000"));
		}

		/// <summary>
		/// Test Parse method.
		/// </summary>
		[Test( Description="Test Parse method.")]
		public void Parse() {

			ParserContext parseContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			BinaryField field;
			BinaryFieldFormatter formatter;

			// Setup data for three complete fields an one with partial data.
			parseContext.Write( "DATA17DATA TO BE PARSED009SOME DATA00");

			// Test fixed length parse.
			formatter = new BinaryFieldFormatter( 37, new FixedLengthManager( 4),
				DataEncoder.GetInstance());
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.ToString().Equals( "DATA"));

			// Test variable length parse.
			formatter  = new BinaryFieldFormatter( 48, new VariableLengthManager( 1, 20,
				StringLengthEncoder.GetInstance( 99)), DataEncoder.GetInstance(),
				string.Empty);
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.ToString().Equals( "DATA TO BE PARSED"));

			formatter  = new BinaryFieldFormatter( 48, new VariableLengthManager( 1,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.ToString().Equals( "SOME DATA"));

			// Test partial variable length parse.
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "9MORE D");
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "ATA");
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.ToString().Equals( "MORE DATA"));

			// Test partial fixed.
			formatter = new BinaryFieldFormatter( 37, new FixedLengthManager( 8),
				DataEncoder.GetInstance());
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "ONE ");
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNull( field);
			parseContext.Write( "MORE");
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsTrue( field.ToString().Equals( "ONE MORE"));

			// Test variable length header with zero length.
			formatter  = new BinaryFieldFormatter( 48, new VariableLengthManager( 0,
				999, StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance());
			parseContext.Write( "000");
			field = ( BinaryField)formatter.Parse( ref parseContext);
			Assert.IsNotNull( field);
			parseContext.ResetDecodedLength();
			Assert.IsNull( field.Value);
		}
		#endregion
	}
}
