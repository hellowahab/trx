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

namespace Trx.Communication.Channels.Sinks.Framing
{
    /// <summary>
    /// Implements a sink framing the data in fixed bytes count.
    /// </summary>
    public class NboFrameLengthSink : ISink
    {
        private readonly int _bytesInHeader;
        private readonly long[] _maxLengths = new long[] { 0, 65535, 0, 4294967295 };

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="bytesInHeader">
        /// Bytes to use in the frame header, can be 2 or 4.
        /// </param>
        public NboFrameLengthSink(int bytesInHeader)
        {
            _bytesInHeader = bytesInHeader;
            if (bytesInHeader != 2 && bytesInHeader != 4)
                throw new ArgumentException("Must be 2 or 4.", "bytesInHeader");
            MaxFrameLength = int.MaxValue;
        }

        /// <summary>
        /// Returns bytes to use in the frame header.
        /// </summary>
        public int BytesInHeader
        {
            get { return _bytesInHeader; }
        }

        /// <summary>
        /// If true add the header length to the total frame length itself.
        /// </summary>
        public bool IncludeHeaderLength { get; set; }

        /// <summary>
        /// Get or sets the maximum frame length supported.
        /// </summary>
        public int MaxFrameLength { get; set; }

        #region ISink Members
        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>
        /// A copy of the instance.
        /// </returns>
        public object Clone()
        {
            return new NboFrameLengthSink(_bytesInHeader)
                       {
                           IncludeHeaderLength = IncludeHeaderLength,
                           MaxFrameLength = MaxFrameLength
                       };
        }

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
            if (!(context.MessageToSend is IBuffer))
                throw new ChannelException("This sink implementation only support to send messages of type IBuffer.");

            var buffer = context.MessageToSend as IBuffer;
            int length = buffer.DataLength + (IncludeHeaderLength ? _bytesInHeader : 0);

            if (length > _maxLengths[_bytesInHeader - 1])
                throw new ChannelException(string.Format("A length of {0} bytes is not supported by this framing sink.",
                    length));

            buffer.Write(true, _bytesInHeader == 2
                ? new[] {(byte) (length >> 8), (byte) length}
                : new[] {(byte) (length >> 24), (byte) (length >> 16), (byte) (length >> 8), (byte) (length)});
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

            var buffer = context.ReceivedMessage as IBuffer;

            do {
                if (buffer.DataLength < _bytesInHeader) {
                    // Still we need more data to parse the header length.
                    context.ExpectedBytes = _bytesInHeader;
                    return false;
                }

                byte[] header = buffer.Read( false, _bytesInHeader );
                if (_bytesInHeader == 2)
                    context.ExpectedBytes = ( header[0] << 8 ) | header[1];
                else
                    context.ExpectedBytes = ( header[0] << 24 ) | ( header[1] << 16 ) | ( header[2] << 8 ) | header[3];

                if (IncludeHeaderLength)
                    context.ExpectedBytes -= _bytesInHeader;

                if (context.ExpectedBytes == 0)
                {
                    buffer.Discard(_bytesInHeader);
                    context.Channel.Logger.Debug("Discarding zero length message frame.");
                }

                if (context.ExpectedBytes > MaxFrameLength)
                    throw new ChannelException(string.Format("Packet size of {0} byte/s is greater than " +
                        "maximum supported of {1} byte/s, closing channel.", context.ExpectedBytes, MaxFrameLength));

                // Consume 0 length frames.
            } while (context.ExpectedBytes == 0);

            if (buffer.DataLength >= (context.ExpectedBytes + _bytesInHeader))
            {
                buffer.Discard(_bytesInHeader);
                if (context.Channel.Logger.IsDebugEnabled())
                    context.Channel.Logger.Debug(string.Format("Frame delimiter of {0} byte/s fulfilled.", context.ExpectedBytes));
                return true;
            }

            // Buffer must contain the header indicated bytes plus the bytes for the header itself.
            context.ExpectedBytes += _bytesInHeader;

            if (context.Channel.Logger.IsDebugEnabled())
                context.Channel.Logger.Debug(string.Format("Frame delimiter {0} of {1} byte/s NOT fulfilled, more data expected.",
                    context.ExpectedBytes + _bytesInHeader, buffer.DataLength));

            return false;
        }
        #endregion
    }
}