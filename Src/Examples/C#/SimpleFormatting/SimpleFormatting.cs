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

namespace SimpleFormatting
{
    internal class SimpleFormatting
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var formatterContext =
                new FormatterContext(FormatterContext.DefaultBufferSize);

            var formatter = new Iso8583MessageFormatter(@"..\..\..\Formatters\Iso8583Ascii1993.xml");

            var sequencer = new VolatileStanSequencer();

            var message = new Iso8583Message(1600);

            message.Fields.Add(11, sequencer.Increment().ToString());
            message.Fields.Add(101, "Trx.txt");

            formatter.Format(message, ref formatterContext);

            string formattedData = formatterContext.GetDataAsString();

            Console.WriteLine(string.Format("Formatted data: {0}", formattedData));

            var parserContext = new ParserContext(ParserContext.DefaultBufferSize);

            parserContext.Write(formattedData);

            Message parsedMessage = formatter.Parse(ref parserContext);

            if (parsedMessage is Iso8583Message)
                Console.WriteLine("We have an ISO 8583 message again!");

            if (parsedMessage.Fields.Contains(11))
                Console.WriteLine("Field 11: {0}", parsedMessage.Fields[11]);

            if (parsedMessage.Fields.Contains(11))
                Console.WriteLine("Field 101: {0}", parsedMessage.Fields[101]);

            Console.WriteLine("Press any key to exit...");

            Console.ReadLine();
        }
    }
}