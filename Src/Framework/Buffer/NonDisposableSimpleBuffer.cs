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

namespace Trx.Buffer
{
    /// <summary>
    /// A <see cref="SingleChunkBuffer"/> non disposable by the channel, the user is responsible for
    /// destroying it.
    /// </summary>
    /// <remarks>
    /// Use with extremely care because channels access internal buffer regions in asynchronous
    /// pending sends/receives. Try to use <see cref="SingleChunkBuffer"/> if you are not sure
    /// about internal mechanics of Trx Framework.
    /// </remarks>
    internal class NonDisposableSimpleBuffer : SingleChunkBuffer
    {
        public NonDisposableSimpleBuffer( int chunkSize ) : base( chunkSize )
        {
        }

        public NonDisposableSimpleBuffer()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// In this implementation, this method does nothing.
        /// </remarks>
        public sealed override void Dispose()
        {
        }
    }
}
