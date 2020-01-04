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
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

    /// <summary>
    /// Test fixture for SelfAnnouncedStringFieldFormatter.
    /// </summary>
    [TestFixture( Description = "Self announced string field formatter tests." )]
    public class SelfAnnouncedStringFieldFormatterTest {

        #region Constructors
        /// <summary>
        /// It builds and initializes a new instance of the class
        /// <see cref="SelfAnnouncedStringFieldFormatterTest"/>.
        /// </summary>
        public SelfAnnouncedStringFieldFormatterTest() {

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
                new SelfAnnouncedFieldNumberManager( StringLengthEncoder.GetInstance( 99 ) );
            SelfAnnouncedStringFieldFormatter formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), true, safnm,
                DataEncoder.GetInstance(),
                SpacePaddingRight.GetInstance( false ), NumericValidator.GetInstance(),
                null, "My formatter" );

            Assert.IsTrue( formatter.FieldNumber == 37 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 12 );
            Assert.IsTrue( formatter.Description.Equals( "My formatter" ) );
            Assert.IsTrue( formatter.IncludeSelfAnnouncementInLength );
            Assert.IsTrue( formatter.SelfAnnounceManager == safnm );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false ) );
            Assert.IsTrue( formatter.Validator == NumericValidator.GetInstance() );
            Assert.IsNull( formatter.ValueFormatter );

            // Test variable length properties without padding.
            formatter = new SelfAnnouncedStringFieldFormatter( 63, new VariableLengthManager( 1, 800,
                StringLengthEncoder.GetInstance( 5 ) ), false, safnm, DataEncoder.GetInstance() );

            Assert.IsTrue( formatter.FieldNumber == 63 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 800 );
            Assert.IsTrue( formatter.Description.Equals( string.Empty ) );
            Assert.IsFalse( formatter.IncludeSelfAnnouncementInLength );
            Assert.IsTrue( formatter.SelfAnnounceManager == safnm );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.Padding == null );
            Assert.IsTrue( formatter.Validator == null );

            // Test variable length properties with padding.
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 3 ) ), false, safnm, DataEncoder.GetInstance(),
                SpacePaddingRight.GetInstance( false ), "My formatter" );

            Assert.IsTrue( formatter.FieldNumber == 48 );
            Assert.IsTrue( formatter.LengthManager.MaximumLength == 999 );
            Assert.IsTrue( formatter.Description.Equals( "My formatter" ) );
            Assert.IsFalse( formatter.IncludeSelfAnnouncementInLength );
            Assert.IsTrue( formatter.SelfAnnounceManager == safnm );
            Assert.IsTrue( formatter.Encoder == DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.Padding == SpacePaddingRight.GetInstance( false ) );
            Assert.IsTrue( formatter.Validator == null );
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
            StringField field = new StringField( 37, "DATA" );
            SelfAnnouncedStringFieldFormatter formatter;
            string formattedData;

            // Test fixed length formatting.
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), false, safnm, DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "37DATA        " ) );

            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), true, safnm, DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "37DATA      " ) );

            // Test variable length formatting without padding.
            formatterContext.Clear();
            field = new StringField( 48 );
            field.Value = "DATA";
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), false, safnm,
                DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00448DATA" ) );
            formatterContext.Clear();
            formatter.Format( new StringField( 5, null ), ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00005" ) );

            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), true, safnm,
                DataEncoder.GetInstance() );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00648DATA" ) );
            formatterContext.Clear();
            formatter.Format( new StringField( 5, null ), ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "00205" ) );

            // Test variable length formatting with padding.
            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 10,
                10, StringLengthEncoder.GetInstance( 10 ) ), false, safnm,
                DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( false ),
                string.Empty );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "1048      DATA" ) );

            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 2,
                12, StringLengthEncoder.GetInstance( 12 ) ), true, safnm,
                DataEncoder.GetInstance(), SpacePaddingLeft.GetInstance( false ),
                string.Empty );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "1248      DATA" ) );

            // Test validator with fixed length formatting.
            formatterContext.Clear();
            field = new StringField( 37 );
            field.FieldValue = "000000001500";
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), false, safnm, DataEncoder.GetInstance(),
                NumericValidator.GetInstance(), string.Empty );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "37000000001500" ) );

            formatterContext.Clear();
            field = new StringField( 37 );
            field.FieldValue = "0000001500";
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), true, safnm, DataEncoder.GetInstance(),
                NumericValidator.GetInstance(), string.Empty );
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "370000001500" ) );

            // Try with an invalid value.
            formatterContext.Clear();
            field.FieldValue = "37D1500";
            try {
                formatter.Format( field, ref formatterContext );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }

            // Test validator with fixed length formatting and numeric padding.
            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), false, safnm, DataEncoder.GetInstance(),
                ZeroPaddingLeft.GetInstance( false, true ), NumericValidator.GetInstance() );
            field.FieldValue = "56600";
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "37000000056600" ) );

            formatterContext.Clear();
            formatter = new SelfAnnouncedStringFieldFormatter( 37,
                new FixedLengthManager( 12 ), true, safnm, DataEncoder.GetInstance(),
                ZeroPaddingLeft.GetInstance( false, true ), NumericValidator.GetInstance() );
            field.FieldValue = "56600";
            formatter.Format( field, ref formatterContext );
            formattedData = formatterContext.GetDataAsString();
            Assert.IsTrue( formattedData.Equals( "370000056600" ) );

            // Try with an invalid value.
            formatterContext.Clear();
            field.FieldValue = "37D1500";
            try {
                formatter.Format( field, ref formatterContext );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }
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
            StringField field;
            SelfAnnouncedStringFieldFormatter formatter;
            int fieldNumber;

            // Setup data for six complete fields an one with partial data.
            parseContext.Write(
                "37DATA        37DATA      2030   DATA TO BE PARSED2030 DATA TO BE PARSED00920SOME DATA01120SOME DATA01" );

            // Test fixed length parse.
            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                false, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 37 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "DATA" ) );

            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                true, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 37 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "DATA" ) );

            // Test variable length parse with padding.
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1, 20,
                StringLengthEncoder.GetInstance( 99 ) ), false, safnm, DataEncoder.GetInstance(),
                SpacePaddingLeft.GetInstance( false ), string.Empty );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 30 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "DATA TO BE PARSED" ) );

            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1, 20,
                StringLengthEncoder.GetInstance( 99 ) ), true, safnm, DataEncoder.GetInstance(),
                SpacePaddingLeft.GetInstance( false ), string.Empty );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 30 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "DATA TO BE PARSED" ) );

            // Test variable length parse without padding.
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 999 ) ), false, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "SOME DATA" ) );

            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 999 ) ), true, safnm, DataEncoder.GetInstance() );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "SOME DATA" ) );

            // Test partial variable length parse without padding.
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "12" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2MORE D" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 22 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ATA" );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "MORE DATA" ) );

            // Test partial fixed parse with padding.
            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                false, safnm, DataEncoder.GetInstance() );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "6" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 62 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ONE MORE" );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "    " );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "ONE MORE" ) );

            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                true, safnm, DataEncoder.GetInstance() );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "6" );
            Assert.IsFalse( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            parseContext.Write( "2" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 62 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "ONE MORE" );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNull( field );
            parseContext.Write( "  " );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "ONE MORE" ) );
    
            // Test variable length header with zero length.
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), false, safnm, DataEncoder.GetInstance() );
            parseContext.Write( "00020" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsNull( field.FieldValue );

            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 0,
                999, StringLengthEncoder.GetInstance( 999 ) ), true, safnm, DataEncoder.GetInstance() );
            parseContext.Write( "00220" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 20 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsNull( field.FieldValue );

            // Test fixed length parse with validation.
            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                false, safnm, DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance( false, true ),
                NumericValidator.GetInstance() );
            parseContext.Write( "04000000145000" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 4 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "145000" ) );

            formatter = new SelfAnnouncedStringFieldFormatter( 37, new FixedLengthManager( 12 ),
                true, safnm, DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance( false, true ),
                NumericValidator.GetInstance() );
            parseContext.Write( "040000145000" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 4 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "145000" ) );

            // Try with an invalid value.
            parseContext.Write( "0400001450F0" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 4 );
            try {
                field = ( StringField )formatter.Parse( ref parseContext );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }

            // Test variable length parse with validation.
            parseContext.Clear();
            parseContext.ResetDecodedLength();
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 999 ) ), false, safnm, DataEncoder.GetInstance(),
                NumericValidator.GetInstance() );
            parseContext.Write( "0051432000" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 14 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "32000" ) );

            parseContext.Clear();
            parseContext.ResetDecodedLength();
            formatter = new SelfAnnouncedStringFieldFormatter( 48, new VariableLengthManager( 1,
                999, StringLengthEncoder.GetInstance( 999 ) ), true, safnm, DataEncoder.GetInstance(),
                NumericValidator.GetInstance() );
            parseContext.Write( "0071432000" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 14 );
            field = ( StringField )formatter.Parse( ref parseContext );
            Assert.IsNotNull( field );
            parseContext.ResetDecodedLength();
            Assert.IsTrue( field.FieldValue.Equals( "32000" ) );

            // Try with an invalid value.
            parseContext.Write( "00704350F0" );
            Assert.IsTrue( formatter.GetFieldNumber( ref parseContext, out fieldNumber ) );
            Assert.IsTrue( fieldNumber == 4 );
            try {
                field = ( StringField )formatter.Parse( ref parseContext );
                Assert.Fail();
            }
            catch ( StringValidationException ) {
            }
        }
        #endregion
    }
}
