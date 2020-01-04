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
    /// Test fixture for ConditionalOrOperator.
    /// </summary>
    [TestFixture( Description = "ConditionalOrOperator functionality tests." )]
    public class ConditionalOrOperatorTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="ConditionalOrOperatorTest"/> constructor.
        /// </summary>
        public ConditionalOrOperatorTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            ConditionalOrOperator op = new ConditionalOrOperator();

            Assert.IsNull( op.LeftExpression );
            Assert.IsNull( op.RightExpression );

            MockBooleanExpression left = new MockBooleanExpression( true );
            MockBooleanExpression right = new MockBooleanExpression( true );

            op = new ConditionalOrOperator( left, right );

            Assert.IsTrue( op.LeftExpression == left );
            Assert.IsTrue( op.RightExpression == right );

            op.LeftExpression = null;
            op.RightExpression = null;

            Assert.IsNull( op.LeftExpression );
            Assert.IsNull( op.RightExpression );
        }

        /// <summary>
        /// Evaluation test.
        /// </summary>
        [Test( Description = "Evaluation test" )]
        public void Evaluate() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            ConditionalOrOperator op = new ConditionalOrOperator(
                new MockBooleanExpression( true ), new MockBooleanExpression( true ) );

            Assert.IsTrue( op.EvaluateParse( ref pc ) );
            Assert.IsTrue( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            op = new ConditionalOrOperator(
                new MockBooleanExpression( false ), new MockBooleanExpression( true ) );

            Assert.IsTrue( op.EvaluateParse( ref pc ) );
            Assert.IsTrue( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            op = new ConditionalOrOperator(
                new MockBooleanExpression( true ), new MockBooleanExpression( false ) );

            Assert.IsTrue( op.EvaluateParse( ref pc ) );
            Assert.IsTrue( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            op = new ConditionalOrOperator(
                new MockBooleanExpression( false ), new MockBooleanExpression( false ) );

            Assert.IsFalse( op.EvaluateParse( ref pc ) );
            Assert.IsFalse( op.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
        }
        #endregion
    }
}
