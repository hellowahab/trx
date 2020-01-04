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

using Trx.Messaging;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

	/// <summary>
	/// Test fixture for FieldAttribute.
	/// </summary>
	[TestFixture( Description="FieldAttribute functionality tests.")]
	public class FieldAttributeTest {

		#region Class constructors
		/// <summary>
		/// Default <see cref="FieldAttributeTest"/> constructor.
		/// </summary>
		public FieldAttributeTest() {

		}
		#endregion

		#region Class methods
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
		public void Instantiation() {

			FieldAttribute attr = new FieldAttribute( 10);

			Assert.IsTrue( attr.FieldNumber == 10);
		}
		#endregion
	}
}