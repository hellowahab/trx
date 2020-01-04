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

namespace Trx.Messaging
{
    [Serializable]
    public class Message : MessagingComponent
    {
        private readonly FieldCollection _fields;

        [NonSerialized] private IMessageFormatter _formatter;
        [NonSerialized] private MessageSecuritySchema _messageSecuritySchema;
        private MessageHeader _header;

        private object _identifier;
        private Message _parent;

        public Message()
        {
            _header = null;
            _fields = new FieldCollection();
            _identifier = null;
            _parent = null;
        }

        public MessageHeader Header
        {
            get { return _header; }

            set { _header = value; }
        }

        public IMessageFormatter Formatter
        {
            get { return _formatter; }

            set { _formatter = value; }
        }

        public MessageSecuritySchema MessageSecuritySchema
        {
            get
            {
                if (_messageSecuritySchema == null && _formatter != null)
                    return _formatter.MessageSecuritySchema;

                return _messageSecuritySchema;
            }

            set { _messageSecuritySchema = value; }
        }

        public FieldCollection Fields
        {
            get { return _fields; }
        }

        public Field this[int fieldNumber]
        {
            get { return _fields[fieldNumber]; }
        }

        public virtual object Identifier
        {
            get { return _identifier; }

            set { _identifier = value; }
        }

        /// <summary>
        /// It returns or sets the parent message.
        /// </summary>
        /// <remarks>
        /// This property is intended to be set by the message formatter.
        /// </remarks>
        public Message Parent
        {
            get { return _parent; }

            set { _parent = value; }
        }

        public virtual void CopyTo(Message message)
        {
            if (_header != null)
                message.Header = (MessageHeader) Header.Clone();

            foreach (Field field in _fields)
                message.Fields.Add((Field) (field.Clone()));
        }

        public virtual void CopyTo(Message message, int[] fieldsNumbers)
        {
            if (_header != null)
                message.Header = (MessageHeader) Header.Clone();

            foreach (int t in fieldsNumbers)
                if (_fields.Contains(t))
                    message.Fields.Add((Field)
                        (_fields[t].Clone()));
        }

        public override object Clone()
        {
            var message = new Message();

            CopyTo(message);

            return message;
        }

        public override byte[] GetBytes()
        {
            byte[] data = null;

            if (_formatter != null)
            {
                var formatterContext = new FormatterContext(
                    FormatterContext.DefaultBufferSize);

                _formatter.Format(this, ref formatterContext);

                data = formatterContext.GetData();
            }

            return data;
        }

        public override string ToString()
        {
            CorrectBitMapsValues();

            var rendered = new StringBuilder();

            bool appended = false;

            if (_header != null)
            {
                rendered.Append("H:");
                rendered.Append(_header.ToString());
                appended = true;
            }

            if (_fields.Count > 0)
            {
                int j = _fields.MaximumFieldNumber;
                for (int i = 0; i <= j; i++)
                {
                    Field field;
                    if ((field = _fields[i]) != null)
                    {
                        if (appended)
                            rendered.Append(",");
                        rendered.Append(i);
                        rendered.Append(":");
                        if (MessageSecuritySchema == null || MessageSecuritySchema.FieldCanBeLogged(i))
                            if (field is InnerMessageField)
                            {
                                rendered.Append("{");
                                rendered.Append(field.ToString());
                                rendered.Append("}");
                            }
                            else
                                rendered.Append(field.ToString());
                        else
                            rendered.Append(MessageSecuritySchema.ObfuscateFieldData(field));
                        appended = true;
                    }
                }
            }

            return rendered.ToString();
        }

        protected void XmlRenderBody(StringBuilder sb, string prefix, string indentation,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (Header != null)
                Header.XmlRender(sb, prefix, indentation, xmlRenderConfig);

            CorrectBitMapsValues();
            if (Fields.Count > 0)
            {
                Field field;
                int j = Fields.MaximumFieldNumber;
                for (int i = 0; i <= j; i++)
                    if ((field = Fields[i]) != null)
                        field.XmlRender(sb, prefix, indentation,
                            _formatter == null ? null : _formatter.MessageSecuritySchema, xmlRenderConfig);
            }
        }

        protected virtual void XmlRenderHeader(StringBuilder sb, string prefix,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("<");
            sb.Append(xmlRenderConfig.MessageTag);
            sb.Append(">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);
        }

        protected virtual void XmlRenderTail(StringBuilder sb, string prefix, MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("</");
            sb.Append(xmlRenderConfig.MessageTag);
            sb.Append(">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);
        }

        public override void XmlRender(StringBuilder sb, string prefix, string indentation,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            XmlRenderHeader(sb, prefix, xmlRenderConfig);
            XmlRenderBody(sb, prefix + indentation, indentation, xmlRenderConfig);
            XmlRenderTail(sb, prefix, xmlRenderConfig);
        }

        public virtual void XmlRenderAsInner(StringBuilder sb, string prefix, string indentation, int fieldNumber,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("<");
            sb.Append(xmlRenderConfig.MessageTag);
            sb.Append(" ");
            sb.Append(xmlRenderConfig.NumberAttr);
            sb.Append("=\"");
            sb.Append(fieldNumber);
            sb.Append("\"");
            sb.Append(">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);

            XmlRenderBody(sb, prefix + indentation, indentation, xmlRenderConfig);
            XmlRenderTail(sb, prefix, xmlRenderConfig);
        }

        public void MergeFields(Message message)
        {
            foreach (Field field in message.Fields)
                _fields.Add(field);
        }

        public void CopyFields(Message message)
        {
            foreach (Field field in message.Fields)
                _fields.Add((Field) field.Clone());
        }

        public virtual void CorrectBitMapsValues()
        {
            Field field;

            if (_fields.Count == 0 || !_fields.Dirty)
                return;

            for (int i = 0; i < _fields.MaximumFieldNumber; i++)
                if (((field = _fields[i]) != null) &&
                    (field is BitMapField))
                {
                    var bitMap = (BitMapField) field;
                    bitMap.Clear();
                    for (int j = bitMap.LowerFieldNumber;
                        (j <= bitMap.UpperFieldNumber) &&
                            (j <= _fields.MaximumFieldNumber);
                        j++)
                        bitMap[j] = _fields.Contains(j);
                }

            _fields.Dirty = false;
        }

        public override MessagingComponent NewComponent()
        {
            return new Message();
        }
    }
}