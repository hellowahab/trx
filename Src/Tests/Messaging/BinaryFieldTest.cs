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
	/// Test fixture for BinaryField.
	/// </summary>
	[TestFixture( Description="Binary field tests.")]
	public class BinaryFieldTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="BinaryFieldTest"/>.
		/// </summary>
		public BinaryFieldTest() {

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

			byte[] value = Encoding.UTF7.GetBytes( "Test object instantiation.");
			BinaryField field = new BinaryField( 30);

			Assert.IsTrue( field.FieldNumber == 30);
			Assert.IsNull( field.Value);

			field = new BinaryField( 15, value);

			Assert.IsTrue( field.FieldNumber == 15);
			Assert.IsTrue( field.Value is byte[]);
			byte[] fieldValue = ( byte[])( field.Value);
			Assert.IsTrue( value.Length == fieldValue.Length);
			for ( int i = 0; i < value.Length; i++) {
				Assert.IsTrue( value[i] == fieldValue[i]);
			}
		}

		/// <summary>
		/// Test Value property.
		/// </summary>
		[Test( Description="Test Value property.")]
		public void Value() {

			byte[] value = Encoding.UTF7.GetBytes( "Test FieldValue property.");
			string stringValue = "Another value.";
			BinaryField field = new BinaryField( 18);

			Assert.IsNull( field.Value);

			field.Value = value;
			Assert.IsTrue( field.Value is byte[]);
			byte[] fieldValue = ( byte[])( field.Value);
			Assert.IsTrue( value.Length == fieldValue.Length);
			for ( int i = 0; i < value.Length; i++) {
				Assert.IsTrue( value[i] == fieldValue[i]);
			}

			field.Value = stringValue;
			fieldValue = ( byte[])( field.Value);
			Assert.IsTrue( stringValue.Equals( Encoding.UTF7.GetString( fieldValue)));

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

			byte[] value = Encoding.UTF7.GetBytes( "Test ToString method.");
			BinaryField field = new BinaryField( 12);

			Assert.IsTrue( string.Empty.Equals( field.ToString()));
			field.Value = value;

			Assert.IsTrue( Encoding.UTF7.GetString( value).Equals( field.ToString()));
		}

		/// <summary>
		/// Test GetBytes method.
		/// </summary>
		[Test( Description="Test GetBytes method.")]
		public void GetBytes() {

			byte[] value = Encoding.UTF7.GetBytes( "Test GetBytes method.");
			BinaryField field = new BinaryField( 19, value);

			byte[] binaryValue = field.GetBytes();
			Assert.IsTrue( Encoding.UTF7.GetString(
				binaryValue).Equals( Encoding.UTF7.GetString( value)));

			field.Value = null;
			binaryValue = field.GetBytes();
			Assert.IsNull( binaryValue);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void TestClone() {

			byte[] value = Encoding.UTF7.GetBytes( "Test Clone method.");
			BinaryField field = new BinaryField( 14);

			BinaryField clonedField = ( BinaryField)( field.Clone());

			Assert.IsNull( clonedField.Value);
			Assert.IsTrue( field.FieldNumber == clonedField.FieldNumber);

			field.Value = value;
			clonedField = ( BinaryField)( field.Clone());

			Assert.IsTrue( field.ToString().Equals( clonedField.ToString()));
			Assert.IsTrue( field.Value != clonedField.Value);
		}

		/// <summary>
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			byte[] value = Encoding.UTF7.GetBytes( "Test NewComponent method.");
			BinaryField field = new BinaryField( 18, value);
			MessagingComponent component = field.NewComponent();

			Assert.IsTrue( component is BinaryField);
			Assert.IsTrue( field.FieldNumber ==
				( ( BinaryField)component).FieldNumber);
			Assert.IsNull( ( ( BinaryField)component).Value);
		}
		#endregion
	}
}
