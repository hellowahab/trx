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
using Trx.Messaging.Iso8583;
using NUnit.Framework;

namespace Tests.Trx.Messaging.Iso8583 {

	/// <summary>
	/// Test fixture for Iso8583Message.
	/// </summary>
	[TestFixture( Description="Test fixture for Iso8583Message.")]
	public class Iso8583MessageTest {

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
		/// <see cref="Iso8583MessageTest"/>.
		/// </summary>
		public Iso8583MessageTest() {

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
		/// Test instantiation and properties.
		/// </summary>
		[Test( Description="Test instantiation and properties.")]
		public void InstantiationAndProperties() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1200;
			Assert.IsTrue( message.MessageTypeIdentifier == 1200);

			message = new Iso8583Message( 1220);
			message.MessageTypeIdentifier = 1220;
		}

		/// <summary>
		/// Test IsRequest method.
		/// </summary>
		[Test( Description="Test IsRequest method.")]
		public void IsRequest() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1200;
			Assert.IsTrue( message.IsRequest());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsRequest());

			message.MessageTypeIdentifier = 1420;
			Assert.IsTrue( message.IsRequest());

			message.MessageTypeIdentifier = 1430;
			Assert.IsFalse( message.IsRequest());
		}

		/// <summary>
		/// Test IsAdvice method.
		/// </summary>
		[Test( Description="Test IsAdvice method.")]
		public void IsAdvice() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1100;
			Assert.IsFalse( message.IsAdvice());

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsAdvice());

			message.MessageTypeIdentifier = 1220;
			Assert.IsTrue( message.IsAdvice());

			message.MessageTypeIdentifier = 1230;
			Assert.IsTrue( message.IsAdvice());
		}

		/// <summary>
		/// Test SetResponseMessageTypeIdentifier method.
		/// </summary>
		[Test( Description="Test SetResponseMessageTypeIdentifier method.")]
		public void SetResponseMessageTypeIdentifier() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1100;
			message.SetResponseMessageTypeIdentifier();
			Assert.IsTrue( message.MessageTypeIdentifier == 1110);

			message.MessageTypeIdentifier = 1101;
			message.SetResponseMessageTypeIdentifier();
			Assert.IsTrue( message.MessageTypeIdentifier == 1110);

			message.MessageTypeIdentifier = 1220;
			message.SetResponseMessageTypeIdentifier();
			Assert.IsTrue( message.MessageTypeIdentifier == 1230);

			message.MessageTypeIdentifier = 1230;
			try {
				message.SetResponseMessageTypeIdentifier();
				Assert.Fail();
			} catch ( MessagingException) {
			}
		}

		/// <summary>
		/// Test IsAuthorization method.
		/// </summary>
		[Test( Description="Test IsAuthorization method.")]
		public void IsAuthorization() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsTrue( message.IsAuthorization());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsAuthorization());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsAuthorization());
		}

		/// <summary>
		/// Test IsFinancial method.
		/// </summary>
		[Test( Description="Test IsFinancial method.")]
		public void IsFinancial() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsFinancial());

			message.MessageTypeIdentifier = 1210;
			Assert.IsTrue( message.IsFinancial());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsFinancial());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsFinancial());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsFinancial());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsAdvice());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsFinancial());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsFinancial());
		}

		/// <summary>
		/// Test IsFileAction method.
		/// </summary>
		[Test( Description="Test IsFileAction method.")]
		public void IsFileAction() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1310;
			Assert.IsTrue( message.IsFileAction());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsFileAction());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsFileAction());
		}

		/// <summary>
		/// Test IsReversalOrChargeBack method.
		/// </summary>
		[Test( Description="Test IsReversalOrChargeBack method.")]
		public void IsReversalOrChargeBack() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1410;
			Assert.IsTrue( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsReversalOrChargeBack());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsReversalOrChargeBack());
		}

		/// <summary>
		/// Test IsReconciliation method.
		/// </summary>
		[Test( Description="Test IsReconciliation method.")]
		public void IsReconciliation() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1510;
			Assert.IsTrue( message.IsReconciliation());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsReconciliation());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsReconciliation());
		}

		/// <summary>
		/// Test IsAdministrative method.
		/// </summary>
		[Test( Description="Test IsAdministrative method.")]
		public void IsAdministrative() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1610;
			Assert.IsTrue( message.IsAdministrative());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsAdministrative());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsAdministrative());
		}

		/// <summary>
		/// Test IsFeeCollection method.
		/// </summary>
		[Test( Description="Test IsFeeCollection method.")]
		public void IsFeeCollection() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1710;
			Assert.IsTrue( message.IsFeeCollection());

			message.MessageTypeIdentifier = 1810;
			Assert.IsFalse( message.IsFeeCollection());
		}

		/// <summary>
		/// Test IsNetworkManagement method.
		/// </summary>
		[Test( Description="Test IsNetworkManagement method.")]
		public void IsNetworkManagement() {

			Iso8583Message message = new Iso8583Message();

			message.MessageTypeIdentifier = 1110;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1210;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1310;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1410;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1510;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1610;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1710;
			Assert.IsFalse( message.IsNetworkManagement());

			message.MessageTypeIdentifier = 1810;
			Assert.IsTrue( message.IsNetworkManagement());
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void Clone() {

			Iso8583Message message = new Iso8583Message( 1200);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				message.Fields.Add( _fields[i]);
			}

			message.Header = new StringMessageHeader( "HEADER");

			Iso8583Message cloned = ( Iso8583Message)message.Clone();

			Assert.IsTrue( cloned.Fields.Count == message.Fields.Count);
			Assert.IsNotNull( cloned.Header);
			Assert.IsTrue( cloned.MessageTypeIdentifier == message.MessageTypeIdentifier);

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

			Iso8583Message message = new Iso8583Message( 1220);

			// Add fields.
			for ( int i = 0; i < _fields.Length; i++) {
				message.Fields.Add( _fields[i]);
			}

			message.Header = new StringMessageHeader( "HEADER");

			string data = message.ToString();

			Assert.IsNotNull( data);
			Assert.IsTrue( data.Equals( "H:HEADER,M:1220,0:[1,2,3,4,11,12,25,48,49,64],1:[],2:1,3:000000,4:000000000100,11:000015,12:010102,25:0,48:SOME DATA,49:840,64:FFFFFFFFFFFFFFFF"));
		}

		/// <summary>
		/// Test CopyTo method (all fields).
		/// </summary>
		[Test( Description="Test CopyTo method (all fields).")]
		public void CopyTo() {

			Iso8583Message src = new Iso8583Message( 1200);
			Iso8583Message dst = new Iso8583Message( 1100);

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

			Assert.IsTrue( src.MessageTypeIdentifier == dst.MessageTypeIdentifier);
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

			Iso8583Message src = new Iso8583Message( 1200);
			Iso8583Message dst = new Iso8583Message( 1100);

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

			Assert.IsTrue( src.MessageTypeIdentifier == dst.MessageTypeIdentifier);

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
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			Iso8583Message message = new Iso8583Message();
			object newComponent = message.NewComponent();

			Assert.IsTrue( newComponent is Iso8583Message);
		}
		#endregion
	}
}
