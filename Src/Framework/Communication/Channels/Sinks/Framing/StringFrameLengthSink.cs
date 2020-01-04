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
    public class StringFrameLengthSink : ISink
    {
        private readonly int _digitsInHeader;
        private readonly int[] _maxLengths = new [] { 9, 99, 999, 9999, 99999, 999999, 9999999, 99999999 };

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="digitsInHeader">
        /// Bytes to use in the frame header, can be a value between 1 and 8.
        /// </param>
        public StringFrameLengthSink(int digitsInHeader)
        {
            _digitsInHeader = digitsInHeader;
            if (digitsInHeader < 1 || digitsInHeader > 8)
                throw new ArgumentOutOfRangeException("digitsInHeader", digitsInHeader, "Must be between 1 and 8.");
            MaxFrameLength = int.MaxValue;
        }

        /// <summary>
        /// Returns bytes to use in the frame header.
        /// </summary>
        public int DigitsInHeader
        {
            get { return _digitsInHeader; }
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
            return new StringFrameLengthSink(_digitsInHeader)
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
            int length = buffer.DataLength + (IncludeHeaderLength ? _digitsInHeader : 0);

            if (length > _maxLengths[_digitsInHeader - 1])
                throw new ChannelException(string.Format("A length of {0} bytes is not supported by this framing sink.",
                    length));

            var header = new byte[_digitsInHeader];
            for (int i = _digitsInHeader - 1; i >= 0; i--)
            {
                header[i] = Convert.ToByte((length % 10) + 0x30);
                length /= 10;
            }

            buffer.Write(true, header);
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

            do
            {
                if (buffer.DataLength < _digitsInHeader)
                {
                    // Still we need more data to parse the header length.
                    context.ExpectedBytes = _digitsInHeader;
                    return false;
                }

                string header = buffer.ReadAsString(false, _digitsInHeader);
                int expectedBytes;
                if (!int.TryParse(header, out expectedBytes))
                    throw new ChannelException(string.Format("Cannot convert '{0}' to a valid length.", header));
                context.ExpectedBytes = expectedBytes - (IncludeHeaderLength ? _digitsInHeader : 0);

                if (context.ExpectedBytes == 0)
                {
                    buffer.Discard(_digitsInHeader);
                    context.Channel.Logger.Debug("Discarding zero length message frame.");
                }

                if (context.ExpectedBytes > MaxFrameLength)
                    throw new ChannelException(string.Format("Packet size of {0} byte/s is greater than " +
                        "maximum supported of {1} byte/s, closing channel.", context.ExpectedBytes, MaxFrameLength));

                // Consume 0 length frames.
            } while (context.ExpectedBytes == 0);

            if (buffer.DataLength >= (context.ExpectedBytes + _digitsInHeader))
            {
                buffer.Discard(_digitsInHeader);
                if (context.Channel.Logger.IsDebugEnabled())
                    context.Channel.Logger.Debug(string.Format("Frame delimiter of {0} byte/s fulfilled.", context.ExpectedBytes));
                return true;
            }

            // Buffer must contain the header indicated bytes plus the digits for the header itself.
            context.ExpectedBytes += _digitsInHeader;

            if (context.Channel.Logger.IsDebugEnabled())
                context.Channel.Logger.Debug(string.Format("Frame delimiter {0} of {1} byte/s NOT fulfilled, more data expected.",
                    context.ExpectedBytes + _digitsInHeader, buffer.DataLength));

            return false;
        }
        #endregion
    }
}