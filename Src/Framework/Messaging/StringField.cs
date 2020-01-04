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
using Trx.Utilities;

namespace Trx.Messaging {

    [Serializable]
    public class StringField : Field {

        private string _value;

        public StringField( int fieldNumber )
            : base( fieldNumber ) {

            _value = null;
        }

        public StringField( int fieldNumber, string value )
            : base( fieldNumber ) {

            _value = value;
        }

        public string FieldValue {

            get {

                return _value;
            }

            set {

                _value = value;
            }
        }

        public override object Value {

            get {

                return _value;
            }

            set {

                if ( value is string ) {
                    _value = ( string )value;
                } else if ( value == null ) {
                    _value = null;
                } else if ( value is byte[] ) {
                    _value = FrameworkEncoding.GetInstance().Encoding.GetString( ( byte[] )value );
                } else {
                    throw new ArgumentException( "Can't handle parameter type.", "value" );
                }
            }
        }

        public override string ToString()
        {
            if ( _value == null ) {
                return string.Empty;
            }

            return _value;
        }

        public override byte[] GetBytes() {

            if ( _value == null ) {
                return null;
            }

            return FrameworkEncoding.GetInstance().Encoding.GetBytes( _value );
        }

        public override object Clone()
        {
            if ( _value == null ) {
                return new StringField( FieldNumber );
            }

            return new StringField( FieldNumber,
                string.Copy( _value ) );
        }

        public override MessagingComponent NewComponent() {

            return new StringField( FieldNumber );
        }
    }
}
