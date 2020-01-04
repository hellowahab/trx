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
using Trx.Buffer;

namespace Trx.Messaging
{
    /// <summary>
    /// It implements a simple messages formatter, that can be utilized as base
    /// for other more sophisticated.
    /// </summary>
    /// <remarks>
    /// This formatter can handle messages with all the types of fields that are
    /// implemented in the framework, string fields, raw byte fields and bitmaps.
    /// There's a special handling, updating its values depending of the presence
    /// or not of their associated fields.
    /// </remarks>
    public class BasicMessageFormatter : BaseMessageFormatter
    {
        /// <summary>
        /// It builds a new messages formatter.
        /// </summary>
        public BasicMessageFormatter()
        {
        }

        /// <summary>
        /// Creates a message formatter with the definition in a Xml cofiguration file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        public BasicMessageFormatter(string xmlFileName)
            : base(xmlFileName, "Trx.Messaging.FormatterDefinition")
        {
        }

        protected virtual void BeforeFieldsFormatting(Message message, ref FormatterContext formatterContext)
        {
        }

        protected virtual bool BeforeFieldsParsing(Message message, ref ParserContext parserContext)
        {
            return true;
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
            formatterContext.CurrentMessage = message;
            message.Formatter = this;

            // Write header.
            if (PacketHeader != null)
                formatterContext.Write(PacketHeader);

            // Format header if we have one header formatter.
            if (MessageHeaderFormatter != null)
                try
                {
                    MessageHeaderFormatter.Format(message.Header, ref formatterContext);
                }
                catch (Exception e)
                {
                    throw new MessagingException("Can't format message header.", e);
                }

            // Allow subclasses to put information between header data and
            // fields data.
            BeforeFieldsFormatting(message, ref formatterContext);

            // If bitmaps are present in the message formatter, check if we
            // must add them to the message.
            bool atLeastOne = false;
            int firstBitmap = -1;
            for (int i = Bitmaps.Length - 1; (i >= 0) && (Bitmaps[i] >= 0); i--)
            {
                firstBitmap = i;
                if (message.Fields.Contains(Bitmaps[i]))
                {
                    // Check if present field is a bitmap.
                    if (!(message[Bitmaps[i]] is BitMapField))
                        throw new MessagingException(string.Format("Field number {0} must be a bitmap.", Bitmaps[i]));
                    atLeastOne = true;
                }
                else
                {
                    // Get bitmap from field formatters collection.
                    var bitmap = (BitMapFieldFormatter) GetFieldFormatter(Bitmaps[i]);

                    // Add bitmap to message if required.
                    if (message.Fields.ContainsAtLeastOne(bitmap.LowerFieldNumber, bitmap.UpperFieldNumber))
                    {
                        message.Fields.Add(new BitMapField(bitmap.FieldNumber,
                            bitmap.LowerFieldNumber, bitmap.UpperFieldNumber));
                        atLeastOne = true;
                    }
                }
            }
            if (!atLeastOne && firstBitmap > -1)
            {
                // In a bitmaped message, at least the first bitmap must be present.
                var bitmap = (BitMapFieldFormatter) GetFieldFormatter(Bitmaps[firstBitmap]);

                message.Fields.Add(new BitMapField(bitmap.FieldNumber,
                    bitmap.LowerFieldNumber, bitmap.UpperFieldNumber));
            }

            // Correct bitmap values.
            message.CorrectBitMapsValues();

            // Format fields.
            for (int i = 0; i <= message.Fields.MaximumFieldNumber; i++)
                if (message.Fields.Contains(i))
                {
                    // If we haven't the field formatter, throw an exception.
                    if (!FieldFormatterIsPresent(i))
                        throw new MessagingException(string.Format("Unknown formatter for field number {0}.", i));

                    // Set parent message.
                    if (message.Fields[i] is InnerMessageField)
                    {
                        var innerMessageField = message.Fields[i] as InnerMessageField;
                        if (innerMessageField != null)
                        {
                            var innerMessage = innerMessageField.Value as Message;
                            if (innerMessage != null)
                                innerMessage.Parent = message;
                        }
                    }

                    try
                    {
                        int len = formatterContext.Buffer.DataLength;
                        GetFieldFormatter(i).Format(message.Fields[i], ref formatterContext);
                        if (MessageSecuritySchema != null && !MessageSecuritySchema.FieldCanBeLogged(i))
                            // Set the added data as secure.
                            formatterContext.Buffer.AddSecureArea(
                                new BufferSecureArea(
                                    formatterContext.Buffer.UpperDataBound - (formatterContext.Buffer.DataLength - len),
                                    formatterContext.Buffer.UpperDataBound - 1));
                    }
                    catch (Exception e)
                    {
                        throw new MessagingException(string.Format("Can't format field number {0}.", i), e);
                    }
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
            // Create a new message if we are parsing a new one.
            if (parserContext.CurrentMessage == null)
            {
                parserContext.CurrentMessage = NewMessage();
                // The message must known its formatter for message.ToString()
                parserContext.CurrentMessage.Formatter = this;
            }

            // Remove packet header data.
            if ((PacketHeader != null) && !parserContext.PacketHeaderDataStripped)
            {
                parserContext.Consumed(PacketHeader.Length);
                parserContext.PacketHeaderDataStripped = true;
            }

            // Parse header if we have a header formatter.
            if (MessageHeaderFormatter != null) // Check if the header hasn't been parsed yet.
                if (parserContext.CurrentMessage.Header == null)
                {
                    try
                    {
                        parserContext.CurrentMessage.Header =
                            MessageHeaderFormatter.Parse(ref parserContext);
                    }
                    catch (Exception e)
                    {
                        throw new MessagingException("Can't parse message header.", e);
                    }

                    // If null, more data is needed.
                    if (parserContext.CurrentMessage.Header == null)
                        return null;

                    parserContext.ResetDecodedLength();
                }

            // Allow subclasses to get information between header data and
            // fields data.
            if (!parserContext.Signaled)
            {
                if (!BeforeFieldsParsing(parserContext.CurrentMessage, ref parserContext))
                    return null;
                parserContext.Signaled = true;
            }

            for (int i = parserContext.CurrentField; i <= MaximumFieldFormatterNumber(); i++)
            {
                // If we have a bitmap use it to detect present fields,
                // otherwise parse known message formatter fields.
                if (parserContext.CurrentBitMap != null)
                {
                    // Check if field number is out of bitmap bounds.
                    if (i > parserContext.CurrentBitMap.UpperFieldNumber)
                    {
                        // Locate another bitmap.
                        bool found = false;
                        for (int j = parserContext.CurrentBitMap.FieldNumber + 1; j < i; j++)
                            if (parserContext.CurrentMessage.Fields.Contains(j))
                            {
                                Field field = parserContext.CurrentMessage.Fields[j];
                                if (field is BitMapField)
                                {
                                    // Another bitmap found.
                                    parserContext.CurrentBitMap = (BitMapField) field;
                                    found = true;
                                    break;
                                }
                            }

                        if (!found)
                        {
                            parserContext.CurrentBitMap = null;

                            // No more bitmaps, continue with posible mandatory fields
                            // (start from last field covered by known bitmaps, plus one).
                            i = ((BitMapFieldFormatter)
                                (GetFieldFormatter(Bitmaps[Bitmaps.Length - 1]))).UpperFieldNumber + 1;
                            continue;
                        }
                    }

                    if (!parserContext.CurrentBitMap[i])
                    {
                        // Save current field number in context.
                        parserContext.CurrentField = i;

                        // Bit is not set, field is not present in the received data.
                        continue;
                    }
                }

                // Save current field number in context.
                parserContext.CurrentField = i;

                if (FieldFormatterIsPresent(i))
                {
                    // Get field formatter.
                    FieldFormatter fieldFormatter = GetFieldFormatter(i);

                    // If no bitmap is present, check if the field it's a self announced field.
                    if (parserContext.CurrentBitMap == null)
                    {
                        var aaff =
                            fieldFormatter as ISelfAnnouncedFieldFormatter;

                        if ((aaff != null) && !parserContext.AnnouncementHasBeenConsumed)
                        {
                            if ((parserContext.LowerDataBound == parserContext.FrontierUpperBound) ||
                                (parserContext.DataLength == 0))
                            {
                                // If we've reched the upper data bound frontier of the message
                                // frame or no data is available in the parser context, we assume
                                // the self announced field isn't present and the message
                                // has been entirely parsed.
                                i = MaximumFieldFormatterNumber();
                                continue;
                            }

                            int fieldNumber;
                            if (!aaff.GetFieldNumber(ref parserContext, out fieldNumber))
                                // More data needed to parse message.
                                return null;

                            // Turn on the flag which indicates that the field announcement has been
                            // consumed (this prevent a second announcement consume attempt if this
                            // function is callend more than one time for the same message).
                            parserContext.AnnouncementHasBeenConsumed = true;

                            if (fieldNumber != i)
                            {
                                // Save current field number in context.
                                i = fieldNumber;
                                parserContext.CurrentField = i;
                                if (!FieldFormatterIsPresent(i))
                                    // The message announces a field which isn't known by the
                                    // message formatter.
                                    throw new MessagingException(
                                        string.Format(
                                            "Field number {0} is present in message, but we don't know it's formatter.",
                                            i));

                                fieldFormatter = GetFieldFormatter(i);
                            }
                        }
                    }

                    Field field;
                    try
                    {
                        int lowerDataBound = parserContext.Buffer.LowerDataBound;
                        int dataLength = parserContext.Buffer.DataLength;
                        // to parse field.
                        field = fieldFormatter.Parse(ref parserContext);
                        if (MessageSecuritySchema != null && !MessageSecuritySchema.FieldCanBeLogged(i))
                        {
                            int consumed = parserContext.Buffer.LowerDataBound - lowerDataBound;
                            if (consumed <= 0)
                                consumed = dataLength;
                            parserContext.Buffer.AddSecureArea(new BufferSecureArea(lowerDataBound,
                                lowerDataBound + consumed - 1));
                        }
                    }
                    catch (Exception e)
                    {
                        throw new MessagingException(string.Format("Can't parse field number {0}.", i), e);
                    }

                    if (field == null)
                    {
                        if (Logger.IsDebugEnabled())
                            Logger.Debug("More data needed to parse message.");

                        // More data needed to parse message.
                        return null;
                    }
                    parserContext.CurrentMessage.Fields.Add(field);

                    // Set parent message.
                    if (field is InnerMessageField)
                    {
                        var innerMessageField = field as InnerMessageField;
                        var innerMessage = innerMessageField.Value as Message;
                        if (innerMessage != null)
                            innerMessage.Parent = parserContext.CurrentMessage;
                    }

                    if (Logger.IsDebugEnabled())
                        if (field is BitMapField)
                            Logger.Debug(string.Format("Decoded field {0,5}: {1}", field.FieldNumber,
                                MessageSecuritySchema == null ||
                                    MessageSecuritySchema.FieldCanBeLogged(field.FieldNumber)
                                    ? field.ToString()
                                    : MessageSecuritySchema.ObfuscateFieldData(field)));
                        else
                            Logger.Debug(string.Format("Decoded field {0,5}: [{1}]", field.FieldNumber,
                                MessageSecuritySchema == null ||
                                    MessageSecuritySchema.FieldCanBeLogged(field.FieldNumber)
                                    ? field.ToString()
                                    : MessageSecuritySchema.ObfuscateFieldData(field)));

                    parserContext.ResetDecodedLength();

                    // If this is the first located bitmap, save it.
                    if ((parserContext.CurrentBitMap == null) &&
                        (field is BitMapField))
                        parserContext.CurrentBitMap = (BitMapField) field;
                }
                else if (parserContext.CurrentBitMap != null)
                    // A field is present in current bitmap, but message formatter
                    // can't parse it because field formatter isn't present.
                    throw new MessagingException(
                        string.Format("Field number {0} is present in the message but we don't know its formatter.", i));
            }

            // We have a new message, get it and initialize parsing context.
            Message parsedMessage = parserContext.CurrentMessage;

            // Reset parser context.
            parserContext.MessageHasBeenConsumed();

            return parsedMessage;
        }

        public override object Clone()
        {
            var formatter = new BasicMessageFormatter();
            CopyTo(formatter);
            return formatter;
        }
    }
}