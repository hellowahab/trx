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
	/// Test fixture for FieldCollection.
	/// </summary>
	[TestFixture( Description="Field collection tests.")]
	public class FieldCollectionTest {

		private Field[] _fields = {
									  new BitMapField( 0, 1, 64),
									  new StringField( 48, "SOME DATA"),
									  new StringField( 25, "0"),
									  new StringField( 3, "000000"),
									  new StringField( 11, "000015"),
									  new BinaryField( 64, Encoding.UTF7.GetBytes( "FFFFFFFFFFFFFFFF")),
									  new BitMapField( 1, 65, 128),
									  new StringField( 12, "010102"),
									  new StringField( 2, "1"),
									  new BinaryField( 49, Encoding.UTF7.GetBytes( "840")),
									  new StringField( 4, "000000000100")};

		private int[] _exists =    {  0, 48, 25,  3, 11, 64,  1, 12,  2, 49,  4};
		private int[] _notExists = {  5,  6, 13, 15, 28, 16, 33, 41, 42, 63, 60};
		private int[] _maximum =   { 64, 64, 64, 64, 64, 49, 49, 49, 49, 4};

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="FieldCollectionTest"/>.
		/// </summary>
		public FieldCollectionTest() {

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
		/// Test index property.
		/// </summary>
		[Test( Description="Test index property.")]
		public void Index() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				fields.Add( _fields[i]);
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Check valid fields.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fields[_exists[i]]);
				Assert.IsTrue( fields.Contains( _exists[i]));
			}

			// Check invalid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
			}

			Assert.IsTrue( fields.MaximumFieldNumber == 64);
			Assert.IsTrue( fields.Count == _fields.Length);
		}

		/// <summary>
		/// Test Add method with a Field as a parameter.
		/// </summary>
		[Test( Description="Test Add method with a Field as a parameter.")]
		public void AddField() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Check valid fields.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fields[_exists[i]]);
				Assert.IsTrue( fields.Contains( _exists[i]));
			}

			// Check invalid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
			}

			Assert.IsTrue( fields.MaximumFieldNumber == 64);
			Assert.IsTrue( fields.Count == _fields.Length);
		}

		/// <summary>
		/// Test Add method with a string value as a parameter.
		/// </summary>
		[Test( Description="Test Add method with a string value as a parameter.")]
		public void AddStringField() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				if ( _fields[i] is StringField) {
					fields.Add( ( ( StringField)( _fields[i])).FieldNumber,
						( ( StringField)( _fields[i])).FieldValue);
					Assert.IsTrue( fields.Dirty);
					fields.Dirty = false;
				}
			}

			// Check valid fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				if ( _fields[i] is StringField) {
					Assert.IsNotNull( ( ( StringField)( _fields[i])).FieldNumber);
					Assert.IsTrue( fields.Contains( ( ( StringField)( _fields[i])).FieldNumber));
				}
			}

			// Check invalid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
			}

			Assert.IsTrue( fields.MaximumFieldNumber == 48);
			Assert.IsTrue( fields.Count == 7);
		}

		/// <summary>
		/// Test Add method with a binary value as a parameter.
		/// </summary>
		[Test( Description="Test Add method with a binary value as a parameter.")]
		public void AddBinaryField() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				if ( _fields[i] is BinaryField) {
					fields.Add( ( ( BinaryField)( _fields[i])).FieldNumber,
						( ( BinaryField)( _fields[i])).GetBytes());
					Assert.IsTrue( fields.Dirty);
					fields.Dirty = false;
				}
			}

			// Check valid fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				if ( _fields[i] is BinaryField) {
					Assert.IsNotNull( ( ( BinaryField)( _fields[i])).FieldNumber);
					Assert.IsTrue( fields.Contains( ( ( BinaryField)( _fields[i])).FieldNumber));
				}
			}

			// Check invalid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
			}

			Assert.IsTrue( fields.MaximumFieldNumber == 64);
			Assert.IsTrue( fields.Count == 2);
		}

		/// <summary>
		/// Test Remove method (only one field at a time).
		/// </summary>
		[Test( Description="Test Remove method (only one field at a time).")]
		public void RemoveOne() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Remove.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fields[_exists[i]]);
				Assert.IsTrue( fields.Contains( _exists[i]));
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
				fields.Remove( _exists[i]);
				fields.Remove( _notExists[i]);
				Assert.IsNull( fields[_exists[i]]);
				Assert.IsFalse( fields.Contains( _exists[i]));
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
				if ( i < _maximum.Length) {
					Assert.IsTrue( fields.MaximumFieldNumber == _maximum[i]);
				} else {
					try {
						int max = fields.MaximumFieldNumber;
						Assert.Fail();
					} catch ( ApplicationException) {
					}
				}
			}
		}

		/// <summary>
		/// Test Remove method (many fields at a time).
		/// </summary>
		[Test( Description="Test Remove method (many fields at a time).")]
		public void RemoveMany() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Remove all.
			Assert.IsTrue( fields.Count == _fields.Length);
			fields.Remove( _notExists);
			Assert.IsTrue( fields.Count == _fields.Length);
			fields.Remove( _exists);
			Assert.IsTrue( fields.Count == 0);

			// Add fields again.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Remove.
			int[] fieldsToRemove = new int[4];
			for ( int i = 0; i < _exists.Length; i += 2) {

				if ( ( i + 1) >= _exists.Length) {
					break;
				}

				fieldsToRemove[0] = _exists[i];
				fieldsToRemove[1] = _notExists[i];
				fieldsToRemove[2] = _exists[i + 1];
				fieldsToRemove[3] = _notExists[i + 1];

				Assert.IsNotNull( fields[_exists[i]]);
				Assert.IsTrue( fields.Contains( _exists[i]));
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
				Assert.IsNotNull( fields[_exists[i + 1]]);
				Assert.IsTrue( fields.Contains( _exists[i + 1]));
				Assert.IsNull( fields[_notExists[i + 1]]);
				Assert.IsFalse( fields.Contains( _notExists[i + 1]));

				fields.Remove( fieldsToRemove);

				Assert.IsNull( fields[_exists[i]]);
				Assert.IsFalse( fields.Contains( _exists[i]));
				Assert.IsNull( fields[_notExists[i]]);
				Assert.IsFalse( fields.Contains( _notExists[i]));
				Assert.IsNull( fields[_exists[i + 1]]);
				Assert.IsFalse( fields.Contains( _exists[i + 1]));
				Assert.IsNull( fields[_notExists[i + 1]]);
				Assert.IsFalse( fields.Contains( _notExists[i + 1]));

				Assert.IsTrue( fields.MaximumFieldNumber == _maximum[i + 1]);
			}

			Assert.IsTrue( fields.Count == 1);
			Assert.IsTrue( fields.MaximumFieldNumber == _maximum[_maximum.Length - 1]);
		}

		/// <summary>
		/// Test Clear method.
		/// </summary>
		[Test( Description="Test Clear method.")]
		public void Clear() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			fields.Clear();
			Assert.IsTrue( fields.Dirty);
			Assert.IsTrue( fields.Count == 0);
			try {
				int max = fields.MaximumFieldNumber;
				Assert.Fail();
			} catch ( ApplicationException) {
			}
			fields.Dirty = false;
			fields.Clear();
			Assert.IsFalse( fields.Dirty);
		}

		/// <summary>
		/// Test Contains method (only one field at a time).
		/// </summary>
		[Test( Description="Test Contains method (only one field at a time).")]
		public void ContainsOne() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			// Check valid fields.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsTrue( fields.Contains( _exists[i]));
			}

			// Check invalid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsFalse( fields.Contains( _notExists[i]));
			}
		}

		/// <summary>
		/// Test Contains method (many fields at a time).
		/// </summary>
		[Test( Description="Test Contains method (many fields at a time).")]
		public void ContainsMany() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			Assert.IsTrue( fields.Contains( _exists));
			Assert.IsFalse( fields.Contains( _notExists));
			int[] more = new int[4];
			more[0] = _exists[0];
			more[1] = _notExists[0];
			more[2] = _exists[1];
			more[3] = _notExists[1];
			Assert.IsFalse( fields.Contains( more));
			more[0] = _exists[0];
			more[1] = _exists[1];
			more[2] = _exists[2];
			more[3] = _exists[3];
			Assert.IsTrue( fields.Contains( more));
		}

		/// <summary>
		/// Test ContainsAtLeastOne method (array parameter).
		/// </summary>
		[Test( Description="Test ContainsAtLeastOne method (array parameter).")]
		public void ContainsAtLeastOneInArray() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			Assert.IsTrue( fields.ContainsAtLeastOne( _exists));
			Assert.IsFalse( fields.ContainsAtLeastOne( _notExists));
			int[] more = new int[4];
			more[0] = _notExists[0];
			more[1] = _notExists[1];
			more[2] = _exists[0];
			more[3] = _notExists[2];
			Assert.IsTrue( fields.ContainsAtLeastOne( more));
			more[0] = _notExists[0];
			more[1] = _notExists[1];
			more[2] = _notExists[2];
			more[3] = _notExists[3];
			Assert.IsFalse( fields.ContainsAtLeastOne( more));
			more[0] = _exists[0];
			more[1] = _exists[1];
			more[2] = _exists[2];
			more[3] = _exists[3];
			Assert.IsTrue( fields.ContainsAtLeastOne( more));
		}

		/// <summary>
		/// Test ContainsAtLeastOne method (range parameters).
		/// </summary>
		[Test( Description="Test ContainsAtLeastOne method (range parameters).")]
		public void ContainsAtLeastOneInRange() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			Assert.IsTrue( fields.ContainsAtLeastOne( 1, 5));
			Assert.IsTrue( fields.ContainsAtLeastOne( 1, 64));
			Assert.IsFalse( fields.ContainsAtLeastOne( 40, 45));
			Assert.IsFalse( fields.ContainsAtLeastOne( 65, 65));
			Assert.IsFalse( fields.ContainsAtLeastOne( 5, 1));
		}

		/// <summary>
		/// Test MoveField method.
		/// </summary>
		[Test( Description="Test MoveField method.")]
		public void MoveField() {

			FieldCollection fields = new FieldCollection();

			Assert.IsFalse( fields.Dirty);

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
				Assert.IsTrue( fields.Dirty);
				fields.Dirty = false;
			}

			try {
				fields.MoveField( _notExists[0], _exists[0]);
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName.Equals( "oldFieldNumber"));
			}

			for ( int i = 0; i < _exists.Length; i++) {
				fields.MoveField( _exists[i], _notExists[i]);
			}

			// Check valid fields.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsTrue( fields.Contains( _notExists[i]));
			}

			// Check invalid fields.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsFalse( fields.Contains( _exists[i]));
			}
		}

		/// <summary>
		/// Test enumeration.
		/// </summary>
		[Test( Description="Test enumeration.")]
		public void Enumeration() {

			FieldCollection fields = new FieldCollection();
			FieldCollection copy = new FieldCollection();

			// Add fields.
			for ( int i = _fields.Length - 1; i >= 0; i--) {
				fields.Add( ( Field)( _fields[i]));
			}

			// Copy collection.
			foreach ( Field field in fields) {
				copy.Add( field);
			}

			// Check valid fields.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( copy[_exists[i]]);
				Assert.IsTrue( copy.Contains( _exists[i]));
			}
		}
		#endregion
	}
}
