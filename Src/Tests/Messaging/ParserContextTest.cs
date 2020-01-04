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
	/// Test fixture for ParserContext.
	/// </summary>
	[TestFixture( Description="Parser context tests.")]
	public class ParserContextTest {

		private string _data;
		private byte[] _binaryData;

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="ParserContextTest"/>.
		/// </summary>
		public ParserContextTest() {

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
		/// Test parser context creation and initialization.
		/// </summary>
		[Test( Description="Test parser context creation and initialization.")]
		public void CreationAndInitialization() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			Assert.IsTrue( parserContext.BufferSize ==
				ParserContext.DefaultBufferSize);

			Assert.IsTrue( parserContext.LowerDataBound == 0);
			Assert.IsTrue( parserContext.UpperDataBound == 0);
			Assert.IsTrue( parserContext.DecodedLength == int.MinValue);
			Assert.IsTrue( parserContext.CurrentField == 0);
			Assert.IsFalse( parserContext.Signaled);
            Assert.IsFalse( parserContext.AnnouncementHasBeenConsumed );
            Assert.IsTrue( parserContext.FrontierUpperBound == int.MinValue );

			Assert.IsTrue( parserContext.DataLength == 0);

			Assert.IsTrue( parserContext.FreeBufferSpace ==
				parserContext.BufferSize);

			Assert.IsTrue( parserContext.FreeBufferSpace > 0);
		}

		/// <summary>
		/// Test explicit buffer resizing.
		/// </summary>
		[Test( Description="Test explicit buffer resizing.")]
		public void ExplicitBufferResizing() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.ResizeBuffer( 1);
			Assert.IsTrue( parserContext.FreeBufferSpace ==
				( ParserContext.DefaultBufferSize * 2));

			parserContext.ResizeBuffer( ParserContext.DefaultBufferSize + 1);
			Assert.IsTrue( parserContext.FreeBufferSpace ==
				( ParserContext.DefaultBufferSize * 4));

			try {
				parserContext.ResizeBuffer( 0);
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

			ParserContext parserContext = new ParserContext( _data.Length);

			Assert.IsTrue( parserContext.BufferSize == _data.Length);
			Assert.IsTrue( parserContext.DataLength == 0);

			parserContext.Write( _data);
			Assert.IsTrue( parserContext.DataLength == _data.Length);
			Assert.IsTrue( parserContext.BufferSize == _data.Length);

			parserContext.Clear();
			Assert.IsTrue( parserContext.BufferSize == _data.Length);
			Assert.IsTrue( parserContext.DataLength == 0);

			parserContext.Write( _data);
			Assert.IsTrue( parserContext.DataLength == _data.Length);
			Assert.IsTrue( parserContext.BufferSize == _data.Length);

			parserContext.Consumed( 5);
			Assert.IsTrue( parserContext.GetDataAsString( false).Equals(
				_data.Substring( 5)));				
			Assert.IsTrue( parserContext.BufferSize == _data.Length);
			Assert.IsTrue( parserContext.DataLength == _data.Length - 5);

			parserContext.Write( _data.Substring( 0, 5));
			Assert.IsTrue( parserContext.BufferSize == _data.Length);
			Assert.IsTrue( parserContext.DataLength == _data.Length);
			Assert.IsTrue( parserContext.GetDataAsString( false).Equals(
				"e dataSampl"));				

			parserContext.Consumed( 5);
			parserContext.Write( _data);
			Assert.IsTrue( parserContext.DataLength == _data.Length + _data.Length - 5);
			Assert.IsTrue( parserContext.GetDataAsString( false).Equals(
				"aSamplSample data"));
		}

		/// <summary>
		/// Test string writing.
		/// </summary>
		[Test( Description="Test string writing.")]
		public void StringWriting() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.Write( _data);
			byte[] availableData = parserContext.GetData( false);

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

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.Write( _binaryData);
			byte[] availableData = parserContext.GetData( false);

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

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			int binaryDataOffset = 3;

			parserContext.Write( _binaryData, binaryDataOffset,
				_binaryData.Length - binaryDataOffset);
			byte[] availableData = parserContext.GetData( false);

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

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.Write( _data);

			Assert.IsNull( parserContext.GetData( true, 0));

			byte[] availableData = parserContext.GetData( false);

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == _binaryData.Length);
			Assert.IsTrue( _data.Equals( Encoding.UTF7.GetString( availableData)));

			parserContext.Clear();
			availableData = parserContext.GetData( false);
			Assert.IsNull( availableData);
		}

		/// <summary>
		/// Test partial GetData and consume.
		/// </summary>
		[Test( Description="Test partial GetData and consume.")]
		public void PartialGetData() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.Write( _data);
			parserContext.Write( _data);
			byte[] partialData = parserContext.GetData( true, _data.Length - 1);

			Assert.IsNotNull( partialData);
			Assert.IsTrue( parserContext.DataLength == _data.Length + 1);
			Assert.IsTrue( _data.Substring( 0, _data.Length - 1).Equals(
				Encoding.UTF7.GetString( partialData)));

			partialData = parserContext.GetData( true, 1);

			Assert.IsNotNull( partialData);
			Assert.IsTrue( partialData.Length == 1);
			Assert.IsTrue( partialData[0] == _binaryData[_binaryData.Length - 1]);
			Assert.IsTrue( parserContext.DataLength == _data.Length);

			partialData = parserContext.GetData( true, _data.Length);
			Assert.IsNotNull( partialData);
			Assert.IsTrue( parserContext.DataLength == 0);
			Assert.IsTrue( _data.Equals( Encoding.UTF7.GetString( partialData)));

			try {
				partialData = parserContext.GetData( true, _data.Length);
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName == "count");
			}
		}

		/// <summary>
		/// Test GetDataAsString method.
		/// </summary>
		[Test( Description="Test GetDataAsString method.")]
		public void GetDataAsString() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			Assert.IsNull( parserContext.GetDataAsString( true, 0));

			parserContext.Write( _data);
			parserContext.Write( _data);
			string partialData = parserContext.GetDataAsString(
				true, _data.Length - 1);

			Assert.IsNotNull( partialData);
			Assert.IsTrue( parserContext.DataLength == _data.Length + 1);
			Assert.IsTrue( _data.Substring( 0,
				_data.Length - 1).Equals( partialData));

			partialData = parserContext.GetDataAsString( true, 1);

			Assert.IsNotNull( partialData);
			Assert.IsTrue( partialData.Length == 1);
			Assert.IsTrue( partialData[0] == _data[_data.Length - 1]);
			Assert.IsTrue( parserContext.DataLength == _data.Length);

			partialData = parserContext.GetDataAsString( true, _data.Length);
			Assert.IsNotNull( partialData);
			Assert.IsTrue( parserContext.DataLength == 0);
			Assert.IsTrue( _data.Equals( partialData));

			parserContext.Write( _data);
			partialData = parserContext.GetDataAsString( true, 1);
			partialData = parserContext.GetDataAsString( true);
			Assert.IsNotNull( partialData);
			Assert.IsTrue( parserContext.DataLength == 0);
			Assert.IsTrue( _data.Substring( 1, _data.Length - 1).Equals( partialData));

			try {
				partialData = parserContext.GetDataAsString( true, _data.Length);
				Assert.Fail();
			} catch ( ArgumentException e) {
				Assert.IsTrue( e.ParamName == "count");
			}
		}

		/// <summary>
		/// Test Consumed method.
		/// </summary>
		[Test( Description="Test Consumed method.")]
		public void Consumed() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			int consumedCount = 3;

			parserContext.Write( _data);
			parserContext.Consumed( consumedCount);

			byte[] availableData = parserContext.GetData( false);

			Assert.IsNotNull( availableData);
			Assert.IsTrue( availableData.Length == ( _binaryData.Length - consumedCount));

			Assert.IsTrue( _data.Substring( consumedCount).Equals(
				Encoding.UTF7.GetString( availableData)));

			try {
				parserContext.Consumed( parserContext.BufferSize + 1);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "count");
			}

			try {
				parserContext.Consumed( -1);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "count");
			}
		}

		/// <summary>
		/// Test the consumption of all the data.
		/// </summary>
		[Test( Description="Test the consumption of all the data.")]
		public void ConsumeAllData() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			parserContext.Write( _data);

			parserContext.Consumed( 1);
			Assert.IsTrue( parserContext.DataLength == ( _data.Length - 1));

			parserContext.Consumed( _data.Length - 1);
			Assert.IsTrue( parserContext.DataLength == 0);

			Assert.IsTrue( parserContext.LowerDataBound == 0);
			Assert.IsTrue( parserContext.UpperDataBound == 0);

			try {
				parserContext.Consumed( parserContext.BufferSize + 1);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName == "count");
			}
		}

		/// <summary>
		/// Test ResetDecodedLength method.
		/// </summary>
		[Test( Description="Test ResetDecodedLength method.")]
		public void ResetDecodedLength() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			Assert.IsTrue( parserContext.DecodedLength == int.MinValue);
			parserContext.DecodedLength = 15;
			Assert.IsTrue( parserContext.DecodedLength == 15);
            Assert.IsFalse( parserContext.AnnouncementHasBeenConsumed );
            parserContext.AnnouncementHasBeenConsumed = true;
            Assert.IsTrue( parserContext.AnnouncementHasBeenConsumed );
            parserContext.ResetDecodedLength();
			Assert.IsTrue( parserContext.DecodedLength == int.MinValue);
            Assert.IsFalse( parserContext.AnnouncementHasBeenConsumed );
        }

		/// <summary>
		/// Test MessageHasBeenConsumed method.
		/// </summary>
		[Test( Description="Test MessageHasBeenConsumed method.")]
		public void MessageHasBeenConsumed() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);

			Assert.IsTrue( parserContext.CurrentField == 0);
			Assert.IsNull( parserContext.CurrentBitMap);
			Assert.IsNull( parserContext.CurrentMessage);
			Assert.IsFalse( parserContext.Signaled);
			parserContext.CurrentField = 1;
			parserContext.Signaled = true;
			Assert.IsTrue( parserContext.Signaled);
			Assert.IsTrue( parserContext.CurrentField == 1);
			parserContext.CurrentBitMap = new BitMapField( 0, 1, 64);
			Assert.IsNotNull( parserContext.CurrentBitMap);
			parserContext.CurrentMessage = new Message();
			Assert.IsNotNull( parserContext.CurrentMessage);
            Assert.IsTrue( parserContext.FrontierUpperBound == int.MinValue );
            parserContext.FrontierUpperBound = 15;
            Assert.IsTrue( parserContext.FrontierUpperBound == 15 );
			parserContext.MessageHasBeenConsumed();
            Assert.IsTrue( parserContext.CurrentField == 0 );
			Assert.IsNull( parserContext.CurrentBitMap);
			Assert.IsNull( parserContext.CurrentMessage);
			Assert.IsFalse( parserContext.Signaled);
            Assert.IsTrue( parserContext.FrontierUpperBound == int.MinValue );
        }
		#endregion
	}
}