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
using System.IO;
using System.Text;
using System.Xml;
using Trx.Buffer;
using Trx.Exceptions;
using Trx.Utilities;

namespace Trx.Messaging
{
    /// <summary>
    /// Xml message formatter implementation.
    /// </summary>
    /// <remarks>
    /// Because <see cref="XmlReader"/> throws an exception when the input buffer EOF, on the
    /// fly deformatting is implemented catching exceptions thrown by the reader, this isn't
    /// ideally but it's the only way we found using .Net Framework Xml parsers.
    /// 
    /// To minimize exception noise, we recomend to use a frame delimiting in the pipeline.
    /// </remarks>
    public class XmlMessageFormatter : BaseMessageFormatter
    {
        private MessagingComponentXmlRenderConfig _xmlRenderConfig =
            MessagingComponent.XmlRenderConfig.Clone() as MessagingComponentXmlRenderConfig;

        /// <summary>
        /// It builds a new messages formatter.
        /// </summary>
        public XmlMessageFormatter()
        {
            _xmlRenderConfig.ObfuscateFieldData = false;
        }

        /// <summary>
        /// Creates a message formatter with the definition in file given Xml cofiguration file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        public XmlMessageFormatter(string xmlFileName)
            : base(xmlFileName, "Trx.Messaging.FormatterDefinition")
        {
            _xmlRenderConfig.ObfuscateFieldData = false;
        }

        public MessagingComponentXmlRenderConfig XmlRenderConfig
        {
            get { return _xmlRenderConfig; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _xmlRenderConfig = value;
            }
        }

        public override object Clone()
        {
            var formatter = new XmlMessageFormatter();
            CopyTo(formatter);
            return formatter;
        }

        /// <summary>
        /// It formats a message.
        /// </summary>
        /// <param name="message">
        /// It's the message to be formatted.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to be used in the format.
        /// </param>
        public override void Format(Message message, ref FormatterContext formatterContext)
        {
            var sb = new StringBuilder();
            if (_xmlRenderConfig.IncludeXmlDeclaration)
            {
                sb.Append("<?xml version=\"1.0\" encoding=\"");
                sb.Append(formatterContext.Buffer.Encoding.HeaderName);
                sb.Append("\" ?>");
                if (_xmlRenderConfig.Indent)
                    sb.Append(Environment.NewLine);
            }
            message.XmlRender(sb, string.Empty, "   ", _xmlRenderConfig);
            formatterContext.Write(sb.ToString());
        }

        /// <summary>
        /// Compute the index position within the Xml from the current reader line number and line position.
        /// </summary>
        /// <param name="lineNumber">
        /// Reader line number in the input stream.
        /// </param>
        /// <param name="linePosition">
        /// Reader line position in the input stream.
        /// </param>
        /// <param name="xml">
        /// The Xml text document.
        /// </param>
        /// <returns>
        /// The translated position.
        /// </returns>
        private int GetPositionInXml(int lineNumber, int linePosition, string xml)
        {
            int idx = 0;
            while (--lineNumber > 0)
            {
                // Get first \r or \n
                idx = xml.IndexOfAny(StringUtilities.NewLineChars, idx) + 1;
                // Check if we have a \r\n
                if (xml[idx - 1] == '\r' && xml.Length > idx && xml[idx] == '\n')
                    idx++;
            }

            return idx + linePosition - 1;
        }

        private int GetXmlLength(byte[] data, XmlTextReader reader, Encoding encoding, string xml)
        {
            if (xml == null)
                xml = encoding.GetString(data);

            int idx = GetPositionInXml(reader.LineNumber, reader.LinePosition, xml);
            for (; idx < xml.Length; idx++)
                if (xml[idx] == '>')
                    break;

            return encoding.GetByteCount(xml.Substring(0, ++idx));
        }

        protected virtual string GetMessageTag()
        {
            return _xmlRenderConfig.MessageTag;
        }

        protected virtual string MatchMessageTag(string tag)
        {
            if (tag == _xmlRenderConfig.MessageTag)
                return _xmlRenderConfig.MessageTag;

            return null;
        }

        protected virtual Message NewMessage(string tag)
        {
            if (tag == _xmlRenderConfig.MessageTag)
                return new Message();

            throw new BugException();
        }

        /// <summary>
        /// Called to allow subclasses attribute processing of the message tag.
        /// </summary>
        /// <param name="message">
        /// Message being parsed.
        /// </param>
        /// <param name="reader">
        /// The Xml reader.
        /// </param>
        /// <param name="parserContext">
        /// It's the context holding the information to produce a new message instance.
        /// </param>
        /// <param name="xml">
        /// The whole Xml string. Can be null.
        /// </param>
        protected virtual void OnMessageTag(Message message, XmlTextReader reader, ParserContext parserContext,
            ref string xml)
        {
        }

        private void ParseMessage(Message message, XmlTextReader reader, ParserContext parserContext,
            ref string xml, Encoding encoding)
        {
            while (reader.NodeType != XmlNodeType.Element)
                if (!reader.Read())
                    break;

            string messageTag = MatchMessageTag(reader.LocalName);
            if (messageTag == null)
                throw new MessagingException(
                    string.Format("'{0}' (or one of its superclasses) node element expected, got '{1}'.",
                        GetMessageTag(), reader.LocalName));

            OnMessageTag(message, reader, parserContext, ref xml);

            bool loop = reader.Read();
            while (loop)
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        int startLineNumber = reader.LineNumber;
                        int startLinePosition = reader.LinePosition;

                        string tag = reader.LocalName;
                        String type = null;
                        String value = null;
                        String number = null;

                        while (reader.MoveToNextAttribute())
                            if (reader.LocalName == _xmlRenderConfig.TypeAttr)
                                type = reader.Value;
                            else if (reader.LocalName == _xmlRenderConfig.ValueAttr)
                                value = reader.Value;
                            else if (reader.LocalName == _xmlRenderConfig.NumberAttr)
                                number = reader.Value;
                        reader.MoveToElement();

                        messageTag = MatchMessageTag(tag);
                        if (messageTag != null)
                        {
                            int fldNumber;
                            if (string.IsNullOrEmpty(number) || !int.TryParse(number, out fldNumber))
                                throw new MessagingException(
                                    string.Format("Invalid inner field number (provided: '{0}').",
                                        string.IsNullOrEmpty(number) ? "[empty value]" : number));

                            Message innerMessage = NewMessage(messageTag);
                            innerMessage.Formatter = this;
                            innerMessage.Parent = message;
                            ParseMessage(innerMessage, reader, parserContext, ref xml, encoding);
                            message.Fields.Add(fldNumber, innerMessage);
                            reader.Skip();
                            continue;
                        }

                        if (string.IsNullOrEmpty(value))
                            value = reader.ReadElementContentAsString();
                        else
                            reader.Skip();

                        if (tag == _xmlRenderConfig.HeaderTag)
                        {
                            if (!string.IsNullOrEmpty(value))
                                if (string.IsNullOrEmpty(type) || type == _xmlRenderConfig.StringVal)
                                    message.Header =
                                        new StringMessageHeader(_xmlRenderConfig.HeaderValueInHex
                                            ? StringUtilities.FromHexStringToString(value)
                                            : value);
                                else
                                    throw new MessagingException(string.Format("'{0}' header type isn't supported.",
                                        type));
                        }
                        else if (tag == _xmlRenderConfig.FieldTag)
                        {
                            int fldNumber;
                            if (string.IsNullOrEmpty(number) || !int.TryParse(number, out fldNumber))
                                throw new MessagingException(string.Format("Invalid field number (provided: '{0}').",
                                    string.IsNullOrEmpty(number) ? "[empty value]" : number));

                            if (string.IsNullOrEmpty(type) || type == _xmlRenderConfig.StringVal)
                                // Type not specified or string.
                                message.Fields.Add(fldNumber, value);
                            else if (type == _xmlRenderConfig.BinaryVal)
                                // Binary field.
                                if (value == null)
                                    message.Fields.Add(new BinaryField(fldNumber));
                                else if (value.Length >= 2 &&
                                    (value.Substring(0, 2) == "0x" || value.Substring(0, 2) == "0X"))
                                    message.Fields.Add(fldNumber,
                                        StringUtilities.FromHexStringToByte(value.Substring(2)));
                                else
                                    message.Fields.Add(fldNumber, StringUtilities.FromHexStringToByte(value));

                            if (Logger.IsDebugEnabled() && MessageSecuritySchema != null && !MessageSecuritySchema.FieldCanBeLogged(fldNumber))
                            {
                                // Field obfuscation computed only for Debug wich is the mode where the framework dumps the buffer
                                if (xml == null)
                                    xml = parserContext.GetDataAsString(false);
                                int start = GetPositionInXml(startLineNumber, startLinePosition, xml);
                                int end = GetPositionInXml(reader.LineNumber, reader.LinePosition, xml);
                                int len =
                                    encoding.GetByteCount(xml.Substring(--start,
                                        end - (xml[end - 1] == '/' ? 3 : 2) - start));
                                start += parserContext.Buffer.LowerDataBound;
                                parserContext.Buffer.AddSecureArea(new BufferSecureArea(start, start + len));
                            }
                        }
                        continue;

                    case XmlNodeType.EndElement:
                        loop = false;
                        break;

                    default:
                        reader.Skip();
                        break;
                }
        }

        /// <summary>
        /// It parses the data contained in the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// It's the context holding the information to produce a new message instance.
        /// </param>
        /// <returns>
        /// The parsed message, or a null reference if the data contained in the context
        /// is insufficient to produce a new message.
        /// </returns>
        public override Message Parse(ref ParserContext parserContext)
        {
            if (parserContext.Buffer.DataLength == 0)
                return null;

            BufferWrapperStream bws = parserContext.FrameSize > 0
                ? new BufferWrapperStream(parserContext.Buffer, parserContext.FrameSize) // The size is delimited.
                : new BufferWrapperStream(parserContext.Buffer); // Unknown length, try to determine it.

            using (var reader = new XmlTextReader(bws) {WhitespaceHandling = WhitespaceHandling.None})
            {
                Message message = NewMessage();
                message.Formatter = this;
                Encoding encoding;
                string xml = null;
                try
                {
                    reader.MoveToContent(); // Move to the Xml content (and read the encoding :)
                    encoding = reader.Encoding ?? Encoding.UTF8;
                    ParseMessage(message, reader, parserContext, ref xml, encoding);
                }
                catch (XmlException)
                {
                    if (bws.Eof)
                        if (parserContext.FrameSize > 0)
                            // Frame has not enough data to parse the Xml.
                            throw;
                        else
                            // More data needed to parse the message.
                            return null;
                    // Assume a bad Xml format.
                    throw;
                }

                parserContext.Consumed(parserContext.FrameSize > 0
                    ? parserContext.FrameSize // Discard the whole frame.
                    : GetXmlLength(parserContext.Buffer.Read(false), reader, encoding, xml));

                parserContext.MessageHasBeenConsumed();

                return message;
            }
        }

        #region Nested type: BufferWrapperStream
        /// <summary>
        /// Wrap an <see cref="IBuffer"/> in a stream wich is the interface <see cref="XmlTextReader"/> needs
        /// to read data.
        /// 
        /// This implementation handles the read of the last byte of the <see cref="IBuffer"/> setting the
        /// <see ref="Eof"/> to true. This allows us to detect the Xml parser reached the end of the data.
        /// </summary>
        private class BufferWrapperStream : Stream
        {
            private readonly IBuffer _buffer;
            private readonly int _frameSize = int.MaxValue;
            private int _position;

            public BufferWrapperStream(IBuffer buffer)
            {
                _buffer = buffer;
                _position = buffer.LowerDataBound;
                Eof = false;
            }

            public BufferWrapperStream(IBuffer buffer, int frameSize)
            {
                _buffer = buffer;
                _frameSize = frameSize;
                _position = buffer.LowerDataBound;
                Eof = false;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public bool Eof { get; private set; }

            public override long Length
            {
                get { return _frameSize < _buffer.DataLength ? _frameSize : _buffer.DataLength; }
            }

            public override long Position
            {
                get { return _position; }
                set { throw new NotSupportedException(); }
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw new ArgumentNullException("buffer");

                if (offset < 0)
                    throw new ArgumentOutOfRangeException("offset", "Can not be a negative number.");

                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "Can not be a negative number.");

                if (buffer.Length - offset < count)
                    throw new ArgumentException("Not enough data in destination buffer.");

                int left = (int) Length - _position;
                if (left == 0)
                {
                    Eof = true;
                    return 0;
                }
                count = Math.Min(count, Math.Max(1, left - 1));

                for (int i = 0; i < count; i++)
                    buffer[offset + i] = _buffer[_position++];

                return count;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
        #endregion
    }
}