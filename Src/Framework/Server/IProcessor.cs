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

namespace Trx.Server
{
    /// <summary>
    /// Defines the interface of a Trx Server component wich process Trx Server messages. Can be encapsulated
    /// in a <see cref="Trx.Server.Services.ProcessorService"/>.
    /// </summary>
    public interface IProcessor
    {
        ITrxService TrxService { get; set; }

        /// <summary>
        /// Process the message.
        /// </summary>
        /// <param name="tupleSpace">
        /// The Trx Server tuple space where the message was read.
        /// </param>
        /// <param name="message">
        /// The message to process.
        /// </param>
        /// <param name="timeInTupleSpace">
        /// This parameter returns the time in milliseconds the object was stored in the tuple space waiting to be processed.
        /// </param>
        /// <param name="ttl">
        /// Write timeout in the tuple space.
        /// </param>
        void Process(ITrxServerTupleSpace tupleSpace, TrxServiceMessage message, int timeInTupleSpace, int ttl);
    }
}