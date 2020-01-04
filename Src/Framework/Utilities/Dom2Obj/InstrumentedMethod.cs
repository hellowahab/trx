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
    public class InstrumentedMethod
    {
        private readonly Type[] _argsTypes;
        private readonly object _instance;
        private readonly MethodInfo _methodInfo;

        public InstrumentedMethod(object instance, MethodInfo methodInfo, XmlNode node, Type[] argsTypes)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            if (node == null)
                throw new ArgumentNullException("node");

            _instance = instance;
            _methodInfo = methodInfo;
            _argsTypes = argsTypes;
            Node = node;
        }

        public Type[] ArgsTypes
        {
            get { return _argsTypes; }
        }

        internal XmlNode Node { get; set; }

        internal MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        internal object Instance
        {
            get { return _instance; }
        }

        public string Description { get; set; }

        public void Invoke(object[] args)
        {
            _methodInfo.Invoke(_instance, args);
        }
    }
}