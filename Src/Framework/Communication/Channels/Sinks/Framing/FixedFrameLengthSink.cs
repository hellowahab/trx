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
    public class FixedFrameLengthSink : ISink
    {
        private readonly int _frameSize;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="frameSize">
        /// Frame size.
        /// </param>
        public FixedFrameLengthSink(int frameSize)
        {
            _frameSize = frameSize;
            if (frameSize < 1)
                throw new ArgumentOutOfRangeException("frameSize", "The size must be greater than zero.");
        }

        /// <summary>
        /// Returns the frame size.
        /// </summary>
        public int FrameSize
        {
            get { return _frameSize; }
        }

        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>
        /// A copy of the instance.
        /// </returns>
        public object Clone()
        {
            return new FixedFrameLengthSink(_frameSize);
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
            // Neutral to send operations
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

            context.ExpectedBytes = _frameSize;

            return (context.ReceivedMessage as IBuffer).DataLength >= _frameSize;
        }
    }
}
