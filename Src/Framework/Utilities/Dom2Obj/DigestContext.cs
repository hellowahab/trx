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
using System.IO;
using System.Xml;

namespace Trx.Utilities.Dom2Obj
{
    public class DigestContext
    {
        private readonly Stack<InstantiationContext> _instantiationContexts = new Stack<InstantiationContext>();
        private readonly List<InstrumentedMethod> _instrumentedMethods = new List<InstrumentedMethod>();
        private readonly List<InstrumentedProperty> _instrumentedProperties = new List<InstrumentedProperty>();

        private readonly Dictionary<string, string> _namespaceList = new Dictionary<string, string>();
        private readonly List<XmlNodeInformation> _parserXmlNodeInformation = new List<XmlNodeInformation>(256);
        private readonly XmlTextReader _reader;
        private readonly InstantiationContext _rootContext;

        internal DigestContext(bool isFileName, string xmlSource)
        {
            if (string.IsNullOrEmpty(xmlSource))
                throw new ArgumentNullException("xmlSource");

            Document = new XmlDocument();

            Document.NodeInserting += OnDocumentNodeInserting;
            try
            {
                _reader = isFileName ? new XmlTextReader(xmlSource) : new XmlTextReader(new StringReader(xmlSource));
                Document.Load(_reader);
            }
            finally
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }
            }
            Document.NodeInserting -= OnDocumentNodeInserting;

            if (Document.DocumentElement == null || Document.DocumentElement.NodeType != XmlNodeType.Element)
                throw new ApplicationException("Root document element not found.");

            // Get namespaces (Digester map Xml namespaces to .Net ones)
            foreach (XmlAttribute attribute in Document.DocumentElement.Attributes)
            {
                string assemblyFullName;
                if (string.IsNullOrEmpty(attribute.Prefix) && attribute.LocalName.Equals("xmlns"))
                {
                    // Default namespace.
                    assemblyFullName = Digester.BuildNamespaceAndAssemblyFullName(attribute.Value);
                    if (assemblyFullName == null)
                        throw new ApplicationException(string.Format("Can't locate default assembly '{0}'.",
                            attribute.Value));

                    _namespaceList.Add(string.Empty, assemblyFullName);
                }
                else if (attribute.Prefix != null && attribute.Prefix.Equals("xmlns"))
                {
                    assemblyFullName = Digester.BuildNamespaceAndAssemblyFullName(attribute.Value);
                    if (assemblyFullName == null) // Assembly wasn't found.
                        throw new ApplicationException(string.Format("Can't locate assembly '{0}'.",
                            Digester.GetAssemblyName(attribute.Value)));

                    _namespaceList.Add(attribute.LocalName, assemblyFullName);
                }
            }

            if (!_namespaceList.ContainsKey(string.Empty))
                _namespaceList.Add(string.Empty, Digester.BuildNamespaceAndAssemblyFullName(Digester.DefaultNamespace));

            _rootContext = new InstantiationContext(Document.DocumentElement);
            _instantiationContexts.Push(_rootContext);
        }

        public XmlDocument Document { get; private set; }

        internal Dictionary<string, string> NamespaceList
        {
            get { return _namespaceList; }
        }

        internal Stack<InstantiationContext> InstantiationContexts
        {
            get { return _instantiationContexts; }
        }

        internal InstantiationContext RootContext
        {
            get { return _rootContext; }
        }

        internal List<XmlNodeInformation> ParserXmlNodeInformation
        {
            get { return _parserXmlNodeInformation; }
        }

        public List<InstrumentedProperty> InstrumentedProperties
        {
            get { return _instrumentedProperties; }
        }

        public List<InstrumentedMethod> InstrumentedMethods
        {
            get { return _instrumentedMethods; }
        }

        private void OnDocumentNodeInserting(object sender, XmlNodeChangedEventArgs e)
        {
            IXmlLineInfo lineInfo = _reader;
            if (lineInfo.HasLineInfo())
                _parserXmlNodeInformation.Add(
                    new XmlNodeInformation(e.Node, lineInfo.LineNumber, lineInfo.LinePosition));
        }
    }
}