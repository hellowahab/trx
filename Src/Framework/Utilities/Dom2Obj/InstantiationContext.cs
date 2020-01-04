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
using System.Xml;

namespace Trx.Utilities.Dom2Obj
{
    /// <summary>
    /// Represents the contextual information of and object instantiation.
    /// </summary>
    public class InstantiationContext
    {
        private readonly Dictionary<string, object> _declarations;
        private readonly object _instance;
        private readonly XmlNode _node;
        private readonly InstantiationContext _rootInstanceContext;

        /// <summary>
        /// Initializes a new instance of an <see cref="InstantiationContext"/>.
        /// </summary>
        /// <param name="node">
        /// The root node of the instantiation context.
        /// </param>
        public InstantiationContext(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            _node = node;
            _declarations = new Dictionary<string, object>();
            _rootInstanceContext = null;
        }

        /// <summary>
        /// Initializes a new instance of an <see cref="InstantiationContext"/>.
        /// </summary>
        /// <param name="node">
        /// The root node of the instantiation context.
        /// </param>
        /// <param name="instance">
        /// The instanced object.
        /// </param>
        public InstantiationContext(XmlNode node, object instance)
            : this(node)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            _instance = instance;
            _rootInstanceContext = this;
        }

        /// <summary>
        /// Initializes a new instance of an <see cref="InstantiationContext"/>.
        /// </summary>
        /// <param name="node">
        /// The root node of the instantiation context.
        /// </param>
        /// <param name="instance">
        /// The instanced object.
        /// </param>
        /// <param name="rootInstanceContext">
        /// The root instance context.
        /// </param>
        public InstantiationContext(XmlNode node, object instance, InstantiationContext rootInstanceContext)
            : this(node, instance)
        {
            if (rootInstanceContext == null)
                throw new ArgumentNullException("rootInstanceContext");

            _rootInstanceContext = rootInstanceContext;
        }

        public XmlNode Node
        {
            get { return _node; }
        }

        public object Instance
        {
            get { return _instance; }
        }

        public InstantiationContext RootInstanceContext
        {
            get { return _rootInstanceContext; }
        }

        /// <summary>
        /// Returns a previously registered and declared object.
        /// </summary>
        /// <param name="name">
        /// The object name.
        /// </param>
        /// <returns>
        /// The declared object if found, null if not.
        /// </returns>
        public object GetDeclaredObject(string name)
        {
            if (!_declarations.ContainsKey(name))
                return null;

            return _declarations[name];
        }

        /// <summary>
        /// Register a declared object.
        /// </summary>
        /// <param name="name">
        /// The object name.
        /// </param>
        /// <param name="declaredObject">
        /// The declared object.
        /// </param>
        public void RegisterDeclaredObject(string name, object declaredObject)
        {
            _declarations[name] = declaredObject;
        }
    }
}