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
	/// Test fixture for StringMessageHeader.
	/// </summary>
	[TestFixture( Description="String message header tests.")]
	public class StringMessageHeaderTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="StringMessageHeaderTest"/>.
		/// </summary>
		public StringMessageHeaderTest() {

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
			StringMessageHeader header = new StringMessageHeader();

			Assert.IsNull( header.Value);

			header = new StringMessageHeader( value);

			Assert.IsTrue( value.Equals( header.Value));
		}

		/// <summary>
		/// Test ToString method.
		/// </summary>
		[Test( Description="Test ToString method.")]
		public void TestToString() {

			string value = "Test ToString method.";
			StringMessageHeader header = new StringMessageHeader();

			Assert.IsTrue( string.Empty.Equals( header.ToString()));
			header.Value = value;
			Assert.IsTrue( value.Equals( header.ToString()));
		}

		/// <summary>
		/// Test GetBytes method.
		/// </summary>
		[Test( Description="Test GetBytes method.")]
		public void GetBytes() {

			string value = "Test GetBytes method.";
			StringMessageHeader header = new StringMessageHeader( value);

			byte[] binaryValue = header.GetBytes();
			Assert.IsTrue( Encoding.UTF7.GetString(
				binaryValue).Equals( value));

			header.Value = null;
			binaryValue = header.GetBytes();
			Assert.IsNull( binaryValue);
		}

		/// <summary>
		/// Test Clone method.
		/// </summary>
		[Test( Description="Test Clone method.")]
		public void TestClone() {

			string value = "Test Clone method.";
			StringMessageHeader header = new StringMessageHeader();

			StringMessageHeader clonedField = ( StringMessageHeader)( header.Clone());

			Assert.IsNull( clonedField.Value);

			header.Value = value;
			clonedField = ( StringMessageHeader)( header.Clone());

			Assert.IsTrue( header.Value.Equals( clonedField.Value));
			Assert.IsTrue( ( ( object)( header.Value)) !=
				( ( object)( clonedField.Value)));

			header.Value = string.Empty;
			clonedField = ( StringMessageHeader)( header.Clone());

			Assert.IsTrue( header.Value.Equals( clonedField.Value));
			Assert.IsTrue( ( ( object)( header.Value)) !=
				( ( object)( clonedField.Value)));
		}

		/// <summary>
		/// Test NewComponent method.
		/// </summary>
		[Test( Description="Test NewComponent method.")]
		public void NewComponent() {

			string value = "Test NewComponent method.";
			StringMessageHeader header = new StringMessageHeader( value);
			MessagingComponent component = header.NewComponent();

			Assert.IsTrue( component is StringMessageHeader);
			Assert.IsNull( ( ( StringMessageHeader)component).Value);
		}
		#endregion
	}
}
