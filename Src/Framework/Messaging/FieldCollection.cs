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
    [Serializable]
    public class FieldCollection : IEnumerable
    {
        private readonly Hashtable _fields;
        private bool _dirty;
        private int _maxField;
        private bool _maxFieldDirty;

        public FieldCollection()
        {
            _fields = new Hashtable(64);
            _maxField = int.MinValue;
            _dirty = false;
            _maxFieldDirty = false;
        }

        public Field this[int fieldNumber]
        {
            get { return (Field) _fields[fieldNumber]; }
        }

        public int Count
        {
            get { return _fields.Count; }
        }

        public int MaximumFieldNumber
        {
            get
            {
                if (_fields.Count == 0)
                    throw new ApplicationException("The collection is empty.");

                if (_maxFieldDirty)
                {
                    _maxField = int.MinValue;

                    foreach (DictionaryEntry field in _fields)
                        if (((Field) (field.Value)).FieldNumber > _maxField)
                            _maxField = ((Field) (field.Value)).FieldNumber;
                    _maxFieldDirty = false;
                }

                return _maxField;
            }
        }

        public bool Dirty
        {
            get { return _dirty; }

            set { _dirty = value; }
        }

        public void Add(Field field)
        {
            if (field == null)
                return;

            _fields[field.FieldNumber] = field;

            if (_maxField < field.FieldNumber)
                _maxField = field.FieldNumber;

            _dirty = true;
        }

        public void Add(int fieldNumber, string fieldValue)
        {
            Add(new StringField(fieldNumber, fieldValue));
        }

        public void Add(int fieldNumber, byte[] fieldValue)
        {
            Add(new BinaryField(fieldNumber, fieldValue));
        }

        public void Add(int fieldNumber, Message fieldValue)
        {
            Add(new InnerMessageField(fieldNumber, fieldValue));
        }

        public void Remove(int fieldNumber)
        {
            _fields.Remove(fieldNumber);

            _dirty = true;

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
            if (_fields.Count == 0)
                return;

            _fields.Clear();
            _dirty = true;
            _maxField = int.MinValue;
        }

        public bool Contains(int fieldNumber)
        {
            return _fields.Contains(fieldNumber);
        }

        public bool Contains(int[] fieldsNumbers)
        {
            foreach (int t in fieldsNumbers)
                if (!Contains(t))
                    return false;

            return true;
        }

        public bool ContainsAtLeastOne(int[] fieldsNumbers)
        {
            foreach (int t in fieldsNumbers)
                if (Contains(t))
                    return true;

            return false;
        }

        public bool ContainsAtLeastOne(int lowerFieldNumber, int upperFieldNumber)
        {
            for (int i = lowerFieldNumber; i <= upperFieldNumber; i++)
                if (Contains(i))
                    return true;

            return false;
        }

        public void MoveField(int oldFieldNumber, int newFieldNumber)
        {
            if (!_fields.Contains(oldFieldNumber))
                throw new ArgumentException("Field doesn't exists.", "oldFieldNumber");

            Field field = this[oldFieldNumber];
            Remove(oldFieldNumber);
            var newField = (Field) field.NewComponent();
            newField.SetFieldNumber(newFieldNumber);
            newField.Value = field.Value;
            Add(newField);
        }

        #region Implementation of IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new FieldsEnumerator(_fields);
        }

        /// <summary>
        /// Implementa el enumerador de la colección.
        /// </summary>
        private class FieldsEnumerator : IEnumerator
        {
            private readonly IEnumerator _fieldsEnumerator;

            public FieldsEnumerator(Hashtable fields)
            {
                _fieldsEnumerator = fields.GetEnumerator();
            }

            #region Implementation of IEnumerator
            public void Reset()
            {
                _fieldsEnumerator.Reset();
            }

            public bool MoveNext()
            {
                return _fieldsEnumerator.MoveNext();
            }

            public object Current
            {
                get { return ((DictionaryEntry) _fieldsEnumerator.Current).Value; }
            }
            #endregion
        }
        #endregion
    }
}