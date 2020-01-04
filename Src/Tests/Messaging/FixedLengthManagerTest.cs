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
	/// Test fixture for FixedLengthManager.
	/// </summary>
	[TestFixture( Description="Fixed length manager tests.")]
	public class FixedLengthManagerTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="FixedLengthManagerTest"/>.
		/// </summary>
		public FixedLengthManagerTest() {

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
		/// Test MaximumLength property.
		/// </summary>
		[Test( Description="Test MaximumLength property.")]
		public void MaximumLength() {

			FixedLengthManager lengthManager = new FixedLengthManager( 20);

			Assert.IsTrue( lengthManager.MaximumLength == 20);
		}

		/// <summary>
		/// Test ReadLength method.
		/// </summary>
		[Test( Description="Test ReadLength method.")]
		public void ReadLength() {

			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			FixedLengthManager lengthManager = new FixedLengthManager( 10);

			Assert.IsTrue( lengthManager.ReadLength( ref parserContext) == 10);
		}

		/// <summary>
		/// Test WriteLength method.
		/// </summary>
		[Test( Description="Test WriteLength method.")]
		public void WriteLength() {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			FixedLengthManager lengthManager = new FixedLengthManager( 10);

			lengthManager.WriteLength( null, 10, 10, ref formatterContext);
			try {
				lengthManager.WriteLength( null, 5, 5, ref formatterContext);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "dataLength"));
			}
		}
		#endregion
	}
}
