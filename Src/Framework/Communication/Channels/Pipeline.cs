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
using System.Collections.Generic;
using Trx.Buffer;
using Trx.Logging;

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Defines the pipeline who process the receives and sends from and to the channel.
    /// </summary>
    public class Pipeline : ICloneable
    {
        private readonly LinkedList<ISink> _sinks = new LinkedList<ISink>();

        internal LinkedList<ISink> Sinks
        {
            get { return _sinks; }
        }

        /// <summary>
        /// Get or sets the buffer factory the channel and the sinks must use. If null, those
        /// objects must use a default internal value.
        /// </summary>
        public BufferFactory BufferFactory { get; set; }

        #region ICloneable Members
        /// <summary>
        /// Clones the current object.
        /// </summary>
        /// <returns>
        /// A copy of the instance.
        /// </returns>
        public object Clone()
        {
            var clone = new Pipeline {BufferFactory = BufferFactory};

            LinkedListNode<ISink> last = _sinks.Last;
            while (last != null)
            {
                clone.Push(last.Value.Clone() as ISink);
                last = last.Previous;
            }

            return clone;
        }
        #endregion

        /// <summary>
        /// Add a sink to the front of the pipeline stack.
        /// </summary>
        /// <param name="sink">
        /// Sink to add.
        /// </param>
        public void Push(ISink sink)
        {
            _sinks.AddFirst(sink);
        }

        /// <summary>
        /// Send a message through the pipeline.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <remarks>
        /// Called by the channel.
        /// </remarks>
        public void Send(PipelineContext context)
        {
            foreach (ISink sink in _sinks)
                sink.Send(context);
        }

        /// <summary>
        /// Called by the channel when input data is available.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <returns>
        /// Returns true if a message is received, otherwise false.
        /// </returns>
        /// <remarks>
        /// Called by the channel.
        ///
        /// The received message is stored in <see cref="PipelineContext.ReceivedMessage"/>.
        /// Created by the channel itself or one of the sinks in the pipeline.
        /// </remarks>
        public bool Receive(PipelineContext context)
        {
            LinkedListNode<ISink> last = context.PrevCallReceiveSink;
            if (last == null)
            {
                last = _sinks.Last;
                context.PrevCallReceiveSink = last;
            }
            while (last != null)
            {
                try
                {
                    if (!last.Value.Receive(context))
                        // The sink needs more data.
                        return false;
                }
                catch
                {
                    context.Reset();
                    throw;
                }

                if (context.ReceivedMessage == null)
                {
                    // The message has been consumed by the sink.
                    context.Reset();
                    return true;
                }

                last = last.Previous;
                context.PrevCallReceiveSink = last;
            }

            return true;
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
        /// <param name="ignoreException">
        /// If true an exception raise in the pipeline will interrupt the event broadcasting but
        /// the procedure will not raise it.
        /// </param>
        /// <param name="logger">
        /// Logger used to record errors.
        /// </param>
        /// <remarks>
        /// The first sink to get the channel event is the last one in pipeline, then the previuos up to
        /// the first sink in the pipeline.
        /// </remarks>
        public void ProcessChannelEvent(PipelineContext context, ChannelEvent channelEvent, bool ignoreException,
            ILogger logger)
        {
            try
            {
                LinkedListNode<ISink> last = _sinks.Last;
                while (last != null)
                {
                    if (!last.Value.OnEvent(context, channelEvent))
                        break;
                    last = last.Previous;
                }
            }
            catch (Exception ex)
            {
                if (ignoreException)
                {
                    logger.Error("Exception caught firing Disconnected event in the pipeline. " +
                        "This is a non critical error, system will continue normal operation", ex);
                    return;
                }

                throw;
            }
        }
    }
}