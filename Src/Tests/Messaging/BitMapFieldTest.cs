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
	/// Test fixture for BitMapField.
	/// </summary>
	[TestFixture( Description="Bitmap field tests.")]
	public class BitMapFieldTest {

		private int[] firstFields = { 1, 8, 20, 24, 64};
		private byte[] firstBitmap = { 0x81, 0, 0x11, 0, 0, 0, 0, 1};

		private int[] secondFields = { 22, 29, 41, 45, 46, 47, 48};
		private byte[] secondBitmap = { 0x81, 0, 0x11, 0xE0};

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="BitMapFieldTest"/>.
		/// </summary>
		public BitMapFieldTest() {

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

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			Assert.IsTrue( bitmap.FieldNumber == 0);
			Assert.IsTrue( bitmap.LowerFieldNumber == 1);
			Assert.IsTrue( bitmap.UpperFieldNumber == 64);
			Assert.IsTrue( bitmap.GetBytes().Length == 8);
			byte[] bytes = ( byte[])bitmap.Value;
			for ( int i = 0; i < bytes.Length; i++) {
				Assert.IsTrue( bytes[i] == 0);
			}

			bitmap = new BitMapField( 0, 1, 64, firstBitmap);
			bytes = ( byte[])bitmap.Value;
			for ( int i = 0; i < firstBitmap.Length; i++) {
				Assert.IsTrue( bytes[i] == firstBitmap[i]);
			}

			bitmap = new BitMapField( 1, bitmap);
			Assert.IsTrue( bitmap.FieldNumber == 1);
			Assert.IsTrue( bitmap.LowerFieldNumber == 1);
			Assert.IsTrue( bitmap.UpperFieldNumber == 64);
			Assert.IsTrue( bitmap.GetBytes().Length == 8);
			bytes = ( byte[])bitmap.Value;
			for ( int i = 0; i < bytes.Length; i++) {
				Assert.IsTrue( bytes[i] == firstBitmap[i]);
			}

			bitmap = new BitMapField( 65, 129, 150);
			Assert.IsTrue( bitmap.FieldNumber == 65);
			Assert.IsTrue( bitmap.LowerFieldNumber == 129);
			Assert.IsTrue( bitmap.UpperFieldNumber == 150);
			Assert.IsTrue( bitmap.GetBytes().Length == 3);
			bytes = ( byte[])bitmap.Value;
			for ( int i = 0; i < bytes.Length; i++) {
				Assert.IsTrue( bytes[i] == 0);
			}

			bitmap = new BitMapField( 0, 30, 30);
			Assert.IsTrue( bitmap.FieldNumber == 0);
			Assert.IsTrue( bitmap.LowerFieldNumber == 30);
			Assert.IsTrue( bitmap.UpperFieldNumber == 30);
			Assert.IsTrue( bitmap.GetBytes().Length == 1);

			bitmap = new BitMapField( 0, 1, 65);
			Assert.IsTrue( bitmap.FieldNumber == 0);
			Assert.IsTrue( bitmap.LowerFieldNumber == 1);
			Assert.IsTrue( bitmap.UpperFieldNumber == 65);
			Assert.IsTrue( bitmap.GetBytes().Length == 9);

			try {
				bitmap = new BitMapField( 65, 150, 129);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "lowerFieldNumber"));
			}

			try {
				bitmap = new BitMapField( 0, 1, 64, secondBitmap);
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName.Equals( "value"));
			}
		}

		/// <summary>
		/// Test Set method.
		/// </summary>
		[Test( Description="Test Set method.")]
		public void Set() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			for ( int i = 0; i < firstFields.Length; i++) {
				bitmap.Set( firstFields[i], true);
			}
			byte[] bitmapBytes = bitmap.GetBytes();
			for ( int i = 0; i < firstBitmap.Length; i++) {
				Assert.IsTrue( bitmapBytes[i] == firstBitmap[i]);
			}

			bitmap = new BitMapField( 0, 22, 48);
			for ( int i = 0; i < secondFields.Length; i++) {
				bitmap.Set( secondFields[i], true);
			}
			bitmapBytes = ( byte[])bitmap.Value;
			for ( int i = 0; i < secondBitmap.Length; i++) {
				Assert.IsTrue( bitmapBytes[i] == secondBitmap[i]);
			}

			try {
				bitmap.Set( 49, true);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "fieldNumber"));
			}
		}

		/// <summary>
		/// Test IsSet method.
		/// </summary>
		[Test( Description="Test IsSet method.")]
		public void IsSet() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			for ( int i = bitmap.LowerFieldNumber; i <= bitmap.UpperFieldNumber; i++) {
				Assert.IsFalse( bitmap.IsSet( i));
			}
			for ( int i = 0; i < firstFields.Length; i++) {
				bitmap.Set( firstFields[i], true);
			}
			for ( int i = 0; i < firstFields.Length; i++) {
				Assert.IsTrue( bitmap.IsSet( firstFields[i]));
			}

			bitmap = new BitMapField( 0, 22, 48);
			for ( int i = bitmap.LowerFieldNumber; i <= bitmap.UpperFieldNumber; i++) {
				Assert.IsFalse( bitmap.IsSet( i));
			}
			for ( int i = 0; i < secondFields.Length; i++) {
				bitmap.Set( secondFields[i], true);
			}
			for ( int i = 0; i < secondFields.Length; i++) {
				Assert.IsTrue( bitmap.IsSet( secondFields[i]));
			}
		}

		/// <summary>
		/// Test Set and IsSet methods.
		/// </summary>
		[Test( Description="Test Set and IsSet methods.")]
		public void SetAndIsSet() {

			BitMapField bitmap = new BitMapField( 0, 22, 48);
			bitmap.Set( 25, true);
			Assert.IsTrue( bitmap.IsSet( 25));
			bitmap.Set( 25, false);
			Assert.IsFalse( bitmap.IsSet( 25));
			Assert.IsFalse( bitmap.IsSet( 30));
			bitmap.Set( 30, true);
			Assert.IsTrue( bitmap.IsSet( 30));
			Assert.IsFalse( bitmap.IsSet( 31));
			bitmap.Set( 31, true);
			Assert.IsTrue( bitmap.IsSet( 30));
			Assert.IsTrue( bitmap.IsSet( 31));
		}

		/// <summary>
		/// Test Clear method.
		/// </summary>
		[Test( Description="Test Clear method.")]
		public void Clear() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			for ( int i = 0; i < firstFields.Length; i++) {
				bitmap.Set( firstFields[i], true);
			}
			bitmap.Clear();
			for ( int i = bitmap.LowerFieldNumber; i <= bitmap.UpperFieldNumber; i++) {
				Assert.IsFalse( bitmap.IsSet( i));
			}
		}

		/// <summary>
		/// Test GetBytes method.
		/// </summary>
		[Test( Description="Test GetBytes method.")]
		public void GetBytes() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			for ( int i = 0; i < firstFields.Length; i++) {
				bitmap.Set( firstFields[i], true);
			}

			byte[] bytes = bitmap.GetBytes();

			Assert.IsTrue( bytes.Length == firstBitmap.Length);
			for ( int i = 0; i < bytes.Length; i++) {
				Assert.IsTrue( bytes[i] == firstBitmap[i]);
			}
		}

		/// <summary>
		/// Test SetValue method.
		/// </summary>
		[Test( Description="Test SetValue method.")]
		public void SetValue() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			bitmap.SetFieldValue( firstBitmap);
			for ( int i = 0; i < firstFields.Length; i++) {
				Assert.IsTrue( bitmap.IsSet( firstFields[i]));
			}
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void TestClone() {

			BitMapField bitmap = new BitMapField( 0, 1, 64);
			for ( int i = 0; i < firstFields.Length; i++) {
				bitmap.Set( firstFields[i], true);
			}
			BitMapField clonedBitmap = ( BitMapField)( bitmap.Clone());

			Assert.IsTrue( bitmap.FieldNumber == clonedBitmap.FieldNumber);
			byte[] firstBitmapBytes = bitmap.GetBytes();
			byte[] clonedBitmapBytes = clonedBitmap.GetBytes();
			for ( int i = 0; i < firstBitmapBytes.Length; i++) {
				Assert.IsTrue( firstBitmapBytes[i] == clonedBitmapBytes[i]);
			}
		}

		/// <summary>
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			BitMapField field = new BitMapField( 1, 65, 128);
			MessagingComponent component = field.NewComponent();

			Assert.IsTrue( component is BitMapField);
			Assert.IsTrue( field.FieldNumber ==
				( ( BitMapField)component).FieldNumber);
		}
		#endregion
	}
}