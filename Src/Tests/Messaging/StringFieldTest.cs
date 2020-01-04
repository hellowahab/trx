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
	/// Test fixture for StringField.
	/// </summary>
	[TestFixture( Description="String field tests.")]
	public class StringFieldTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="StringFieldTest"/>.
		/// </summary>
		public StringFieldTest() {

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
		/// Test object instantiation.
		/// </summary>
		[Test( Description="Test object instantiation.")]
		public void Instantiation() {

			string value = "Test object instantiation.";
			StringField field = new StringField( 30);

			Assert.IsTrue( field.FieldNumber == 30);
			Assert.IsNull( field.FieldValue);

			field = new StringField( 15, value);

			Assert.IsTrue( field.FieldNumber == 15);
			Assert.IsTrue( value.Equals( field.FieldValue));
		}

		/// <summary>
		/// Test FieldValue property.
		/// </summary>
		[Test( Description="Test FieldValue property.")]
		public void FieldValue() {

			string value = "Test FieldValue property.";
			byte[] binaryValue = Encoding.UTF7.GetBytes( "Another value.");
			StringField field = new StringField( 18);

			Assert.IsNull( field.FieldValue);
			field.FieldValue = value;
			Assert.IsTrue( value.Equals( ( field.FieldValue)));
			field.Value = null;
			Assert.IsNull( field.FieldValue);

			field.Value = binaryValue;
			Assert.IsTrue( Encoding.UTF7.GetString( binaryValue).Equals( field.FieldValue));

			try {
				field.Value = field.FieldNumber;
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName == "value");
			}
		}

		/// <summary>
		/// Test ToString method.
		/// </summary>
		[Test( Description="Test ToString method.")]
		public void TestToString() {

			string value = "Test ToString method.";
			StringField field = new StringField( 12);

			Assert.IsTrue( string.Empty.Equals( field.ToString()));
			field.FieldValue = value;
			Assert.IsTrue( value.Equals( field.ToString()));
		}

		/// <summary>
		/// Test GetBytes method.
		/// </summary>
		[Test( Description="Test GetBytes method.")]
		public void GetBytes() {

			string value = "Test GetBytes method.";
			StringField field = new StringField( 19, value);

			byte[] binaryValue = field.GetBytes();
			Assert.IsTrue( Encoding.UTF7.GetString(
				binaryValue).Equals( value));

			field.FieldValue = null;
			binaryValue = field.GetBytes();
			Assert.IsNull( binaryValue);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void TestClone() {

			string value = "Test Clone method.";
			StringField field = new StringField( 14);

			StringField clonedField = ( StringField)( field.Clone());

			Assert.IsNull( clonedField.FieldValue);
			Assert.IsTrue( field.FieldNumber == clonedField.FieldNumber);

			field.FieldValue = value;
			clonedField = ( StringField)( field.Clone());

			Assert.IsTrue( field.FieldValue.Equals( clonedField.FieldValue));
			Assert.IsTrue( ( ( object)( field.FieldValue)) !=
				( ( object)( clonedField.FieldValue)));

			field.FieldValue = string.Empty;
			clonedField = ( StringField)( field.Clone());

			Assert.IsTrue( field.FieldValue.Equals( clonedField.FieldValue));
			Assert.IsTrue( ( ( object)( field.FieldValue)) !=
				( ( object)( clonedField.FieldValue)));
		}

		/// <summary>
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			string value = "Test NewComponent method.";
			StringField field = new StringField( 18, value);
			MessagingComponent component = field.NewComponent();

			Assert.IsTrue( component is StringField);
			Assert.IsTrue( field.FieldNumber ==
				( ( StringField)component).FieldNumber);
			Assert.IsNull( ( ( StringField)component).FieldValue);
		}
		#endregion
	}
}
