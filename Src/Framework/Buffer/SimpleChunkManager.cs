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

namespace Trx.Buffer
{
    /// <summary>
    /// Implements a dummy chunk manager wich creates a new chunk every time a buffer ask for one.
    /// </summary>
    public class SimpleChunkManager : IChunkManager
    {
        private readonly int _chunkSize;

        public SimpleChunkManager(int chunkSize)
        {
            if (chunkSize < 1)
                throw new ArgumentOutOfRangeException("chunkSize", chunkSize,
                    "A zero or negative chunk size is not supported");

            _chunkSize = chunkSize;
        }

        /// <summary>
        /// Get a chunk.
        /// </summary>
        /// <returns>
        /// A chunk to be used.
        /// </returns>
        public ArraySegment<byte> CheckOut()
        {
            return new ArraySegment<byte>(new byte[_chunkSize]);
        }

        /// <summary>
        /// Get a chunk.
        /// </summary>
        /// <param name="chunkSize">
        /// Requested chunk size.
        /// </param>
        /// <returns>
        /// A chunk to be used.
        /// </returns>
        public ArraySegment<byte> CheckOut(int chunkSize)
        {
            if (chunkSize < 1)
                throw new ArgumentOutOfRangeException("chunkSize", chunkSize,
                    "A zero or negative chunk size is not supported");

            return new ArraySegment<byte>(new byte[chunkSize]);
        }

        /// <summary>
        /// Return a chunk to the manager.
        /// </summary>
        /// <param name="buffer">
        /// The chunk to return.
        /// </param>
        public void CheckIn(ArraySegment<byte> buffer)
        {
        }
    }
}
