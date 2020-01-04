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

namespace Trx.Messaging
{
    public delegate void FieldFormatterAddedEventHandler(
        object sender, FieldFormatterEventArgs e);

    public delegate void FieldFormatterRemovedEventHandler(
        object sender, FieldFormatterEventArgs e);

    public delegate void FieldFormatterClearedEventHandler(
        object sender, EventArgs e);

    public class FieldFormatterCollection : IEnumerable
    {
        private readonly Hashtable _fieldsFormatters;
        private int _maxField;
        private bool _maxFieldDirty;

        public FieldFormatterCollection()
        {
            _fieldsFormatters = new Hashtable(64);
            _maxField = int.MinValue;
            _maxFieldDirty = false;
        }

        public FieldFormatter this[int fieldNumber]
        {
            get { return (FieldFormatter) _fieldsFormatters[fieldNumber]; }
        }

        public int Count
        {
            get { return _fieldsFormatters.Count; }
        }

        public int MaximumFieldFormatterNumber
        {
            get
            {
                if (_fieldsFormatters.Count == 0)
                    throw new ApplicationException("The collection is empty.");

                if (_maxFieldDirty)
                {
                    _maxField = int.MinValue;

                    foreach (DictionaryEntry fieldFormatter in _fieldsFormatters)
                        if (((FieldFormatter) (fieldFormatter.Value)).FieldNumber > _maxField)
                            _maxField = ((FieldFormatter) (fieldFormatter.Value)).FieldNumber;

                    _maxFieldDirty = false;
                }

                return _maxField;
            }
        }

        public event FieldFormatterAddedEventHandler Added;

        public event FieldFormatterRemovedEventHandler Removed;

        public event FieldFormatterClearedEventHandler Cleared;

        public void Add(FieldFormatter fieldFormatter)
        {
            if (_fieldsFormatters.Contains(fieldFormatter.FieldNumber))
                Remove(fieldFormatter.FieldNumber);

            _fieldsFormatters[fieldFormatter.FieldNumber] = fieldFormatter;

            if (Added != null)
                Added(this, new FieldFormatterEventArgs(fieldFormatter));

            if (_maxField < fieldFormatter.FieldNumber)
                _maxField = fieldFormatter.FieldNumber;
        }

        public void Remove(int fieldNumber)
        {
            FieldFormatter fieldFormatter = null;

            if (Removed != null)
                if (_fieldsFormatters.Contains(fieldNumber))
                    fieldFormatter = this[fieldNumber];

            _fieldsFormatters.Remove(fieldNumber);

            if (fieldFormatter != null)
                Removed(this, new FieldFormatterEventArgs(fieldFormatter));

            if (fieldNumber == _maxField)
                _maxFieldDirty = true;
        }

        public void Remove(int[] fieldsNumbers)
        {
            foreach (int t in fieldsNumbers)
                Remove(t);
        }

        public void Clear()
        {
            if (_fieldsFormatters.Count == 0)
                return;

            if (Cleared != null)
                Cleared(this, EventArgs.Empty);

            _fieldsFormatters.Clear();
            _maxField = int.MinValue;
        }

        public bool Contains(int fieldNumber)
        {
            return _fieldsFormatters.Contains(fieldNumber);
        }

        public bool Contains(int[] fieldsNumbers)
        {
            foreach (int t in fieldsNumbers)
                if (!Contains(t))
                    return false;

            return true;
        }

        #region Implementation of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new FieldFormattersEnumerator(_fieldsFormatters);
        }

        private class FieldFormattersEnumerator : IEnumerator
        {
            private readonly IEnumerator _fieldFormattersEnumerator;

            public FieldFormattersEnumerator(IDictionary fieldFormatters)
            {
                _fieldFormattersEnumerator = fieldFormatters.GetEnumerator();
            }

            #region IEnumerator Members
            public void Reset()
            {
                _fieldFormattersEnumerator.Reset();
            }

            public bool MoveNext()
            {
                return _fieldFormattersEnumerator.MoveNext();
            }

            public object Current
            {
                get { return ((DictionaryEntry) _fieldFormattersEnumerator.Current).Value; }
            }
            #endregion
        }
        #endregion
    }
}