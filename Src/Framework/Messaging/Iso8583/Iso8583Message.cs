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
using System.Globalization;
using System.Text;

namespace Trx.Messaging.Iso8583
{
    /// <summary>
    /// It defines an ISO 8583 message.
    /// </summary>
    [Serializable]
    public class Iso8583Message : Message
    {
        private int _mti;

        /// <summary>
        /// It initializes a new ISO 8583 message.
        /// </summary>
        public Iso8583Message()
        {
            _mti = int.MinValue;
        }

        /// <summary>
        /// It initializes a new ISO 8583 message.
        /// </summary>
        /// <param name="messageTypeIdentifier">
        /// It's the message type identifier.
        /// </param>
        public Iso8583Message(int messageTypeIdentifier)
        {
            _mti = messageTypeIdentifier;
        }

        /// <summary>
        /// It returns or sets the message type identifier.
        /// </summary>
        public int MessageTypeIdentifier
        {
            get { return _mti; }

            set { _mti = value; }
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a request.
        /// </summary>
        /// <returns>
        /// true if the message is a request, otherwise false.
        /// </returns>
        public bool IsRequest()
        {
            return ((_mti/10)%2) == 0;
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's an advice.
        /// </summary>
        /// <returns>
        /// true if the message is an advice, otherwise false.
        /// </returns>
        public bool IsAdvice()
        {
            return (((_mti%100)/20) == 1) || (((_mti%100)/40) == 1);
        }

        /// <summary>
        /// If the message is a request, the MTI is changed to be a response.
        /// </summary>
        /// <exception cref="MessagingException">
        /// If the message isn't a request.
        /// </exception>
        public void SetResponseMessageTypeIdentifier()
        {
            if (!IsRequest())
                throw new MessagingException("Can't set MTI as response because message isn't a request.");

            if ((_mti%2) == 1)
                _mti--;

            _mti += 10;
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's an authorization.
        /// </summary>
        /// <returns>
        /// true if the message is an authorization, otherwise false.
        /// </returns>
        public bool IsAuthorization()
        {
            return (((_mti%1000)/100) == 1) && (((_mti%1000)%100) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a financial message.
        /// </summary>
        /// <returns>
        /// true if the message is financial, otherwise false.
        /// </returns>
        public bool IsFinancial()
        {
            return (((_mti%1000)/200) == 1) && (((_mti%1000)%200) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a file action message.
        /// </summary>
        /// <returns>
        /// true if the message a file action message, otherwise false.
        /// </returns>
        public bool IsFileAction()
        {
            return (((_mti%1000)/300) == 1) && (((_mti%1000)%300) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a reversal or chargeback.
        /// </summary>
        /// <returns>
        /// true if the message a reversal or chargeback, otherwise false.
        /// </returns>
        public bool IsReversalOrChargeBack()
        {
            return (((_mti%1000)/400) == 1) && (((_mti%1000)%400) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a reconciliation.
        /// </summary>
        /// <returns>
        /// true if the message a reconciliation, otherwise false.
        /// </returns>
        public bool IsReconciliation()
        {
            return (((_mti%1000)/500) == 1) && (((_mti%1000)%500) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's an administrative message.
        /// </summary>
        /// <returns>
        /// true if the message an administrative message, otherwise false.
        /// </returns>
        public bool IsAdministrative()
        {
            return (((_mti%1000)/600) == 1) && (((_mti%1000)%600) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a fee collection message.
        /// </summary>
        /// <returns>
        /// true if the message a fee collection message, otherwise false.
        /// </returns>
        public bool IsFeeCollection()
        {
            return (((_mti%1000)/700) == 1) && (((_mti%1000)%700) < 100);
        }

        /// <summary>
        /// It checks the MTI to inform if the message it's a network management message.
        /// </summary>
        /// <returns>
        /// true if the message a network management message, otherwise false.
        /// </returns>
        public bool IsNetworkManagement()
        {
            return (((_mti%1000)/800) == 1) && (((_mti%1000)%800) < 100);
        }

        /// <summary>
        /// It returns a string representation of the ISO 8583 message.
        /// </summary>
        /// <returns>
        /// A string representation of the ISO 8583 message.
        /// </returns>
        public override string ToString()
        {
            CorrectBitMapsValues();

            var rendered = new StringBuilder();

            if (Header != null)
            {
                rendered.Append("H:");
                rendered.Append(Header.ToString());
                rendered.Append(",");
            }

            rendered.Append("M:");
            rendered.Append(_mti.ToString(CultureInfo.InvariantCulture));

            if (Fields.Count > 0)
            {
                int j = Fields.MaximumFieldNumber;
                for (int i = 0; i <= j; i++)
                {
                    Field field;
                    if ((field = Fields[i]) != null)
                    {
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
                    }
                }
            }

            return rendered.ToString();
        }

        /// <summary>
        /// It clones the ISO 8583 message instance.
        /// </summary>
        /// <returns>
        /// A clone of the message instance.
        /// </returns>
        public override object Clone()
        {
            var clon = new Iso8583Message(_mti);
            CopyTo(clon);

            return clon;
        }

        /// <summary>
        /// It copies the message instance data into the provided message.
        /// </summary>
        /// <param name="message">
        /// It's the message where the message instance data is copied.
        /// </param>
        public override void CopyTo(Message message)
        {
            if (message is Iso8583Message)
                ((Iso8583Message) message).MessageTypeIdentifier = _mti;

            base.CopyTo(message);
        }

        /// <summary>
        /// It copies the message instance data and the specified fields into
        /// the provided message.
        /// </summary>
        /// <param name="message">
        /// It's the message where the message instance data is copied.
        /// </param>
        /// <param name="fieldsNumbers">
        /// The fields numbers to be copied into the provided message.
        /// </param>
        public override void CopyTo(Message message, int[] fieldsNumbers)
        {
            if (message is Iso8583Message)
                ((Iso8583Message) message).MessageTypeIdentifier = _mti;

            base.CopyTo(message, fieldsNumbers);
        }

        protected override void XmlRenderHeader(StringBuilder sb, string prefix,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("<");
            sb.Append(xmlRenderConfig.Iso8583MessageTag);
            sb.Append(" ");
            sb.Append(xmlRenderConfig.Iso8583MtiAttr);
            sb.Append("=\"");
            sb.Append(MessageTypeIdentifier);
            sb.Append("\">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);
        }

        protected override void XmlRenderTail(StringBuilder sb, string prefix,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("</");
            sb.Append(xmlRenderConfig.Iso8583MessageTag);
            sb.Append(">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);
        }

        public override void XmlRenderAsInner(StringBuilder sb, string prefix, string indentation, int fieldNumber,
            MessagingComponentXmlRenderConfig xmlRenderConfig)
        {
            if (xmlRenderConfig.Indent)
                sb.Append(prefix);
            sb.Append("<");
            sb.Append(xmlRenderConfig.Iso8583MessageTag);
            if (xmlRenderConfig.MtiOnIso8583InnerMessage)
            {
                sb.Append(" ");
                sb.Append(xmlRenderConfig.Iso8583MtiAttr);
                sb.Append("=\"");
                sb.Append(MessageTypeIdentifier);
                sb.Append("\"");
            }
            sb.Append(" ");
            sb.Append(xmlRenderConfig.NumberAttr);
            sb.Append("=\"");
            sb.Append(fieldNumber);
            sb.Append("\">");
            if (xmlRenderConfig.Indent)
                sb.Append(Environment.NewLine);

            XmlRenderBody(sb, prefix + indentation, indentation, xmlRenderConfig);
            XmlRenderTail(sb, prefix, xmlRenderConfig);
        }

        /// <summary>
        /// It builds a new component of <see cref="Iso8583Message"/> type.
        /// </summary>
        /// <returns>
        /// A new ISO 8583 message.
        /// </returns>
        public override MessagingComponent NewComponent()
        {
            return new Iso8583Message();
        }
    }
}