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
using Trx.Messaging.ConditionalFormatting;
using Trx.Messaging.Iso8583;
using Trx.Utilities;

namespace ConditionalFormatting
{
    /// <summary>
    /// This example shows how to use conditional formatting capabilities of Trx Framework.
    /// Conditional formatting is a very useful feature when you need to format a field
    /// depending on the data contained in another.
    /// </summary>
    internal class ConditionalFormatting
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

            // Setup some fields.
            message.Fields.Add(11, sequencer.Increment().ToString());
            message.Fields.Add(12, "103400");
            message.Fields.Add(13, "1124");
            // NII Field is defined as 3 chars length and zero padded, we will get
            // 009 in the formatted result.
            message.Fields.Add(24, "9");
            message.Fields.Add(41, "TEST1");

            // Create an inner message compatible with CustomMessageFormatter
            var innerFixedSizeMsg = new Message();
            innerFixedSizeMsg.Fields.Add(3, "A");
            innerFixedSizeMsg.Fields.Add(6, "123");
            innerFixedSizeMsg.Fields.Add(8, "The key");

            // Create an inner message compatible with CustomVariableLengthMessageFormatter
            var innerVarSizeMsg = new Message();
            innerVarSizeMsg.Fields.Add(1, "4");
            innerVarSizeMsg.Fields.Add(2, "101");
            innerVarSizeMsg.Fields.Add(4, "John Doe");
            innerVarSizeMsg.Fields.Add(6, "67");
            innerVarSizeMsg.Fields.Add(7, "34");

            // Create an inner message compatible with Iso8583Ascii1987MessageFormatter
            var innerIsoMsg = new Iso8583Message(600);
            innerIsoMsg.Fields.Add(13, "0606");
            innerIsoMsg.Fields.Add(42, "SOME DATA");

            // Try first with CustomVariableLengthMessageFormatter
            Console.WriteLine("Try with CustomVariableLengthMessageFormatter...");
            message.Fields.Add(3, "000000");
            message.Fields.Add(63, innerVarSizeMsg);

            formatter.Format(message, ref formatterContext);
            string rawData = formatterContext.GetDataAsString();
            Console.WriteLine(string.Format("Formatted data: {0}", rawData));
            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);
            parserContext.Write(rawData);
            Message parsedMessage = formatter.Parse(ref parserContext);
            Console.WriteLine(string.Format("Parsed message: {0}", parsedMessage));

            Console.WriteLine();

            // Now try with CustomMessageFormatter
            Console.WriteLine("Now try with CustomMessageFormatter...");
            formatterContext.Clear();
            message.Fields.Add(3, "300000");
            message.Fields.Add(63, innerFixedSizeMsg);

            formatter.Format(message, ref formatterContext);
            rawData = formatterContext.GetDataAsString();
            Console.WriteLine(string.Format("Formatted data: {0}", rawData));
            parserContext.Write(rawData);
            parsedMessage = formatter.Parse(ref parserContext);
            Console.WriteLine(string.Format("Parsed message: {0}", parsedMessage));

            Console.WriteLine();

            // And finally with Iso8583Ascii1987MessageFormatter
            Console.WriteLine("And finally with Iso8583Ascii1987MessageFormatter...");
            formatterContext.Clear();
            message.Fields.Add(3, "999999");
            message.Fields.Add(63, innerIsoMsg);

            formatter.Format(message, ref formatterContext);
            rawData = formatterContext.GetDataAsString();
            Console.WriteLine(string.Format("Formatted data: {0}", rawData));
            parserContext.Write(rawData);
            parsedMessage = formatter.Parse(ref parserContext);
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

                // Here we setup the formatter to format/parse with
                // CustomVariableLengthMessageFormatter when the field 3
                // contains 000000 or starts with 02, when it starts with
                // 30 we use CustomMessageFormatter, for other contents we
                // will use Iso8583Ascii1987MessageFormatter.

                FieldFormatters.Add(new ConditionalFieldFormatter(63, "3 = '000000' or 3[0,2] = '02'",
                    new InnerMessageFieldFormatter(
                        63, new VariableLengthManager(0, 99, StringLengthEncoder.GetInstance(99)),
                        DataEncoder.GetInstance(), new CustomVariableLengthMessageFormatter(),
                        "Inner non ISO variable length size message"),
                    new ConditionalFieldFormatter(63, "3[0,2] = '30'",
                        new InnerMessageFieldFormatter(
                            63, new FixedLengthManager(20),
                            DataEncoder.GetInstance(), new CustomMessageFormatter(),
                            "Inner non ISO fixed size message"),
                        new InnerMessageFieldFormatter(
                            63, new VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)),
                            DataEncoder.GetInstance(), new Iso8583MessageFormatter(@"..\..\..\Formatters\Iso8583Ascii1987.xml"),
                            "Inner ISO 8583 message"))));
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