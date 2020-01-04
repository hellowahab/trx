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
    public class DummyConsumer : TrxServiceBase
    {
        private bool _continue;

        private void Consumer(object state)
        {
            while (_continue)
            {
                var obj = TrxServer.TupleSpaces[0].Take(null, 100, "ts");
                if (obj != null)
                    Logger.Info(string.Format("Received from tuple space: {0}", obj.Message));
            }
        }

        protected override void ProtectedStart()
        {
            _continue = true;
            new Thread(Consumer).Start();
        }

        protected override void ProtectedStop()
        {
            _continue = false;
        }
    }
}
