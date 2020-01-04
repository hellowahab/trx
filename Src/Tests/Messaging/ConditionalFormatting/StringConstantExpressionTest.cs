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

using Trx.Messaging.ConditionalFormatting;
using NUnit.Framework;

namespace Tests.Trx.Messaging.ConditionalFormatting {

    /// <summary>
    /// Test fixture for StringConstantExpression.
    /// </summary>
    [TestFixture( Description = "StringConstantExpression functionality tests." )]
    public class StringConstantExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="StringConstantExpressionTest"/> constructor.
        /// </summary>
        public StringConstantExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            StringConstantExpression sve = new StringConstantExpression();

            Assert.IsTrue( sve.Constant == null );

            sve = new StringConstantExpression( "Test" );
            Assert.IsTrue( sve.Constant == "Test" );

            sve.Constant = null;
            Assert.IsTrue( sve.Constant == null );

            sve.Constant = "Another test";
            Assert.IsTrue( sve.Constant == "Another test" );
        }
        #endregion
    }
}
