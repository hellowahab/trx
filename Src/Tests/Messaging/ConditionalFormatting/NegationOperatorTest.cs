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
using Trx.Messaging.ConditionalFormatting;
using NUnit.Framework;

namespace Tests.Trx.Messaging.ConditionalFormatting {

    /// <summary>
    /// Test fixture for NegationOperator.
    /// </summary>
    [TestFixture( Description = "NegationOperator functionality tests." )]
    public class NegationOperatorTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="NegationOperatorTest"/> constructor.
        /// </summary>
        public NegationOperatorTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            NegationOperator op = new NegationOperator();

            Assert.IsNull( op.Expression );

            MockBooleanExpression expression = new MockBooleanExpression( true );

            op = new NegationOperator( expression );

            Assert.IsTrue( op.Expression == expression );

            op.Expression = null;

            Assert.IsNull( op.Expression );
        }

        /// <summary>
        /// Evaluation test.
        /// </summary>
        [Test( Description = "Evaluation test" )]
        public void Evaluate() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            NegationOperator op = new NegationOperator( new MockBooleanExpression( true ) );

            Assert.IsFalse( op.EvaluateParse( ref pc ) );
            Assert.IsFalse( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            op = new NegationOperator( new MockBooleanExpression( false ) );

            Assert.IsTrue( op.EvaluateParse( ref pc ) );
            Assert.IsTrue( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
        }
        #endregion
    }
}
