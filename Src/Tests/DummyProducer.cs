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

using System.Threading;
using Trx.Server;

namespace Tests.Trx
{
    public class DummyProducer : TrxServiceBase
    {
        private bool _continue;
        private Thread _t;

        public string TupleSpaceName { get; set; }

        private void Producer(object state)
        {
            while (_continue)
            {
                var tupleSpace = TrxServer.TupleSpaces[0];
                if (tupleSpace != null)
                    tupleSpace.Write(new TrxServiceMessage("ts", "Hello from producer Trx Server instance"), Timeout.Infinite, "ts");
                try
                {
                    Thread.Sleep(10000);
                } catch (ThreadInterruptedException)
                {
                }
            }
        }

        protected override void ProtectedStart()
        {
            _continue = true;
            _t = new Thread(Producer);
            _t.Start();
        }

        protected override void ProtectedStop()
        {
            _continue = false;
            _t.Interrupt();
        }
    }
}
