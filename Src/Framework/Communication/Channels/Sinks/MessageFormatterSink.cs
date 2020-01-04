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
using Trx.Messaging;

namespace Trx.Communication.Channels.Sinks
{
    /// <summary>
    /// A sink to format and parse messages of type <see cref="Message"/>.
    /// </summary>
    /// <remarks>
    /// This sink is state aware, cannot be used in two pipelines.
    /// </remarks>
    public class MessageFormatterSink : ISink
    {
        private readonly IMessageFormatter _formatter;
        private ParserContext _parserContext;

        public MessageFormatterSink(IMessageFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            _formatter = formatter;
            BytesToReserveHeadOfFormatterContext = 64;
        }

        /// <summary>
        /// Sets the formatter used by the sink.
        /// </summary>
        public IMessageFormatter Formatter
        {
            get { return _formatter; }
        }

        /// <summary>
        /// Number of bytes to reserver in the start of the formatter context.
        /// </summary>
        /// <remarks>
        /// This is implemented for performance purposes to allow other sinks in the pipeline
        /// to prepend bytes without moving data within the buffer (i.e. frame delimiters sinks)
        /// By default 64 bytes are reserved.
        /// </remarks>
        public int BytesToReserveHeadOfFormatterContext { get; set; }

        #region ISink Members
        /// <summary>
        /// Called when a significant event (other than send or receive) was caught in the channel.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <param name="channelEvent">
        /// The event.
        /// </param>
        /// <returns>
        /// True if the event can be informed to the next sink in the pipeline, otherwise false.
        /// </returns>
        public bool OnEvent(PipelineContext context, ChannelEvent channelEvent)
        {
            if (channelEvent.EventType == ChannelEventType.Disconnected && _parserContext != null)
                _parserContext.Initialize();

            return true;
        }

        /// <summary>
        /// Process the message to be sent.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <remarks>
        /// The message to be sent is stored in the <see cref="PipelineContext.MessageToSend"/>. If
        /// null is set by the sink the message is consumed and it stop going through the pipeline,
        /// whereby the channel doesn't send it.
        /// </remarks>
        public void Send(PipelineContext context)
        {
            if (!(context.MessageToSend is Message))
                throw new ChannelException("This sink implementation only support to send messages of type Message.");

            FormatterContext formatterContext = context.Channel.Pipeline.BufferFactory == null
                ? new FormatterContext(new SingleChunkBuffer())
                : new FormatterContext(context.Channel.Pipeline.BufferFactory.GetInstance());

            // Reserve bytes in the beginning of the buffer.
            if (formatterContext.Buffer.Capacity < BytesToReserveHeadOfFormatterContext)
                formatterContext.Buffer.Capacity += BytesToReserveHeadOfFormatterContext;
            formatterContext.Buffer.LowerDataBound =
                formatterContext.Buffer.UpperDataBound = BytesToReserveHeadOfFormatterContext;

            _formatter.Format(context.MessageToSend as Message, ref formatterContext);
            context.MessageToSend = formatterContext.Buffer;
        }

        /// <summary>
        /// Process the received message.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <returns>
        /// True if the pipeline can continue with the next sink, otherwise false.
        /// </returns>
        /// <remarks>
        /// The received message (if the sink creates one) is be stored in the
        /// <see cref="PipelineContext.ReceivedMessage"/> property.  If null is set by the sink the
        /// message is consumed and it stop going through the pipeline, whereby the channel doesn't 
        /// put in the receive tuple space.
        /// </remarks>
        public bool Receive(PipelineContext context)
        {
            if (!(context.ReceivedMessage is IBuffer))
                throw new ChannelException("This sink implementation only support to receive messages of type IBuffer.");

            if (_parserContext == null)
                _parserContext = new ParserContext(context.ReceivedMessage as IBuffer);

            _parserContext.FrameSize = context.ExpectedBytes;
            Message message = _formatter.Parse(ref _parserContext);
            if (message != null)
            {
                context.ReceivedMessage = message;
                return true;
            }

            // More data needed to parse a message
            return false;
        }

        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>
        /// A copy of the instance.
        /// </returns>
        public object Clone()
        {
            // No need to clone the formatter because it's stateless.
            return new MessageFormatterSink(_formatter);
        }
        #endregion
    }
}