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
using Trx.Messaging.Iso8583;
using Trx.Utilities;

namespace InnerMessageFormatting
{
    /// <summary>
    /// This example shows how to include messages in a field of another one.
    /// This is very useful when we need to handle structured fields, and we
    /// wish to delegate their parsing and validation to Trx Framework.
    /// </summary>
    internal class InnerMessageFormatting
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // A formatter context handles the state of a formatting operation.
            var formatterContext =
                new FormatterContext(FormatterContext.DefaultBufferSize);

            var formatter =
                new CustomizedIso8583Ascii1987MessageFormatter();

            var sequencer = new VolatileStanSequencer();

            var message = new Iso8583Message(500);

            message.Fields.Add(3, "999999");
            message.Fields.Add(11, sequencer.Increment().ToString());
            message.Fields.Add(12, "103400");
            message.Fields.Add(13, "1124");
            // NII Field is defined as 3 chars length and zero padded, we will get
            // 009 in the formatted result.
            message.Fields.Add(24, "9");
            message.Fields.Add(41, "TEST1");

            var innerFixedSizeMsg = new Message();
            innerFixedSizeMsg.Fields.Add(3, "A");
            innerFixedSizeMsg.Fields.Add(6, "123");
            innerFixedSizeMsg.Fields.Add(8, "The key");
            message.Fields.Add(61, innerFixedSizeMsg);

            var innerVarSizeMsg = new Message();
            innerVarSizeMsg.Fields.Add(1, "4");
            innerVarSizeMsg.Fields.Add(2, "101");
            innerVarSizeMsg.Fields.Add(4, "John Doe");
            innerVarSizeMsg.Fields.Add(6, "67");
            innerVarSizeMsg.Fields.Add(7, "34");
            message.Fields.Add(62, innerVarSizeMsg);

            var innerIsoMsg = new Iso8583Message(600);
            innerIsoMsg.Fields.Add(13, "0606");
            innerIsoMsg.Fields.Add(42, "SOME DATA");
            message.Fields.Add(63, innerIsoMsg);

            formatter.Format(message, ref formatterContext);

            // The formatter knows how to format a message, the bitmaps are added and
            // computed automatically in the Format method. After Format the bitmaps
            // are available in the message and we can access them.
            if (message.Fields.Contains(0))
                Console.WriteLine(string.Format("First bitmap data for our message: {0}",
                    message.Fields[0]));
            if (message.Fields.Contains(1)) // To get the second bitmap we must add a field with number 65 or above.
                Console.WriteLine(string.Format("Second bitmap data for our message: {0}",
                    message.Fields[1]));

            // The formatted data is in the formatter context buffer, get it.
            // This data includes MTI and bitmap data.
            string rawData = formatterContext.GetDataAsString();

            // Write the formatted raw data for our message, both MTI and bitmap are
            // automatically included by the formatter. The first 4 bytes are the MTI,
            // the next 16 are the first bitmap in hexadecimal (because we have
            // used Iso8583Ascii1987MessageFormatter, if you need to use binary encoding
            // you must use Iso8583Bin1987MessageFormatter).
            Console.WriteLine(string.Format("Formatted data: {0}", rawData));

            // Now try to parse the raw data.

            // A parser context handles the state of an unformat operation, we need to
            // put the raw ISO 8583 data in its buffer, this is the way the formatter
            // works to Parse a message.
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);
            parserContext.Write(rawData);

            // Ask the formatter to Parse the data in the parser context.
            Message parsedMessage = formatter.Parse(ref parserContext);

            if (parsedMessage is Iso8583Message)
                Console.WriteLine("We have an ISO 8583 message again!");

            // All the fields in the persedMessage are available to us, including MTI and
            // bitmaps.
            Console.WriteLine(string.Format("Parsed message: {0}", parsedMessage));

            Console.WriteLine("Press any key to exit...");

            Console.ReadLine();
        }

        #region Nested type: CustomMessageFormatter
        /// <summary>
        /// A non ISO fixed size message formatter.
        /// </summary>
        public class CustomMessageFormatter : BasicMessageFormatter
        {
            #region Constructors
            /// <summary>
            /// It initializes a new instance of our customized non ISO fixed size message formatter.
            /// </summary>
            public CustomMessageFormatter()
            {
                SetupFields();
            }
            #endregion

            #region Methods
            /// <summary>
            /// It initializes the fields formatters list.
            /// </summary>
            private void SetupFields()
            {
                FieldFormatters.Add(new StringFieldFormatter(3, new FixedLengthManager(1),
                    DataEncoder.GetInstance(), "Operation type"));
                FieldFormatters.Add(new StringFieldFormatter(6, new FixedLengthManager(9),
                    DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(false, true), "Id"));
                FieldFormatters.Add(new StringFieldFormatter(8, new FixedLengthManager(10),
                    DataEncoder.GetInstance(), "Key"));
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
                var formatter = new CustomMessageFormatter();

                CopyTo(formatter);

                return formatter;
            }
            #endregion
        }
        #endregion

        #region Nested type: CustomVariableLengthMessageFormatter
        /// <summary>
        /// A non ISO variable length size message.
        /// </summary>
        public class CustomVariableLengthMessageFormatter : BasicMessageFormatter
        {
            #region Constructors
            /// <summary>
            /// It initializes a new instance of our customized non ISO variable length message formatter.
            /// </summary>
            public CustomVariableLengthMessageFormatter()
            {
                SetupFields();
            }
            #endregion

            #region Methods
            /// <summary>
            /// It initializes the fields formatters list.
            /// </summary>
            private void SetupFields()
            {
                FieldFormatters.Add(new StringFieldFormatter(1, new FixedLengthManager(3),
                    DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(false, true), "Branch Id."));
                FieldFormatters.Add(new StringFieldFormatter(2, new FixedLengthManager(3),
                    DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(false, true), "Pos Id."));

                // From this point, then message is variable length.

                FieldFormatters.Add(new BitMapFieldFormatter(3, 4, 7,
                    HexDataEncoder.GetInstance(), "Bitmap"));

                FieldFormatters.Add(new StringFieldFormatter(4, new FixedLengthManager(30),
                    DataEncoder.GetInstance(), "Operator name"));
                FieldFormatters.Add(new StringFieldFormatter(5, new FixedLengthManager(10),
                    DataEncoder.GetInstance(), "Operator Id."));
                FieldFormatters.Add(new StringFieldFormatter(6, new FixedLengthManager(6),
                    DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(false, true),
                    "Invoice #"));
                FieldFormatters.Add(new StringFieldFormatter(7, new FixedLengthManager(6),
                    DataEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(false, true),
                    "Audit #"));
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
                var formatter = new CustomVariableLengthMessageFormatter();

                CopyTo(formatter);

                return formatter;
            }
            #endregion
        }
        #endregion

        #region Nested type: CustomizedIso8583Ascii1987MessageFormatter
        /// <summary>
        /// Customizes the standard ISO 8583-87 message formatter.
        /// </summary>
        public class CustomizedIso8583Ascii1987MessageFormatter : Iso8583MessageFormatter
        {
            #region Constructors
            /// <summary>
            /// It initializes a new instance of our customized ISO 8583 message formatter.
            /// </summary>
            public CustomizedIso8583Ascii1987MessageFormatter()
                : base(@"..\..\..\Formatters\Iso8583Ascii1987.xml")
            {
                SetupFields();
            }
            #endregion

            #region Methods
            /// <summary>
            /// It initializes the fields formatters list.
            /// </summary>
            private void SetupFields()
            {
                // The other fields formatting definition is inherited
                // from Iso8583Ascii1987MessageFormatter

                FieldFormatters.Add(new InnerMessageFieldFormatter(
                    61, new FixedLengthManager(20),
                    DataEncoder.GetInstance(), new CustomMessageFormatter(),
                    "Inner non ISO fixed size message"));

                FieldFormatters.Add(new InnerMessageFieldFormatter(
                    62, new VariableLengthManager(0, 99, StringLengthEncoder.GetInstance(99)),
                    DataEncoder.GetInstance(), new CustomVariableLengthMessageFormatter(),
                    "Inner non ISO variable length size message"));

                FieldFormatters.Add(new InnerMessageFieldFormatter(
                    63, new VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)),
                    DataEncoder.GetInstance(), new Iso8583MessageFormatter(@"..\..\..\Formatters\Iso8583Ascii1987.xml"),
                    "Inner ISO 8583 message"));
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
                var formatter =
                    new CustomizedIso8583Ascii1987MessageFormatter();

                CopyTo(formatter);

                return formatter;
            }
            #endregion
        }
        #endregion
    }
}