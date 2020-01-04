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

using Trx.Logging;

namespace Trx.Communication.Channels
{
    public interface IChannel
    {
        /// <summary>
        /// It returns or sets the channel name.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the channel.
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// Returns the channel messages pipeline.
        /// </summary>
        Pipeline Pipeline
        {
            get;
        }

        /// <summary>
        /// Tells if the channel is connected.
        /// </summary>
        bool IsConnected
        {
            get;
        }

        ILogger Logger { get; set; }

        string LoggerName { get; set; }

        /// <summary>
        /// Current channel address, by default is a direct reference to the channel instance.
        /// </summary>
        IChannelAddress ChannelAddress { get; set; }

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Close channel and release all the allocated resources. In most cases the channel cannot be used again.
        /// </summary>
        void Close();
    }
}
