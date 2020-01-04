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
    /// Test fixture for FieldValueEqualsStringOperator.
    /// </summary>
    [TestFixture( Description = "FieldValueEqualsStringOperator functionality tests." )]
    public class FieldValueEqualsStringOperatorTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="FieldValueEqualsStringOperatorTest"/> constructor.
        /// </summary>
        public FieldValueEqualsStringOperatorTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            FieldValueEqualsStringOperator ee = new FieldValueEqualsStringOperator();

            Assert.IsNull( ee.ValueExpression );
            Assert.IsNull( ee.MessageExpression );

            StringConstantExpression sce = new StringConstantExpression( "202030302020" );
            MessageExpression me = new MessageExpression( 3 );
            ee = new FieldValueEqualsStringOperator( me, sce );

            Assert.IsTrue( ee.ValueExpression == sce );
            Assert.IsTrue( ee.MessageExpression == me );

            ee.ValueExpression = null;
            ee.MessageExpression = null;

            Assert.IsNull( ee.ValueExpression );
            Assert.IsNull( ee.MessageExpression );

            try {
                ee.ValueExpression = new BinaryConstantExpression( "30313233" );
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

            FieldValueEqualsStringOperator ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 1 ), new StringConstantExpression( string.Empty ) );

            Message msg = new Message();
            msg.Fields.Add( new StringField( 1, null ) );

            // Both values are empty.
            pc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = msg;
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 1 ), new StringConstantExpression( "1520253035404550" ) );
            // Field value is null.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( null ) );
            msg = MessagesProvider.GetMessage();
            // Constant is null.
            pc.CurrentMessage = msg;
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            fc.CurrentMessage = msg;
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "152025303540" ) );
            // Different lengths.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "1520253035404551" ) );
            // Different data.
            Assert.IsFalse( ee.EvaluateParse( ref pc ) );
            Assert.IsFalse( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );

            ee = new FieldValueEqualsStringOperator(
                new MessageExpression( 3 ), new StringConstantExpression( "999999" ) );
            // Equals.
            Assert.IsTrue( ee.EvaluateParse( ref pc ) );
            Assert.IsTrue( ee.EvaluateFormat( new StringField( 3, "000000" ), ref fc ) );
        }
        #endregion
    }
}
