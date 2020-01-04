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

namespace Trx.Messaging
{
    /// <summary>
    /// This class contains some properties to configure the Xml dump of <see cref="MessagingComponent"/> objects.
    /// </summary>
    public class MessagingComponentXmlRenderConfig : ICloneable
    {
        public const string XmlMessageTag = "Message";
        public const string XmlFieldTag = "Field";
        public const string XmlHeaderTag = "Header";
        public const string XmlBitMapTag = "Bitmap";
        public const string XmlNumberAttr = "Number";
        public const string XmlValueAttr = "Value";
        public const string XmlTypeAttr = "Type";
        public const string XmlStringVal = "string";
        public const string XmlBinaryVal = "binary";
        public const string XmlBitMapVal = "bitmap";
        public const string XmlComponentVal = "component";
        public const string XmlIso8583MessageTag = "Iso8583Message";
        public const string XmlIso8583MtiAttr = "Mti";

        private string _binaryVal;
        private string _bitMapTag;
        private string _bitMapVal;
        private Dictionary<string, bool> _boolProperties;
        private string _componentVal;
        private string _fieldTag;
        private string _headerTag;
        private string _iso8583MessageTag;
        private string _iso8583MtiAttr;
        private string _messageTag;
        private string _numberAttr;
        private Dictionary<string, string> _stringProperties;
        private string _stringVal;
        private string _typeAttr;
        private string _valueAttr;

        public MessagingComponentXmlRenderConfig()
        {
            // Set default values.
            _messageTag = XmlMessageTag;
            _fieldTag = XmlFieldTag;
            _headerTag = XmlHeaderTag;
            _bitMapTag = XmlBitMapTag;
            _numberAttr = XmlNumberAttr;
            _valueAttr = XmlValueAttr;
            _typeAttr = XmlTypeAttr;
            _stringVal = XmlStringVal;
            _binaryVal = XmlBinaryVal;
            _bitMapVal = XmlBitMapVal;
            _componentVal = XmlComponentVal;
            _iso8583MessageTag = XmlIso8583MessageTag;
            _iso8583MtiAttr = XmlIso8583MtiAttr;

            Indent = true;
            PrefixHexString = true;
            IncludeTypeForStringField = true;
            IncludeTypeForStringHeader = true;
            IncludeXmlDeclaration = false;
            ObfuscateFieldData = true;
        }

        internal Dictionary<string, bool> BoolProperties
        {
            get { return _boolProperties; }
            set { _boolProperties = value; }
        }

        internal Dictionary<string, string> StringProperties
        {
            get { return _stringProperties; }
            set { _stringProperties = value; }
        }

        public string MessageTag
        {
            get { return _messageTag; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _messageTag = value;
            }
        }

        public string FieldTag
        {
            get { return _fieldTag; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _fieldTag = value;
            }
        }

        public string HeaderTag
        {
            get { return _headerTag; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _headerTag = value;
            }
        }

        public string BitMapTag
        {
            get { return _bitMapTag; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _bitMapTag = value;
            }
        }

        public string NumberAttr
        {
            get { return _numberAttr; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _numberAttr = value;
            }
        }

        public string ValueAttr
        {
            get { return _valueAttr; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _valueAttr = value;
            }
        }

        public string TypeAttr
        {
            get { return _typeAttr; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _typeAttr = value;
            }
        }

        public string StringVal
        {
            get { return _stringVal; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _stringVal = value;
            }
        }

        public string BinaryVal
        {
            get { return _binaryVal; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _binaryVal = value;
            }
        }

        public string BitMapVal
        {
            get { return _bitMapVal; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _bitMapVal = value;
            }
        }

        public string ComponentVal
        {
            get { return _componentVal; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _componentVal = value;
            }
        }

        public string Iso8583MessageTag
        {
            get { return _iso8583MessageTag; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _iso8583MessageTag = value;
            }
        }

        public string Iso8583MtiAttr
        {
            get { return _iso8583MtiAttr; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _iso8583MtiAttr = value;
            }
        }

        /// <summary>
        /// Enable or disable Xml indentation (new line & line prefix).
        /// </summary>
        public bool Indent { get; set; }

        /// <summary>
        /// Enable or disable prefixing hex string dump with 0x.
        /// </summary>
        public bool PrefixHexString { get; set; }

        /// <summary>
        /// If true, the type is specified for <see cref="StringField"/> fields types.
        /// </summary>
        public bool IncludeTypeForStringField { get; set; }

        /// <summary>
        /// If true, the field value goes in the inner Xml of the element.
        /// </summary>
        public bool FieldValueInContent { get; set; }

        /// <summary>
        /// If true, the field value goes in the inner Xml of the element.
        /// </summary>
        public bool HeaderValueInContent { get; set; }

        /// <summary>
        /// If true, the type is specified for <see cref="StringMessageHeader"/> header types.
        /// </summary>
        public bool IncludeTypeForStringHeader { get; set; }

        public bool IncludeXmlDeclaration { get; set; }

        public bool ObfuscateFieldData { get; set; }

        public bool MtiOnIso8583InnerMessage { get; set; }

        public bool HeaderValueInHex { get; set; }

        #region ICloneable Members
        public object Clone()
        {
            var clone = new MessagingComponentXmlRenderConfig
                            {
                                MessageTag = MessageTag,
                                FieldTag = FieldTag,
                                HeaderTag = HeaderTag,
                                BitMapTag = BitMapTag,
                                NumberAttr = NumberAttr,
                                ValueAttr = ValueAttr,
                                TypeAttr = TypeAttr,
                                StringVal = StringVal,
                                BinaryVal = BinaryVal,
                                BitMapVal = BitMapVal,
                                ComponentVal = ComponentVal,
                                Iso8583MessageTag = Iso8583MessageTag,
                                Iso8583MtiAttr = Iso8583MtiAttr,
                                Indent = Indent,
                                PrefixHexString = PrefixHexString,
                                IncludeTypeForStringField = IncludeTypeForStringField,
                                FieldValueInContent = FieldValueInContent,
                                HeaderValueInContent = HeaderValueInContent,
                                IncludeTypeForStringHeader = IncludeTypeForStringHeader,
                                IncludeXmlDeclaration = IncludeXmlDeclaration,
                                ObfuscateFieldData = ObfuscateFieldData,
                                MtiOnIso8583InnerMessage = MtiOnIso8583InnerMessage,
                                HeaderValueInHex = HeaderValueInHex
                            };

            if (_stringProperties != null)
            {
                clone.StringProperties = new Dictionary<string, string>();
                foreach (var item in _stringProperties)
                    clone.StringProperties.Add(item.Key, item.Value);
            }

            if (_boolProperties != null)
            {
                clone.BoolProperties = new Dictionary<string, bool>();
                foreach (var item in _boolProperties)
                    clone.BoolProperties.Add(item.Key, item.Value);
            }

            return clone;
        }
        #endregion

        private string GetPropertyKey(string name, string domain)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (domain == null)
                throw new ArgumentNullException("domain");

            return string.Format("{0}:{1}", domain, name);
        }

        public void SetBoolProperty(string name, string domain, bool value)
        {
            string key = GetPropertyKey(domain, name);
            if (_boolProperties == null)
                _boolProperties = new Dictionary<string, bool> {{key, value}};
            else if (_boolProperties.ContainsKey(key))
                _boolProperties[key] = value;
            else
                _boolProperties.Add(key, value);
        }

        public bool GetBoolProperty(string name, string domain)
        {
            string key = GetPropertyKey(domain, name);
            if (_boolProperties == null)
                return false;

            return _boolProperties.ContainsKey(key) && _boolProperties[key];
        }

        public void SetStringProperty(string name, string domain, string value)
        {
            string key = GetPropertyKey(domain, name);

            if (_stringProperties == null)
            {
                if (value == null)
                    return;
                _stringProperties = new Dictionary<string, string> {{key, value}};
            }
            else if (_boolProperties.ContainsKey(key))
                if (value == null)
                    _stringProperties.Remove(key);
                else
                    _stringProperties[key] = value;
            else if (value != null)
                _stringProperties.Add(key, value);
        }

        public string GetStringProperty(string name, string domain)
        {
            string key = GetPropertyKey(domain, name);
            if (_stringProperties == null)
                return null;

            return _stringProperties.ContainsKey(key) ? _stringProperties[key] : null;
        }
    }
}