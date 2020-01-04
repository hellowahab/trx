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
	/// Test fixture for FormatterContext.
	/// </summary>
	[TestFixture( Description="Formatter context tests.")]
	public class FormatterContextTest {

		private string _data;
		private byte[] _binaryData;

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="FormatterContextTest"/>.
		/// </summary>
		public FormatterContextTest() {

		}
		#endregion

		#region Methods
		/// <summary>
		/// This method will be called by NUnit for test setup.
		/// </summary>
		[SetUp]
		public void SetUp() {

			_data  = "Sample data";
			_binaryData = Encoding.UTF7.GetBytes( _data);
		}

		/// <summary>
		/// Test formatter context creation and initialization.
		/// </summary>
		[Test( Description="Test formatter context creation and initialization.")]
		public void CreationAndInitialization() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			Assert.IsTrue( formatterContext.BufferSize ==
				FormatterContext.DefaultBufferSize);

			Assert.IsTrue( formatterContext.UpperDataBound == 0);

			Assert.IsTrue( formatterContext.DataLength == 0);

			Assert.IsTrue( formatterContext.FreeBufferSpace ==
				formatterContext.BufferSize);

			Assert.IsTrue( formatterContext.FreeBufferSpace > 0);

            Assert.IsNull( formatterContext.CurrentMessage );
            Message msg = new Message();
            formatterContext.CurrentMessage = msg;
            Assert.IsTrue( formatterContext.CurrentMessage == msg );
		}

		/// <summary>
		/// Test explicit buffer resizing.
		/// </summary>
		[Test( Description="Test explicit buffer resizing.")]
		public void ExplicitBufferResizing() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			formatterContext.ResizeBuffer( 1);
			Assert.IsTrue( formatterContext.FreeBufferSpace ==
				( FormatterContext.DefaultBufferSize * 2));

			formatterContext.ResizeBuffer( FormatterContext.DefaultBufferSize + 1);
			Assert.IsTrue( formatterContext.FreeBufferSpace ==
				( FormatterContext.DefaultBufferSize * 4));

			try {
				formatterContext.ResizeBuffer( 0);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "count");
			}
		}

		/// <summary>
		/// Test implicit buffer resizing.
		/// </summary>
		[Test( Description="Test implicit buffer resizing.")]
		public void ImplicitBufferResizing() {

			FormatterContext formatterContext = new FormatterContext( _data.Length);

			Assert.IsTrue( formatterContext.BufferSize == _data.Length);
			Assert.IsTrue( formatterContext.DataLength == 0);

			formatterContext.Write( _data);
			Assert.IsTrue( formatterContext.DataLength == _data.Length);
			Assert.IsTrue( formatterContext.BufferSize == _data.Length);

			formatterContext.Clear();
			Assert.IsTrue( formatterContext.BufferSize == _data.Length);
			Assert.IsTrue( formatterContext.DataLength == 0);

			formatterContext.Write( _data);
			Assert.IsTrue( formatterContext.DataLength == _data.Length);
			Assert.IsTrue( formatterContext.BufferSize == _data.Length);
			formatterContext.Write( _data);
			Assert.IsTrue( formatterContext.DataLength == ( _data.Length * 2));
		}

		/// <summary>
		/// Test string writing.
		/// </summary>
		[Test( Description="Test string writing.")]
		public void StringWriting() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			formatterContext.Write( _data);
			byte[] availableData = formatterContext.GetData();

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == _binaryData.Length);

			for ( int i = availableData.Length - 1; i >= 0; i--) {
				Assert.IsTrue( availableData[i] == _binaryData[i]);
			}
		}

		/// <summary>
		/// Test binary writing.
		/// </summary>
		[Test( Description="Test binary writing.")]
		public void BinaryWriting() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			formatterContext.Write( _binaryData);
			byte[] availableData = formatterContext.GetData();

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == _binaryData.Length);

			for ( int i = availableData.Length - 1; i >= 0; i--) {
				Assert.IsTrue( availableData[i] == _binaryData[i]);
			}
		}

		/// <summary>
		/// Test partial binary writing.
		/// </summary>
		[Test( Description="Test partial binary writing.")]
		public void PartialBinaryWriting() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			int binaryDataOffset = 3;

			formatterContext.Write( _binaryData, binaryDataOffset,
				_binaryData.Length - binaryDataOffset);
			byte[] availableData = formatterContext.GetData();

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length ==
				( _binaryData.Length - binaryDataOffset));

			for ( int i = availableData.Length - 1; i >= 0; i--) {
				Assert.IsTrue( availableData[i] ==
					_binaryData[i + binaryDataOffset]);
			}
		}

		/// <summary>
		/// Test GetData and Clear methods.
		/// </summary>
		[Test( Description="Test GetData and Clear methods.")]
		public void GetDataAndClear() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			formatterContext.Write( _data);
			byte[] availableData = formatterContext.GetData();

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == _binaryData.Length);
			Assert.IsTrue( _data.Equals( Encoding.UTF7.GetString( availableData)));

			formatterContext.Clear();
			availableData = formatterContext.GetData();
			Assert.IsNull( availableData);
		}

		/// <summary>
		/// Test GetDataAsString and Clear methods.
		/// </summary>
		[Test( Description="Test GetDataAsString and Clear methods.")]
		public void GetDataAsStringAndClear() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);

			formatterContext.Write( _data);
			string availableData = formatterContext.GetDataAsString();

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == _binaryData.Length);
			Assert.IsTrue( _data.Equals( availableData));

			formatterContext.Clear();
			availableData = formatterContext.GetDataAsString();
			Assert.IsNull( availableData);
		}
		#endregion
	}
}
