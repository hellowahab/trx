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
    /// Test fixture for MessageExpression.
    /// </summary>
    [TestFixture( Description = "MessageExpression functionality tests." )]
    public class MessageExpressionTest {

        #region Class constructors
        /// <summary>
        /// Default <see cref="MessageExpressionTest"/> constructor.
        /// </summary>
        public MessageExpressionTest() {

        }
        #endregion

        #region Class methods
        /// <summary>
        /// Instantiation test.
        /// </summary>
        [Test( Description = "Instantiation and properties test" )]
        public void InstantiationAndProperties() {

            MessageExpression me = new MessageExpression();
            Assert.IsTrue( me.FieldNumber == -1 );

            me = new MessageExpression( 11 );
            Assert.IsTrue( me.FieldNumber == 11 );

            me.FieldNumber = 5;
            Assert.IsTrue( me.FieldNumber == 5 );
        }

        /// <summary>
        /// Test GetLeafMessage function.
        /// </summary>
        [Test( Description = "Test GetLeafMessage function" )]
        public void GetLeafMessage() {

            MessageExpression me = new MessageExpression();
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            // Passing null message (as parameter and in the contexts).
            try {
                me.GetLeafMessage( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                me.GetLeafMessage( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            Message msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( me.GetLeafMessage( ref fc, msg ) == msg );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( me.GetLeafMessage( ref fc, msg ) == msg );
            Assert.IsTrue( me.GetLeafMessage( ref fc, null ) == anotherMsg );

            Assert.IsTrue( me.GetLeafMessage( ref pc, msg ) == msg );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( me.GetLeafMessage( ref pc, msg ) == msg );
            Assert.IsTrue( me.GetLeafMessage( ref pc, null ) == anotherMsg );
        }

        /// <summary>
        /// Test GetLeafFieldNumber.
        /// </summary>
        [Test( Description = "Test GetLeafFieldNumber" )]
        public void GetLeafFieldNumber() {

            MessageExpression me = new MessageExpression();
            Assert.IsTrue( me.GetLeafFieldNumber() == -1 );

            me = new MessageExpression( 11 );
            Assert.IsTrue( me.GetLeafFieldNumber() == 11 );

            me.FieldNumber = 5;
            Assert.IsTrue( me.GetLeafFieldNumber() == 5 );
        }

        /// <summary>
        /// Test GetLeafFieldValueString function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueString function" )]
        public void GetLeafFieldValueString() {

            MessageExpression me = new MessageExpression( 3 );
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            // Passing null message (as parameter and in the contexts).
            try {
                me.GetLeafFieldValueString( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                me.GetLeafFieldValueString( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            Message msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( me.GetLeafFieldValueString( ref fc, msg ) == "999999" );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( me.GetLeafFieldValueString( ref fc, msg ) == "999999" );
            Assert.IsTrue( me.GetLeafFieldValueString( ref fc, null ) == "111111" );

            Assert.IsTrue( me.GetLeafFieldValueString( ref pc, msg ) == "999999" );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( me.GetLeafFieldValueString( ref pc, msg ) == "999999" );
            Assert.IsTrue( me.GetLeafFieldValueString( ref pc, null ) == "111111" );
        }

        /// <summary>
        /// Test GetLeafFieldValueBytes function.
        /// </summary>
        [Test( Description = "Test GetLeafFieldValueBytes function" )]
        public void GetLeafFieldValueBytes() {

            MessageExpression me = new MessageExpression( 52 );
            ParserContext pc = new ParserContext( ParserContext.DefaultBufferSize );
            FormatterContext fc = new FormatterContext( FormatterContext.DefaultBufferSize );

            // Passing null message (as parameter and in the contexts).
            try {
                me.GetLeafFieldValueBytes( ref fc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }
            try {
                me.GetLeafFieldValueBytes( ref pc, null );
                Assert.Fail();
            }
            catch ( ExpressionEvaluationException ) {
            }

            Message msg = MessagesProvider.GetMessage();
            Message anotherMsg = MessagesProvider.GetAnotherMessage();

            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            fc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref fc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref fc, null ),
                new byte[] { 0x55, 0x60, 0x65, 0x70, 0x75, 0x80, 0x85, 0x90 } ) );

            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            pc.CurrentMessage = anotherMsg;
            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref pc, msg ),
                new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } ) );
            Assert.IsTrue( MessagesProvider.CompareByteArrays( me.GetLeafFieldValueBytes( ref pc, null ),
                new byte[] { 0x55, 0x60, 0x65, 0x70, 0x75, 0x80, 0x85, 0x90 } ) );
        }
        #endregion
    }
}
