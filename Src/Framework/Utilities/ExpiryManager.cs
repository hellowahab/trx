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
using System.Collections.Generic;
using System.Threading;

namespace Trx.Utilities
{
    /// <summary>
    /// Useful class to track object expiration.
    /// </summary>
    /// <typeparam name="TKey">
    /// Object key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// Object class type.
    /// </typeparam>
    public class ExpiryManager<TKey, TValue>
    {
        private readonly Dictionary<TKey, Entry> _activeEntries = new Dictionary<TKey, Entry>();
        private readonly int _defaultExpiration;
        private readonly object _lockObj = new object();

        public delegate void ExpirationCallback(TKey key, TValue value);

        public ExpiryManager(int defaultExpiration)
        {
            if (defaultExpiration < 1)
                throw new ArgumentOutOfRangeException("defaultExpiration", defaultExpiration,
                    "Must be greater than zero.");

            _defaultExpiration = defaultExpiration;
        }

        public int DefaultExpiration
        {
            get { return _defaultExpiration; }
        }

        public TValue InternalCancel(TKey key)
        {
            if (_activeEntries.ContainsKey(key))
            {
                Entry entry = _activeEntries[key];
                entry.Timer.Change(Timeout.Infinite, Timeout.Infinite);
                _activeEntries.Remove(key);
                return entry.Value;
            }

            return default(TValue);
        }

        public TValue Manage(ExpirationCallback callback, TKey key, TValue value, int expiration)
        {
            lock (_lockObj)
            {
                TValue replaced = InternalCancel(key);
                var entry = new Entry
                                {
                                    Key = key,
                                    Value = value,
                                    Callback = callback
                                };
                entry.Timer = new Timer(OnTimer, entry, expiration, Timeout.Infinite);
                _activeEntries.Add(key, entry);

                return replaced;
            }
        }

        public TValue Manage(ExpirationCallback callback, TKey key, TValue value)
        {
            return Manage(callback, key, value, _defaultExpiration);
        }

        public TValue Cancel(TKey key)
        {
            lock (_lockObj)
            {
                return InternalCancel(key);
            }
        }

        private void OnTimer(object state)
        {
            var entry = state as Entry;
            if (entry == null)
                return;

            TKey key = entry.Key;
            lock (_lockObj)
            {
                if (!_activeEntries.ContainsKey(key))
                    return;

                Entry registered = _activeEntries[key];
                if (!ReferenceEquals(entry, registered))
                    // Replaced.
                    return;

                _activeEntries.Remove(key);
            }

            entry.Callback(key, entry.Value);
        }

        #region Nested type: Entry
        private class Entry
        {
            public TKey Key;
            public Timer Timer;
            public TValue Value;
            public ExpirationCallback Callback;
        }
        #endregion
    }
}