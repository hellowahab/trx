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
using Trx.Messaging.FlowControl;
using Trx.Utilities;
using log4net;

namespace Trx.Messaging.Channels {

	/// <summary>
	/// It's the <see cref="IChannel.Connected"/> event delegate.
	/// </summary>
	public delegate void ChannelConnectedEventHandler( object sender, EventArgs e);

	/// <summary>
	/// It's the <see cref="IChannel.Disconnected"/> event delegate.
	/// </summary>
	public delegate void ChannelDisconnectedEventHandler( object sender, EventArgs e);

	/// <summary>
	/// It's the <see cref="IChannel.Receive"/> event delegate.
	/// </summary>
	public delegate void ChannelReceiveEventHandler( object sender, ReceiveEventArgs e);

	/// <summary>
	/// It's the <see cref="IChannel.Error"/> event delegate.
	/// </summary>
	public delegate void ChannelErrorEventHandler( object sender, ErrorEventArgs e);

	/// <summary>
	/// Defines a channel capable of interchanging messages
	/// with another system.
	/// </summary>
	public interface IChannel : ICloneable {

		/// <summary>
		/// It's raised when the channel has been connected.
		/// </summary>
		event ChannelConnectedEventHandler Connected;

		/// <summary>
		/// It's raised when the channel has been disconnected.
		/// </summary>
		event ChannelDisconnectedEventHandler Disconnected;

		/// <summary>
		/// It's raised when a message has been received.
		/// </summary>
		event ChannelReceiveEventHandler Receive;

		/// <summary>
		/// It's raised when an error has been catched in the
		/// internal channel processing. 
		/// </summary>
		/// <remarks>
		/// This event is received from the channel when a catched error
		/// causes its inhabilitation, it's necessary to call
		/// <see cref="Connect"/> again to use it.
		/// </remarks>
		event ChannelErrorEventHandler Error;

		/// <summary>
		/// Returns the messages formatter used every time
		/// a message is sent.
		/// </summary>
		/// <remarks>
		/// If a message has a formatter, it's the one used
		/// when it's sent.
		/// </remarks>
		IMessageFormatter Formatter {
			get;
		}

		/// <summary>
		/// It returns or sets the logger associated to the channel.
		/// </summary>
		ILog Logger {
			get;
			set;
		}

		/// <summary>
		/// It returns or sets the logger name associated to the channel.
		/// </summary>
		string LoggerName {
			get;
			set;
		}

		/// <summary>
		/// It returns or sets the channel name.
		/// </summary>
		string Name {
			get;
			set;
		}

		/// <summary>
		/// It starts the connection of the channel with the remote system.
		/// </summary>
		/// <returns>
		/// Returns true if connection has been started, otherwise
		/// returns false.
		/// </returns>
		bool Connect();

		/// <summary>
		/// Tells if the channel is connected.
		/// </summary>
		bool IsConnected {
			get;
		}

		/// <summary>
		/// Returns or sets the received messages processor.
		/// </summary>
		IMessageProcessor MessageProcessor {
			get;
			set;
		}

		/// <summary>
		/// Close the connection, if exists, stablished with
		/// the remote system.
		/// </summary>
		void Close();

		/// <summary>
		/// It sends the specified message to the remote system.
		/// </summary>
		/// <param name="message">
		/// It's the message to be sent.
		/// </param>
		void Send( Message message);

		/// <summary>
		/// It begins the association of a channel with a connection accepted
		/// by a listener.
		/// </summary>
		/// <param name="connectionData">
		/// It's the data of the connection accepted by
		/// the listener.
		/// </param>
		void BeginBind( object connectionData);

		/// <summary>
		/// It ends the association of a channel with a connection accepted
		/// by a listener.
		/// </summary>
		void EndBind();
	}
}