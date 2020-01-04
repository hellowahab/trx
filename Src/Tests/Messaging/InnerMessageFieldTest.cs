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
using System.Text;

using Trx.Messaging;
using Trx.Utilities;
using NUnit.Framework;

namespace Tests.Trx.Messaging {

    /// <summary>
    /// Test fixture for InnerMessageField.
    /// </summary>
    [TestFixture( Description = "Inner message field tests." )]
    public class InnerMessageFieldTest {

        #region Constructors
        /// <summary>
        /// It builds and initializes a new instance of the class
        /// <see cref="InnerMessageFieldTest"/>.
        /// </summary>
        public InnerMessageFieldTest() {

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
        /// Test object instantiation.
        /// </summary>
        [Test( Description = "Test object instantiation." )]
        public void Instantiation() {

            Message value = new Message();
            InnerMessageField field = new InnerMessageField( 30 );

            Assert.IsTrue( field.FieldNumber == 30 );
            Assert.IsNull( field.Value );

            field = new InnerMessageField( 15, value );

            Assert.IsTrue( field.FieldNumber == 15 );
            Assert.IsTrue( field.Value is Message );
            Message fieldValue = field.Value as Message;
            Assert.IsTrue( value == fieldValue );
        }

        /// <summary>
        /// Test Value property.
        /// </summary>
        [Test( Description = "Test Value property." )]
        public void Value() {

            Message value = new Message();
            InnerMessageField field = new InnerMessageField( 18 );

            Assert.IsNull( field.Value );

            field.Value = value;
            Assert.IsTrue( field.Value is Message );
            Message fieldValue = field.Value as Message;
            Assert.IsTrue( value == fieldValue );

            try {
                field.Value = field.FieldNumber;
                Assert.Fail();
            }
            catch ( ArgumentException e ) {
                Assert.IsTrue( e.ParamName == "value" );
            }
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
        /// Test ToString method.
        /// </summary>
        [Test( Description = "Test ToString method." )]
        public void TestToString() {

            Message value = new Message();
            value.Formatter = GetFormatter( _fixedMessageFormatter );
            value.Fields.Add( 1, "98" );
            value.Fields.Add( 2, "345" );

            InnerMessageField field = new InnerMessageField( 12 );

            Assert.IsTrue( string.Empty == field.ToString() );
            field.Value = value;

            Assert.IsTrue( field.ToString() == "1:98,2:345" );
        }

        /// <summary>
        /// Test GetBytes method.
        /// </summary>
        [Test( Description = "Test GetBytes method." )]
        public void GetBytes() {

            Message value = new Message();

            InnerMessageField field = new InnerMessageField( 19, value );

            Assert.IsNull( field.GetBytes() );

            value.Formatter = GetFormatter( _fixedMessageFormatter );
            value.Fields.Add( 1, "HE" );
            value.Fields.Add( 2, "LLO" );

            Assert.IsTrue(
                FrameworkEncoding.GetInstance().Encoding.GetString( field.GetBytes() ) == "HELLO" );
        }

        /// <summary>
        /// Test Clone method.
        /// </summary>
        [Test( Description = "Test Clone method." )]
        public void TestClone() {

            Message value = new Message();
            value.Formatter = GetFormatter( _fixedMessageFormatter );
            value.Fields.Add( 1, "12" );
            value.Fields.Add( 2, "345" );

            InnerMessageField field = new InnerMessageField( 14 );

            InnerMessageField clonedField = ( InnerMessageField )( field.Clone() );

            Assert.IsNull( clonedField.Value );
            Assert.IsTrue( field.FieldNumber == clonedField.FieldNumber );

            field.Value = value;
            clonedField = ( InnerMessageField )( field.Clone() );

            Assert.IsTrue( field.ToString() == clonedField.ToString() );
            Assert.IsTrue( field.Value != clonedField.Value );
        }

        /// <summary>
        /// Test NewComponent method.
        /// </summary>
        [Test( Description = "Test NewComponent method." )]
        public void NewComponent() {

            Message value = new Message();
            value.Formatter = GetFormatter( _fixedMessageFormatter );
            value.Fields.Add( 1, "12" );
            value.Fields.Add( 2, "345" );
            InnerMessageField field = new InnerMessageField( 18, value );
            MessagingComponent component = field.NewComponent();

            Assert.IsTrue( component is InnerMessageField );
            Assert.IsTrue( field.FieldNumber ==
                ( ( InnerMessageField )component ).FieldNumber );
            Assert.IsNull( ( ( InnerMessageField )component ).Value );
        }
        #endregion
    }
}
