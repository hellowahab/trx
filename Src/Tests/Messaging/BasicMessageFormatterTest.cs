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
	/// Test fixture for BasicMessageFormatter.
	/// </summary>
	[TestFixture( Description="Basic message formatter tests.")]
	public class BasicMessageFormatterTest : BasicMessageFormatterBaseTest {

		private string[] _fieldValues = {
			"XT", "FDRT", "ROOT", "POS115", "BRANCH 15", "SOME DATA",
			null, null, null, null, null, null, null, null, "CUSTOMER NAME",
			null, null, null, null, "ADDITIONAL DATA",
			null, null, null, null, null, null, null, null, null, null,
			"RECEIPT", "MESSAGE", null, null, "TI", null, null, null, "RC", null,
			null, "PR5", null, null, null, null, "LAST", null};

		// Configure some fields for a fixed length message formatter.
		private FieldFormatter[] _fixedMessageFormatter = {
			new StringFieldFormatter( 1, new FixedLengthManager( 2), DataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 3, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 4, new FixedLengthManager( 8), DataEncoder.GetInstance()),
			new StringFieldFormatter( 5, new FixedLengthManager( 15), DataEncoder.GetInstance()),
			new StringFieldFormatter( 6, new FixedLengthManager( 20), DataEncoder.GetInstance(),
				SpacePaddingLeft.GetInstance( false), string.Empty)};

		private MessageFormatterTestConfig[] _fixedMessageFormatterTests = {
				new MessageFormatterTestConfig(
					new int[] { 1, 3, 4, 2, 6, 5},
					null,
					null,
					"XTFDRTROOTPOS115  BRANCH 15                 SOME DATA"),
				new MessageFormatterTestConfig(
					new int[] { 1, 5, 4, 3, 2, 6},
					new StringMessageHeaderFormatter( new FixedLengthManager( 10),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					"HEADER    XTFDRTROOTPOS115  BRANCH 15                 SOME DATA"),
				new MessageFormatterTestConfig(
					new int[] { 1, 5, 4, 3, 2, 6},
					new StringMessageHeaderFormatter( new VariableLengthManager( 1, 10,
						StringLengthEncoder.GetInstance( 10)), DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					"06HEADERXTFDRTROOTPOS115  BRANCH 15                 SOME DATA"),
				new MessageFormatterTestConfig(
					new int[] { 1, 5, 4, 3, 2, 6},
					new StringMessageHeaderFormatter( new FixedLengthManager( 10),
						DataEncoder.GetInstance()),
					null,
					"          XTFDRTROOTPOS115  BRANCH 15                 SOME DATA"),
				new MessageFormatterTestConfig(
					new int[] { 1, 2, 3, 4, 5, 6},
					new StringMessageHeaderFormatter( new FixedLengthManager( 10),
						DataEncoder.GetInstance()),
					new StringMessageHeader( null),
					"          XTFDRTROOTPOS115  BRANCH 15                 SOME DATA")
			};

		// Configure some fields for a fixed length message formatter.
		private FieldFormatter[] _bitmappedMessageFormatter = {
			new BitMapFieldFormatter( 0, 1, 32, HexDataEncoder.GetInstance()),
			new StringFieldFormatter( 1, new FixedLengthManager( 2), DataEncoder.GetInstance()),
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
			new BitMapFieldFormatter( 32, 33, 40, HexDataEncoder.GetInstance()),
			null, null,
			new StringFieldFormatter( 35, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null, null, null,
			new StringFieldFormatter( 39, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new BitMapFieldFormatter( 40, 41, 48, HexDataEncoder.GetInstance()),
			new StringFieldFormatter( 42, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null, null, null, null, null,
			new StringFieldFormatter( 47, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			null};
		private MessageFormatterTestConfig[] _bitmappedMessageFormatterTests = {
				new MessageFormatterTestConfig(
					new int[] { 1, 31, 3, 4, 15},
					null,
					null,
					"B0020002XTROOTPOS115  13CUSTOMER NAME9RECEIPT  "), 
				new MessageFormatterTestConfig(
					new int[] { 1, 31, 3, 4, 15},
					new StringMessageHeaderFormatter( new FixedLengthManager( 10),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					"HEADER    B0020002XTROOTPOS115  13CUSTOMER NAME9RECEIPT  "),
				new MessageFormatterTestConfig(
					new int[] { 20, 31, 35},
					null,
					null,
					"00001003015ADDITIONAL DATA9RECEIPT  20TI  "), 
				new MessageFormatterTestConfig(
					new int[] { 35, 39},
					new StringMessageHeaderFormatter( new FixedLengthManager( 15),
						DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
					new StringMessageHeader( "HEADER DATA"),
					"    HEADER DATA0000000122TI  RC  "),
				new MessageFormatterTestConfig(
					new int[] { 2, 3, 6, 20},
					null,
					null,
					"64001000FDRTROOT           SOME DATA015ADDITIONAL DATA"), 
				new MessageFormatterTestConfig(
					new int[] { 2, 3, 6, 20},
					new StringMessageHeaderFormatter( new FixedLengthManager( 6),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					"HEADER64001000FDRTROOT           SOME DATA015ADDITIONAL DATA"),
				new MessageFormatterTestConfig(
					new int[] { 47},
					new StringMessageHeaderFormatter( new FixedLengthManager( 15),
						DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
					new StringMessageHeader( "HEADER DATA"),
					"    HEADER DATA000000010102LAST"),
				new MessageFormatterTestConfig(
					new int[] { 47},
					null,
					null,
					"000000010102LAST"),
				new MessageFormatterTestConfig(
					new int[] { 1, 2, 3, 4, 5, 6, 15, 20, 31, 35, 39, 42, 47},
					new StringMessageHeaderFormatter( new FixedLengthManager( 6),
						DataEncoder.GetInstance()),
					new StringMessageHeader( "HEADER"),
					"HEADERFC021003XTFDRTROOTPOS115  BRANCH 15                 SOME DATA13CUSTOMER NAME015ADDITIONAL DATA9RECEIPT  23TI  RC  42PR5 LAST")
			};

		// Configure some fields for a fixed length message formatter.
		private FieldFormatter[] _mixedMessageFormatter = {
			new StringFieldFormatter( 1, new FixedLengthManager( 2), DataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 3, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new BitMapFieldFormatter( 4, 5, 32, HexDataEncoder.GetInstance()),
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
			null};
		private MessageFormatterTestConfig[] _mixedMessageFormatterTests = {
					new MessageFormatterTestConfig(
						new int[] { 1, 31, 3, 2, 6},
						null,
						null,
						"XTFDRTROOT40000020           SOME DATA9RECEIPT  "),
					new MessageFormatterTestConfig(
						new int[] { 1, 5, 3, 2, 6},
						new StringMessageHeaderFormatter( new FixedLengthManager( 10),
							DataEncoder.GetInstance()),
						new StringMessageHeader( "HEADER"),
						"HEADER    XTFDRTROOTC0000000BRANCH 15                 SOME DATA"),
					new MessageFormatterTestConfig(
						new int[] { 1, 2, 3, 20, 31},
						null,
						null,
						"XTFDRTROOT00010020015ADDITIONAL DATA9RECEIPT  "), 
					new MessageFormatterTestConfig(
						new int[] { 1, 2, 3, 20, 31},
						new StringMessageHeaderFormatter( new FixedLengthManager( 15),
							DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
						new StringMessageHeader( "HEADER DATA"),
						"    HEADER DATAXTFDRTROOT00010020015ADDITIONAL DATA9RECEIPT  ")
				};

		// Configure some fields for a fixed length message formatter.
		private FieldFormatter[] _mixedExtendedMessageFormatter = {
			new StringFieldFormatter( 1, new FixedLengthManager( 2), DataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new StringFieldFormatter( 3, new FixedLengthManager( 4), DataEncoder.GetInstance()),
			new BitMapFieldFormatter( 4, 5, 20, HexDataEncoder.GetInstance()),
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
			new StringFieldFormatter( 31, new FixedLengthManager( 10), DataEncoder.GetInstance()),
			new StringFieldFormatter( 32, new FixedLengthManager( 12), DataEncoder.GetInstance(),
				SpacePaddingLeft.GetInstance( false), string.Empty),
		};
		private MessageFormatterTestConfig[] _mixedExtendedMessageFormatterTests = {
					new MessageFormatterTestConfig(
						new int[] { 1, 32, 31, 3, 2, 6, 15, 20},
						null,
						null,
						"XTFDRTROOT4021           SOME DATA13CUSTOMER NAME015ADDITIONAL DATARECEIPT        MESSAGE"),
					new MessageFormatterTestConfig(
						new int[] { 1, 32, 31, 3, 2, 6},
						null,
						null,
						"XTFDRTROOT4000           SOME DATARECEIPT        MESSAGE"),
					new MessageFormatterTestConfig(
						new int[] { 1, 32, 31, 3, 2, 6, 15, 20},
						new StringMessageHeaderFormatter( new FixedLengthManager( 10),
							DataEncoder.GetInstance()),
						new StringMessageHeader( "HEADER"),
						"HEADER    XTFDRTROOT4021           SOME DATA13CUSTOMER NAME015ADDITIONAL DATARECEIPT        MESSAGE"),
					new MessageFormatterTestConfig(
						new int[] { 1, 32, 31, 3, 2, 6},
						new StringMessageHeaderFormatter( new FixedLengthManager( 15),
							DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( true)),
						new StringMessageHeader( "HEADER DATA"),
						"    HEADER DATAXTFDRTROOT4000           SOME DATARECEIPT        MESSAGE")
				};

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="BasicMessageFormatterTest"/>.
		/// </summary>
		public BasicMessageFormatterTest() {

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
		/// Test instantiation.
		/// </summary>
		[Test( Description="Test instantiation and properties.")]
		public void InstantiationAndProperties() {

			string name = "Basic formatter";
			string description = "My basic formatter";
			StringMessageHeaderFormatter headerFormatter =
				new StringMessageHeaderFormatter( new FixedLengthManager( 2),
				DataEncoder.GetInstance());
			BasicMessageFormatter formatter = new BasicMessageFormatter();

			// Set properties.
			formatter.Name = name;
			formatter.Description = description;
			formatter.MessageHeaderFormatter =
				( IMessageHeaderFormatter)headerFormatter;

			// Check properties.
			Assert.IsTrue( formatter.Name.Equals( name));
			Assert.IsTrue( formatter.Description.Equals( description));
			Assert.IsNotNull( formatter.Logger);
			Assert.IsNotNull( formatter.FieldFormatters);
			Assert.IsNotNull( formatter.MessageHeaderFormatter);
			Assert.IsTrue( formatter.MessageHeaderFormatter == headerFormatter);
			Assert.IsNull( formatter[1]);

			// Add field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				formatter.FieldFormatters.Add( _fixedMessageFormatter[i]);
			}

			// Check index property.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				Assert.IsNotNull( formatter[i + 1]);
				Assert.IsTrue( formatter[i + 1] == _fixedMessageFormatter[i]);
			}
		}

		/// <summary>
		/// Fixed message length formatting/parsing.
		/// </summary>
		[Test( Description="Fixed message length formatting/parsing.")]
		public void FixedMessage() {

			DoTest( _fieldValues, _fixedMessageFormatter,
				_fixedMessageFormatterTests, false);
		}

		/// <summary>
		/// Fixed message length formatting/parsing with partial parsing.
		/// </summary>
		[Test( Description="Fixed message length formatting/parsing with partial parsing.")]
		public void FixedMessagePartialParsing() {

			DoTest( _fieldValues, _fixedMessageFormatter,
				_fixedMessageFormatterTests, true);
		}

		/// <summary>
		/// Bitmapped message formatting/parsing.
		/// </summary>
		[Test( Description="Bitmapped message formatting/parsing.")]
		public void BitmappedMessage() {

			DoTest( _fieldValues, _bitmappedMessageFormatter,
				_bitmappedMessageFormatterTests, false);
		}

		/// <summary>
		/// Bitmapped message formatting/parsing with partial parsing.
		/// </summary>
		[Test( Description="Bitmapped message formatting/parsing with partial parsing.")]
		public void BitmappedMessagePartialParsing() {

			DoTest( _fieldValues, _bitmappedMessageFormatter,
				_bitmappedMessageFormatterTests, true);
		}

		/// <summary>
		/// Mixed message formatting/parsing.
		/// </summary>
		[Test( Description="Mixed message formatting/parsing.")]
		public void MixedMessage() {

			DoTest( _fieldValues, _mixedMessageFormatter,
				_mixedMessageFormatterTests, false);
		}

		/// <summary>
		/// Mixed message formatting/parsing with partial parsing.
		/// </summary>
		[Test( Description="Mixed message formatting/parsing with partial parsing.")]
		public void MixedMessagePartialParsing() {

			DoTest( _fieldValues, _mixedMessageFormatter,
				_mixedMessageFormatterTests, true);
		}

		
		/// <summary>
		/// Mixed extended message formatting/parsing.
		/// </summary>
		[Test( Description="Mixed extended message formatting/parsing.")]
		public void MixedExtendedMessage() {

			DoTest( _fieldValues, _mixedExtendedMessageFormatter,
				_mixedExtendedMessageFormatterTests, false);
		}

		/// <summary>
		/// Mixed extended message formatting/parsing with partial parsing.
		/// </summary>
		[Test( Description="Mixed extended message formatting/parsing with partial parsing.")]
		public void MixedExtendedMessagePartialParsing() {

			DoTest( _fieldValues, _mixedExtendedMessageFormatter,
				_mixedExtendedMessageFormatterTests, true);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void Clone() {

			string name = "Basic formatter";
			string description = "My basic formatter";
			StringMessageHeaderFormatter headerFormatter =
				new StringMessageHeaderFormatter( new FixedLengthManager( 2),
				DataEncoder.GetInstance());
			BasicMessageFormatter formatter = new BasicMessageFormatter();

			// Set properties.
			formatter.Name = name;
			formatter.Description = description;
			formatter.MessageHeaderFormatter =
				( IMessageHeaderFormatter)headerFormatter;

			// Add field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				formatter.FieldFormatters.Add( _fixedMessageFormatter[i]);
			}

			BasicMessageFormatter clonedFormatter = 
				( BasicMessageFormatter)formatter.Clone();

			Assert.IsNotNull( clonedFormatter);

			// Check properties.
			Assert.IsTrue( clonedFormatter.Name.Equals( name));
			Assert.IsTrue( clonedFormatter.Description.Equals( description));
			Assert.IsNotNull( formatter.Logger == clonedFormatter.Logger);
			Assert.IsNotNull( clonedFormatter.FieldFormatters);
			Assert.IsNotNull( clonedFormatter.MessageHeaderFormatter);
			Assert.IsTrue( formatter.MessageHeaderFormatter == headerFormatter);

			foreach ( FieldFormatter fieldFormatter in formatter.FieldFormatters) {
				Assert.IsTrue( clonedFormatter.FieldFormatters.Contains(
					fieldFormatter.FieldNumber));
				Assert.IsTrue( clonedFormatter[fieldFormatter.FieldNumber] ==
					fieldFormatter);
			}
		}

		/// <summary>
		/// Test GetBitMapFieldNumbers method.
		/// </summary>
		[Test( Description="Test GetBitMapFieldNumbers method.")]
		public void GetBitMapFieldNumbers() {

			BasicMessageFormatter formatter = new BasicMessageFormatter();

			Assert.IsNull( formatter.GetBitMapFieldNumbers());

			// Add fixed field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				formatter.FieldFormatters.Add( _fixedMessageFormatter[i]);
			}

			Assert.IsNull( formatter.GetBitMapFieldNumbers());

			formatter.FieldFormatters.Clear();

			// Add mixed field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				formatter.FieldFormatters.Add( _mixedMessageFormatter[i]);
			}

			int[] fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 1);
			Assert.IsTrue( fieldNumbers[0] == 4);

			formatter.FieldFormatters.Remove( 4);
			Assert.IsNull( formatter.GetBitMapFieldNumbers());

			formatter.FieldFormatters.Clear();

			formatter.FieldFormatters.Add( new BitMapFieldFormatter(
				1, 65, 128, HexDataEncoder.GetInstance()));
			formatter.FieldFormatters.Add( new BitMapFieldFormatter(
				0, 1, 64, HexDataEncoder.GetInstance()));
			formatter.FieldFormatters.Add( new BitMapFieldFormatter(
				65, 129, 192, HexDataEncoder.GetInstance()));

			fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 3);
			Assert.IsTrue( fieldNumbers[0] == 0);
			Assert.IsTrue( fieldNumbers[1] == 1);
			Assert.IsTrue( fieldNumbers[2] == 65);

			formatter.FieldFormatters.Remove( 65);

			fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 2);
			Assert.IsTrue( fieldNumbers[0] == 0);
			Assert.IsTrue( fieldNumbers[1] == 1);

			formatter.FieldFormatters.Remove( 0);

			fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 1);
			Assert.IsTrue( fieldNumbers[0] == 1);

			formatter.FieldFormatters.Add( new BitMapFieldFormatter(
				65, 129, 192, HexDataEncoder.GetInstance()));

			fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 2);
			Assert.IsTrue( fieldNumbers[0] == 1);
			Assert.IsTrue( fieldNumbers[1] == 65);

			formatter.FieldFormatters.Add( new BitMapFieldFormatter(
				0, 1, 64, HexDataEncoder.GetInstance()));

			fieldNumbers = formatter.GetBitMapFieldNumbers();
			Assert.IsNotNull( fieldNumbers);
			Assert.IsTrue( fieldNumbers.Length == 3);
			Assert.IsTrue( fieldNumbers[0] == 0);
			Assert.IsTrue( fieldNumbers[1] == 1);
			Assert.IsTrue( fieldNumbers[2] == 65);
		}

		/// <summary>
		/// Test NewMessage method.
		/// </summary>
		[Test( Description="Test NewMessage method.")]
		public void NewMessage() {

			BasicMessageFormatter formatter = new BasicMessageFormatter();

			object message = formatter.NewMessage();

			Assert.IsTrue( message is Message);
		}

		private class SimpleTransaction {

			private string _branch;
			private int _terminal;
			private string _customerName;
			private string _additionalData;

			public bool InvalidPropertyHasBeenTaken;
			public bool InvalidPropertyHasBeenAssigned;

			#region Constructors
			public SimpleTransaction() {

				_branch = null;
				_terminal = 0;
				_customerName = null;
				InvalidPropertyHasBeenTaken = false;
				InvalidPropertyHasBeenAssigned = false;
			}
			#endregion

			#region Properties
			[Field( 1)]
			public string Branch {

				set {

					_branch = value;
				}
			}

			[Field( 2)]
			public string this[int index] {

				get {

					InvalidPropertyHasBeenTaken = true;
					return string.Empty;
				}

				set {

					InvalidPropertyHasBeenAssigned = true;
				}
			}

			public string PrivateUse {

				get {

					InvalidPropertyHasBeenTaken = true;
					return string.Empty;
				}

				set {

					InvalidPropertyHasBeenAssigned = true;
				}
			}

			[Field( 5)]
			public int Terminal {

				get {

					return _terminal;
				}

				set {

					_terminal = value;
				}
			}

			[Field( 6)]
			public string CustomerName {

				get {

					return _customerName;
				}

				set {

					_customerName = value;
				}
			}

			[Field( 30)]
			public string AdditionalData {

				get {

					return _additionalData;
				}

				set {

					_additionalData = value;
				}
			}
			#endregion

			#region Methods
			public string RetrieveBranch() {

				return _branch;
			}
			#endregion
		}

		/// <summary>
		/// Test AssignFields method.
		/// </summary>
		[Test( Description="Test AssignFields method.")]
		public void AssignFields() {

			BasicMessageFormatter formatter = new BasicMessageFormatter();
			Message message = new Message();

			// Add field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				if ( _fixedMessageFormatter[i] != null) {
					formatter.FieldFormatters.Add( _fixedMessageFormatter[i]);
				}
			}

			// Replace field formatter for field number 6, to perform
			// a complete AssignFields test.
			formatter.FieldFormatters.Add( new BinaryFieldFormatter( 6,
				new FixedLengthManager( 20), DataEncoder.GetInstance()));

			// Perform test for invalid parameters.
			try {
				formatter.AssignFields( null, null);
				Assert.Fail();
			} catch ( ArgumentNullException e) {
				Assert.IsTrue( e.ParamName == "message");
			}

			try {
				formatter.AssignFields( message, null);
				Assert.Fail();
			} catch ( ArgumentNullException e) {
				Assert.IsTrue( e.ParamName == "fieldsContainer");
			}

			// Perform a simple test.
			SimpleTransaction simpleTransaction = new SimpleTransaction();
			simpleTransaction.Terminal = 106;
			simpleTransaction.CustomerName = "John Doe";
			simpleTransaction.AdditionalData = "Some data";
			formatter.AssignFields( message, simpleTransaction);

			Assert.IsFalse( simpleTransaction.InvalidPropertyHasBeenTaken);
			Assert.IsFalse( message.Fields.Contains( 1));
			Assert.IsFalse( message.Fields.Contains( 2));
			Assert.IsTrue( message.Fields.Contains( 5));
			Assert.IsTrue( simpleTransaction.Terminal.ToString().Equals(
				message[5].ToString()));
			Assert.IsTrue( message.Fields.Contains( 6));
			Assert.IsTrue( simpleTransaction.CustomerName.Equals(
				message[6].ToString()));
			Assert.IsTrue( message.Fields.Contains( 30));
			Assert.IsTrue( simpleTransaction.AdditionalData.Equals(
				message[30].ToString()));
		}

		/// <summary>
		/// Test RetrieveFields method.
		/// </summary>
		[Test( Description="Test RetrieveFields method.")]
		public void RetrieveFields() {

			BasicMessageFormatter formatter = new BasicMessageFormatter();
			Message message = new Message();

			// Add field Formatters.
			for ( int i = 0; i < _fixedMessageFormatter.Length; i++) {
				if ( _fixedMessageFormatter[i] != null) {
					formatter.FieldFormatters.Add( _fixedMessageFormatter[i]);
				}
			}

			// Replace field formatter for field number 6, to perform
			// a complete AssignFields test.
			formatter.FieldFormatters.Add( new BinaryFieldFormatter( 6,
				new FixedLengthManager( 20), DataEncoder.GetInstance()));

			// Perform test for invalid parameters.
			try {
				formatter.RetrieveFields( null, null);
				Assert.Fail();
			} catch ( ArgumentNullException e) {
				Assert.IsTrue( e.ParamName == "message");
			}

			try {
				formatter.RetrieveFields( message, null);
				Assert.Fail();
			} catch ( ArgumentNullException e) {
				Assert.IsTrue( e.ParamName == "fieldsContainer");
			}

			// Perform a simple test.
			message.Fields.Add( 1, "01");
			message.Fields.Add( 2, "02");
			message.Fields.Add( 5, "1385");
			message.Fields.Add( 6, "John Doe");
			message.Fields.Add( 30, "Random data");
			SimpleTransaction simpleTransaction = new SimpleTransaction();
			formatter.RetrieveFields( message, simpleTransaction);

			Assert.IsFalse( simpleTransaction.InvalidPropertyHasBeenAssigned);
			Assert.IsNotNull( simpleTransaction.RetrieveBranch());
			Assert.IsTrue( simpleTransaction.RetrieveBranch().Equals( "01"));
			Assert.IsTrue( simpleTransaction.Terminal == 1385);
			Assert.IsTrue( simpleTransaction.AdditionalData.Equals( "Random data"));
		}
		#endregion
	}
}
