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
using System.Reflection;
using System.Xml;

namespace Trx.Utilities.Dom2Obj
{
    public class InstrumentedProperty
    {
        private readonly object _instance;
        private readonly PropertyInfo _propertyInfo;
        private readonly bool _canWrite;
        private readonly bool _canRead;

        public InstrumentedProperty(object instance, PropertyInfo propertyInfo, XmlNode node, bool canWrite, bool canRead)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            if (node == null)
                throw new ArgumentNullException("node");

            _instance = instance;
            _propertyInfo = propertyInfo;
            Node = node;
            _canWrite = propertyInfo.CanWrite && canWrite;
            _canRead = propertyInfo.CanRead && canRead;
        }

        public bool CanWrite
        {
            get { return _canWrite; }
        }

        public bool CanRead
        {
            get { return _canRead; }
        }

        internal XmlNode Node { get; set; }

        internal PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        internal object Instance
        {
            get { return _instance; }
        }

        public string Description { get; set; }

        public void SetValue(object value)
        {
            if (_canWrite)
                _propertyInfo.SetValue(_instance, value, null);
        }

        public object GetValue()
        {
            if (_canRead)
                return _propertyInfo.GetValue(_instance, null);

            throw new NotSupportedException("Cannot read the property.");
        }
    }
}