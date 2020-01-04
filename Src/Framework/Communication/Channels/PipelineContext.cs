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

using System.Collections.Generic;

namespace Trx.Communication.Channels
{
    public class PipelineContext
    {
        private readonly IChannel _channel;
        private readonly IMessagesIdentifier _messagesIdentifier;
        private object _messageToSend;
        private object _messageToSendId;
        private object _receivedMessage;
        private object _receivedMessageId;

        public PipelineContext(IChannel channel)
        {
            _channel = channel;
            if (_channel is IReceiverChannel)
                _messagesIdentifier = ((IReceiverChannel) _channel).MessagesIdentifier;
        }

        public IChannel Channel
        {
            get { return _channel; }
        }

        /// <summary>
        /// If the channel is an <see ref="IReceiverChannel"/> ans has messages identifier, this
        /// property has it (present in context to easy sink task if it needs this field).
        /// </summary>
        public IMessagesIdentifier MessagesIdentifier
        {
            get { return _messagesIdentifier; }
        }

        /// <summary>
        /// It's the message to send.
        /// </summary>
        public object MessageToSend
        {
            get { return _messageToSend; }
            set
            {
                _messageToSend = value;
                _messageToSendId = null;
            }
        }

        /// <summary>
        /// Allows a sink to set the id of the message to send.
        /// </summary>
        public object MessageToSendId
        {
            set { _messageToSendId = value; }
            get { return _messageToSendId; }
        }

        /// <summary>
        /// It's the received message.
        /// </summary>
        public object ReceivedMessage
        {
            get { return _receivedMessage; }
            set
            {
                _receivedMessage = value;
                _receivedMessageId = null;
            }
        }

        /// <summary>
        /// Allows a sink to set the id of the received message.
        /// </summary>
        public object ReceivedMessageId
        {
            set { _receivedMessageId = value; }
            get { return _receivedMessageId; }
        }

        /// <summary>
        /// Set by a sink to tell the channel the expected amount of bytes before the data is pushed 
        /// again through the pipeline.
        /// </summary>
        /// <remarks>
        /// The sink must to set the total bytes needed.
        /// 
        /// The next sinks in the pipeline can use this property as the frame size.
        /// </remarks>
        public int ExpectedBytes { get; set; }

        internal LinkedListNode<ISink> PrevCallReceiveSink { get; set; }

        public void Reset()
        {
            PrevCallReceiveSink = null;
            ExpectedBytes = 0;
            ReceivedMessage = null;
            MessageToSend = null;
        }
    }
}