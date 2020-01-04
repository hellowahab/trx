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

using System.Text;

using Trx.Messaging;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for Message.
	/// </summary>
	[TestFixture( Description="Message tests.")]
	public class MessageTest {

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

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="MessageTest"/>.
		/// </summary>
		public MessageTest() {

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
		[Test( Description="Test instantiation.")]
		public void Instantiation() {

			Message message = new Message();

			Assert.IsNull( message.Header);
			Assert.IsNotNull( message.Fields);
		}

		/// <summary>
		/// Test properties.
		/// </summary>
		[Test( Description="Test properties.")]
		public void Properties() {

			Message message = new Message();

			Assert.IsNull( message.Header);
			StringMessageHeader header = new StringMessageHeader( "HEADER");
			message.Header = header;
			Assert.IsNotNull( message.Header);
			Assert.IsTrue( message.Header == header);
			Assert.IsTrue( ( ( StringMessageHeader)
				( message.Header)).Value.Equals( "HEADER"));
			Assert.IsNotNull( message.Fields);

			Assert.IsNull( message.Formatter);
			BasicMessageFormatter formatter = new BasicMessageFormatter();
			message.Formatter = formatter;
			Assert.IsNotNull( message.Formatter);
			Assert.IsTrue( message.Formatter == formatter);

			Assert.IsNull( message[3]);
			message.Fields.Add( 3, "000000");
			Assert.IsNotNull( message[3]);
		}

		/// <summary>
		/// Test CopyTo method (all fields).
		/// </summary>
		[Test( Description="Test CopyTo method (all fields).")]
		public void CopyTo() {

			Message src = new Message();
			Message dst = new Message();

			Assert.IsTrue( src.Fields.Count == 0);
			Assert.IsNull( src.Header);
			Assert.IsTrue( dst.Fields.Count == 0);
			Assert.IsNull( dst.Header);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				src.Fields.Add( _fields[i]);
			}

			src.Header = new StringMessageHeader( "HEADER");

			Assert.IsTrue( src.Fields.Count == _fields.Length);

			src.CopyTo( dst);

			Assert.IsTrue( dst.Fields.Count == _fields.Length);
			Assert.IsNotNull( dst.Header);

			Assert.IsTrue( src.Header != dst.Header);
			for ( int i = 0; i < _fields.Length; i++) {
				Assert.IsTrue( src[_fields[i].FieldNumber] !=
					dst[_fields[i].FieldNumber]);
			}
		}

		/// <summary>
		/// Test CopyTo method (selected fields).
		/// </summary>
		[Test( Description="Test CopyTo method (selected fields).")]
		public void CopyToWithSelection() {

			Message src = new Message();
			Message dst = new Message();

			Assert.IsTrue( src.Fields.Count == 0);
			Assert.IsNull( src.Header);
			Assert.IsTrue( dst.Fields.Count == 0);
			Assert.IsNull( dst.Header);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				src.Fields.Add( _fields[i]);
			}

			src.Header = new StringMessageHeader( "HEADER");

			Assert.IsTrue( src.Fields.Count == _fields.Length);

			int[] fields = { 1, 2, 3, 4, 5, 6, 7};
			src.CopyTo( dst, fields);

			Assert.IsTrue( dst.Fields.Count == 4);
			Assert.IsNotNull( dst.Header);
			Assert.IsNotNull( dst[1]);
			Assert.IsNotNull( dst[2]);
			Assert.IsNotNull( dst[3]);
			Assert.IsNotNull( dst[4]);
			Assert.IsNull( dst[5]);
			Assert.IsNull( dst[6]);
			Assert.IsNull( dst[7]);

			Assert.IsTrue( src.Header != dst.Header);
			Assert.IsTrue( src[1] != dst[1]);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void Clone() {

			Message message = new Message();

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				message.Fields.Add( _fields[i]);
			}

			message.Header = new StringMessageHeader( "HEADER");

			Message cloned = ( Message)message.Clone();

			Assert.IsTrue( cloned.Fields.Count == message.Fields.Count);
			Assert.IsNotNull( cloned.Header);

			Assert.IsTrue( message.Header != cloned.Header);
			for ( int i = 0; i < _fields.Length; i++) {
				Assert.IsTrue( message[_fields[i].FieldNumber] !=
					cloned[_fields[i].FieldNumber]);
			}
		}

		/// <summary>
		/// Test ToString method.
		/// </summary>
		[Test( Description="Test ToString method.")]
		public void ToStringMethod() {

			Message message = new Message();

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				message.Fields.Add( _fields[i]);
			}

			message.Header = new StringMessageHeader( "HEADER");

			string data = message.ToString();

			Assert.IsNotNull( data);
			Assert.IsTrue( data.Equals( "H:HEADER,0:[1,2,3,4,11,12,25,48,49,64],1:[],2:1,3:000000,4:000000000100,11:000015,12:010102,25:0,48:SOME DATA,49:840,64:FFFFFFFFFFFFFFFF"));
		}

		/// <summary>
		/// Test MergeFields method.
		/// </summary>
		[Test( Description="Test MergeFields method.")]
		public void MergeFields() {

			Message src = new Message();
			Message dst = new Message();

			Assert.IsTrue( src.Fields.Count == 0);
			Assert.IsNull( src.Header);
			Assert.IsTrue( dst.Fields.Count == 0);
			Assert.IsNull( dst.Header);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				src.Fields.Add( _fields[i]);
			}
			for ( int i = _fields.Length - 7; i >= 0; i--) {
				dst.Fields.Add( _fields[i]);
			}
			dst.Fields.Add( 20, "ADDITIONAL FIELD");

			src.Header = new StringMessageHeader( "HEADER");

			Assert.IsTrue( src.Fields.Count == _fields.Length);

			dst.MergeFields( src);

			Assert.IsTrue( dst.Fields.Count == _fields.Length + 1);
			Assert.IsNull( dst.Header);

			for ( int i = 0; i < _fields.Length; i++) {
				Assert.IsTrue( src[_fields[i].FieldNumber] ==
					dst[_fields[i].FieldNumber]);
			}
		}

		/// <summary>
		/// Test CopyFields method.
		/// </summary>
		[Test( Description="Test CopyFields method.")]
		public void CopyFields() {

			Message src = new Message();
			Message dst = new Message();

			Assert.IsTrue( src.Fields.Count == 0);
			Assert.IsNull( src.Header);
			Assert.IsTrue( dst.Fields.Count == 0);
			Assert.IsNull( dst.Header);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				src.Fields.Add( _fields[i]);
			}
			for ( int i = _fields.Length - 7; i >= 0; i--) {
				dst.Fields.Add( _fields[i]);
			}
			dst.Fields.Add( 20, "ADDITIONAL FIELD");

			src.Header = new StringMessageHeader( "HEADER");

			Assert.IsTrue( src.Fields.Count == _fields.Length);

			dst.CopyFields( src);

			Assert.IsTrue( dst.Fields.Count == _fields.Length + 1);
			Assert.IsNull( dst.Header);

			for ( int i = 0; i < _fields.Length; i++) {
				Assert.IsTrue( src[_fields[i].FieldNumber] !=
					dst[_fields[i].FieldNumber]);
			}
		}

		/// <summary>
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			Message message = new Message();
			object newComponent = message.NewComponent();

			Assert.IsTrue( newComponent is Message);
		}
		
		/// <summary>
		/// Test CorrectBitMapsValues method.
		/// </summary>
		[Test( Description="Test CorrectBitMapsValues method.")]
		public void CorrectBitMapsValues() {

			Message message = new Message();

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				message.Fields.Add( _fields[i]);
				if ( _fields[i] is BitMapField) {
					( ( BitMapField)( _fields[i])).Clear();
				}
			}

			byte[] bytes = ( ( BitMapField)( message[0])).GetBytes();

			for ( int i = 0; i < bytes.Length; i++) {
				Assert.IsTrue( bytes[i] == 0);
			}

			message.CorrectBitMapsValues();

			for ( int i = 0; i < _exists.Length; i++) {
				if ( ( _exists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _exists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( ( ( BitMapField)( message[0])).IsSet( _exists[i]));
				if ( ( _notExists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _notExists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( !( ( BitMapField)( message[0])).IsSet( _notExists[i]));
			}

			message.Fields.Remove( 1);

			message.CorrectBitMapsValues();

			for ( int i = 0; i < _exists.Length; i++) {
				if ( _exists[i] == 1) {
					continue;
				}
				if ( ( _exists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _exists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( ( ( BitMapField)( message[0])).IsSet( _exists[i]));
				if ( ( _notExists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _notExists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( !( ( BitMapField)( message[0])).IsSet( _notExists[i]));
			}

			message.Fields.Remove( 3);
			message.Fields.Remove( 64);

			message.CorrectBitMapsValues();

			for ( int i = 0; i < _exists.Length; i++) {
				if ( ( _exists[i] == 1) || ( _exists[i] == 3) || ( _exists[i] == 64)) {
					continue;
				}
				if ( ( _exists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _exists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( ( ( BitMapField)( message[0])).IsSet( _exists[i]));
				if ( ( _notExists[i] < ( ( BitMapField)( message[0])).LowerFieldNumber) ||
					( _notExists[i] > ( ( BitMapField)( message[0])).UpperFieldNumber)) {
					continue;
				}
				Assert.IsTrue( !( ( BitMapField)( message[0])).IsSet( _notExists[i]));
			}
		}

		/// <summary>
		/// Test GetBytes method.
		/// </summary>
		[Test( Description="Test GetBytes method.")]
		public void GetBytes() {

			Message message = new Message();

			// Add fields.
			message.Fields.Add( 1, "DATA");

			byte[] messageBytes = message.GetBytes();

			Assert.IsNull( messageBytes);

			message.Formatter = new BasicMessageFormatter();

			( ( BasicMessageFormatter)( message.Formatter)).FieldFormatters.Add(
				new StringFieldFormatter( 1, new FixedLengthManager( 15), DataEncoder.GetInstance()));

			messageBytes = message.GetBytes();

			Assert.IsNotNull( messageBytes);
			Assert.IsTrue( Encoding.UTF7.GetString( messageBytes).Equals( "DATA           "));
		}
		#endregion
	}
}
