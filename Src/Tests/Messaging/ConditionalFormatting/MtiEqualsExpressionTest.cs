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
    /// Test fixture for MtiEqualsExpression.
    /// </summary>
    [TestFixture( Description = "MtiEqualsExpression functionality tests." )]
    public class MtiEqualsExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="MtiEqualsExpressionTest"/> constructor.
        /// </summary>
        public MtiEqualsExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            MtiEqualsExpression ee = new MtiEqualsExpression();

            Assert.IsTrue( ee.Mti == -1 );
            Assert.IsNull( ee.MessageExpression );

            MessageExpression me = new MessageExpression();
            ee = new MtiEqualsExpression( 200, me );

            Assert.IsTrue( ee.Mti == 200 );
            Assert.IsTrue( ee.MessageExpression == me );

            ee.Mti = 210;
            ee.MessageExpression = null;

            Assert.IsTrue( ee.Mti == 210 );
            Assert.IsNull( ee.MessageExpression );
        }

        /// <summary>
        /// Evaluation test.
        /// </summary>
        [Test( Description = "Evaluation test" )]
        public void Evaluate() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            MtiEqualsExpression ee = new MtiEqualsExpression( 200, new MessageExpression() );

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

            // Evaluate with incorrect message type.
            Message msg = MessagesProvider.GetMessage();
            pc.CurrentMessage = msg;
            try {
                ee.EvaluateParse( ref pc );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            fc.CurrentMessage = msg;
            try {
                ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // Evaluate against an ISO 8583 message.
            Iso8583Message isoMsg = MessagesProvider.GetIso8583Message();
            pc.CurrentMessage = isoMsg;
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = isoMsg;
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
            isoMsg = MessagesProvider.GetAnotherIso8583Message();
            pc.CurrentMessage = isoMsg;
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = isoMsg;
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
        }
        #endregion
    }
}
