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
using Trx.Logging;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Acquirer
{
    /// <summary>
    /// This class accept a connection in port 8583 and responds to
    /// any received echo test messages.
    /// </summary>
    public class Acquirer
    {
        private const int Field39ResponseCode = 39;

        private int _requestsCnt;
        private bool _stop;

        /// <summary>
        /// Returns the number of requests made.
        /// </summary>
        public int RequestsCount
        {
            get { return _requestsCnt; }
        }

        public void Stop()
        {
            _stop = true;
        }

        private void Receiver(object state)
        {
            var pipeline = new Pipeline();
            pipeline.Push(new NboFrameLengthSink(2) {IncludeHeaderLength = false, MaxFrameLength = 1024});
            pipeline.Push(
                new MessageFormatterSink(new Iso8583MessageFormatter((@"..\..\..\Formatters\Iso8583Bin1987.xml"))));
            var ts = new TupleSpace<ReceiveDescriptor>();

            var server = new TcpServerChannel(new Pipeline(), new ClonePipelineFactory(pipeline), ts,
                new FieldsMessagesIdentifier(new[] {11, 41}))
                             {
                                 Port = 8583,
                                 LocalInterface = (string) state,
                                 Name = "Acquirer"
                             };

            server.StartListening();

            while (!_stop)
            {
                ReceiveDescriptor rcvDesc = ts.Take(null, 100);
                if (rcvDesc == null)
                    continue;
                _requestsCnt++;
                var message = rcvDesc.ReceivedMessage as Iso8583Message;
                if (message == null)
                    continue;
                message.SetResponseMessageTypeIdentifier();
                message.Fields.Add(Field39ResponseCode, "00");
                var addr = rcvDesc.ChannelAddress as ReferenceChannelAddress;
                if (addr == null)
                    continue;
                var child = addr.Channel as ISenderChannel;
                if (child != null)
                    child.Send(message);
            }

            // Stop listening and shutdown the connection with the sender.
            server.StopListening();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            LogManager.LoggerFactory = new Log4NetLoggerFactory();
            LogManager.Renderer = new Renderer();
            ILogger logger = LogManager.GetLogger("root");

            string localInterface = "localhost";
            if (args.Length > 0)
                localInterface = args[0];
            var a = new Acquirer();

            var receiver = new Thread(a.Receiver);
            receiver.Start(localInterface);

            logger.Info("Acquirer is running, press any key to stop it...");
            Console.ReadLine();
            a.Stop();
            logger.Info(string.Format("Processed requests: {0}", a.RequestsCount));
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}