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

namespace Performance
{
    internal class Performance
    {
        private const int MessagesToProcess = 100000;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var formatterContext =
                new FormatterContext(FormatterContext.DefaultBufferSize);

            var formatter = new Iso8583MessageFormatter(@"..\..\..\Formatters\Iso8583Bin1987.xml");

            var message = new Iso8583Message(800);

            message.Fields.Add(2, "40000000000002");
            message.Fields.Add(3, "000000");
            message.Fields.Add(11, "000001");
            message.Fields.Add(12, "0000");
            message.Fields.Add(13, "0101");
            message.Fields.Add(14, "0505");
            message.Fields.Add(15, "0202");
            message.Fields.Add(28, "C1200");
            message.Fields.Add(34, "12345");
            message.Fields.Add(41, "12345678");
            message.Fields.Add(42, "123456789012345");
            message.Fields.Add(60, "TEST DATA");
            message.Fields.Add(61, "LONGER TEST DATA");
            message.Fields.Add(62, "The quick brown fox jumped over the lazy dog");
            message.Fields.Add(70, "301");

            DateTime startTime = DateTime.Now;

            //var parserContext = new ParserContext(ParserContext.DefaultBufferSize);
            for (int i = 0; i < MessagesToProcess; i++)
            {
                formatter.Format(message, ref formatterContext);
                //parserContext.Write(formatterContext.GetBuffer(), 0, formatterContext.UpperDataBound);
                formatterContext.Clear();
                //var parsedMessage = formatter.Parse(ref parserContext) as Iso8583Message;
            }

            TimeSpan elapsed = DateTime.Now - startTime;

            Console.WriteLine(string.Format("Elapsed seconds: {0}", elapsed.TotalSeconds));
            Console.WriteLine(string.Format("Format/Parse per second: {0}",
                MessagesToProcess*1000/elapsed.TotalMilliseconds));
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}