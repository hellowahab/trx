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
using System.Threading;
using Trx.Communication.Channels;
using Trx.Communication.Channels.Sinks;
using Trx.Communication.Channels.Sinks.Framing;
using Trx.Communication.Channels.Tcp;
using Trx.Coordination.TupleSpace;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Performance2
{
    internal class Performance2
    {
        private const int MessagesToInterchange = 20000;

        private static int _rcvCnt;

        private static void Sender()
        {
            var pipeline = new Pipeline();
            pipeline.Push(new NboFrameLengthSink(2) { IncludeHeaderLength = false, MaxFrameLength = 1024 });
            pipeline.Push(new MessageFormatterSink(new Iso8583MessageFormatter((@"..\..\..\Formatters\Iso8583Bin1993.xml"))));
            var ts = new TupleSpace<ReceiveDescriptor>();

            var client = new TcpClientChannel(pipeline, ts, new FieldsMessagesIdentifier(new[] { 11, 41 }))
            {
                RemotePort = 9999,
                RemoteInterface = "localhost",
                Name = "Sender"
            };

            var ctrl = client.Connect();
            ctrl.WaitCompletion();

            if (!ctrl.Successful)
            {
                Console.WriteLine("Sender: can't connect to receiver... aborting test");
                if (ctrl.Error != null)
                    Console.WriteLine(ctrl.Error);
                return;
            }

            DateTime startTime = DateTime.Now;

            var stan = new VolatileStanSequencer();

            // Start sending messages.
            for (int i = 0; i < MessagesToInterchange; i++)
            {
                var message = new Iso8583Message(200);

                message.Fields.Add(2, "40000000000002");
                message.Fields.Add(3, "000000");
                int trace = stan.Increment();
                message.Fields.Add(11, trace.ToString());
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

                var sndCtrl = client.SendExpectingResponse(message, 15000, false, null);
                sndCtrl.WaitCompletion(); // Wait send completion.
                sndCtrl.Request.WaitResponse();
                if (!sndCtrl.Successful)
                {
                    Console.WriteLine(string.Format("Sender: unsuccessful request # {0} ({1}.", trace, sndCtrl.Message));
                    if (sndCtrl.Error != null)
                        Console.WriteLine(sndCtrl.Error);
                }
            }

            TimeSpan elapsed = DateTime.Now - startTime;

            Console.WriteLine(string.Format("Sender: elapsed seconds: {0}", elapsed.TotalSeconds));
            Console.WriteLine(string.Format("Sender: sends per second: {0}",
                MessagesToInterchange*1000/elapsed.TotalMilliseconds));

            client.Close();
        }

        private static void Receiver()
        {
            var pipeline = new Pipeline();
            pipeline.Push(new NboFrameLengthSink(2) { IncludeHeaderLength = false, MaxFrameLength = 1024 });
            pipeline.Push(new MessageFormatterSink(new Iso8583MessageFormatter((@"..\..\..\Formatters\Iso8583Bin1993.xml"))));
            var ts = new TupleSpace<ReceiveDescriptor>();

            var server = new TcpServerChannel(new Pipeline(), new ClonePipelineFactory(pipeline), ts, new FieldsMessagesIdentifier(new[] { 11, 41 }))
            {
                Port = 9999,
                LocalInterface = "localhost",
                Name = "Receiver"
            };

            server.StartListening();

            DateTime startTime = DateTime.Now;

            while (_rcvCnt < MessagesToInterchange)
            {
                var rcvDesc = ts.Take(null, 15000);
                if (rcvDesc == null)
                {
                    Console.WriteLine("Receiver: error, timeout reading messages after 15 seconds");
                    break;
                }
                _rcvCnt++;
                var message = rcvDesc.ReceivedMessage as Iso8583Message;
                if (message == null)
                    continue;
                message.SetResponseMessageTypeIdentifier();
                var addr = rcvDesc.ChannelAddress as ReferenceChannelAddress;
                if (addr == null)
                    continue;
                var child = addr.Channel as ISenderChannel;
                if (child != null)
                    child.Send(message);
            }

            TimeSpan elapsed = DateTime.Now - startTime;

            Console.WriteLine(string.Format("Receiver: elapsed seconds: {0}", elapsed.TotalSeconds));
            Console.WriteLine(string.Format("Receiver: receives per second: {0}", _rcvCnt*1000/elapsed.TotalMilliseconds));

            // Stop listening and shutdown the connection with the sender.
            server.StopListening();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var sender = new Thread(Sender);
            sender.Start();
            var receiver = new Thread(Receiver);
            receiver.Start();

            if (!sender.Join(60000))
            {
                Console.WriteLine(
                    string.Format("A problem was detected with Sender thread... aborting test. _rcvCnt = {0}.", _rcvCnt));
                sender.Abort();
                receiver.Abort();
            }
            else if (!receiver.Join(60000))
            {
                Console.WriteLine(
                    string.Format("A problem was detected with Receiver thread... aborting test. _rcvCnt = {0}.",
                        _rcvCnt));
                receiver.Abort();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}