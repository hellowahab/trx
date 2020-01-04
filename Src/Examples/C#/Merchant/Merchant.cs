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

namespace Merchant
{
    /// <summary>
    /// This class tries to connect to the port 8583 and when the connection
    /// is established, it periodically send echo test messages.
    /// </summary>
    /// <remarks>
    /// This example must be used in conjunction with the Acquirer example.
    /// </remarks>
    public class Merchant
    {
        private const int Field3ProcCode = 3;
        private const int Field7TransDateTime = 7;
        private const int Field11Trace = 11;
        private const int Field24Nii = 24;
        private const int Field41TerminalCode = 41;
        private const int Field42MerchantCode = 42;

        private readonly TcpClientChannel _client;
        private readonly VolatileStanSequencer _sequencer;

        private readonly string _terminalCode;

        private int _expiredRequests;
        private int _requestsCnt;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="hostname">
        /// The host name of the computer where the Acquirer.exe program is running.
        /// </param>
        public Merchant(string hostname, string terminalCode)
        {
            var pipeline = new Pipeline();
            pipeline.Push(new ReconnectionSink());
            pipeline.Push(new NboFrameLengthSink(2) {IncludeHeaderLength = false, MaxFrameLength = 1024});
            pipeline.Push(
                new MessageFormatterSink(new Iso8583MessageFormatter((@"..\..\..\Formatters\Iso8583Bin1987.xml"))));
            var ts = new TupleSpace<ReceiveDescriptor>();

            // Create a client peer to connect to remote system. The messages
            // will be matched using fields 41 and 11.
            _client = new TcpClientChannel(pipeline, ts, new FieldsMessagesIdentifier(new[] {11, 41}))
                          {
                              RemotePort = 8583,
                              RemoteInterface = hostname,
                              Name = "Merchant"
                          };

            _terminalCode = terminalCode;

            _sequencer = new VolatileStanSequencer();
        }

        /// <summary>
        /// Returns the number of requests made.
        /// </summary>
        public int RequestsCount
        {
            get { return _requestsCnt; }
        }

        /// <summary>
        /// Returns the number of expired requests (not responded by the remote peer).
        /// </summary>
        public int ExpiredRequests
        {
            get { return _expiredRequests; }
        }

        /// <summary>
        /// Called when the timer ticks.
        /// </summary>
        /// <param name="state">
        /// Null.
        /// </param>
        private void OnTimer(object state)
        {
            lock (this)
            {
                if (_client.IsConnected)
                {
                    // Build echo test message.
                    var echoMsg = new Iso8583Message(800);
                    echoMsg.Fields.Add(Field3ProcCode, "990000");
                    DateTime transmissionDate = DateTime.Now;
                    echoMsg.Fields.Add(Field7TransDateTime, string.Format("{0}{1}",
                        string.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day),
                        string.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour,
                            transmissionDate.Minute, transmissionDate.Second)));
                    echoMsg.Fields.Add(Field11Trace, _sequencer.Increment().ToString());
                    echoMsg.Fields.Add(Field24Nii, "101");
                    echoMsg.Fields.Add(Field41TerminalCode, _terminalCode);
                    echoMsg.Fields.Add(Field42MerchantCode, "MC-1");

                    SendRequestHandlerCtrl sndCtrl = _client.SendExpectingResponse(echoMsg, 1000, false, null);
                    sndCtrl.WaitCompletion(); // Wait send completion.
                    if (!sndCtrl.Successful)
                    {
                        Console.WriteLine(string.Format("Merchant: unsuccessful request # {0} ({1}.",
                            _sequencer.CurrentValue(), sndCtrl.Message));
                        if (sndCtrl.Error != null)
                            Console.WriteLine(sndCtrl.Error);
                    }
                    sndCtrl.Request.WaitResponse();

                    if (sndCtrl.Request.IsExpired)
                        _expiredRequests++;
                    else
                        _requestsCnt++;
                }
            }
        }

        public bool Start()
        {
            _timer = new Timer(OnTimer, null, 1000, 2000);

            ChannelRequestCtrl ctrl = _client.Connect();
            ctrl.WaitCompletion();

            if (!ctrl.Successful)
            {
                Console.WriteLine("Merchant: can't connect to acquirer...");
                if (ctrl.Error != null)
                    Console.WriteLine(ctrl.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stop merchant activity.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine(
                "Please enter a terminal code (use a different value for each concurrent Merchant2 instance).");
            string terminalCode = null;
            while (string.IsNullOrEmpty(terminalCode))
            {
                Console.Write("Code? ");
                terminalCode = Console.ReadLine();
            }

            LogManager.LoggerFactory = new Log4NetLoggerFactory();
            LogManager.Renderer = new Renderer();
            ILogger logger = LogManager.GetLogger("root");

            Merchant m = args.Length > 0 ? new Merchant(args[0], terminalCode) : new Merchant("localhost", terminalCode);

            m.Start();
            logger.Info("Merchant is running, press any key to stop it...");
            Console.ReadLine();
            m.Stop();
            logger.Info(string.Format("Successful requests: {0}", m.RequestsCount));
            logger.Info(string.Format("Expired requests   : {0}", m.ExpiredRequests));
            logger.Info("Press any key to exit...");
            Console.ReadLine();
        }
    }
}