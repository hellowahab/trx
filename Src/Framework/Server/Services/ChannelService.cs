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
using Trx.Communication.Channels;
using Trx.Coordination.TupleSpace;
using Trx.Exceptions;

namespace Trx.Server.Services
{
    public abstract class ChannelService : TrxServiceBase, ITupleSpace<ReceiveDescriptor>
    {
        public bool KeepSentMessageOnRequest { get; set; }

        private void Write(bool useReturn, ReceiveDescriptor entry, int ttl)
        {
            var request = entry as Request;
            if (request != null && !KeepSentMessageOnRequest)
                // To improve performance we don't return the sent message.
                request.SentMessage = null;

            var svcMessage = new TrxServiceMessage(InputContext, OutputContext, entry);
            if (useReturn)
                TrxServerTupleSpace.Return(svcMessage, ttl, InputContext);
            else
                TrxServerTupleSpace.Write(svcMessage, ttl, InputContext);
        }

        /// <summary>
        /// Write an entry into the tuple space.
        /// </summary>
        /// <param name="entry">
        /// The object to be written.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the tuple space.
        /// </param>
        public void Write(ReceiveDescriptor entry, int ttl)
        {
            Write(false, entry, ttl);
        }

        /// <summary>
        /// Return an entry into the tuple space.
        /// </summary>
        /// <param name="entry">
        /// The object to be returned.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the tuple space.
        /// </param>
        /// <remarks>
        /// Similar to <see ref="Write"/> but intended to be used when a user wants to return the object
        /// to the tuple space.
        /// </remarks>
        public void Return(ReceiveDescriptor entry, int ttl)
        {
            Write(true, entry, ttl);
        }

        /// <summary>
        /// Get an object from the tuple space removing it if available.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the tuple space.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is
        /// blocked until an object is available, if 0 was provided the call will not block the take if an
        /// object is not available.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public ReceiveDescriptor Take(object template, int timeout)
        {
            throw new InvalidOperationException("Tuple space not intended to be read.");
        }

        /// <summary>
        /// Get an object from the tuple space removing it if available.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the tuple space.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is
        /// blocked until an object is available, if 0 was provided the call will not block the take if an
        /// object is not available.
        /// </param>
        /// <param name="timeInTupleSpace">
        /// If a value is returned, this parameter returns the time in milliseconds the object was stored 
        /// in the tuple space.
        /// </param>
        /// <param name="ttl">
        /// Write timeout.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public ReceiveDescriptor Take(object template, int timeout, out int timeInTupleSpace, out int ttl)
        {
            throw new InvalidOperationException("Tuple space not intended to be read.");
        }

        /// <summary>
        /// Get an object from the tuple space without removing it.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the tuple space.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is 
        /// blocked until an object is available, if 0 was provided the call will not block the take if an 
        /// object is not available.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public ReceiveDescriptor Read(object template, int timeout)
        {
            throw new InvalidOperationException("Tuple space not intended to be read.");
        }

        protected override void ProtectedInit()
        {
            base.ProtectedInit();

            if (!string.IsNullOrEmpty(TrxServerTupleSpaceName) && TrxServerTupleSpace == null)
                throw new ConfigurationException(string.Format("Trx Server tuple space {0} not found.", TrxServerTupleSpaceName));
        }
    }
}
