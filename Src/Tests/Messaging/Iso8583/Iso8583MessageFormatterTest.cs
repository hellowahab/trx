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
using Trx.Messaging.Iso8583;
using Trx.Utilities;
using NUnit.Framework;
using log4net;

namespace Tests.Trx.Messaging.Iso8583 {

	/// <summary>
	/// Test fixture for Iso8583MessageFormatter.
	/// </summary>
	[TestFixture( Description="ISO 8583 message formatter tests.")]
	public class Iso8583MessageFormatterTest : Iso8583MessageFormatterBaseTest {

		private string[] _fieldValues = {
/*   1..  8 */	null, "FDRT", "ROOT", "POS115", "BRANCH 15", "SOME DATA", null, null,
/*   9.. 16 */	null, null, null, null, null, null, "CUSTOMER NAME", null,
/*  17.. 24 */	null, null, null, "ADDITIONAL DATA", null, null, null, null,
/*  25.. 32 */	null, null, null, null, null, null, "RECEIPT", "MESSAGE",
/*  33.. 40 */	null, null, "TI", null, null, null, "RC", null,
/*  41.. 48 */	null, "PR5", null, null, null, null, "LAST", null,
/*  49.. 56 */	null, null, null, null, null, null, null, null,
/*  57.. 64 */	null, null, null, null, null, null, null, null,
/*  65.. 72 */	null, null, null, null, null, "300", null, null,
/*  73.. 80 */	null, null, null, null, null, null, null, null,
/*  81.. 88 */	null, null, null, null, null, null, null, null,
/*  89.. 96 */	null, null, null, null, null, null, null, null,
/*  97..104 */	null, null, null, null, null, null, null, null,
/* 105..112 */	null, null, null, null, null, null, null, null,
/* 113..120 */	null, null, null, null, null, null, null, null,
/* 121..128 */	null, null, null, null, null, null, null, null};

		// Configure some fields for a fixed length message formatter.
		private FieldFormatter[] _messageFormatter = {
			new BitMapFieldFormatter( 0, 1, 64, HexDataEncoder.GetInstance()),
			new BitMapFieldFormatter( 1, 65, 128, HexDataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 3, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 4, new FixedLengthManager( 8), DataEncoder.GetInstance()),
			new StringFieldFormatter( 5, new FixedLengthManager( 15), DataEncoder.GetInstance()),
			new StringFieldFormatter( 6, new FixedLengthManager( 20), DataEncoder.GetInstance(),
				SpacePaddingLeft.GetInstance( false), string.Empty),
			null, null, null, null, null, null, null, null,
			new StringFieldFormatter( 15, new VariableLengthManager( 0, 99,
				StringLengthEncoder.GetInstance( 99)), DataEncoder.GetInstance()),
			null, null, null, null,
			new StringFieldFormatter( 20, new VariableLengthManager( 0, 999,
				StringLengthEncoder.GetInstance( 999)), DataEncoder.GetInstance()),
			null, null, null, null, null, null, null, null, null, null,
			new StringFieldFormatter( 31, new VariableLengthManager( 0, 9,
				StringLengthEncoder.GetInstance( 9)), DataEncoder.GetInstance(),
				SpacePaddingRight.GetInstance( false), string.Empty),
			new StringFieldFormatter( 32, new FixedLengthManager( 15), DataEncoder.GetInstance()),
			null, null,
			new StringFieldFormatter( 35, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null, null, null,
			new StringFieldFormatter( 39, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 40, new VariableLengthManager( 0, 99,
				StringLengthEncoder.GetInstance( 99)), DataEncoder.GetInstance()),
			new StringFieldFormatter( 42, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null, null, null, null, null,
			new StringFieldFormatter( 47, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null,
			new StringFieldFormatter( 70, new FixedLengthManager( 3), DataEncoder.GetInstance()),
			null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null};
		private Iso8583MessageFormatterTestConfig[] _messageFormatterTests = {
				new Iso8583MessageFormatterTestConfig(
					new int[] { 31, 3, 4, 15},
					null,
					null,
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					200,
					"02003002000200000000ROOTPOS115  13CUSTOMER NAME9RECEIPT  "), 
				new Iso8583MessageFormatterTestConfig(
					new int[] { 31, 3, 4, 15},
					new StringMessageHeaderFormatter( new FixedLengthManager( 10),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1220,
					"HEADER    12203002000200000000ROOTPOS115  13CUSTOMER NAME9RECEIPT  "),
				new Iso8583MessageFormatterTestConfig(
					new int[] { 20, 31, 35, 70},
					null,
					null,
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					800,
					"080080001002200000000400000000000000015ADDITIONAL DATA9RECEIPT  TI  300"), 
				new Iso8583MessageFormatterTestConfig(
					new int[] { 35, 39},
					new StringMessageHeaderFormatter( new FixedLengthManager( 15),
						DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
					new StringMessageHeader( "HEADER DATA"),
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					510,
					"    HEADER DATA05100000000022000000TI  RC  "),
				new Iso8583MessageFormatterTestConfig(
					new int[] { 2, 3, 6, 20},
					null,
					null,
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1520,
					"15206400100000000000FDRTROOT           SOME DATA015ADDITIONAL DATA"), 
				new Iso8583MessageFormatterTestConfig(
					new int[] { 2, 3, 6, 20},
					new StringMessageHeaderFormatter( new FixedLengthManager( 6),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1100,
					"HEADER11006400100000000000FDRTROOT           SOME DATA015ADDITIONAL DATA"),
				new Iso8583MessageFormatterTestConfig(
					new int[] { 47},
					new StringMessageHeaderFormatter( new FixedLengthManager( 15),
						DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
					new StringMessageHeader( "HEADER DATA"),
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1110,
					"    HEADER DATA11100000000000020000LAST"),
				new Iso8583MessageFormatterTestConfig(
					new int[] { 47},
					null,
					null,
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1420,
					"14200000000000020000LAST"),
				new Iso8583MessageFormatterTestConfig(
					new int[] { 2, 3, 4, 5, 6, 15, 20, 31, 35, 39, 42, 47, 70},
					new StringMessageHeaderFormatter( new FixedLengthManager( 6),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					new StringFieldFormatter( -1, new FixedLengthManager( 4),
						DataEncoder.GetInstance(),
						ZeroPaddingLeft.GetInstance( false, true), string.Empty),
					1430,
					"HEADER1430FC021002224200000400000000000000FDRTROOTPOS115  BRANCH 15                 SOME DATA13CUSTOMER NAME015ADDITIONAL DATA9RECEIPT  TI  RC  PR5 LAST300")
			};

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="Iso8583MessageFormatterTest"/>.
		/// </summary>
		public Iso8583MessageFormatterTest() {

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
		/// Message formatting/parsing.
		/// </summary>
		[Test( Description="Message formatting/parsing.")]
		public void Message() {

			DoTest( new Iso8583MessageFormatter(), _fieldValues,
				_messageFormatter, _messageFormatterTests, false);
		}

		/// <summary>
		/// Message formatting/parsing with partial parsing.
		/// </summary>
		[Test( Description="Message formatting/parsing with partial parsing.")]
		public void MessagePartialParsing() {

			DoTest( new Iso8583MessageFormatter(), _fieldValues,
				_messageFormatter, _messageFormatterTests, false);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void Clone() {

			string name = "Formatter";
			string description = "ISO 8583 message formatter formatter";
			StringMessageHeaderFormatter headerFormatter =
				new StringMessageHeaderFormatter( new FixedLengthManager( 2),
				DataEncoder.GetInstance());
			StringFieldFormatter mtiFormatter =
				new StringFieldFormatter( -1, new FixedLengthManager( 4),
				DataEncoder.GetInstance());
			Iso8583MessageFormatter formatter = new Iso8583MessageFormatter();

			// Set properties.
			formatter.Name = name;
			formatter.Description = description;
			formatter.MessageHeaderFormatter =
				( IMessageHeaderFormatter)headerFormatter;
			formatter.MessageTypeIdentifierFormatter = mtiFormatter;

			// Add field Formatters.
			for ( int i = 0; i < _messageFormatterTests[0].Fields.Length; i++) {
				formatter.FieldFormatters.Add( _messageFormatter[_messageFormatterTests[0].Fields[i]]);
			}

			Iso8583MessageFormatter clonedFormatter = 
				( Iso8583MessageFormatter)formatter.Clone();

			Assert.IsNotNull( clonedFormatter);

			// Check properties.
			Assert.IsTrue( clonedFormatter.Name.Equals( name));
			Assert.IsTrue( clonedFormatter.Description.Equals( description));
			Assert.IsNotNull( formatter.Logger == clonedFormatter.Logger);
			Assert.IsNotNull( clonedFormatter.FieldFormatters);
			Assert.IsNotNull( clonedFormatter.MessageHeaderFormatter);
			Assert.IsTrue( formatter.MessageHeaderFormatter == headerFormatter);
			Assert.IsTrue( formatter.MessageTypeIdentifierFormatter == mtiFormatter);

			foreach ( FieldFormatter fieldFormatter in formatter.FieldFormatters) {
				Assert.IsTrue( clonedFormatter.FieldFormatters.Contains(
					fieldFormatter.FieldNumber));
				Assert.IsTrue( clonedFormatter[fieldFormatter.FieldNumber] ==
					fieldFormatter);
			}
		}

		/// <summary>
		/// Test NewMessage method.
		/// </summary>
		[Test( Description="Test NewMessage method.")]
		public void NewMessage() {

			Iso8583MessageFormatter formatter = new Iso8583MessageFormatter();

			object message = formatter.NewMessage();

			Assert.IsTrue( message is Iso8583Message);
		}
		#endregion
	}
}
