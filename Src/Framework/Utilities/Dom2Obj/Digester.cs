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
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Trx.Logging;

namespace Trx.Utilities.Dom2Obj
{
    /// <summary>
    /// Digest a XML and creates represented objects types setting it's properties. Allows type constructors 
    /// specification and method invocation, aditionally, properties and methods can be instrumented.
    /// </summary>
    public static class Digester
    {
        public const string DefaultNamespace = "Trx.Messaging, Trx";

        private const string ConstructorElement = "Constructor";
        private const string ParameterElement = "Parameter";
        private const string InvokeElement = "Invoke";
        private const string DeclareElement = "Declare";
        private const string PropertyElement = "Property";

        private const string NameAttr = "Name";
        private const string TypeAttr = "Type";
        private const string ValueAttr = "Value";
        private const string ReferenceAttr = "Reference";
        private const string DeclareAttr = "Declare";
        private const string InstrumentateAttr = "Instrumentate";
        private const string InvocationAttr = "Invocation";
        private const string OptionsAttr = "Options";
        private const string DescriptionAttr = "Description";
        private const string CastAttr = "Cast";

        private const string ParseInvocationOption = "PARSE";
        private const string InstrumentateInvocationOption = "INSTRUMENTATE";
        private const string SetNullOption = "SETNULL";
        private const string SingletonOption = "SINGLETON";
        private const string ReadOnlyInstrumentateOption = "READ ONLY";
        private const string ReadWriteInstrumentateOption = "READ WRITE";

        private static readonly char[] CommaDelimiter = new[] {','};

        /*
         * The supported syntax is:
         * 
         *     <RootElementName [nsX:]Type="typeName" [Declare="reference"]         RootElementName can be any element name (it's descriptive only)
         *        [xmlns="default"] [xmlns:ns1="ns1"] .. [xmlns:nsN="nsN"] /> |
         *     <RootElementName [nsX:]Type="typeName" [Declare="reference"]
         *        [xmlns="default"] [xmlns:ns1="ns1"] .. [xmlns:nsN="nsN"]>
         *        Object
         *     </RootElementName>
         *
         *     Object:                      Object specification, child elements can appear in any order and mixed
         *
         *        Constructor[0..1]         Only the first Constructor element found is processed
         *        Declare[0..n]				Consumed on demand
         *        Invoke[0..n]				Processed in order of appearance
         *        Property[0..n]			Processed in order of appearance
         *
         *     Constructor:
         *
         *        <Constructor>
         *           Parameter[1..n]        Processed in order of appearance
         *        </Constructor>
         *
         *     Declare:                     An object declaration to be referenced in the document
         *
         *        <Declare Name="name" [[nsX:]Type="typeName"] Value="value" /> |
         *        <Declare Name="name" [nsX:]Type="typeName">
         *           Object
         *        </Declare>
         *
         *     Parameter:                   singleton option call GetInstance with no parameters to get the instance
         *
         *        <Parameter [[nsX:]Type="typeName"] { Value="value" | Options="setnull|singleton" } [Declare="reference"] /> |
         *        <Parameter [Reference="reference"] /> |
         *        <Parameter [nsX:]Type="typeName" [Declare="reference"] />
         *           Object
         *        </Parameter>
         *
         *     Invoke:                      parse option call method on digest, additionally method can be instrumented
         *                                  with instrumentate option. If only instrumentate option is declared, method is
         *                                  not invoked on digest. If no Invocation attribute is specified, invokation is
         *                                  done on digest.
         *
         *        <Invoke Name="procOrFuncName" [Invocation="[parse[,instrumentate]]" [Description="description"]] /> |
         *        <Invoke Name="procOrFuncName" [Invocation="[parse[,instrumentate]]" [Description="description"]]>
         *           Parameter[1..n]        Processed in order of appearance
         *        </Invoke>
         *
         *     Property:
         *
         *        <Property Name="name" [[nsX:]Type="typeName"] { Value="value" | Options="setnull|singleton" }
         *           [Instrumentate="read only|read/write" [Description="description"]] [Declare="reference"]/> |
         *        <Property Name="name" [Reference="reference"] /> |
         *        <Property Name="name" [[nsX:]Type="typeName"] [Declare="reference"] />
         *           Object
         *        </Property>
         */

        public static object DigestFile(string fileName, out DigestContext digestContext)
        {
            digestContext = new DigestContext(true, fileName);
            return ProcessNode(digestContext.Document.DocumentElement, digestContext, ComponentType.RootNode, null, null);
        }

        public static object DigestFile(string fileName)
        {
            var digestContext = new DigestContext(true, fileName);
            return ProcessNode(digestContext.Document.DocumentElement, digestContext, ComponentType.RootNode, null, null);
        }

        public static object DigestString(string xml, out DigestContext digestContext)
        {
            digestContext = new DigestContext(false, xml);
            return ProcessNode(digestContext.Document.DocumentElement, digestContext, ComponentType.RootNode, null, null);
        }

        public static object DigestString(string xml)
        {
            var digestContext = new DigestContext(false, xml);
            return ProcessNode(digestContext.Document.DocumentElement, digestContext, ComponentType.RootNode, null, null);
        }

        /// <summary>
        /// Compare context Xml document with a Xml file. Comparison is made in a simple way just
        /// validating the element names, if both Xml documents have the same node element types
        /// in the same order, assume the structure hasn't changed.
        /// </summary>
        /// <param name="context">
        /// The digest context of a previous digest operation.
        /// </param>
        /// <param name="document">
        /// The Xml document to compare to.
        /// </param>
        /// <returns>
        /// True if Xml documents have changed, otherwise false.
        /// </returns>
        public static bool StructureHasChanged(DigestContext context, XmlDocument document)
        {
            // Compare both document elements.
            if (document.DocumentElement == null || document.DocumentElement.NodeType != XmlNodeType.Element ||
                context.Document == null || context.Document.DocumentElement == null ||
                    document.DocumentElement.Name != context.Document.DocumentElement.Name)
                return true;

            // Compare document element type.
            XmlAttribute ctxAttr = GetAttribute(context.Document.DocumentElement, TypeAttr);
            XmlAttribute docAttr = GetAttribute(document.DocumentElement, TypeAttr);
            if (ctxAttr == null || docAttr == null || ctxAttr.Name != docAttr.Name || ctxAttr.Value != docAttr.Value)
                return true;

            return ChildsChanged(context, context.Document.DocumentElement, document.DocumentElement, false, null);
        }

        public static void UpdateValueProperties(DigestContext context, XmlDocument document, ILogger logger)
        {
            if (document.DocumentElement == null || document.DocumentElement.NodeType != XmlNodeType.Element ||
                context.Document == null || context.Document.DocumentElement == null ||
                    document.DocumentElement.Name != context.Document.DocumentElement.Name)
                return;

            // Update document element properties.
            UpdateChildValueProperties(context, context.Document.DocumentElement, document.DocumentElement, logger);
            // Update inner elements properties.
            ChildsChanged(context, context.Document.DocumentElement, document.DocumentElement, true, logger);
        }

        private static bool ChildsChanged(DigestContext context, XmlNode ctxNode, XmlNode node, bool updateValueProps, ILogger logger)
        {
            int nodeIdx = 0;
            foreach (XmlNode ctxChild in ctxNode.ChildNodes)
            {
                if (ctxChild.NodeType != XmlNodeType.Element)
                    continue; // Ignore node.

                if (ctxChild.LocalName != ConstructorElement && ctxChild.LocalName != ParameterElement &&
                    ctxChild.LocalName != InvokeElement && ctxChild.LocalName != DeclareElement &&
                        ctxChild.LocalName != PropertyElement)
                    continue; // Ignore element.

                bool found = false;
                while (nodeIdx < node.ChildNodes.Count)
                {
                    if (node.ChildNodes[nodeIdx].NodeType == XmlNodeType.Element &&
                        (node.ChildNodes[nodeIdx].LocalName == ConstructorElement ||
                        node.ChildNodes[nodeIdx].LocalName == ParameterElement ||
                        node.ChildNodes[nodeIdx].LocalName == InvokeElement ||
                        node.ChildNodes[nodeIdx].LocalName == DeclareElement ||
                        node.ChildNodes[nodeIdx].LocalName == PropertyElement))
                    {
                        if (ctxChild.Name != node.ChildNodes[nodeIdx].Name)
                            return true; // Changed in some way (maybe a new node, namespace or node move).

                        // If it's a property check it's name.
                        if (ctxChild.LocalName == PropertyElement)
                        {
                            XmlAttribute ctxAttr = GetAttribute(ctxChild, NameAttr);
                            XmlAttribute nodeAttr = GetAttribute(node.ChildNodes[nodeIdx], NameAttr);
                            if ((ctxAttr == null && nodeAttr != null) || (ctxAttr != null && nodeAttr == null) ||
                                (ctxAttr != null && ctxAttr.Value != nodeAttr.Value))
                                return true;
                        }

                        // We have a match.
                        found = true;

                        if (updateValueProps)
                            UpdateChildValueProperties(context, ctxChild, node.ChildNodes[nodeIdx], logger);

                        if (ChildsChanged(context, ctxChild, node.ChildNodes[nodeIdx], updateValueProps, logger))
                            return true;

                        nodeIdx++;
                        break;
                    }
                    nodeIdx++;
                }

                if (!found)
                    return true;
            }

            // Process the rest of node childs.
            while (nodeIdx < node.ChildNodes.Count)
                if (node.ChildNodes[nodeIdx].NodeType == XmlNodeType.Element &&
                    (node.ChildNodes[nodeIdx].LocalName == ConstructorElement ||
                    node.ChildNodes[nodeIdx].LocalName == ParameterElement ||
                    node.ChildNodes[nodeIdx].LocalName == InvokeElement ||
                    node.ChildNodes[nodeIdx].LocalName == DeclareElement ||
                    node.ChildNodes[nodeIdx].LocalName == PropertyElement))
                    // A node not matched with one of ctxNode.
                    return true;

            return false;
        }

        private static int ValuePropertyOcurrence(XmlNode node, XmlNode propertyNode, string name)
        {
            int ocurrence = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (ReferenceEquals(propertyNode, child))
                {
                    ocurrence++;
                    return ocurrence;
                }

                XmlAttribute attr = GetAttribute(child, NameAttr);
                if (attr == null || GetAttribute(child, ValueAttr) == null)
                    continue;

                if (attr.Value == name)
                    ocurrence++;
            }

            return 0;
        }

        private static void UpdateChildValueProperties(DigestContext context, XmlNode ctxNode, XmlNode node, ILogger logger)
        {
            foreach (XmlNode ctxChild in ctxNode.ChildNodes)
            {
                if (ctxChild.NodeType != XmlNodeType.Element || ctxChild.LocalName != PropertyElement)
                    continue; // Ignore node.

                XmlAttribute ctxNameAttr = GetAttribute(ctxChild, NameAttr);
                if (ctxNameAttr == null)
                    continue;

                XmlAttribute ctxValueAttr = GetAttribute(ctxChild, ValueAttr);
                if (ctxValueAttr == null)
                    continue;

                int ctxPropOcurr = ValuePropertyOcurrence(ctxNode, ctxChild, ctxNameAttr.Value);
                int nodePropOcurr = 0;
                foreach (XmlNode nodeChild in node)
                {
                    if (nodeChild.NodeType != XmlNodeType.Element || nodeChild.LocalName != PropertyElement)
                        continue; // Ignore node.

                    XmlAttribute nodeCtxAttr = GetAttribute(nodeChild, NameAttr);
                    if (nodeCtxAttr == null || ctxNameAttr.Value != nodeCtxAttr.Value)
                        continue;

                    XmlAttribute nodeValueAttr = GetAttribute(nodeChild, ValueAttr);
                    if (nodeValueAttr == null)
                        continue;

                    nodePropOcurr++;

                    if (ctxPropOcurr == nodePropOcurr)
                        // Found matching property node.
                        if (ctxValueAttr.Value == nodeValueAttr.Value)
                            // Same value, continue with next property.
                            break;
                        else
                        {
                            // Locate node info (wich has object instance and property info).
                            XmlNodeInformation ctxNodeInfo = null;
                            foreach (XmlNodeInformation info in context.ParserXmlNodeInformation)
                                if (ReferenceEquals(info.Node, ctxChild))
                                {
                                    ctxNodeInfo = info;
                                    break;
                                }
                            if (ctxNodeInfo == null || ctxNodeInfo.Instance == null ||
                                ctxNodeInfo.PropertyInfo == null || !ctxNodeInfo.PropertyInfo.CanWrite)
                                continue;

                            // Value has changed, update it.
                            object value = null;
                            if (TryToConvert(nodeValueAttr.Value, ctxNodeInfo.PropertyInfo.PropertyType, ref value))
                            {
                                if (logger != null)
                                    logger.Info(string.Format("Changing property '{0}' from '{1}' to '{2}'.",
                                        ctxNameAttr.Value, ctxValueAttr.Value, nodeValueAttr.Value));
                                ctxNodeInfo.PropertyInfo.SetValue(ctxNodeInfo.Instance, value, null);
                                ctxValueAttr.Value = nodeValueAttr.Value;
                            }
                        }
                }
            }
        }

        private static XmlAttribute GetAttribute(XmlNode objectNode, string name)
        {
            if (objectNode.Attributes == null)
                return null;

            foreach (XmlAttribute attribute in objectNode.Attributes)
                if (attribute.LocalName == name)
                    return attribute;

            return null;
        }

        private static XmlNode GetElement(XmlNode node, string name)
        {
            foreach (XmlNode child in node.ChildNodes)
                if (child.NodeType == XmlNodeType.Element && child.LocalName == name)
                    return child;

            return null;
        }

        private static object[] GetParameters(XmlNode node, DigestContext digestContext)
        {
            var args = new List<object>();
            foreach (XmlNode child in node.ChildNodes)
                if (child.NodeType == XmlNodeType.Element && child.LocalName == ParameterElement)
                    args.Add(ProcessNode(child, digestContext, ComponentType.Parameter, null, null));

            return (args.Count == 0) ? null : args.ToArray();
        }

        private static object[] GetConstructorParameters(XmlNode node, DigestContext digestContext)
        {
            XmlNode ctorNode = GetElement(node, ConstructorElement);

            if (ctorNode == null)
                return null;

            return GetParameters(ctorNode, digestContext);
        }

        internal static string GetAssemblyName(string reference)
        {
            string retValue = reference;

            if (StringUtilities.Count(reference, ',') >= 1)
                retValue = StringUtilities.RightOf(reference, ',').Trim();

            return retValue;
        }

        internal static string GetNamespaceName(string reference)
        {
            string retValue = reference;

            if (StringUtilities.Count(reference, ',') >= 1)
                retValue = StringUtilities.LeftOf(reference, ',').Trim();

            return retValue;
        }

        internal static string BuildNamespaceAndAssemblyFullName(string reference)
        {
            Assembly assembly = Assembly.Load(GetAssemblyName(reference));

            if (assembly == null)
                return null;

            return string.Format("{0}, {1}", GetNamespaceName(reference), assembly.FullName);
        }

        private static string GetNodeInformation(XmlNode node, DigestContext digestContext)
        {
            foreach (XmlNodeInformation info in digestContext.ParserXmlNodeInformation)
                if (ReferenceEquals(info.Node, node))
                    return string.Format("(node on line {0}, position {1})", info.LineNumber, info.LinePosition);

            return "(no node line and position)";
        }

        private static string ComposeFullTypeName(string typeName, string ns)
        {
            typeName = string.Format("{0}.{1}", GetNamespaceName(ns), typeName);
            string assemblyName = GetAssemblyName(ns);
            if (!string.IsNullOrEmpty(assemblyName))
                typeName = string.Format("{0},{1}", typeName, assemblyName);

            return typeName;
        }

        private static Type GetType(string typeName, string ns, DigestContext digestContext, XmlNode node)
        {
            Type retVal;
            if (string.IsNullOrEmpty(ns))
                switch (typeName.ToUpper())
                {
                    case "BOOL":
                    case "BOOLEAN":
                        retVal = typeof (bool);
                        break;
                    case "INT":
                    case "INTEGER":
                        retVal = typeof (int);
                        break;
                    case "CHAR":
                        retVal = typeof (char);
                        break;
                    case "BYTE":
                        retVal = typeof (byte);
                        break;
                    case "LONG":
                        retVal = typeof (long);
                        break;
                    case "FLOAT":
                        retVal = typeof (float);
                        break;
                    case "DOUBLE":
                        retVal = typeof (double);
                        break;
                    case "DECIMAL":
                        retVal = typeof (decimal);
                        break;
                    case "STRING":
                        retVal = typeof (string);
                        break;
                    default:
                        retVal = Type.GetType(typeName) ??
                            // Use default namespace.
                            Type.GetType(ComposeFullTypeName(typeName, digestContext.NamespaceList[string.Empty]));
                        break;
                }
            else
            {
                if (!digestContext.NamespaceList.ContainsKey(ns))
                    throw new ApplicationException(string.Format("Unknown namespace '{0}' {1}.",
                        ns, GetNodeInformation(node, digestContext)));
                retVal = Type.GetType(ComposeFullTypeName(typeName, digestContext.NamespaceList[ns]));
            }

            if ((object) retVal == null)
                throw new ApplicationException(string.Format("Unknown type '{0}' {1}.",
                    typeName, GetNodeInformation(node, digestContext)));

            return retVal;
        }

        private static bool TryToConvert(object data, Type type, ref object obj)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter == null)
                return false;

            if (!converter.CanConvertFrom(data.GetType()))
                return false;

            obj = converter.ConvertFrom(data);
            return true;
        }

        /*
        private static bool TryToConvert(string data, Type type, ref object obj)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter == null)
                return false;

            if (!converter.CanConvertFrom(typeof (string)))
                return false;

            obj = converter.ConvertFromInvariantString(data);
            return true;
        }
         */

        private static XmlNode LocateDeclarationElement(XmlNode node, string name)
        {
            foreach (XmlNode child in node.ChildNodes)
                if (child.NodeType == XmlNodeType.Element && child.LocalName == DeclareElement &&
                    child.Attributes != null)
                    foreach (XmlAttribute attr in child.Attributes)
                        if (attr.LocalName == NameAttr && attr.Value == name)
                            return child;

            return null;
        }

        private static object LocateDeclaredObject(string name, DigestContext digestContext)
        {
            object declaredObject = null;
            foreach (InstantiationContext context in digestContext.InstantiationContexts)
            {
                // Check if we have a reference in the instantiation context.
                declaredObject = context.GetDeclaredObject(name);
                if (declaredObject != null)
                    break;

                // Check if it's declared in the context.
                XmlNode node = LocateDeclarationElement(context.Node, name);
                if (node != null)
                {
                    declaredObject = ProcessNode(node, digestContext, ComponentType.Declaration, null, null);
                    context.RegisterDeclaredObject(name, declaredObject);
                    break;
                }
            }

            return declaredObject;
        }

        private static object CreateInstance(Type type, object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        private static object ProcessNode(XmlNode node, DigestContext digestContext, ComponentType componentType,
            object instance, Type type)
        {
            // Process Value attribute
            XmlAttribute valueAttr = null;
            if (componentType == ComponentType.Property || componentType == ComponentType.Parameter ||
                componentType == ComponentType.Declaration)
                valueAttr = GetAttribute(node, ValueAttr);

            // Get the type info.
            XmlAttribute typeAttr = null;
            Type objType = null;
            if (componentType != ComponentType.Invoke)
            {
                typeAttr = GetAttribute(node, TypeAttr);
                if (typeAttr == null)
                    // If no type specified, default to string
                    objType = type ?? typeof (string);
                else
                    objType = GetType(typeAttr.Value, typeAttr.Prefix, digestContext, typeAttr);
                if (valueAttr != null)
                    if (!TryToConvert(valueAttr.Value, objType, ref instance))
                        throw new ApplicationException(string.Format(
                            "Can't parse value '{0}' {1}.", valueAttr.Value,
                            GetNodeInformation(valueAttr, digestContext)));
            }

            // Check options or if it's a reference then get and return it.
            XmlAttribute refAttr = null;
            if (componentType == ComponentType.Property || componentType == ComponentType.Parameter)
            {
                if ((object)objType != null) // Cast to object type, makes MoMa happy :)
                {
                    XmlAttribute optionsAttr = GetAttribute(node, OptionsAttr);
                    if (optionsAttr != null)
                    {
                        string opt = optionsAttr.Value.ToUpper();
                        switch (opt)
                        {
                            case SetNullOption:
                                return null;

                            case SingletonOption:
                                instance = GetSingleton(node, digestContext, objType);
                                if (instance == null)
                                    throw new ApplicationException(string.Format("Can't get singleton on '{0}' {1}.",
                                        objType, GetNodeInformation(node, digestContext)));
                                break;
                        }
                    }
                }
                
                refAttr = GetAttribute(node, ReferenceAttr);
                if (refAttr != null)
                {
                    instance = LocateDeclaredObject(refAttr.Value, digestContext);
                    if (instance == null)
                        throw new ApplicationException(string.Format(
                            "Can't resolve reference to '{0}' {1}.", refAttr.Value,
                            GetNodeInformation(refAttr, digestContext)));
                }
            }

            if (valueAttr == null && componentType != ComponentType.Invoke)
            {
                // Not a value type or reference. Try to instance by type.
                if (instance == null && typeAttr == null)
                    throw new ApplicationException(string.Format(
                        "Unable to instance {0}.", GetNodeInformation(node, digestContext)));

                // If we don't have an instance or the type is specified, create it.
                if (instance == null || typeAttr != null)
                {
                    object[] ctorArgs = GetConstructorParameters(node, digestContext);
                    instance = CreateInstance(objType, ctorArgs);
                    if (instance == null)
                        throw new ApplicationException(string.Format("Unable to instance '{0}' {1}.",
                            objType, GetNodeInformation(node, digestContext)));
                }
            }

            if (instance != null)
            {
                if (refAttr == null && (componentType == ComponentType.Property ||
                    componentType == ComponentType.Parameter || componentType == ComponentType.RootNode))
                {
                    // Check if we must store a reference of this object. 
                    XmlAttribute decAttr = GetAttribute(node, DeclareAttr);
                    if (decAttr != null)
                        // A declared object goes to the root context.
                        digestContext.RootContext.RegisterDeclaredObject(decAttr.Value, instance);
                }

                if (valueAttr == null && componentType != ComponentType.Invoke)
                {
                    InstantiationContext context = digestContext.InstantiationContexts.Count == 1
                        ? new InstantiationContext(node, instance)
                        : new InstantiationContext(node, instance, digestContext.InstantiationContexts.Peek());

                    digestContext.InstantiationContexts.Push(context);
                    ProcessObject(instance, node, digestContext);
                    digestContext.InstantiationContexts.Pop();
                }
            }

            if (componentType == ComponentType.Parameter)
            {
                // Check if must be converted.
                var castAttr = GetAttribute(node, CastAttr);
                if (castAttr != null)
                {
                    var castType = GetType(castAttr.Value, castAttr.Prefix, digestContext, castAttr);
                    try
                    {
                        instance = Convert.ChangeType(instance, castType);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format(
                            "Can't cast instance to type '{0}' {1}.", castType,
                            GetNodeInformation(valueAttr, digestContext)), ex);
                    }
                }
            }

            return instance;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propertyInfo;

            try
            {
                propertyInfo = type.GetProperty(propertyName);
            }
            catch (AmbiguousMatchException)
            {
                try
                {
                    propertyInfo = type.GetProperty(propertyName, BindingFlags.DeclaredOnly);
                }
                catch (AmbiguousMatchException e)
                {
                    throw new InvalidOperationException("Can't determine correct property to set.", e);
                }
            }

            return propertyInfo;
        }

        private static MethodInfo GetMethodInfo(Type type, string methodName, Type[] args)
        {
            MethodInfo methodInfo;

            try
            {
                methodInfo = args == null ? type.GetMethod(methodName) : type.GetMethod(methodName, args);
            }
            catch (AmbiguousMatchException)
            {
                try
                {
                    methodInfo = type.GetMethod(methodName, BindingFlags.DeclaredOnly, null, args ?? Type.EmptyTypes,
                        null);
                }
                catch (AmbiguousMatchException e)
                {
                    throw new InvalidOperationException("Can't determine correct method.", e);
                }
            }

            return methodInfo;
        }

        private static object GetSingleton(XmlNode node, DigestContext digestContext, Type objType)
        {
            try
            {
                MethodInfo methodInfo = objType.GetMethod("GetInstance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                return methodInfo == null ? null : methodInfo.Invoke(null, null);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Can't get singleton on '{0}' {1}.",
                    objType, GetNodeInformation(node, digestContext)), ex);
            }
        }

        private static void ProcessObject(object instance, XmlNode node, DigestContext digestContext)
        {
            XmlAttribute nameAttr;
            foreach (XmlNode child in node.ChildNodes)
                if (child.NodeType == XmlNodeType.Element)
                    switch (child.LocalName)
                    {
                        case InvokeElement:
                            nameAttr = GetAttribute(child, NameAttr);
                            if (nameAttr == null)
                                throw new ApplicationException(
                                    string.Format("A name must be specified for an invoke {0}.",
                                        GetNodeInformation(child, digestContext)));
                            object[] args = GetParameters(child, digestContext);
                            Type[] argsTypes = null;
                            if (args != null)
                            {
                                argsTypes = new Type[args.Length];
                                for (int i = args.Length - 1; i >= 0; i--)
                                    argsTypes[i] = args[i].GetType();
                            }
                            MethodInfo methodInfo = GetMethodInfo(instance.GetType(), nameAttr.Value,
                                argsTypes);

                            if (methodInfo == null)
                                throw new ApplicationException(
                                    string.Format("Method {0} not found {1}.", nameAttr.Value,
                                        GetNodeInformation(child, digestContext)));

                            XmlAttribute invocationAttr = GetAttribute(child, InvocationAttr);
                            bool invoke = true;
                            bool instrOpt = false;
                            if (invocationAttr != null)
                            {
                                invoke = false;
                                string[] split = invocationAttr.Value.Split(CommaDelimiter);
                                foreach (string option in split)
                                {
                                    string opt = option.ToUpper();
                                    switch (opt)
                                    {
                                        case ParseInvocationOption:
                                            invoke = true;
                                            break;
                                        case InstrumentateInvocationOption:
                                            instrOpt = true;
                                            break;
                                    }
                                }
                            }

                            if (invoke)
                                try
                                {
                                    methodInfo.Invoke(instance, args);
                                }
                                catch (Exception ex)
                                {
                                    throw new ApplicationException(
                                        string.Format("Error calling method {0} {1}.", nameAttr.Value,
                                            GetNodeInformation(child, digestContext)), ex);
                                }

                            if (instrOpt)
                                InstrumentateMethod(digestContext, instance, methodInfo, child, argsTypes);

                            break;

                        case PropertyElement:
                            nameAttr = GetAttribute(child, NameAttr);
                            if (nameAttr == null)
                                throw new ApplicationException(
                                    string.Format("A name must be specified for a property {0}.",
                                        GetNodeInformation(child, digestContext)));

                            PropertyInfo propertyInfo = GetPropertyInfo(instance.GetType(),
                                nameAttr.Value);
                            if (propertyInfo == null)
                                throw new ApplicationException(
                                    string.Format("Property '{0}' not found in the instance {1}.",
                                        nameAttr.Value,
                                        GetNodeInformation(node, digestContext)));

                            object value = null;
                            if (!propertyInfo.PropertyType.IsValueType && propertyInfo.CanRead)
                                value = propertyInfo.GetValue(instance, null);
                            value = ProcessNode(child, digestContext, ComponentType.Property, value,
                                propertyInfo.PropertyType);
                            if (propertyInfo.CanWrite)
                                try
                                {
                                    propertyInfo.SetValue(instance, value, null);
                                }
                                catch (Exception ex)
                                {
                                    throw new ApplicationException(
                                        string.Format("Error setting property {0} {1}.", nameAttr.Value,
                                            GetNodeInformation(child, digestContext)), ex);
                                }

                            // Save object property instance and property info for further updates.
                            foreach (XmlNodeInformation info in digestContext.ParserXmlNodeInformation)
                                if (ReferenceEquals(info.Node, child))
                                {
                                    info.Instance = instance;
                                    info.PropertyInfo = propertyInfo;
                                    break;
                                }

                            // Instrumentate property if it's a value type.
                            if (propertyInfo.PropertyType.IsValueType && GetAttribute(child, ValueAttr) != null)
                                InstrumentateProperty(digestContext, instance, propertyInfo, child);

                            break;
                    }
        }

        private static void InstrumentateProperty(DigestContext digestContext, object instance,
            PropertyInfo propertyInfo, XmlNode node)
        {
            XmlAttribute instrAttr = GetAttribute(node, InstrumentateAttr);
            if (instrAttr != null)
            {
                string[] split = instrAttr.Value.Split(CommaDelimiter);
                string opt = string.Empty;
                if (split.Length > 0)
                    // Use the last value only.
                    opt = split[split.Length - 1].ToUpper();
                var ip = new InstrumentedProperty(instance, propertyInfo, node,
                    opt == ReadWriteInstrumentateOption,
                    opt == ReadWriteInstrumentateOption || opt == ReadOnlyInstrumentateOption);
                XmlAttribute descAttr = GetAttribute(node, DescriptionAttr);
                if (descAttr != null)
                    ip.Description = descAttr.Value;
                digestContext.InstrumentedProperties.Add(ip);
            }
        }

        private static void InstrumentateMethod(DigestContext digestContext, object instance, MethodInfo methodInfo,
            XmlNode node, Type[] argsTypes)
        {
            var im = new InstrumentedMethod(instance, methodInfo, node, argsTypes);
            XmlAttribute descAttr = GetAttribute(node, DescriptionAttr);
            if (descAttr != null)
                im.Description = descAttr.Value;
            digestContext.InstrumentedMethods.Add(im);
        }

        #region Nested type: ComponentType
        private enum ComponentType
        {
            RootNode = 0,
            Parameter = 1,
            Declaration = 2,
            Invoke = 3,
            Property = 4
        }
        #endregion
    }
}