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
    /// Test fixture for ConditionalFieldFormatter.
    /// </summary>
    [TestFixture( Description = "ConditionalFieldFormatter functionality tests." )]
    public class ConditionalFieldFormatterTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="ConditionalFieldFormatterTest"/> constructor.
        /// </summary>
        public ConditionalFieldFormatterTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            MockConditionalFieldEvaluator evaluator = new MockConditionalFieldEvaluator( true );
            MockFieldFormatter trueFormatter = new MockFieldFormatter();
            MockFieldFormatter falseFormatter = new MockFieldFormatter();
            ConditionalFieldFormatter cff = new ConditionalFieldFormatter( 3,
                evaluator, trueFormatter, falseFormatter );

            Assert.IsNotNull( cff.Evaluator );
            Assert.IsTrue( cff.Evaluator == evaluator );
            Assert.IsNotNull( cff.TrueFormatter );
            Assert.IsTrue( cff.TrueFormatter == trueFormatter );
            Assert.IsNotNull( cff.FalseFormatter );
            Assert.IsTrue( cff.FalseFormatter == falseFormatter );
            Assert.IsNull( cff.Expression );
            Assert.IsNull( cff.CompiledExpression );

            cff = new ConditionalFieldFormatter( 3, SemanticParserTest.SimpleExpression1Value,
                trueFormatter, falseFormatter );

            Assert.IsNotNull( cff.Evaluator );
            Assert.IsTrue( cff.Evaluator == cff );
            Assert.IsNotNull( cff.TrueFormatter );
            Assert.IsTrue( cff.TrueFormatter == trueFormatter );
            Assert.IsNotNull( cff.FalseFormatter );
            Assert.IsTrue( cff.FalseFormatter == falseFormatter );
            Assert.IsTrue( cff.Expression == SemanticParserTest.SimpleExpression1Value );
            Assert.IsNotNull( cff.CompiledExpression );
            SemanticParserTest.CheckSimpleExpression1( cff.CompiledExpression as
                FieldValueEqualsStringOperator );
        }

        /// <summary>
        /// Parsing test.
        /// </summary>
        [Test( Description = "Parsing test" )]
        public void Parse() {

            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            MockFieldFormatter trueFormatter = new MockFieldFormatter();
            MockFieldFormatter falseFormatter = new MockFieldFormatter();
            ConditionalFieldFormatter cff = new ConditionalFieldFormatter( 3,
                new MockConditionalFieldEvaluator( true ), trueFormatter, falseFormatter );

            cff.Parse( ref pc );

            Assert.IsTrue( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );

            cff = new ConditionalFieldFormatter( 3,
                new MockConditionalFieldEvaluator( false ), trueFormatter, falseFormatter );
            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();

            cff.Parse( ref pc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsTrue( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );

            cff = new ConditionalFieldFormatter( 3, SemanticParserTest.SimpleExpression1Value,
                trueFormatter, falseFormatter );
            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();
            pc.CurrentMessage = MessagesProvider.GetMessage();

            cff.Parse( ref pc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsTrue( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );

            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();
            pc.CurrentMessage[3].Value = "000000";

            cff.Parse( ref pc );

            Assert.IsTrue( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );
        }

        /// <summary>
        /// Formating test.
        /// </summary>
        [Test( Description = "Formating test" )]
        public void Format() {

            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );
            MockConditionalFieldEvaluator evaluator = new MockConditionalFieldEvaluator( true );
            MockFieldFormatter trueFormatter = new MockFieldFormatter();
            MockFieldFormatter falseFormatter = new MockFieldFormatter();
            ConditionalFieldFormatter cff = new ConditionalFieldFormatter( 3,
                evaluator, trueFormatter, falseFormatter );

            cff.Format( new StringField( 3, "000000" ), ref fc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsTrue( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );

            cff = new ConditionalFieldFormatter( 3,
                new MockConditionalFieldEvaluator( false ), trueFormatter, falseFormatter );
            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();

            cff.Format( new StringField( 3, "000000" ), ref fc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsTrue( falseFormatter.FormatWasCalled );

            cff = new ConditionalFieldFormatter( 3, SemanticParserTest.SimpleExpression1Value,
                trueFormatter, falseFormatter );
            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();
            fc.CurrentMessage = MessagesProvider.GetMessage();

            cff.Format( new StringField( 3, "000000" ), ref fc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsFalse( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsTrue( falseFormatter.FormatWasCalled );

            trueFormatter.ResetFlags();
            falseFormatter.ResetFlags();
            fc.CurrentMessage[3].Value = "000000";

            cff.Format( new StringField( 3, "000000" ), ref fc );

            Assert.IsFalse( trueFormatter.ParseWasCalled );
            Assert.IsTrue( trueFormatter.FormatWasCalled );
            Assert.IsFalse( falseFormatter.ParseWasCalled );
            Assert.IsFalse( falseFormatter.FormatWasCalled );
        }
        #endregion
    }
}
