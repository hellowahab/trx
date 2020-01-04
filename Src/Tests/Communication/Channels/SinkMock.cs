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
using System.Threading;
using NUnit.Framework;
using Trx.Communication.Channels;

namespace Tests.Trx.Communication.Channels
{
    public class SinkMock : ISink
    {
        #region Constructors
        public SinkMock()
        {
            Reset();
        }
        #endregion

        #region Properties
        public bool OnEventReturnValue { get; set; }

        public bool ReceiveReturnValue { get; set; }

        /// <summary>
        /// Received event in <see cref="OnEvent"/> method.
        /// </summary>
        public ChannelEvent ChannelEvent { get; set; }

        /// <summary>
        /// Received event in <see cref="OnEvent"/> method.
        /// </summary>
        public ChannelEvent PreviousChannelEvent { get; set; }

        /// <summary>
        /// Received message in <see cref="Send"/> method.
        /// </summary>
        public object SendMessage { get; set; }

        /// <summary>
        /// Received message in <see cref="Receive"/> method.
        /// </summary>
        public object ReceiveMessage { get; set; }

        public DateTime OnEventUtcDateTime { get; set; }

        public DateTime SendUtcDateTime { get; set; }

        public DateTime ReceiveUtcDateTime { get; set; }

        public string ExceptionMessage { get; set; }

        public bool ConsumeMessageOnSend { get; set; }

        public int ExpectedBytes
        {
            get;
            set;
        }
        #endregion

        #region Methods
        public virtual bool OnEvent(PipelineContext context, ChannelEvent channelEvent)
        {
            Assert.NotNull(context);
            Assert.NotNull(channelEvent);

            PreviousChannelEvent = ChannelEvent;
            ChannelEvent = channelEvent;

            Thread.Sleep(20);
            OnEventUtcDateTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(ExceptionMessage))
                throw new ChannelException(ExceptionMessage);

            return OnEventReturnValue;
        }

        public void Send(PipelineContext context)
        {
            Assert.NotNull(context);

            SendMessage = context.MessageToSend;

            if (ConsumeMessageOnSend)
                context.MessageToSend = null;

            Thread.Sleep(20);
            SendUtcDateTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(ExceptionMessage))
                throw new ChannelException(ExceptionMessage);
        }

        public bool Receive(PipelineContext context)
        {
            Assert.NotNull(context);

            ReceiveMessage = context.ReceivedMessage;

            Thread.Sleep(20);
            ReceiveUtcDateTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(ExceptionMessage))
                throw new ChannelException(ExceptionMessage);

            context.ExpectedBytes = ExpectedBytes;

            return ReceiveReturnValue;
        }

        public void Reset()
        {
            OnEventReturnValue = true;
            ReceiveReturnValue = true;

            OnEventUtcDateTime = DateTime.MinValue;
            SendUtcDateTime = DateTime.MinValue;
            ReceiveUtcDateTime = DateTime.MinValue;

            SendMessage = null;

            ReceiveMessage = null;

            ChannelEvent = null;
            PreviousChannelEvent = null;

            ExceptionMessage = null;

            ConsumeMessageOnSend = false;

            ExpectedBytes = 0;
        }
        #endregion

        #region ISink Members
        /// <summary>
        /// Crea un nuevo objeto copiado de la instancia actual.
        /// </summary>
        /// <returns>
        /// Nuevo objeto que es una copia de esta instancia.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new SinkMock();
        }
        #endregion
    }
}