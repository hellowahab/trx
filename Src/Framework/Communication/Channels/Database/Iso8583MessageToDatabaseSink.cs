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
using System.Data;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Trx.Communication.Channels.Database
{
    /// <summary>
    /// Maps a message to and from database.
    /// </summary>
    public class Iso8583MessageToDatabaseSink : MessageToDatabaseSink
    {
        /// <summary>
        /// Get and set the message type identifier parameter name.
        /// </summary>
        public string MessageTypeIdentifierParameterName { get; set; }

        /// <summary>
        /// Crea un nuevo objeto copiado de la instancia actual.
        /// </summary>
        /// <returns>
        /// Nuevo objeto que es una copia de esta instancia.
        /// </returns>
        public override object Clone()
        {
            return new Iso8583MessageToDatabaseSink();
        }

        protected override Message NewMessage()
        {
            return new Iso8583Message();
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
        public override void Send(PipelineContext context)
        {
            object messageToSend = context.MessageToSend;
            var message = messageToSend as Iso8583Message;
            if (message == null)
                throw new ChannelException(
                    "This sink implementation only support to send messages of type Iso8583Message.");

            base.Send(context);

            if (string.IsNullOrEmpty(MessageTypeIdentifierParameterName))
                return;

            var list = context.MessageToSend as List<DatabaseChannelParameter>;
            if (list != null)
                list.Add(new DatabaseChannelParameter
                             {
                                 DbType = DbType.Int32,
                                 ParameterName = MessageTypeIdentifierParameterName,
                                 Value = message.MessageTypeIdentifier
                             });
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
        public override bool Receive(PipelineContext context)
        {
            object receivedMessage = context.ReceivedMessage;
            var list = receivedMessage as List<DatabaseChannelParameter>;
            if (base.Receive(context) && list != null && !string.IsNullOrEmpty(MessageTypeIdentifierParameterName))
            {
                var message = context.ReceivedMessage as Iso8583Message;
                if (message != null)
                    foreach (DatabaseChannelParameter dbParam in list)
                        if (dbParam.ParameterName == MessageTypeIdentifierParameterName &&
                            (dbParam.DbType == DbType.Int16 || dbParam.DbType == DbType.Int32 || dbParam.DbType == DbType.Int64))
                            message.MessageTypeIdentifier = Convert.ToInt32(dbParam.Value);
            }

            return true;
        }
    }
}