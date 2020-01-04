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

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Defines a channel capable to send messages.
    /// </summary>
    public interface ISenderChannel : IChannel
    {
        /// <summary>
        /// It sends the specified message asynchronously.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        ChannelRequestCtrl Send(object message);

        /// <summary>
        /// Send a request message expecting a response from remote peer.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds for a response.
        /// </param>
        /// <param name="sendToTupleSpace">
        /// If true, the request is sent to the channel tuple space on completion or time out.
        /// </param>
        /// <param name="key">
        /// Request key, can be null.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        /// <remarks>
        /// Completed or timed out requests are stored in the receive tuple space if <paramref name="sendToTupleSpace"/>
        /// was set to true. If false synchronous wait of the response from the calling thread is assumed via the 
        /// <see cref="Request.WaitResponse"/> method.
        /// </remarks>
        SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout, bool sendToTupleSpace, object key);

        /// <summary>
        /// Send a request message expecting a response from remote peer.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds for a response.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        /// <remarks>
        /// Completed or timed out requests are stored in the receive tuple space.
        /// </remarks>
        SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout);
    }
}
