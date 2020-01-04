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
using Trx.Messaging.Iso8583;
using Trx.Messaging.ConditionalFormatting;
using NUnit.Framework;

namespace Tests.Trx.Messaging.ConditionalFormatting {

    /// <summary>
    /// Test fixture for IsSetExpression.
    /// </summary>
    [TestFixture( Description = "IsSetExpression functionality tests." )]
    public class IsSetExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="IsSetExpressionTest"/> constructor.
        /// </summary>
        public IsSetExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            IsSetExpression ee = new IsSetExpression();

            Assert.IsNull( ee.MessageExpression );

            MessageExpression me = new MessageExpression( 3 );
            ee = new IsSetExpression( me );

            Assert.IsTrue( ee.MessageExpression.GetLeafFieldNumber() == 3 );
            Assert.IsTrue( ee.MessageExpression == me );

            me.FieldNumber = 5;
            Assert.IsTrue( ee.MessageExpression.GetLeafFieldNumber() == 5 );

            ee.MessageExpression = null;
            Assert.IsNull( ee.MessageExpression );
        }

        /// <summary>
        /// Evaluation test.
        /// </summary>
        [Test( Description = "Evaluation test" )]
        public void Evaluate() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            IsSetExpression ee = new IsSetExpression( new MessageExpression( 3 ) );

            // Passing null message (as parameter and in the contexts).
            try {
                ee.EvaluateParse( ref pc );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // Evaluate.
            Message msg = MessagesProvider.GetMessage();
            pc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
            ee = new IsSetExpression( new MessageExpression( 4 ) );
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
        }
        #endregion
    }
}
