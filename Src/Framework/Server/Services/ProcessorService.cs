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
using Trx.Exceptions;

namespace Trx.Server.Services
{
    /// <summary>
    /// Encapsulates an <see cref="IProcessor"/> forwarding messages read from <see ref="TrxServerTupleSpace"/> 
    /// are sent to it.
    /// </summary>
    public class ProcessorService : TrxServiceBase
    {
        private readonly IProcessor _processor;

        protected bool KeepRunning;
        private Thread _workerThread;

        public ProcessorService(string trxServerTupleSpaceName, IProcessor processor)
        {
            if (string.IsNullOrEmpty(trxServerTupleSpaceName))
                throw new ArgumentNullException("trxServerTupleSpaceName");

            if (processor == null)
                throw new ArgumentNullException("processor");

            TrxServerTupleSpaceName = trxServerTupleSpaceName;
            _processor = processor;
        }

        public IProcessor Processor
        {
            get { return _processor; }
        }

        protected override void ProtectedInit()
        {
            base.ProtectedInit();

            _processor.TrxService = this;

            if (TrxServerTupleSpace == null)
                throw new ConfigurationException(string.Format("Trx Server tuple space {0} not found.", TrxServerTupleSpaceName));
        }

        protected override void ProtectedStart()
        {
            base.ProtectedStart();

            _workerThread = new Thread(StartReadingTupleSpace);
            _workerThread.Start();
        }

        protected override void ProtectedStop()
        {
            base.ProtectedStop();

            if (_workerThread != null)
            {
                KeepRunning = false;
                _workerThread.Interrupt();
                _workerThread = null;
            }
        }

        private void StartReadingTupleSpace()
        {
            KeepRunning = true;
            while (KeepRunning)
            {
                try
                {
                    int timeInTupleSpace, ttl;
                    var message = TrxServerTupleSpace.Take(null, Timeout.Infinite,
                        out timeInTupleSpace, out ttl, InputContext);
                    _processor.Process(TrxServerTupleSpace, message, timeInTupleSpace, ttl);
                }
                catch (ConfigurationException ex)
                {
                    Logger.Error(string.Format("{0}: configuration exeption reading on tuplespace {1}, aborting.",
                        Name, TrxServerTupleSpace.Name), ex);
                    KeepRunning = false;
                }
                catch (ThreadInterruptedException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
    }
}