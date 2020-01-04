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
	/// Test fixture for VariableLengthManager.
	/// </summary>
	[TestFixture( Description="Variable length manager tests.")]
	public class VariableLengthManagerTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="VariableLengthManagerTest"/>.
		/// </summary>
		public VariableLengthManagerTest() {

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
		/// Test constructor.
		/// </summary>
		[Test( Description="Test constructor.")]
		public void Constructor() {

			VariableLengthManager lengthManager = new VariableLengthManager( 5,
				40, StringLengthEncoder.GetInstance( 2));

			Assert.IsTrue( lengthManager.MinimumLength == 5);
			Assert.IsTrue( lengthManager.MaximumLength == 40);
			Assert.IsTrue( lengthManager.LengthEncoder ==
				StringLengthEncoder.GetInstance( 2));

			lengthManager = new VariableLengthManager( 20,
				20, StringLengthEncoder.GetInstance( 3));
			
			Assert.IsTrue( lengthManager.MinimumLength == 20);
			Assert.IsTrue( lengthManager.MaximumLength == 20);
			Assert.IsTrue( lengthManager.LengthEncoder ==
				StringLengthEncoder.GetInstance( 3));

			try {
				lengthManager = new VariableLengthManager( -1,
					40, StringLengthEncoder.GetInstance( 2));
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "minimumLength"));
			}

			try {
				lengthManager = new VariableLengthManager( 50,
					40, StringLengthEncoder.GetInstance( 2));
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "minimumLength"));
			}

			try {
				lengthManager = new VariableLengthManager( 10,
					20, null);
				Assert.Fail();
			} catch ( ArgumentNullException e) {
				Assert.IsTrue( e.ParamName.Equals( "lengthEncoder"));
			}
		}

		/// <summary>
		/// Test WriteLength method.
		/// </summary>
		[Test( Description="Test WriteLength method.")]
		public void WriteLength() {

			FormatterContext formatterContext = new FormatterContext( 
				FormatterContext.DefaultBufferSize);
			VariableLengthManager lengthManager = new VariableLengthManager( 8,
				24, StringLengthEncoder.GetInstance( 999));

			lengthManager.WriteLength( null, 8, 8, ref formatterContext);
			string length = formatterContext.GetDataAsString();

			Assert.IsTrue( length.Equals( "008"));
			formatterContext.Clear();

			lengthManager.WriteLength( null, 15, 15, ref formatterContext);
			length = formatterContext.GetDataAsString();

			Assert.IsTrue( length.Equals( "015"));
			formatterContext.Clear();

			lengthManager.WriteLength( null, 24, 24, ref formatterContext);
			length = formatterContext.GetDataAsString();

			Assert.IsTrue( length.Equals( "024"));

			try {
				lengthManager.WriteLength( null, 1, 1, ref formatterContext);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "dataLength"));
			}

			try {
				lengthManager.WriteLength( null, 90, 90, ref formatterContext);
				Assert.Fail();
			} catch ( ArgumentOutOfRangeException e) {
				Assert.IsTrue( e.ParamName.Equals( "dataLength"));
			}
		}

		/// <summary>
		/// Test EnoughData method.
		/// </summary>
		[Test( Description="Test EnoughData method.")]
		public void EnoughData() {

			ParserContext parserContext = new ParserContext( 
				ParserContext.DefaultBufferSize);
			VariableLengthManager lengthManager = new VariableLengthManager( 1,
				480, StringLengthEncoder.GetInstance( 480));

			parserContext.Write( "0");
			Assert.IsFalse( lengthManager.EnoughData( ref parserContext));
			parserContext.Write( "0");
			Assert.IsFalse( lengthManager.EnoughData( ref parserContext));
			parserContext.Write( "9Some data");
			Assert.IsTrue( lengthManager.EnoughData( ref parserContext));
		}

		/// <summary>
		/// Test ReadLength method.
		/// </summary>
		[Test( Description="Test ReadLength method.")]
		public void ReadLength() {

			ParserContext parserContext = new ParserContext( 
				ParserContext.DefaultBufferSize);
			VariableLengthManager lengthManager = new VariableLengthManager( 120,
				180, StringLengthEncoder.GetInstance( 999));

			parserContext.Write( "120");
			int length = lengthManager.ReadLength( ref parserContext);

			Assert.IsTrue( length == 120);

			parserContext.Write( "150");
			length = lengthManager.ReadLength( ref parserContext);

			Assert.IsTrue( length == 150);

			parserContext.Write( "180");
			length = lengthManager.ReadLength( ref parserContext);

			Assert.IsTrue( length == 180);

			parserContext.Write( "005");
			try {
				lengthManager.ReadLength( ref parserContext);
				Assert.Fail();
			} catch ( MessagingException) {
			}

			parserContext.Clear();
			parserContext.Write( "999");
			try {
				lengthManager.ReadLength( ref parserContext);
				Assert.Fail();
			} catch ( MessagingException) {
			}
		}
		#endregion
	}
}