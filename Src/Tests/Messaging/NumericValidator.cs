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
	/// Test fixture for NumericValidator.
	/// </summary>
	[TestFixture( Description="Numeric validator tests.")]
	public class NumericValidatorTest {

		#region Constructors
		/// <summary>
		/// It builds and initializes a new instance of the class
		/// <see cref="NumericValidatorTest"/>.
		/// </summary>
		public NumericValidatorTest() {

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

			NumericValidator validator = NumericValidator.GetInstance();

			Assert.IsTrue( validator == NumericValidator.GetInstance());
            Assert.IsTrue( validator == NumericValidator.GetInstance( false ) );

            validator = NumericValidator.GetInstance( true );
            Assert.IsTrue( validator == NumericValidator.GetInstance( true ) );

            Assert.IsFalse( NumericValidator.GetInstance( true ) ==
                NumericValidator.GetInstance( false ) );
        }

		/// <summary>
		/// Test Validate method.
		/// </summary>
		[Test( Description="Test Validate method.")]
		public void ValidateDontAllowNull() {

			NumericValidator validator = NumericValidator.GetInstance();

			validator.Validate( "452");

			try {
				validator.Validate( "H452");
				Assert.Fail();
			} catch ( StringValidationException) {
			}

            try {
                validator.Validate( "" );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }

            try {
                validator.Validate( null );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }
        }

        /// <summary>
        /// Test Validate method.
        /// </summary>
        [Test( Description = "Test Validate method." )]
        public void ValidateAllowNull() {

            NumericValidator validator = NumericValidator.GetInstance( true );

            validator.Validate( "452" );

            try {
                validator.Validate( "H452" );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }

            try {
                validator.Validate( "" );
            }
            catch ( StringValidationException ) {
                Assert.Fail();
            }

            try {
                validator.Validate( null );
            }
            catch ( StringValidationException ) {
                Assert.Fail();
            }
        }
        #endregion
	}
}