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
using System.Text;
using Trx.Utilities;

namespace Trx.Messaging
{
    [Serializable]
    public abstract class Field : MessagingComponent
    {
        private int _fieldNumber;

        protected Field(int fieldNumber)
        {
            _fieldNumber = fieldNumber;
        }

        public int FieldNumber
        {
            get { return _fieldNumber; }
        }

        public abstract object Value { get; set; }

        internal void SetFieldNumber(int fieldNumber)
        {
            _fieldNumber = fieldNumber;
        }

        public override void XmlRender(StringBuilder sb, string prefix, string indentation,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            XmlRender(sb, prefix, indentation, null, xmlRenderConfig);
        }

        public virtual void XmlRender(StringBuilder sb, string prefix, string indentation,
            MessageSecuritySchema messageSecuritySchema, MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            string type = null;
            bool includeType = true;
            if (this is StringField)
            {
                type = xmlRenderConfig.StringVal;
                includeType = xmlRenderConfig.IncludeTypeForStringField;
            }
            else if (this is BitMapField)
                type = xmlRenderConfig.BitMapVal;
            else if (this is BinaryField)
                type = xmlRenderConfig.BinaryVal;
            else if (this is InnerMessageField && Value != null)
                ((Message)(Value)).XmlRenderAsInner(sb, prefix, indentation, FieldNumber, xmlRenderConfig);
            else if (Value is MessagingComponent)
            {
                if (xmlRenderConfig.Indent)
                    sb.Append(prefix);
                sb.Append("<");
                sb.Append(xmlRenderConfig.FieldTag);
                sb.Append(" ");
                sb.Append(xmlRenderConfig.NumberAttr);
                sb.Append("=\"");
                sb.Append(FieldNumber);
                sb.Append("\" ");
                sb.Append(xmlRenderConfig.TypeAttr);
                sb.Append("=\"");
                sb.Append(xmlRenderConfig.ComponentVal);
                sb.Append("\">");
                if (xmlRenderConfig.Indent)
                    sb.Append(Environment.NewLine);
                ((MessagingComponent)(Value)).XmlRender(sb, prefix + indentation, indentation);
                if (xmlRenderConfig.Indent)
                    sb.Append(prefix);
                sb.Append("</");
                sb.Append(xmlRenderConfig.FieldTag);
                sb.Append(">");
                if (xmlRenderConfig.Indent)
                    sb.Append(Environment.NewLine);
            }

            if (type != null)
            {
                if (xmlRenderConfig.Indent)
                    sb.Append(prefix);
                sb.Append("<");
                sb.Append(xmlRenderConfig.FieldTag);
                sb.Append(" ");
                sb.Append(xmlRenderConfig.NumberAttr);
                sb.Append("=\"");
                sb.Append(FieldNumber);
                sb.Append("\"");
                if (includeType)
                {
                    sb.Append(" ");
                    sb.Append(xmlRenderConfig.TypeAttr);
                    sb.Append("=\"");
                    sb.Append(type);
                    sb.Append("\"");
                }
                if (xmlRenderConfig.FieldValueInContent)
                    sb.Append(">");
                else
                {
                    sb.Append(" ");
                    sb.Append(xmlRenderConfig.ValueAttr);
                    sb.Append("=\"");
                }
                if (!xmlRenderConfig.ObfuscateFieldData || messageSecuritySchema == null ||
                    messageSecuritySchema.FieldCanBeLogged(FieldNumber))
                    if (this is StringField || this is BitMapField)
                        sb.Append(StringUtilities.EncodeAttrValueXml(ToString(), StringUtilities.XmlQuot));
                    else
                    {
                        if (xmlRenderConfig.PrefixHexString)
                            sb.Append("0x");
                        sb.Append(StringUtilities.ToHexString(GetBytes()));
                    }
                else
                    sb.Append(StringUtilities.EncodeAttrValueXml(messageSecuritySchema.ObfuscateFieldData(this),
                        StringUtilities.XmlQuot));

                if (xmlRenderConfig.FieldValueInContent)
                {
                    sb.Append("</");
                    sb.Append(xmlRenderConfig.FieldTag);
                    sb.Append(">");
                }
                else
                    sb.Append("\" />");
                if (xmlRenderConfig.Indent)
                    sb.Append(Environment.NewLine);
            }
        }
    }
}