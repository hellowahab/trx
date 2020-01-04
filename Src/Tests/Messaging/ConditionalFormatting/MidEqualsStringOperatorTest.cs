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
using Trx.Messaging.ConditionalFormatting;
using NUnit.Framework;

namespace Tests.Trx.Messaging.ConditionalFormatting {

    /// <summary>
    /// Test fixture for MidEqualsStringOperator.
    /// </summary>
    [TestFixture( Description = "MidEqualsStringOperator functionality tests." )]
    public class MidEqualsStringOperatorTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="MidEqualsStringOperatorTest"/> constructor.
        /// </summary>
        public MidEqualsStringOperatorTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            MidEqualsStringOperator ee = new MidEqualsStringOperator();

            Assert.IsNull( ee.ValueExpression );
            Assert.IsNull( ee.MessageExpression );
            Assert.IsTrue( ee.StartIndex == 0 );
            Assert.IsTrue( ee.Length == 0 );

            StringConstantExpression sce = new StringConstantExpression( "202030302020" );
            MessageExpression me = new MessageExpression( 3 );
            ee = new MidEqualsStringOperator( me, sce, 0, 3 );

            Assert.IsTrue( ee.ValueExpression == sce );
            Assert.IsTrue( ee.MessageExpression == me );
            Assert.IsTrue( ee.StartIndex == 0 );
            Assert.IsTrue( ee.Length == 3 );

            ee.ValueExpression = null;
            ee.MessageExpression = null;
            ee.StartIndex = 1;
            ee.Length = 2;

            Assert.IsNull( ee.ValueExpression );
            Assert.IsNull( ee.MessageExpression );
            Assert.IsTrue( ee.StartIndex == 1 );
            Assert.IsTrue( ee.Length == 2 );

            try {
                ee.ValueExpression = new BinaryConstantExpression( "30313233" );
                Assert.Fail();
            }
            catch ( ArgumentException ) {
            }

            try {
                ee = new MidEqualsStringOperator( me, sce, -1, 3 );
                Assert.Fail();
            }
            catch ( ArgumentException ) {
            }

            try {
                ee = new MidEqualsStringOperator( me, sce, 0, -1 );
                Assert.Fail();
            }
            catch ( ArgumentException ) {
            }

            try {
                ee.StartIndex = -1;
                Assert.Fail();
            }
            catch ( ArgumentException ) {
            }

            try {
                ee.Length = -1;
                Assert.Fail();
            }
            catch ( ArgumentException ) {
            }
        }

        /// <summary>
        /// Evaluation test.
        /// </summary>
        [Test( Description = "Evaluation test" )]
        public void Evaluate() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            MidEqualsStringOperator ee = new MidEqualsStringOperator(
                new MessageExpression( 1 ), new StringConstantExpression( string.Empty ), 0, 0 );

            Message msg = new Message();
            msg.Fields.Add( new StringField( 1, null ) );

            // Both values are empty.
            pc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new MidEqualsStringOperator(
                new MessageExpression( 1 ), new StringConstantExpression( "1520253035404550" ), 0, 0 );
            // Field value is null.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new MidEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( null ), 0, 1 );
            msg = MessagesProvider.GetMessage();
            // Constant is null.
            pc.CurrentMessage = msg;
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = msg;
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new MidEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "152025303540" ), 0, 1 );
            // Different lengths.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new MidEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "1520253035404551" ), 0, 1 );
            // Different data.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new MidEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "999" ), 2, 3 );
            // Equals.
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            // The start index of the set of bytes is greater than the field value length
            ee = new MidEqualsStringOperator(
                new MessageExpression( 52 ), new StringConstantExpression( "123" ), 6, 3 );
            try {
                Assert.IsTrue( ee.EvaluateParse( ref pc ) );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // There isn't enough data in the field value to get a subset of bytes
            ee = new MidEqualsStringOperator(
                new MessageExpression( 52 ), new StringConstantExpression( "123" ), 4, 5 );
            try {
                Assert.IsTrue( ee.EvaluateParse( ref pc ) );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
        }
        #endregion
    }
}
