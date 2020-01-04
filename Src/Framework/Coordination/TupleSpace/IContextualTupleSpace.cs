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

namespace Trx.Coordination.TupleSpace
{
    public interface IContextualTupleSpace<T>
    {
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
        /// <remarks>
        /// Similar to <see ref="Write"/> but intended to be used when a user wants to return the object
        /// to the tuple space.
        /// </remarks>
        IContext<T> Write(T entry, int ttl, String context);

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
        IContext<T> Return(T entry, int ttl, String context);

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
        T Take(Object template, int timeout, String context);

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
        T Take(Object template, int timeout, out int timeInTupleSpace, out int ttl, String context);

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
        T Read(Object template, int timeout, String context);
    }
}
