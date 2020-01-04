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

using Trx.Utilities;

namespace Trx.Messaging.Iso8583
{
    /// <summary>
    /// It defines an ISO 8583 messages formatter to produce and parse
    /// strings without control characters. The definition is for the
    /// ISO 8583-93.
    /// </summary>
    public class Iso8583Bin1993MessageFormatter : Iso8583MessageFormatter
    {
        /// <summary>
        /// It initializes a new ISO 8583 formatter.
        /// </summary>
        public Iso8583Bin1993MessageFormatter()
        {
            SetupFields();
        }

        /// <summary>
        /// It initializes the fields formatters for this message formatter.
        /// </summary>
        private void SetupFields()
        {
            MessageTypeIdentifierFormatter = new StringFieldFormatter(
                -1, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Message type identifier");

            FieldFormatters.Add(new BitMapFieldFormatter(0, 1, 64,
                DataEncoder.GetInstance(), "Primary bitmap"));
            FieldFormatters.Add(new BitMapFieldFormatter(1, 65, 128,
                DataEncoder.GetInstance(), "Secondary bitmap"));

            FieldFormatters.Add(new StringFieldFormatter(
                2, new VariableLengthManager(0, 19, BcdLengthEncoder.GetInstance(19)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Primary account number"));
            FieldFormatters.Add(new StringFieldFormatter(
                3, new FixedLengthManager(6), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Processing code"));
            FieldFormatters.Add(new StringFieldFormatter(
                4, new FixedLengthManager(12), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Transaction amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                5, new FixedLengthManager(12), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Reconciliation amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                6, new FixedLengthManager(12), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Cardholder billing amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                7, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Transmission date and time"));
            FieldFormatters.Add(new StringFieldFormatter(
                8, new FixedLengthManager(8), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Cardholder billing fee amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                9, new FixedLengthManager(8), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Reconciliation conversion rate"));
            FieldFormatters.Add(new StringFieldFormatter(
                10, new FixedLengthManager(8), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Cardholder billing conversion rate"));
            FieldFormatters.Add(new StringFieldFormatter(
                11, new FixedLengthManager(6), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Systems trace audit number"));
            FieldFormatters.Add(new StringFieldFormatter(
                12, new FixedLengthManager(12), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Local transaction date and time"));
            FieldFormatters.Add(new StringFieldFormatter(
                13, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Effective date"));
            FieldFormatters.Add(new StringFieldFormatter(
                14, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Expiration date"));
            FieldFormatters.Add(new StringFieldFormatter(
                15, new FixedLengthManager(6), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Settlement date"));
            FieldFormatters.Add(new StringFieldFormatter(
                16, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Conversion date"));
            FieldFormatters.Add(new StringFieldFormatter(
                17, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Capture date"));
            FieldFormatters.Add(new StringFieldFormatter(
                18, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Merchant type"));
            FieldFormatters.Add(new StringFieldFormatter(
                19, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Acquiring institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                20, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Primary account number country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                21, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Forwarding institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                22, new FixedLengthManager(12), DataEncoder.GetInstance(),
                "Point of service data code"));
            FieldFormatters.Add(new StringFieldFormatter(
                23, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Card sequence number"));
            FieldFormatters.Add(new StringFieldFormatter(
                24, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Function code"));
            FieldFormatters.Add(new StringFieldFormatter(
                25, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Message reason code"));
            FieldFormatters.Add(new StringFieldFormatter(
                26, new FixedLengthManager(4), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Card acceptor business code"));
            FieldFormatters.Add(new StringFieldFormatter(
                27, new FixedLengthManager(1), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Approval code length"));
            FieldFormatters.Add(new StringFieldFormatter(
                28, new FixedLengthManager(6), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Reconciliation date"));
            FieldFormatters.Add(new StringFieldFormatter(
                29, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Reconciliation indicator"));
            FieldFormatters.Add(new StringFieldFormatter(
                30, new FixedLengthManager(24), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Original amounts"));
            FieldFormatters.Add(new StringFieldFormatter(
                31, new VariableLengthManager(0, 99, BcdLengthEncoder.GetInstance(99)),
                DataEncoder.GetInstance(), "Acquirer reference data"));
            FieldFormatters.Add(new StringFieldFormatter(
                32, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Acquirer institution identification code"));
            FieldFormatters.Add(new StringFieldFormatter(
                33, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Forwarding institution identification code"));
            FieldFormatters.Add(new StringFieldFormatter(
                34, new VariableLengthManager(0, 28, BcdLengthEncoder.GetInstance(28)),
                DataEncoder.GetInstance(), "Extended primary account number"));
            FieldFormatters.Add(new StringFieldFormatter(
                35, new VariableLengthManager(0, 37, BcdLengthEncoder.GetInstance(37)),
                BcdDataEncoder.GetInstance(false, 0), "Track 2 data"));
            FieldFormatters.Add(new StringFieldFormatter(
                36, new VariableLengthManager(0, 104, BcdLengthEncoder.GetInstance(104)),
                DataEncoder.GetInstance(), "Track 3 data"));
            FieldFormatters.Add(new StringFieldFormatter(
                37, new FixedLengthManager(12), DataEncoder.GetInstance(),
                "Retrieval reference number"));
            FieldFormatters.Add(new StringFieldFormatter(
                38, new FixedLengthManager(6), DataEncoder.GetInstance(),
                "Approval code"));
            FieldFormatters.Add(new StringFieldFormatter(
                39, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Action code"));
            FieldFormatters.Add(new StringFieldFormatter(
                40, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Service code"));
            FieldFormatters.Add(new StringFieldFormatter(
                41, new FixedLengthManager(8), DataEncoder.GetInstance(),
                "Card acceptor terminal identification"));
            FieldFormatters.Add(new StringFieldFormatter(
                42, new FixedLengthManager(15), DataEncoder.GetInstance(),
                "Card acceptor identification code"));
            FieldFormatters.Add(new StringFieldFormatter(
                43, new VariableLengthManager(0, 99, BcdLengthEncoder.GetInstance(99)),
                DataEncoder.GetInstance(), "Card acceptor name/location"));
            FieldFormatters.Add(new StringFieldFormatter(
                44, new VariableLengthManager(0, 99, BcdLengthEncoder.GetInstance(99)),
                DataEncoder.GetInstance(), "Additional response data"));
            FieldFormatters.Add(new StringFieldFormatter(
                45, new VariableLengthManager(0, 76, BcdLengthEncoder.GetInstance(76)),
                DataEncoder.GetInstance(), "Track 1 data"));
            FieldFormatters.Add(new StringFieldFormatter(
                46, new VariableLengthManager(0, 204, BcdLengthEncoder.GetInstance(204)),
                DataEncoder.GetInstance(), "Fees amounts"));
            FieldFormatters.Add(new StringFieldFormatter(
                47, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Additional data - national"));
            FieldFormatters.Add(new StringFieldFormatter(
                48, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Additional data - private"));
            FieldFormatters.Add(new StringFieldFormatter(
                49, new FixedLengthManager(3), DataEncoder.GetInstance(),
                "Transaction currency code"));
            FieldFormatters.Add(new StringFieldFormatter(
                50, new FixedLengthManager(3), DataEncoder.GetInstance(),
                "Reconciliation currency code"));
            FieldFormatters.Add(new StringFieldFormatter(
                51, new FixedLengthManager(3), DataEncoder.GetInstance(),
                "Cardholder billing currency code"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                52, new FixedLengthManager(8), DataEncoder.GetInstance(),
                "Personal identification number (PIN) data"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                53, new VariableLengthManager(0, 48, BcdLengthEncoder.GetInstance(48)),
                DataEncoder.GetInstance(), "Security related control information"));
            FieldFormatters.Add(new StringFieldFormatter(
                54, new VariableLengthManager(0, 120, BcdLengthEncoder.GetInstance(120)),
                DataEncoder.GetInstance(), "Amounts, additional"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                55, new VariableLengthManager(0, 255, BcdLengthEncoder.GetInstance(255)),
                DataEncoder.GetInstance(), "IC card system related data"));
            FieldFormatters.Add(new StringFieldFormatter(
                56, new VariableLengthManager(0, 35, BcdLengthEncoder.GetInstance(35)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Original data elements"));
            FieldFormatters.Add(new StringFieldFormatter(
                57, new FixedLengthManager(3), BcdDataEncoder.GetInstance(false, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Authorization life cycle code"));
            FieldFormatters.Add(new StringFieldFormatter(
                58, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Authorizing agent institution Id Code"));
            FieldFormatters.Add(new StringFieldFormatter(
                59, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Transport data"));
            FieldFormatters.Add(new StringFieldFormatter(
                60, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                61, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                62, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new StringFieldFormatter(
                63, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                64, new FixedLengthManager(8), DataEncoder.GetInstance(),
                "Message authentication code field"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                65, new FixedLengthManager(8), DataEncoder.GetInstance(),
                "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                66, new VariableLengthManager(0, 204, BcdLengthEncoder.GetInstance(204)),
                DataEncoder.GetInstance(), "Amounts, original fees"));
            FieldFormatters.Add(new StringFieldFormatter(
                67, new FixedLengthManager(2), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Extended payment data"));
            FieldFormatters.Add(new StringFieldFormatter(
                68, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Receiving institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                69, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Settlement institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                70, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Authorizing agent institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                71, new FixedLengthManager(8), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Message number"));
            FieldFormatters.Add(new StringFieldFormatter(
                72, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Data record"));
            FieldFormatters.Add(new StringFieldFormatter(
                73, new FixedLengthManager(6), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, false), "Action date"));
            FieldFormatters.Add(new StringFieldFormatter(
                74, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                75, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                76, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                77, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                78, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Transfer, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                79, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Transfer, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                80, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Inquiries, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                81, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Authorizations, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                82, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Inquiries, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                83, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Payments, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                84, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Payments, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                85, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Fee collections, number"));
            FieldFormatters.Add(new StringFieldFormatter(
                86, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                87, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, reversal amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                88, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                89, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, reversal amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                90, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Authorizations, reversal number"));
            FieldFormatters.Add(new StringFieldFormatter(
                91, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Transaction destination institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                92, new FixedLengthManager(3), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Transaction originator institution country code"));
            FieldFormatters.Add(new StringFieldFormatter(
                93, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Transaction destination institution Id Code"));
            FieldFormatters.Add(new StringFieldFormatter(
                94, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Transaction originator institution Id Code"));
            FieldFormatters.Add(new StringFieldFormatter(
                95, new VariableLengthManager(0, 99, BcdLengthEncoder.GetInstance(99)),
                DataEncoder.GetInstance(), "Card issuer reference data"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                96, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(48)),
                DataEncoder.GetInstance(), "Key management data"));
            FieldFormatters.Add(new StringFieldFormatter(
                97, new FixedLengthManager(17), BcdDataEncoder.GetInstance(true, 0),
                "Net reconciliation amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                98, new FixedLengthManager(25), DataEncoder.GetInstance(),
                "Payee"));
            FieldFormatters.Add(new StringFieldFormatter(
                99, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Settlement institution Id code"));
            FieldFormatters.Add(new StringFieldFormatter(
                100, new VariableLengthManager(0, 11, BcdLengthEncoder.GetInstance(11)),
                BcdDataEncoder.GetInstance(false, 0), NumericValidator.GetInstance(),
                "Receiving institution Id code"));
            FieldFormatters.Add(new StringFieldFormatter(
                101, new VariableLengthManager(0, 17, BcdLengthEncoder.GetInstance(17)),
                DataEncoder.GetInstance(), "File name"));
            FieldFormatters.Add(new StringFieldFormatter(
                102, new VariableLengthManager(0, 28, BcdLengthEncoder.GetInstance(28)),
                DataEncoder.GetInstance(), "Account identification 1"));
            FieldFormatters.Add(new StringFieldFormatter(
                103, new VariableLengthManager(0, 28, BcdLengthEncoder.GetInstance(28)),
                DataEncoder.GetInstance(), "Account identification 2"));
            FieldFormatters.Add(new StringFieldFormatter(
                104, new VariableLengthManager(0, 100, BcdLengthEncoder.GetInstance(100)),
                DataEncoder.GetInstance(), "Transaction description"));
            FieldFormatters.Add(new StringFieldFormatter(
                105, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, chargeback amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                106, new FixedLengthManager(16), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, chargeback amount"));
            FieldFormatters.Add(new StringFieldFormatter(
                107, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Credits, chargeback number"));
            FieldFormatters.Add(new StringFieldFormatter(
                108, new FixedLengthManager(10), BcdDataEncoder.GetInstance(true, 0),
                ZeroPaddingLeft.GetInstance(false, true), "Debits, chargeback number"));
            FieldFormatters.Add(new StringFieldFormatter(
                109, new VariableLengthManager(0, 84, BcdLengthEncoder.GetInstance(84)),
                DataEncoder.GetInstance(), "Credits, fee amounts"));
            FieldFormatters.Add(new StringFieldFormatter(
                110, new VariableLengthManager(0, 84, BcdLengthEncoder.GetInstance(84)),
                DataEncoder.GetInstance(), "Debits, fee amounts"));
            FieldFormatters.Add(new StringFieldFormatter(
                111, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                112, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                113, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                114, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                115, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for ISO use"));
            FieldFormatters.Add(new StringFieldFormatter(
                116, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                117, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                118, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                119, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                120, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                121, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                122, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for national use"));
            FieldFormatters.Add(new StringFieldFormatter(
                123, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new StringFieldFormatter(
                124, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new StringFieldFormatter(
                125, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new StringFieldFormatter(
                126, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new StringFieldFormatter(
                127, new VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)),
                DataEncoder.GetInstance(), "Reserved for private use"));
            FieldFormatters.Add(new BinaryFieldFormatter(
                128, new FixedLengthManager(8), DataEncoder.GetInstance(),
                "Message authentication code"));
        }

        /// <summary>
        /// It clones the formatter instance.
        /// </summary>
        /// <remarks>
        /// The header, the mti formatter and the fields formatters, aren't cloned,
        /// the new instance and the original shares those object instances.
        /// </remarks>
        /// <returns>
        /// A new instance of the formatter.
        /// </returns>
        public override object Clone()
        {
            var formatter = new Iso8583Bin1993MessageFormatter();

            CopyTo(formatter);

            return formatter;
        }
    }
}