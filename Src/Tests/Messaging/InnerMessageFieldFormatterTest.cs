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
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

    /// <summary>
    /// Test fixture for InnerMessageFieldFormatter.
    /// </summary>
    [TestFixture( Description = "Inner message field formatter tests." )]
    public class InnerMessageFieldFormatterTest {

        #region Constructors
        /// <summary>
        /// It builds and initializes a new instance of the class
        /// <see cref="InnerMessageFieldFormatterTest"/>.
        /// </summary>
        public InnerMessageFieldFormatterTest() {

        }
        #endregion

        #region Methods
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp() {

        }

        // Configure some fields for a fixed length message formatter.
        private FieldFormatter[] _fixedMessageFormatter = {
			new StringFieldFormatter( 1, new FixedLengthManager( 2), DataEncoder.GetInstance()),
			new StringFieldFormatter( 2, new FixedLengthManager( 3), DataEncoder.GetInstance())};

        private BasicMessageFormatter GetFormatter( FieldFormatter[] fieldFormatters ) {

            BasicMessageFormatter formatter = new BasicMessageFormatter();

            // Add field Formatters.
            for ( int i = 0; i < fieldFormatters.Length; i++ ) {
                if ( fieldFormatters[i] != null ) {
                    formatter.FieldFormatters.Add( fieldFormatters[i] );
                }
            }

            return formatter;
        }

        /// <summary>
        /// Test constructors.
        /// </summary>
        [Test( Description = "Test constructors." )]
        public void Constructors() {

            // Test fixed length properties.
            InnerMessageFieldFormatter formatter = new InnerMessageFieldFormatter( 37,
                new FixedLengthManager( 12 ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter), "My formatter" );

            Assert.IsTrue( formatter.FieldNumber == 37 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 12 );
            Assert.IsTrue( formatter.Description.Equals( "My formatter" ) );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );

            // Test variable length properties.
            formatter = new InnerMessageFieldFormatter( 63, new VariableLengthManager( 1, 800,
                StringLengthEncoder.GetInstance( 5 ) ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ) );

            Assert.IsTrue( formatter.FieldNumber == 63 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 800 );
            Assert.IsTrue( formatter.Description.Equals( string.Empty ) );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );
        }

        /// <summary>
        /// Test Format method.
        /// </summary>
        [Test( Description = "Test Format method." )]
        public void Format() {

            FormatterContext formatterContext = new FormatterContext(
                FormatterContext.DefaultBufferSize );
            Message value = new Message();
            value.Formatter = GetFormatter( _fixedMessageFormatter );
            value.Fields.Add( 1, "HE" );
            value.Fields.Add( 2, "LLO" );

            InnerMessageField field = new InnerMessageField( 37, value );

            InnerMessageFieldFormatter formatter;
            string formattedData;

            // Test fixed length formatting.
            formatter = new InnerMessageFieldFormatter( 37,
                new FixedLengthManager( 5 ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ) );

            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "HELLO" ) );

            formatterContext.Clear();
            formatter = new InnerMessageFieldFormatter( 37,
                new FixedLengthManager( 4 ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ) );
            try {
                formatter.Format( field, ref formatterContext );
                Assert.Fail();
            }
            catch ( ArgumentOutOfRangeException e ) {
                Assert.IsTrue( e.ParamName.Equals( "dataLength" ) );
            }

            // Test variable length formatting.
            formatterContext.Clear();
            formatter = new InnerMessageFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ),
                DataEncoder.GetInstance(), GetFormatter( _fixedMessageFormatter ) );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "005HELLO" ) );
            formatterContext.Clear();
            formatter.Format( new InnerMessageField( 35, null ), ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "000" ) );
        }

        /// <summary>
        /// Test Parse method.
        /// </summary>
        [Test( Description = "Test Parse method." )]
        public void Parse() {

            ParserContext parseContext = new ParserContext(
                ParserContext.DefaultBufferSize );
            InnerMessageField field;
            InnerMessageFieldFormatter formatter;

            parseContext.Write( "HELLO0512345005ABCDE00" );

            // Test fixed length parse.
            formatter = new InnerMessageFieldFormatter( 37, new FixedLengthManager( 5 ),
                DataEncoder.GetInstance(), GetFormatter( _fixedMessageFormatter ) );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString() == "1:HE,2:LLO" );

            // Test variable length parse.
            formatter = new InnerMessageFieldFormatter( 48, new VariableLengthManager( 1, 20,
                StringLengthEncoder.GetInstance( 99 ) ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ), string.Empty );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString() == "1:12,2:345" );

            formatter = new InnerMessageFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 999 ) ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ) );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString() == "1:AB,2:CDE" );

            // Test partial variable length parse.
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "5HOU" );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "SE" );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString() == "1:HO,2:USE" );

            // Test partial fixed.
            formatter = new InnerMessageFieldFormatter( 37, new FixedLengthManager( 5 ),
                DataEncoder.GetInstance(), GetFormatter( _fixedMessageFormatter ) );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "34 " );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "56" );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString() == "1:34,2: 56" );

            // Test variable length header with zero length.
            formatter = new InnerMessageFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), DataEncoder.GetInstance(),
                GetFormatter( _fixedMessageFormatter ) );
            parseContext.Write( "000" );
            field = ( InnerMessageField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsNull( field.Value );
        }
        #endregion
    }
}
