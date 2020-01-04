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

using Trx.Messaging.Channels;

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// This interface defines which a class must implement to turn
    /// into a pool of channels.
    /// </summary>
    public interface IChannelPool
    {
        /// <summary>
        /// It returns the length of the pool.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// It returns the name of the pool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Adds a channel to the pool.
        /// </summary>
        /// <param name="channel">
        /// It's the channel to be added to the pool.
        /// </param>
        /// <returns>
        /// A logical value equals to true if the channel was added to the pool,
        /// otherwise false.
        /// </returns>
        bool Add(IChannel channel);

        /// <summary>
        /// Removes a channel from the pool.
        /// </summary>
        /// <returns>
        /// The removed channel, or an invalid reference if the pool was empty.
        /// </returns>
        IChannel Remove();
    }
}