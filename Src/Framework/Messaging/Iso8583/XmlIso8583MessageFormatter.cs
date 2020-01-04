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
using System.Xml;

namespace Trx.Messaging.Iso8583
{
    public class XmlIso8583MessageFormatter : XmlMessageFormatter
    {
        /// <summary>
        /// It builds a new messages formatter.
        /// </summary>
        public XmlIso8583MessageFormatter()
        {
        }

        /// <summary>
        /// Creates a message formatter with the definition in file given Xml cofiguration file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        public XmlIso8583MessageFormatter(string xmlFileName)
        {
            ProcessIso8583FormatterDefinition(xmlFileName);
        }

        public override Message NewMessage()
        {
            return new Iso8583Message();
        }

        protected override Message NewMessage(string tag)
        {
            if (tag == XmlRenderConfig.Iso8583MessageTag)
                return new Iso8583Message();

            return base.NewMessage(tag);
        }

        protected override string GetMessageTag()
        {
            return XmlRenderConfig.Iso8583MessageTag;
        }

        protected override string MatchMessageTag(string tag)
        {
            if (tag == XmlRenderConfig.Iso8583MessageTag)
                return XmlRenderConfig.Iso8583MessageTag;

            return base.MatchMessageTag(tag);
        }

        /// <summary>
        /// Called to allow subclasses attribute processing of the message tag.
        /// </summary>
        /// <param name="message">
        /// Message being parsed.
        /// </param>
        /// <param name="reader">
        /// The Xml reader.</param>
        /// <param name="parserContext">
        /// It's the context holding the information to produce a new message instance.
        /// </param>
        /// <param name="xml">
        /// The whole Xml string. Can be null.
        /// </param>
        protected override void OnMessageTag(Message message, XmlTextReader reader, ParserContext parserContext,
            ref string xml)
        {
            if (!(message is Iso8583Message))
            {
                base.OnMessageTag(message, reader, parserContext, ref xml);
                return;
            }

            if (message.Parent != null && !XmlRenderConfig.MtiOnIso8583InnerMessage)
                // Don't need Mti on inner messages.
                return;

            String mti = null;
            while (reader.MoveToNextAttribute())
                if (reader.LocalName == XmlRenderConfig.Iso8583MtiAttr)
                    mti = reader.Value;

            int intMti;
            if (mti == null || !int.TryParse(mti, out intMti))
                throw new MessagingException("Can't parse the message type identifier.");

            ((Iso8583Message) (message)).MessageTypeIdentifier = intMti;

            reader.MoveToElement();
        }

        /// <summary>
        /// It formats a ISO 8583 message.
        /// </summary>
        /// <param name="message">
        /// It's the message to be formatted.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to be used in the format.
        /// </param>
        /// <exception cref="MessagingException">
        /// If the MTI formatter it's unknown.
        /// </exception>
        public override void Format(Message message, ref FormatterContext formatterContext)
        {
            if (!(message is Iso8583Message))
                throw new ArgumentException("Iso8583Message expected.", "message");

            base.Format(message, ref formatterContext);
        }

        protected void ProcessIso8583FormatterDefinition(string xmlFileName)
        {
            const string typeName = "Trx.Messaging.Iso8583.Iso8583MessageFormatter";
            var formatterDefinition = ProcessFormatterDefinition(xmlFileName, typeName) as Iso8583FormatterDefinition;
            if (formatterDefinition == null)
                throw new MessagingException(string.Format("Invalid root object in config file '{0}', " +
                    "an object of type {1} was expected", xmlFileName, typeName));
        }
    }
}