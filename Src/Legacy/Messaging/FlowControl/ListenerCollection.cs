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
using System.Collections;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// It implements a collection of elements which implements
    /// the interface <see cref="IListener"/>.
    /// </summary>
    public class ListenerCollection : MarshalByRefObject, IEnumerable
    {
        private readonly ArrayList _listeners;

        /// <summary>
        /// It initializes a new instance of the class
        /// <see cref="ListenerCollection"/>.
        /// </summary>
        public ListenerCollection()
        {
            _listeners = new ArrayList();
        }

        /// <summary>
        /// It initializes a new instance of the class
        /// <see cref="ListenerCollection"/> setting the number
        /// of elements which the collection is initially capable of
        /// storing.
        /// </summary>
        /// <param name="capacity">
        /// It's the number of the elements which the collection is
        /// initially capable of storing.
        /// </param>
        public ListenerCollection(int capacity)
        {
            _listeners = new ArrayList(capacity);
        }

        /// <summary>
        /// It returns the number of elements stored in the collection.
        /// </summary>
        public int Count
        {
            get { return _listeners.Count; }
        }

        /// <summary>
        /// It adds an element to the collection.
        /// </summary>
        /// <param name="listener">
        /// It's the element to be added to the collection.
        /// </param>
        public void Add(IListener listener)
        {
            _listeners.Add(listener);
        }

        /// <summary>
        /// Removes an element from the collection.
        /// </summary>
        /// <param name="listener">
        /// It's the element to be removed from the collection.
        /// </param>
        public void Remove(IListener listener)
        {
            _listeners.Remove(listener);
        }

        #region Implementation of IEnumerable
        /// <summary>
        /// It creates and returns an enumerator over the collection.
        /// </summary>
        /// <returns>
        /// It's the collection enumerator.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return new ListenerEnumerator(_listeners);
        }

        /// <summary>
        /// It implements a collection enumerator.
        /// </summary>
        [Serializable]
        private class ListenerEnumerator : IEnumerator
        {
            private readonly IEnumerator _listenerEnumerator;

            /// <summary>
            /// It creates a new instance of the class <see cref="ListenerEnumerator"/>.
            /// </summary>
            /// <param name="listeners">
            /// It's the table which contains the listeners.
            /// </param>
            public ListenerEnumerator(ArrayList listeners)
            {
                _listenerEnumerator = listeners.GetEnumerator();
            }

            #region Implementation of IEnumerator
            /// <summary>
            /// It restarts the enumeration.
            /// </summary>
            public void Reset()
            {
                _listenerEnumerator.Reset();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element;
            /// false if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                return _listenerEnumerator.MoveNext();
            }

            /// <summary>
            /// Gets the current element in the enumeration.
            /// </summary>
            public object Current
            {
                get { return _listenerEnumerator.Current; }
            }
            #endregion
        }
        #endregion
    }
}