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
	/// Test fixture for FieldFormatterEventArgs.
	/// </summary>
	[TestFixture( Description="Field formatter event args tests.")]
	public class FieldFormatterEventArgsTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="FieldFormatterEventArgsTest"/>.
		/// </summary>
		public FieldFormatterEventArgsTest() {

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

			FieldFormatterEventArgs eventArgs = new FieldFormatterEventArgs(
				new StringFieldFormatter( 11, new FixedLengthManager( 6), DataEncoder.GetInstance()));

			Assert.IsNotNull( eventArgs.FieldFormatter);
			Assert.IsTrue( eventArgs.FieldFormatter.FieldNumber == 11);
		}
		#endregion
	}
}
