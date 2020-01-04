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
using Trx.Coordination.TupleSpace;
using Trx.Exceptions;

namespace Trx.Server
{
    /// <summary>
    /// Wraps a reference to a tuple space hosted in another Trx Server instance (via TrxServerTupleSpaceProvider).
    /// </summary>
    public class TrxServerTupleSpaceProxy : ITrxServerTupleSpace
    {
        private readonly object _lockObj = new object();
        private ITrxServerTupleSpace _remoteSpace;

        public string RemoteTrxServerInstanceName { get; set; }

        private string _remoteTupleSpaceName;
        public string RemoteTupleSpaceName
        {
            get { return _remoteTupleSpaceName; }
            set
            {
                _remoteTupleSpaceName = value;
                if (string.IsNullOrEmpty(Name))
                    Name = value;
            }
        }

        internal TrxServerTupleSpaceProvider TupleSpaceProvider { get; set; }

        #region ITrxServerTupleSpace Members
        public string Name { get; set; }

        /// <summary>
        /// Write an entry into the given context tuple space.
        /// </summary>
        /// <param name="entry">
        /// The object to be written.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the tuple space.
        /// </param>
        /// <param name="context">
        /// The context name in the tuple space.
        /// </param>
        /// <returns>
        /// The context where the entry has been written.
        /// </returns>
        public IContext<TrxServiceMessage> Write(TrxServiceMessage entry, int ttl, string context)
        {
            try
            {
                GetTupleSpace().Write(entry, ttl, context);
            }
            catch (AppDomainUnloadedException)
            {
                // Try again
                GetTupleSpace().Write(entry, ttl, context);
            }
            return null;
        }

        /// <summary>
        /// Return an entry into the given context tuple space.
        /// </summary>
        /// <param name="entry">
        /// The object to be written.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the tuple space.
        /// </param>
        /// <param name="context">
        /// The context name in the tuple space.
        /// </param>
        /// <returns>
        /// The context where the entry has been written.
        /// </returns>
        public IContext<TrxServiceMessage> Return(TrxServiceMessage entry, int ttl, string context)
        {
            try
            {
                GetTupleSpace().Return(entry, ttl, context);
            }
            catch (AppDomainUnloadedException)
            {
                // Try again
                GetTupleSpace().Return(entry, ttl, context);
            }
            return null;
        }

        /// <summary>
        /// Get an object from the given context tuple space removing it if available.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the tuple space.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is
        /// blocked until an object is available, if 0 was provided the call will not block the take if an 
        /// object is not available.
        /// </param>
        /// <param name="context">
        /// The context name in the tuple space.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public TrxServiceMessage Take(object template, int timeout, string context)
        {
            try
            {
                return GetTupleSpace().Take(template, timeout, context);
            }
            catch (AppDomainUnloadedException)
            {
                // Try again
                return GetTupleSpace().Take(template, timeout, context);
            }
        }

        /// <summary>
        /// Get an object from the given context tuple space removing it if available.
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
        /// <param name="context">
        /// The context name in the tuple space.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public TrxServiceMessage Take(object template, int timeout, out int timeInTupleSpace, out int ttl, string context)
        {
            try
            {
                return GetTupleSpace().Take(template, timeout, out timeInTupleSpace, out ttl, context);
            }
            catch (AppDomainUnloadedException)
            {
                // Try again
                return GetTupleSpace().Take(template, timeout, out timeInTupleSpace, out ttl, context);
            }
        }

        /// <summary>
        /// Get an object from the given context tuple space without removing it.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the tuple space.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is
        /// blocked until an object is available, if 0 was provided the call will not block the take if an 
        /// object is not available.
        /// </param>
        /// <param name="context">
        /// The context name in the tuple space.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public TrxServiceMessage Read(object template, int timeout, string context)
        {
            try
            {
                return GetTupleSpace().Read(template, timeout, context);
            }
            catch (AppDomainUnloadedException)
            {
                // Try again
                return GetTupleSpace().Read(template, timeout, context);
            }
        }
        #endregion

        private ITrxServerTupleSpace GetTupleSpace()
        {
            if (_remoteSpace != null)
                return _remoteSpace;

            if (string.IsNullOrEmpty(RemoteTrxServerInstanceName))
                throw new ConfigurationException("Remote Trx Server instance name not set in remote tuple space");

            if (string.IsNullOrEmpty(RemoteTupleSpaceName))
                throw new ConfigurationException("Remote tuple space not specified");

            var remoteSpace = TupleSpaceProvider.GetTupleSpaceByName(AppDomain.CurrentDomain.FriendlyName,
                RemoteTrxServerInstanceName, RemoteTupleSpaceName);

            if (remoteSpace == null)
                throw new ConfigurationException(
                    string.Format("Invalid/unknown remote tuple space {0} on instance {1}",
                        RemoteTupleSpaceName, RemoteTrxServerInstanceName));

            // Ensure we return the same object to all requests.
            lock (_lockObj)
            {
                if (_remoteSpace == null)
                    _remoteSpace = remoteSpace;
                else
                    remoteSpace = _remoteSpace;
            }

            return remoteSpace;
        }

        /// <summary>
        /// Remove any reference to a remote tuple space if it's unloading.
        /// </summary>
        /// <param name="instanceName">
        /// The name of the Trx Server instance.
        /// </param>
        internal void TrxServerIsUnloading(string instanceName)
        {
            if (RemoteTrxServerInstanceName.ToLower() != instanceName.ToLower())
                return;
            
            _remoteSpace = null;
        }
    }
}