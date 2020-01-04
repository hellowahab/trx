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

namespace Trx.Coordination.TupleSpace
{
    public class HeapQueueContext<T> : IContext<T>
    {
        private readonly LinkedList<Entry> _queue = new LinkedList<Entry>();

        /// <summary>
        /// Write an entry into the context space.
        /// </summary>
        /// <param name="entry">
        /// The object to be written.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the context space.
        /// </param>
        public void Write(T entry, int ttl)
        {
            lock (_queue)
            {
                _queue.AddLast(new Entry(entry, ttl));
                Monitor.PulseAll(_queue);
            }
        }

        /// <summary>
        /// Return an entry into the context space.
        /// </summary>
        /// <param name="entry">
        /// The object to be written.
        /// </param>
        /// <param name="ttl">
        /// Time to live in milliseconds before being automatically removed from the context space.
        /// </param>
        /// <remarks>
        /// Similar to <see ref="Write"/> but intended to be used when a user wants to return the object
        /// to the tuple space.
        /// </remarks>
        public void Return(T entry, int ttl)
        {
            lock (_queue)
            {
                _queue.AddFirst(new Entry(entry, ttl));
                Monitor.PulseAll(_queue);
            }
        }

        private Entry GetEntry(int timeout, bool isTake)
        {
            var start = DateTime.UtcNow;
            do
            {
                if (_queue.Count == 0)
                {
                    if (timeout == 0)
                        return null;
                }
                else
                {
                    var e = _queue.First.Value;
                    if (e.IsExpired)
                    {
                        _queue.RemoveFirst();
                        continue;
                    }
                    if (isTake)
                        _queue.RemoveFirst();

                    return e;
                }

                try
                {
                    if (timeout == Timeout.Infinite)
                    {
                        Monitor.Wait(_queue);
                    }
                    else
                    {
                        TimeSpan elapsed = DateTime.UtcNow - start;
                        if (elapsed.TotalMilliseconds > timeout)
                            return null;

                        if (!Monitor.Wait(_queue, timeout - (int)elapsed.TotalMilliseconds))
                            return null;
                    }
                }
                catch (ThreadInterruptedException)
                {
                }
            } while (true);
        }

        private T GetObject(int timeout, bool isTake)
        {
            var e = GetEntry(timeout, isTake);
            return e == null ? default(T) : e.Obj;
        }

        /// <summary>
        /// Get an object from the context space removing it if available.
        /// </summary>
        /// <param name="template">
        /// Not used.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is
        /// blocked until an object is available, if 0 was provided the call will not block the take if an
        /// object is not available.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public T Take(object template, int timeout)
        {
            lock (_queue)
                return GetObject(timeout, true);
        }

        /// <summary>
        /// Get an object from the context space removing it if available.
        /// </summary>
        /// <param name="template">
        /// The template used for matching. null to match the first object in the context space.
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
        public T Take(object template, int timeout, out int timeInTupleSpace, out int ttl)
        {
            timeInTupleSpace = 0;
            ttl = 0;
            lock (_queue)
            {
                var e = GetEntry(timeout, true);
                if (e == null)
                    return default(T);
                timeInTupleSpace = (int) ((DateTime.UtcNow - e.WriteUtcDateTime).TotalMilliseconds);
                ttl = e.Ttl;
                return e.Obj;
            }
        }

        /// <summary>
        /// Get an object from the context space without removing it.
        /// </summary>
        /// <param name="template">
        /// Not used.
        /// </param>
        /// <param name="timeout">
        /// Milliseconds to wait until an object is available. If Timeout.Infinite was provided the call is 
        /// blocked until an object is available, if 0 was provided the call will not block the take if an 
        /// object is not available.
        /// </param>
        /// <returns>
        /// The readed object or null if no one was found in the given timeout.
        /// </returns>
        public T Read(object template, int timeout)
        {
            lock (_queue)
                return GetObject(timeout, false);
        }

        private sealed class Entry
        {
            private readonly T _obj;
            private readonly int _ttl;
            private readonly DateTime _writeUtcDateTime;
            private readonly DateTime _expiration;

            public Entry(T obj, int ttl)
            {
                _obj = obj;
                _ttl = ttl;
                _writeUtcDateTime = DateTime.UtcNow;
                _expiration = _writeUtcDateTime.AddMilliseconds(ttl);
            }

            public int Ttl
            {
                get { return _ttl; }
            }

            public DateTime WriteUtcDateTime
            {
                get { return _writeUtcDateTime; }
            }

            public T Obj
            {
                get { return _obj; }
            }

            public bool IsExpired
            {
                get { return (_ttl != Timeout.Infinite) && (_expiration < DateTime.UtcNow); }
            }
        }
    }
}
