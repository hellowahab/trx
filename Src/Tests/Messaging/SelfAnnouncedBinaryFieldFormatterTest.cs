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
    /// Test fixture for SelfAnnouncedBinaryFieldFormatter.
    /// </summary>
    [TestFixture( Description = "Self announced binary field formatter tests." )]
    public class SelfAnnouncedBinaryFieldFormatterTest {

        #region Constructors
        /// <summary>
        /// It builds and initializes a new instance of the class
        /// <see cref="SelfAnnouncedBinaryFieldFormatterTest"/>.
        /// </summary>
        public SelfAnnouncedBinaryFieldFormatterTest() {

        }
        #endregion

        #region Methods
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp() {

        }

        /// <summary>
        /// Test constructors.
        /// </summary>
        [Test( Description = "Test constructors." )]
        public void Constructors() {

            // Test fixed length properties.
            SelfAnnouncedFieldNumberManager safnm =
                new SelfAnnouncedFieldNumberManager( BcdLengthEncoder.GetInstance( 9999 ) );
            SelfAnnouncedBinaryFieldFormatter formatter = new SelfAnnouncedBinaryFieldFormatter( 37,
                new FixedLengthManager( 12 ), true, safnm,
                DataEncoder.GetInstance(), "My formatter" );

            Assert.IsTrue( formatter.FieldNumber == 37 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 12 );
            Assert.IsTrue( formatter.Description.Equals( "My formatter" ) );
            Assert.IsTrue( formatter.IncludeSelfAnnouncementInLength );
            Assert.IsTrue( formatter.SelfAnnounceManager == safnm );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );

            // Test variable length properties.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 63, new VariableLengthManager( 1, 800,
                StringLengthEncoder.GetInstance( 5 ) ), false, safnm, DataEncoder.GetInstance() );

            Assert.IsTrue( formatter.FieldNumber == 63 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 800 );
            Assert.IsTrue( formatter.Description.Equals( string.Empty ) );
            Assert.IsFalse( formatter.IncludeSelfAnnouncementInLength );
            Assert.IsTrue( formatter.SelfAnnounceManager == safnm );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );
        }

        /// <summary>
        /// Test Format method.
        /// </summary>
        [Test( Description = "Test Format method." )]
        public void Format() {

            SelfAnnouncedFieldNumberManager safnm =
                new SelfAnnouncedFieldNumberManager( StringLengthEncoder.GetInstance( 99 ) );
            FormatterContext formatterContext = new FormatterContext(
                FormatterContext.DefaultBufferSize );
            BinaryField field = new BinaryField( 37 );
            field.Value = "DATA";
            SelfAnnouncedBinaryFieldFormatter formatter;
            string formattedData;

            // Test fixed length formatting.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 37,
                new FixedLengthManager( 4 ), false, safnm, DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Console.WriteLine( formattedData );
            Assert.IsTrue( formattedData == "37DATA" );

            formatterContext.Clear();
            field = new BinaryField( 48 );
            field.Value = "MORE DATA";
            try {
                formatter.Format( field, ref formatterContext );
                Assert.Fail();
            }
            catch ( ArgumentOutOfRangeException e ) {
                Assert.IsTrue( e.ParamName.Equals( "dataLength" ) );
            }

            // Test variable length formatting.
            formatterContext.Clear();
            formatter = new SelfAnnouncedBinaryFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), false, safnm,
                DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00948MORE DATA" ) );
            formatterContext.Clear();
            formatter.Format( new BinaryField( 5, null ), ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00005" ) );

            // Test variable length formatting.
            formatterContext.Clear();
            formatter = new SelfAnnouncedBinaryFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), true, safnm,
                DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "01148MORE DATA" ) );
            formatterContext.Clear();
            formatter.Format( new BinaryField( 5, null ), ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00205" ) );
        }

        /// <summary>
        /// Test Parse method.
        /// </summary>
        [Test( Description = "Test Parse method." )]
        public void Parse() {

            SelfAnnouncedFieldNumberManager safnm =
                new SelfAnnouncedFieldNumberManager( StringLengthEncoder.GetInstance( 99 ) );
            ParserContext parseContext = new ParserContext(
                ParserContext.DefaultBufferSize );
            BinaryField field;
            SelfAnnouncedBinaryFieldFormatter formatter;
            int fieldNumber;

            // Setup data for six complete fields an one with partial data.
            parseContext.Write(
                "37DATA37DT1730DATA TO BE PARSED1930DATA TO BE PARSED00921SOME DATA01121SOME DATA01" );

            // Test fixed length parse.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 37, new FixedLengthManager( 4 ),
                false, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 37 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "DATA" ) );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 37, new FixedLengthManager( 4 ),
                true, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 37 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "DT" ) );

            // Test variable length parse.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 1, 20, StringLengthEncoder.GetInstance( 99 ) ),
                false, safnm, DataEncoder.GetInstance(), string.Empty );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 30 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "DATA TO BE PARSED" ) );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 1, 20, StringLengthEncoder.GetInstance( 99 ) ),
                true, safnm, DataEncoder.GetInstance(), string.Empty );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 30 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "DATA TO BE PARSED" ) );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 1, 999, StringLengthEncoder.GetInstance( 999 ) ),
                false, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 21 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "SOME DATA" ) );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 1, 999, StringLengthEncoder.GetInstance( 999 ) ),
                true, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 21 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "SOME DATA" ) );

            // Test partial variable length parse.
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "12" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2MORE D" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 22 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ATA" );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "MORE DATA" ) );

            // Test partial fixed.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 37, new FixedLengthManager( 8 ),
                false, safnm, DataEncoder.GetInstance() );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "6" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 62 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ONE " );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "MORE" );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "ONE MORE" ) );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 37, new FixedLengthManager( 10 ),
                true, safnm, DataEncoder.GetInstance() );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "6" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 62 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ONE " );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "MORE" );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.ToString().Equals( "ONE MORE" ) );

            // Test variable length header with zero length.
            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 0, 999, StringLengthEncoder.GetInstance( 999 ) ),
                false, safnm, DataEncoder.GetInstance() );
            parseContext.Write( "00020" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsNull( field.Value );

            formatter = new SelfAnnouncedBinaryFieldFormatter( 48,
                new VariableLengthManager( 0, 999, StringLengthEncoder.GetInstance( 999 ) ),
                true, safnm, DataEncoder.GetInstance() );
            parseContext.Write( "00220" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( BinaryField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsNull( field.Value );
        }
        #endregion
    }
}
