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
    /// <summary>
    /// This class represents a string message header.
    /// </summary>
    [Serializable]
    public class StringMessageHeader : MessageHeader
    {
        private string _value;

        public StringMessageHeader()
        {
        }

        public StringMessageHeader(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            if (_value == null)
                return string.Empty;

            return _value;
        }

        public override byte[] GetBytes()
        {
            if (_value == null)
                return null;

            return FrameworkEncoding.GetInstance().Encoding.GetBytes(_value);
        }

        public override object Clone()
        {
            if (_value == null)
                return new StringMessageHeader();

            return new StringMessageHeader(string.Copy(_value));
        }

        public override MessagingComponent NewComponent()
        {
            return new StringMessageHeader();
        }

        public override void XmlRender(StringBuilder sb, string prefix, string indentation,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("<");
            sb.Append(xmlRenderConfig.HeaderTag);

            if (xmlRenderConfig.IncludeTypeForStringHeader)
            {
                sb.Append(" ");
                sb.Append(xmlRenderConfig.TypeAttr);
                sb.Append("=\"");
                sb.Append(xmlRenderConfig.StringVal);
                sb.Append("\"");
            }

            if (xmlRenderConfig.HeaderValueInContent)
            {
                sb.Append(">");
                sb.Append(xmlRenderConfig.HeaderValueInHex
                    ? StringUtilities.ToHexString(ToString())
                    : StringUtilities.EncodeElementValueXml(ToString()));
                sb.Append("</");
                sb.Append(xmlRenderConfig.HeaderTag);
                sb.Append(">");
            }
            else
            {
                sb.Append(" ");
                sb.Append(xmlRenderConfig.ValueAttr);
                sb.Append("=\"");
                sb.Append(StringUtilities.EncodeAttrValueXml(ToString(), StringUtilities.XmlQuot));
                sb.Append("\" />");
            }

            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);
        }
    }
}