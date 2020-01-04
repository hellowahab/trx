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

namespace Trx.Coordination.TupleSpace
{
    public class TupleSpace<T> : ITupleSpace<T>, IContextualTupleSpace<T>
    {
        public readonly string DefaultContext = "default";

        private readonly object _lockObj = new object();
        private ContextFactory<T> _contextFactory;
        private IContext<T> _defaultContext;
        private Dictionary<string, IContext<T>> _contexts;

        public string Name { get; set; }

        public ContextFactory<T> ContextFactory
        {
            set
            {
                lock (_lockObj)
                    _contextFactory = value;
            }
        }

        private IContext<T> GetContext(String name)
        {
            lock (_lockObj)
            {
                if (_contexts == null)
                    _contexts = new Dictionary<string, IContext<T>>();

                if (_contexts.ContainsKey(name))
                    return _contexts[name];

                if (_contextFactory == null)
                    _contextFactory = new HeapQueueContextFactory<T>();

                IContext<T> context = _contextFactory.GetInstance(name);
                _contexts.Add(name, context);

                return context;
            }
        }

        private IContext<T> GetDefaultContext()
        {
            return _defaultContext ?? (_defaultContext = GetContext(DefaultContext));
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
        public void Write(T entry, int ttl)
        {
            GetDefaultContext().Write(entry,ttl);
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
        public void Return(T entry, int ttl)
        {
            GetDefaultContext().Return(entry, ttl);
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
        public T Take(object template, int timeout)
        {
            return GetDefaultContext().Take(template, timeout);
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
        public T Take(Object template, int timeout, out int timeInTupleSpace, out int ttl)
        {
            return GetDefaultContext().Take(template, timeout, out timeInTupleSpace, out ttl);
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
        public T Read(object template, int timeout)
        {
            return GetDefaultContext().Read(template, timeout);
        }

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
        public IContext<T> Write(T entry, int ttl, string context)
        {
            IContext<T> ctx = GetContext(context);
            ctx.Write(entry, ttl);
            return ctx;
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
        public IContext<T> Return(T entry, int ttl, string context)
        {
            IContext<T> ctx = GetContext(context);
            ctx.Return(entry, ttl);
            return ctx;
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
        public T Take(object template, int timeout, string context)
        {
            return GetContext(context).Take(template, timeout);
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
        public T Take(object template, int timeout, out int timeInTupleSpace, out int ttl, string context)
        {
            return GetContext(context).Take(template, timeout, out timeInTupleSpace, out ttl);
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
        public T Read(object template, int timeout, string context)
        {
            return GetContext(context).Read(template, timeout);
        }
    }
}
