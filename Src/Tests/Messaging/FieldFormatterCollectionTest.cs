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
	/// Test fixture for FieldFormatterCollection.
	/// </summary>
	[TestFixture( Description="FieldFormatter formatter collection tests.")]
	public class FieldFormatterCollectionTest {

		private FieldFormatter[] _fieldFormatters = {
			new StringFieldFormatter( 0, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 48, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 25, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 3, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 11, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 64, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 1, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 12, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 49, new FixedLengthManager( 1), DataEncoder.GetInstance()),
			new StringFieldFormatter( 4, new FixedLengthManager( 1), DataEncoder.GetInstance())};

		private int[] _exists =    {  0, 48, 25,  3, 11, 64,  1, 12,  2, 49,  4};
		private int[] _notExists = {  5,  6, 13, 15, 28, 16, 33, 41, 42, 63, 60};
		private int[] _maximum =   { 64, 64, 64, 64, 64, 49, 49, 49, 49, 4};

		private bool _clearedEventHasBeenReceived;
		private FieldFormatterEventArgs _addedEventArgs;
		private FieldFormatterEventArgs _removedEventArgs;

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="FieldFormatterCollectionTest"/>.
		/// </summary>
		public FieldFormatterCollectionTest() {

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

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			// Add field Formatters.
			for ( int i = 0; i < _fieldFormatters.Length; i++) {
				fieldFormatters.Add( _fieldFormatters[i]);
			}

			// Check valid field formatters.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fieldFormatters[_exists[i]]);
				Assert.IsTrue( fieldFormatters.Contains( _exists[i]));
			}

			// Check invalid field formatters.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
			}

			Assert.IsTrue( fieldFormatters.MaximumFieldFormatterNumber == 64);
			Assert.IsTrue( fieldFormatters.Count == _fieldFormatters.Length);
		}

		private void OnFieldFormattersAdded( object sender, FieldFormatterEventArgs e) {

			_addedEventArgs = e;
		}

		/// <summary>
		/// Test Add method.
		/// </summary>
		[Test( Description="Test Add method.")]
		public void AddField() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			fieldFormatters.Added += new FieldFormatterAddedEventHandler(
				OnFieldFormattersAdded);
			_removedEventArgs = null;
			fieldFormatters.Removed += new FieldFormatterRemovedEventHandler(
				OnFieldFormattersRemoved);

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				_addedEventArgs = null;
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
				Assert.IsNull( _removedEventArgs);
				Assert.IsNotNull( _addedEventArgs);
				Assert.IsTrue( _addedEventArgs.FieldFormatter ==
					( FieldFormatter)( _fieldFormatters[i]));
			}

			// Check valid field formatters.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fieldFormatters[_exists[i]]);
				Assert.IsTrue( fieldFormatters.Contains( _exists[i]));
			}

			// Check invalid field formatters.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
			}

			Assert.IsTrue( fieldFormatters.MaximumFieldFormatterNumber == 64);
			Assert.IsTrue( fieldFormatters.Count == _fieldFormatters.Length);

			// Substitute one field formatter.
			FieldFormatter toReplace = fieldFormatters[_exists[0]];
			FieldFormatter toAdd = new StringFieldFormatter( _exists[0],
				new FixedLengthManager( 20), DataEncoder.GetInstance());
			_addedEventArgs = null;
			fieldFormatters.Add( toAdd);
			Assert.IsNotNull( _removedEventArgs);
			Assert.IsNotNull( _addedEventArgs);
			Assert.IsTrue( _addedEventArgs.FieldFormatter == toAdd);
			Assert.IsTrue( _removedEventArgs.FieldFormatter == toReplace);
		}

		private void OnFieldFormattersRemoved( object sender, FieldFormatterEventArgs e) {

			_removedEventArgs = e;
		}

		/// <summary>
		/// Test Remove method (only one field formatter at a time).
		/// </summary>
		[Test( Description="Test Remove method (only one field formatter at a time).")]
		public void RemoveOne() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			fieldFormatters.Removed += new FieldFormatterRemovedEventHandler(
				OnFieldFormattersRemoved);

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			// Remove.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( fieldFormatters[_exists[i]]);
				Assert.IsTrue( fieldFormatters.Contains( _exists[i]));
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
				_removedEventArgs = null;
				FieldFormatter fieldFormatter = fieldFormatters[_exists[i]];
				fieldFormatters.Remove( _exists[i]);
				Assert.IsNotNull( _removedEventArgs);
				Assert.IsTrue( _removedEventArgs.FieldFormatter == fieldFormatter);
				_removedEventArgs = null;
				fieldFormatters.Remove( _notExists[i]);
				Assert.IsNull( _removedEventArgs);
				Assert.IsNull( fieldFormatters[_exists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _exists[i]));
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
				if ( i < _maximum.Length) {
					Assert.IsTrue( fieldFormatters.MaximumFieldFormatterNumber == _maximum[i]);
				} else {
					try {
						int max = fieldFormatters.MaximumFieldFormatterNumber;
						Assert.Fail();
					} catch ( ApplicationException) {
					}
				}
			}
		}

		/// <summary>
		/// Test Remove method (many field formatters at a time).
		/// </summary>
		[Test( Description="Test Remove method (many field formatters at a time).")]
		public void RemoveMany() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			// Remove all.
			Assert.IsTrue( fieldFormatters.Count == _fieldFormatters.Length);
			fieldFormatters.Remove( _notExists);
			Assert.IsTrue( fieldFormatters.Count == _fieldFormatters.Length);
			fieldFormatters.Remove( _exists);
			Assert.IsTrue( fieldFormatters.Count == 0);

			// Add field formatters again.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
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

				Assert.IsNotNull( fieldFormatters[_exists[i]]);
				Assert.IsTrue( fieldFormatters.Contains( _exists[i]));
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
				Assert.IsNotNull( fieldFormatters[_exists[i + 1]]);
				Assert.IsTrue( fieldFormatters.Contains( _exists[i + 1]));
				Assert.IsNull( fieldFormatters[_notExists[i + 1]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i + 1]));

				fieldFormatters.Remove( fieldsToRemove);

				Assert.IsNull( fieldFormatters[_exists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _exists[i]));
				Assert.IsNull( fieldFormatters[_notExists[i]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
				Assert.IsNull( fieldFormatters[_exists[i + 1]]);
				Assert.IsFalse( fieldFormatters.Contains( _exists[i + 1]));
				Assert.IsNull( fieldFormatters[_notExists[i + 1]]);
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i + 1]));

				Assert.IsTrue( fieldFormatters.MaximumFieldFormatterNumber == _maximum[i + 1]);
			}

			Assert.IsTrue( fieldFormatters.Count == 1);
			Assert.IsTrue( fieldFormatters.MaximumFieldFormatterNumber == _maximum[_maximum.Length - 1]);
		}

		private void OnFieldFormattersCleared( object sender, EventArgs e) {

			_clearedEventHasBeenReceived = true;
		}

		/// <summary>
		/// Test Clear method.
		/// </summary>
		[Test( Description="Test Clear method.")]
		public void Clear() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			fieldFormatters.Cleared += new FieldFormatterClearedEventHandler(
				OnFieldFormattersCleared);

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			_clearedEventHasBeenReceived = false;
			fieldFormatters.Clear();
			Assert.IsTrue( fieldFormatters.Count == 0);
			Assert.IsTrue( _clearedEventHasBeenReceived);
			_clearedEventHasBeenReceived = false;
			fieldFormatters.Clear();
			Assert.IsFalse( _clearedEventHasBeenReceived);
			try {
				int max = fieldFormatters.MaximumFieldFormatterNumber;
				Assert.Fail();
			} catch ( ApplicationException) {
			}
		}

		/// <summary>
		/// Test Contains method (only one field formatter at a time).
		/// </summary>
		[Test( Description="Test Contains method (only one field formatter at a time).")]
		public void ContainsOne() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			// Check valid field formatters.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsTrue( fieldFormatters.Contains( _exists[i]));
			}

			// Check invalid field formatters.
			for ( int i = 0; i < _notExists.Length; i++) {
				Assert.IsFalse( fieldFormatters.Contains( _notExists[i]));
			}
		}

		/// <summary>
		/// Test Contains method (many field formatters at a time).
		/// </summary>
		[Test( Description="Test Contains method (many field formatters at a time).")]
		public void ContainsMany() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			Assert.IsTrue( fieldFormatters.Contains( _exists));
			Assert.IsFalse( fieldFormatters.Contains( _notExists));
			int[] more = new int[4];
			more[0] = _exists[0];
			more[1] = _notExists[0];
			more[2] = _exists[1];
			more[3] = _notExists[1];
			Assert.IsFalse( fieldFormatters.Contains( more));
			more[0] = _exists[0];
			more[1] = _exists[1];
			more[2] = _exists[2];
			more[3] = _exists[3];
			Assert.IsTrue( fieldFormatters.Contains( more));
		}

		/// <summary>
		/// Test enumeration.
		/// </summary>
		[Test( Description="Test enumeration.")]
		public void Enumeration() {

			FieldFormatterCollection fieldFormatters = new FieldFormatterCollection();
			FieldFormatterCollection copy = new FieldFormatterCollection();

			// Add field formatters.
			for ( int i = _fieldFormatters.Length - 1; i >= 0; i--) {
				fieldFormatters.Add( ( FieldFormatter)( _fieldFormatters[i]));
			}

			// Copy collection.
			foreach ( FieldFormatter fieldFormatter in fieldFormatters) {
				copy.Add( fieldFormatter);
			}

			// Check valid field formatters.
			for ( int i = 0; i < _exists.Length; i++) {
				Assert.IsNotNull( copy[_exists[i]]);
				Assert.IsTrue( copy.Contains( _exists[i]));
			}
		}
		#endregion
	}
}