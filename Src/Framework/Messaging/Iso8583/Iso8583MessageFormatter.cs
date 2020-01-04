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

namespace Trx.Messaging.Iso8583
{
    /// <summary>
    /// It implements an ISO 8583 messages formatter.
    /// </summary>
    public class Iso8583MessageFormatter : BasicMessageFormatter
    {
        private readonly StringField _mtiField;
        private StringFieldFormatter _mtiFormatter;

        /// <summary>
        /// It initializes a new ISO 8583 formatter.
        /// </summary>
        public Iso8583MessageFormatter()
        {
            _mtiFormatter = null;
            _mtiField = new StringField(-1);
        }

        /// <summary>
        /// Creates an ISO 8583 message formatter with the definition in a Xml cofiguration file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        public Iso8583MessageFormatter(string xmlFileName) : this()
        {
            ProcessIso8583FormatterDefinition(xmlFileName);
        }

        /// <summary>
        /// It returns or sets the message type identifier formatter.
        /// </summary>
        public StringFieldFormatter MessageTypeIdentifierFormatter
        {
            get { return _mtiFormatter; }
            set { _mtiFormatter = value; }
        }

        /// <summary>
        /// It builds a new ISO 8583 message.
        /// </summary>
        /// <returns>
        /// A new ISO 8583 message.
        /// </returns>
        public override Message NewMessage()
        {
            return new Iso8583Message();
        }

        /// <summary>
        /// It formats the MTI.
        /// </summary>
        /// <param name="message">
        /// The message to be formatted.
        /// </param>
        /// <param name="formatterContext">
        /// The formatter context.
        /// </param>
        protected override void BeforeFieldsFormatting(Message message, ref FormatterContext formatterContext)
        {
            _mtiField.Value = Convert.ToString(((Iso8583Message)
                (message)).MessageTypeIdentifier, CultureInfo.InvariantCulture);
            _mtiFormatter.Format(_mtiField, ref formatterContext);
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
            if (_mtiFormatter == null)
                throw new MessagingException("Message type identifier formatter isn't set.");

            if (!(message is Iso8583Message))
                throw new ArgumentException("Iso8583Message expected.", "message");

            base.Format(message, ref formatterContext);
        }

        /// <summary>
        /// It parses the MTI.
        /// </summary>
        /// <param name="message">
        /// The message to be parsed.
        /// </param>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// true if the MTI was parsed, otherwise false.
        /// </returns>
        protected override bool BeforeFieldsParsing(Message message,
            ref ParserContext parserContext)
        {
            Field mti = _mtiFormatter.Parse(ref parserContext);

            if (mti == null)
                return false;

            try
            {
                ((Iso8583Message) (message)).MessageTypeIdentifier =
                    Convert.ToInt32(mti.Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new MessagingException("Can't parse the message type identifier.");
            }

            return true;
        }

        /// <summary>
        /// Parses the data in the parser context and builds a new ISO 8583 message.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// A new ISO 8583 message if the data was parsed correctly, otherwise null.
        /// </returns>
        /// <exception cref="MessagingException">
        /// If the MTI formatter it's unknown.
        /// </exception>
        public override Message Parse(ref ParserContext parserContext)
        {
            if (_mtiFormatter == null)
                throw new MessagingException("Message type identifier formatter isn't set.");

            return base.Parse(ref parserContext);
        }

        /// <summary>
        /// It copies the message formatter instance data into the provided message formatter.
        /// </summary>
        /// <param name="messageFormatter">
        /// It's the message formatter where the message formatter instance data is copied.
        /// </param>
        /// <remarks>
        /// The header, the mti formatter and the fields formatters, aren't cloned,
        /// the new instance and the original shares those object instances.
        /// </remarks>
        public override void CopyTo(BaseMessageFormatter messageFormatter)
        {
            base.CopyTo(messageFormatter);

            if (messageFormatter is Iso8583MessageFormatter)
            {
                messageFormatter.MessageHeaderFormatter = MessageHeaderFormatter;
                ((Iso8583MessageFormatter) (messageFormatter)).MessageTypeIdentifierFormatter = _mtiFormatter;
            }
        }

        /// <summary>
        /// It clones the formatter instance.
        /// </summary>
        /// <remarks>
        /// The header, the mti formatter and the fields formatters, aren't cloned,
        /// the new instance and the original shares those object instances.
        /// </remarks>
        /// <returns>
        /// A new instance of the formatter.
        /// </returns>
        public override object Clone()
        {
            var formatter = new Iso8583MessageFormatter();
            CopyTo(formatter);
            return formatter;
        }

        protected void ProcessIso8583FormatterDefinition(string xmlFileName)
        {
            const string typeName = "Trx.Messaging.Iso8583.Iso8583MessageFormatter";
            var formatterDefinition = ProcessFormatterDefinition(xmlFileName, typeName) as Iso8583FormatterDefinition;
            if (formatterDefinition == null)
                throw new MessagingException(string.Format("Invalid root object in config file '{0}', " +
                    "an object of type {1} was expected", xmlFileName, typeName));

            if (formatterDefinition.ClearMessageTypeIdentifierFormatter)
                MessageTypeIdentifierFormatter = null;

            if (formatterDefinition.MessageTypeIdentifierFormatter != null)
                MessageTypeIdentifierFormatter =
                    formatterDefinition.MessageTypeIdentifierFormatter.GetInstance() as StringFieldFormatter;
        }
    }
}