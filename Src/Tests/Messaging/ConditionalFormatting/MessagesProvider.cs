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
using Trx.Messaging.Iso8583;

namespace Tests.Trx.Messaging.ConditionalFormatting {

    internal class MessagesProvider {

        // TODO: Pasarla a Trx.Utils.
        public static bool CompareByteArrays( byte[] data1, byte[] data2 ) {

            // If both are null, they're equal
            if ( data1 == null && data2 == null ) {
                return true;
            }

            // If either but not both are null, they're not equal
            if ( data1 == null || data2 == null ) {
                return false;
            }

            if ( data1.Length != data2.Length ) {
                return false;
            }

            for ( int i = data1.Length - 1; i >= 0; i-- ) {
                if ( data1[i] != data2[i] ) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// It ad several fields to the specified message.
        /// </summary>
        /// <param name="message"></param>
        private static void AddFields( Message message ) {

            message.Fields.Add( 3, "999999" );
            message.Fields.Add( 11, "000001" );
            message.Fields.Add( 12, "103400" );
            message.Fields.Add( 13, "1124" );
            message.Fields.Add( 24, "9" );
            message.Fields.Add( 41, "TEST1" );
            message.Fields.Add( 52, new byte[] { 0x15, 0x20, 0x25, 0x30, 0x35, 0x40, 0x45, 0x50 } );

            Iso8583Message innerFixedSizeMsg = new Iso8583Message( 800 );
            innerFixedSizeMsg.Fields.Add( 3, "A" );
            innerFixedSizeMsg.Fields.Add( 6, "123" );
            innerFixedSizeMsg.Fields.Add( 8, "The key" );
            message.Fields.Add( 61, innerFixedSizeMsg );
            innerFixedSizeMsg.Parent = message; // This is done by the message formatter.

            Message innerVarSizeMsg = new Message();
            innerVarSizeMsg.Fields.Add( 1, "4" );
            innerVarSizeMsg.Fields.Add( 2, "101" );
            innerVarSizeMsg.Fields.Add( 4, "John Doe" );
            innerVarSizeMsg.Fields.Add( 6, "67" );
            innerVarSizeMsg.Fields.Add( 7, new byte[] { 0x75, 0xB0, 0xB5 } );
            message.Fields.Add( 62, innerVarSizeMsg );
            innerVarSizeMsg.Parent = message;   // This is done by the message formatter.
        }

        /// <summary>
        /// It ad several fields to the specified message.
        /// </summary>
        /// <param name="message"></param>
        private static void AddAnotherFields( Message message ) {

            message.Fields.Add( 3, "111111" );
            message.Fields.Add( 11, "1000001" );
            message.Fields.Add( 12, "103401" );
            message.Fields.Add( 13, "1224" );
            message.Fields.Add( 24, "8" );
            message.Fields.Add( 41, "TEST2" );
            message.Fields.Add( 52, new byte[] { 0x55, 0x60, 0x65, 0x70, 0x75, 0x80, 0x85, 0x90 } );

            Iso8583Message innerFixedSizeMsg = new Iso8583Message( 810 );
            innerFixedSizeMsg.Fields.Add( 3, "B" );
            innerFixedSizeMsg.Fields.Add( 6, "456" );
            innerFixedSizeMsg.Fields.Add( 8, "Another key" );
            message.Fields.Add( 61, innerFixedSizeMsg );
            innerFixedSizeMsg.Parent = message; // This is done by the message formatter.

            Message innerVarSizeMsg = new Message();
            innerVarSizeMsg.Fields.Add( 1, "5" );
            innerVarSizeMsg.Fields.Add( 2, "109" );
            innerVarSizeMsg.Fields.Add( 4, "John Peter Doe" );
            innerVarSizeMsg.Fields.Add( 6, "167" );
            innerVarSizeMsg.Fields.Add( 7, new byte[] { 0x95, 0xA0, 0xA5 } );
            message.Fields.Add( 62, innerVarSizeMsg );
            innerVarSizeMsg.Parent = message;   // This is done by the message formatter.
        }

        /// <summary>
        /// It builds and returns a message suitable for testings.
        /// </summary>
        /// <returns></returns>
        public static Message GetMessage() {

            Message message = new Message();

            AddFields( message );

            return message;
        }

        /// <summary>
        /// It builds and returns a message suitable for testings.
        /// </summary>
        /// <returns></returns>
        public static Message GetAnotherMessage() {

            Message message = new Message();

            AddAnotherFields( message );

            return message;
        }

        /// <summary>
        /// It builds and returns am ISO 8583 message suitable for testings.
        /// </summary>
        /// <returns></returns>
        public static Iso8583Message GetIso8583Message() {

            Iso8583Message message = new Iso8583Message( 200 );

            AddFields( message );

            return message;
        }

        /// <summary>
        /// It builds and returns am ISO 8583 message suitable for testings.
        /// </summary>
        /// <returns></returns>
        public static Iso8583Message GetAnotherIso8583Message() {

            Iso8583Message message = new Iso8583Message( 210 );

            AddAnotherFields( message );

            return message;
        }
    }
}
