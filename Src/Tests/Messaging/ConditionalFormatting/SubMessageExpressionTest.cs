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
    /// Test fixture for SubMessageExpression.
    /// </summary>
    [TestFixture( Description = "SubMessageExpression functionality tests." )]
    public class SubMessageExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="SubMessageExpressionTest"/> constructor.
        /// </summary>
        public SubMessageExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation and properties test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            SubMessageExpression sme = new SubMessageExpression();

            Assert.IsTrue( sme.FieldNumber == -1 );
            Assert.IsNull( sme.MessageExpression );

            MessageExpression me1 = new MessageExpression();
            sme = new SubMessageExpression( 5, me1 );

            Assert.IsTrue( sme.FieldNumber == 5 );
            Assert.IsTrue( sme.MessageExpression == me1 );

            sme.FieldNumber = 9;
            Assert.IsTrue( sme.FieldNumber == 9 );

            MessageExpression me2 = new MessageExpression();
            sme.MessageExpression = me2;
            Assert.IsTrue( sme.MessageExpression == me2 );
        }

        /// <summary>
        /// Test GetLeafMessage function.
        /// </summary>
        [Test( Description = "Test GetLeafMessage function" )]
        public void GetLeafMessage() {

            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            SubMessageExpression sme = new SubMessageExpression( 24, null );

            // Sub field of a field isn't an inner message.
            try {
                sme.GetLeafMessage( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            sme = new SubMessageExpression( 61, null );
            msg.Fields.Add( new InnerMessageField( 61 ) );
            try {
                sme.GetLeafMessage( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            sme = new SubMessageExpression( 61, new MessageExpression() );

            // Passing null message (as parameter and in the contexts).
            try {
                sme.GetLeafMessage( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                sme.GetLeafMessage( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( ( sme.GetLeafMessage( ref fc, msg )[6].Value as string ) == "123" );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( ( sme.GetLeafMessage( ref fc, msg )[6].Value as string ) == "123" );
            Assert.IsTrue( ( sme.GetLeafMessage( ref fc, null )[6].Value as string ) == "456" );

            Assert.IsTrue( ( sme.GetLeafMessage( ref pc, msg )[6].Value as string ) == "123" );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( ( sme.GetLeafMessage( ref pc, msg )[6].Value as string ) == "123" );
            Assert.IsTrue( ( sme.GetLeafMessage( ref pc, null )[6].Value as string ) == "456" );
        }

        /// <summary>
        /// Test GetLeafFieldNumber.
        /// </summary>
        [Test( Description = "Test GetLeafFieldNumber" )]
        public void GetLeafFieldNumber() {

            MessageExpression me = new MessageExpression( 4 );
            SubMessageExpression sme = new SubMessageExpression( 5, me );

            Assert.IsTrue( sme.GetLeafFieldNumber() == 4 );

            me = new MessageExpression( 11 );
            sme.MessageExpression = me;
            Assert.IsTrue( sme.GetLeafFieldNumber() == 11 );

            me.FieldNumber = 5;
            Assert.IsTrue( sme.GetLeafFieldNumber() == 5 );
        }

        /// <summary>
        /// Test GetLeafFieldValueString function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueString function" )]
        public void GetLeafFieldValueString() {

            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            SubMessageExpression sme = new SubMessageExpression( 24, null );

            // Sub field of a field isn't an inner message.
            try {
                sme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            sme = new SubMessageExpression( 61, null );
            msg.Fields.Add( new InnerMessageField( 61 ) );
            try {
                sme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            sme = new SubMessageExpression( 61, new MessageExpression( 6 ) );

            // Passing null message (as parameter and in the contexts).
            try {
                sme.GetLeafFieldValueString( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                sme.GetLeafFieldValueString( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( sme.GetLeafFieldValueString( ref fc, msg ) == "123" );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( sme.GetLeafFieldValueString( ref fc, msg ) == "123" );
            Assert.IsTrue( sme.GetLeafFieldValueString( ref fc, null ) == "456" );

            Assert.IsTrue( sme.GetLeafFieldValueString( ref pc, msg ) == "123" );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( sme.GetLeafFieldValueString( ref pc, msg ) == "123" );
            Assert.IsTrue( sme.GetLeafFieldValueString( ref pc, null ) == "456" );
        }

        /// <summary>
        /// Test GetLeafFieldValueBytes function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueBytes function" )]
        public void GetLeafFieldValueBytes() {


            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            SubMessageExpression sme = new SubMessageExpression( 24, null );

            // Sub field of a field isn't an inner message.
            try {
                sme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            sme = new SubMessageExpression( 61, null );
            msg.Fields.Add( new InnerMessageField( 61 ) );
            try {
                sme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            sme = new SubMessageExpression( 62, new MessageExpression( 7 ) );

            // Passing null message (as parameter and in the contexts).
            try {
                sme.GetLeafFieldValueBytes( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                sme.GetLeafFieldValueBytes( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x75, 0xB0, 0xB5 } ) );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x75, 0xB0, 0xB5 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref fc, null ),
                new byte[] { 0x95, 0xA0, 0xA5 } ) );

            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x75, 0xB0, 0xB5 } ) );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x75, 0xB0, 0xB5 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( sme.GetLeafFieldValueBytes( ref pc, null ),
                new byte[] { 0x95, 0xA0, 0xA5 } ) );
        }
        #endregion
    }
}
