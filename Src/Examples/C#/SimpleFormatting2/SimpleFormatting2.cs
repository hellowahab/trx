#region Copyright (C) 2004-2006 Diego Zabaleta, Leonardo Zabaleta
//
// Copyright © 2004-2006 Diego Zabaleta, Leonardo Zabaleta
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#endregion

using System;
using System.Text;
using Trx.Messaging;
using Trx.Messaging.Iso8583;
using Trx.Utilities;

namespace SimpleFormatting2
{
    internal class SimpleFormatting2
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Try another Encoding mechanism.
            FrameworkEncoding.GetInstance().Encoding = Encoding.Default;

            // A formatter context handles the state of a formatting operation.
            var formatterContext =
                new FormatterContext(FormatterContext.DefaultBufferSize);

            var formatter = new Iso8583MessageFormatter(@"..\..\..\Formatters\Iso8583Ascii1993.xml");

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
    }
}