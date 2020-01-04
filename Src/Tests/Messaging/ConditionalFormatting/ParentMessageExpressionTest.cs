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
    /// Test fixture for ParentMessageExpression.
    /// </summary>
    [TestFixture( Description = "ParentMessageExpression functionality tests." )]
    public class ParentMessageExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="ParentMessageExpressionTest"/> constructor.
        /// </summary>
        public ParentMessageExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation and properties test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            ParentMessageExpression pme = new ParentMessageExpression();

            Assert.IsNull( pme.MessageExpression );

            MessageExpression me = new MessageExpression();
            pme = new ParentMessageExpression( me );

            Assert.IsTrue( pme.MessageExpression == me );

            MessageExpression fve2 = new MessageExpression();
            pme.MessageExpression = fve2;
            Assert.IsTrue( pme.MessageExpression == fve2 );
        }

        /// <summary>
        /// Test GetLeafMessage function.
        /// </summary>
        [Test( Description = "Test GetLeafMessage function" )]
        public void GetLeafMessage() {

            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            ParentMessageExpression pme = new ParentMessageExpression(
                new MessageExpression() );

            // Parent of a message without one.
            try {
                pme.GetLeafMessage( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // Passing null message (as parameter and in the contexts).
            try {
                pme.GetLeafMessage( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                pme.GetLeafMessage( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = msg[61].Value as Message;
            Message anotherMsg = MessagesProvider.GetAnotherMessage();
            anotherMsg = anotherMsg[62].Value as Message;

            Assert.IsTrue( ( pme.GetLeafMessage( ref fc, msg )[41].Value as string ) == "TEST1" );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( ( pme.GetLeafMessage( ref fc, msg )[41].Value as string ) == "TEST1" );
            Assert.IsTrue( ( pme.GetLeafMessage( ref fc, null )[41].Value as string ) == "TEST2" );

            Assert.IsTrue( ( pme.GetLeafMessage( ref pc, msg )[41].Value as string ) == "TEST1" );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( ( pme.GetLeafMessage( ref pc, msg )[41].Value as string ) == "TEST1" );
            Assert.IsTrue( ( pme.GetLeafMessage( ref pc, null )[41].Value as string ) == "TEST2" );
        }

        /// <summary>
        /// Test GetLeafFieldNumber.
        /// </summary>
        [Test( Description = "Test GetLeafFieldNumber" )]
        public void GetLeafFieldNumber() {

            MessageExpression me = new MessageExpression( 4 );
            ParentMessageExpression pme = new ParentMessageExpression( me );

            Assert.IsTrue( pme.GetLeafFieldNumber() == 4 );

            me = new MessageExpression( 11 );
            pme.MessageExpression = me;
            Assert.IsTrue( pme.GetLeafFieldNumber() == 11 );

            me.FieldNumber = 5;
            Assert.IsTrue( pme.GetLeafFieldNumber() == 5 );
        }

        /// <summary>
        /// Test GetLeafFieldValueString function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueString function" )]
        public void GetLeafFieldValueString() {

            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            ParentMessageExpression pme = new ParentMessageExpression(
                new MessageExpression( 41 ) );

            // Parent of a message without one.
            try {
                pme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // Passing null message (as parameter and in the contexts).
            try {
                pme.GetLeafFieldValueString( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                pme.GetLeafFieldValueString( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = msg[61].Value as Message;
            Message anotherMsg = MessagesProvider.GetAnotherMessage();
            anotherMsg = anotherMsg[62].Value as Message;

            Assert.IsTrue( pme.GetLeafFieldValueString( ref fc, msg ) == "TEST1" );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( pme.GetLeafFieldValueString( ref fc, msg ) == "TEST1" );
            Assert.IsTrue( pme.GetLeafFieldValueString( ref fc, null ) == "TEST2" );

            Assert.IsTrue( pme.GetLeafFieldValueString( ref pc, msg ) == "TEST1" );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( pme.GetLeafFieldValueString( ref pc, msg ) == "TEST1" );
            Assert.IsTrue( pme.GetLeafFieldValueString( ref pc, null ) == "TEST2" );
        }

        /// <summary>
        /// Test GetLeafFieldValueBytes function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueBytes function" )]
        public void GetLeafFieldValueBytes() {

            Message msg = MessagesProvider.GetMessage();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            ParentMessageExpression pme = new ParentMessageExpression(
                new MessageExpression( 52 ) );

            // Parent of a message without one.
            try {
                pme.GetLeafFieldValueString( ref fc, msg );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            // Passing null message (as parameter and in the contexts).
            try {
                pme.GetLeafFieldValueString( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                pme.GetLeafFieldValueString( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            msg = msg[62].Value as Message;
            Message anotherMsg = MessagesProvider.GetAnotherMessage();
            anotherMsg = anotherMsg[61].Value as Message;

            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref fc, null ),
                new byte[] { 0x55, 0x60, 0x65, 0x70, 0x75, 0x80, 0x85, 0x90 } ) );

            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( pme.GetLeafFieldValueBytes( ref pc, null ),
                new byte[] { 0x55, 0x60, 0x65, 0x70, 0x75, 0x80, 0x85, 0x90 } ) );
        }
        #endregion
    }
}
