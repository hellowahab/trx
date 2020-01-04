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
using Trx.Buffer;

namespace Trx.Messaging
{
    /// <summary>
    /// Context to format <see cref="Message"/>.
    /// </summary>
    public class FormatterContext
    {
        public const int DefaultBufferSize = 2048;

        private readonly IBuffer _internalBuffer;

        /// <summary>
        /// Creates a new formatter context using the given <see cref="IBuffer"/>.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to use.
        /// </param>
        public FormatterContext(IBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            _internalBuffer = buffer;
        }

        /// <summary>
        /// Creates a new formatter context using an internal buffer with at least 
        /// <paramref name="bufferSize"/> <see cref="IBuffer.Capacity"/>.
        /// </summary>
        /// <param name="bufferSize">
        /// The minimum <see cref="IBuffer.Capacity"/>.
        /// </param>
        public FormatterContext(int bufferSize) : this(new SingleChunkBuffer(bufferSize))
        {
        }

        /// <summary>
        /// It returns or sets the current message.
        /// </summary>
        public Message CurrentMessage { get; set; }

        /// <summary>
        /// Returns the upper offset within chunks where data ends.
        /// </summary>
        public int UpperDataBound
        {
            get { return _internalBuffer.UpperDataBound; }
            set { _internalBuffer.UpperDataBound = value; }
        }

        /// <summary>
        /// Returns the free space at the end of the buffer.
        /// </summary>
        public int FreeBufferSpace
        {
            get { return _internalBuffer.FreeSpaceAtTheEnd; }
        }

        /// <summary>
        /// Returns total buffer capacity.
        /// </summary>
        public int BufferSize
        {
            get { return _internalBuffer.Capacity; }
        }

        /// <summary>
        /// Returns stored data length.
        /// </summary>
        public int DataLength
        {
            get { return _internalBuffer.DataLength; }
        }

        /// <summary>
        /// Returns the internal buffer used to store data.
        /// </summary>
        public IBuffer Buffer
        {
            get { return _internalBuffer; }
        }

        /// <summary>
        /// Access the underlying byte array of the buffer.
        /// </summary>
        /// <returns>
        /// A byte array used by the buffer.
        /// </returns>
        /// <remarks>
        /// Some <see cref="IBuffer"/> don't support this operation, instead we recommend
        /// to use <see cref="Buffer"/> property.
        /// </remarks>
        [Obsolete("Use Buffer property instead")]
        public byte[] GetBuffer()
        {
            return _internalBuffer.GetArray();
        }

        /// <summary>
        /// Expand capacity adding the given length.
        /// </summary>
        public void ResizeBuffer(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count",
                    count, "count must be grater than zero");

            _internalBuffer.Capacity += (count%DefaultBufferSize) == 0
                ? count
                : ((count/DefaultBufferSize) + 1)*DefaultBufferSize;
        }

        /// <summary>
        /// Clear the data in the buffer.
        /// </summary>
        public void Clear()
        {
            _internalBuffer.Clear();
        }

        /// <summary>
        /// Clear the data in the buffer, setting as used the given <paramref name="reserve"/> bytes.
        /// </summary>
        /// <param name="reserve">
        /// Set as used the given count of bytes.
        /// </param>
        [Obsolete(
            "Use IBuffer write prepend features instead of reserving space in the buffer and access internal backing array"
            )]
        public void Clear(int reserve)
        {
            if (_internalBuffer.Capacity < reserve)
                ResizeBuffer(reserve);

            _internalBuffer.LowerDataBound = 0;
            _internalBuffer.UpperDataBound = reserve;
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="data">
        /// The data to write.
        /// </param>
        public void Write(string data)
        {
            _internalBuffer.Write(false, data);
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="data">
        /// The data to write.
        /// </param>
        /// <param name="offset">
        /// Offset within the given data.
        /// </param>
        /// <param name="count">
        /// Bytes to write.
        /// </param>
        public void Write(string data, int offset, int count)
        {
            _internalBuffer.Write(false, _internalBuffer.Encoding.GetBytes(data), offset, count);
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="data">
        /// The data to write.
        /// </param>
        public void Write(byte[] data)
        {
            _internalBuffer.Write(false, data);
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="data">
        /// The data to write.
        /// </param>
        /// <param name="offset">
        /// Offset within the given buffer.
        /// </param>
        /// <param name="count">
        /// Bytes to write.
        /// </param>
        public void Write(byte[] data, int offset, int count)
        {
            _internalBuffer.Write(false, data, offset, count);
        }

        /// <summary>
        /// Read data from the buffer not consuming it.
        /// </summary>
        /// <returns>
        /// The readed data.
        /// </returns>
        public byte[] GetData()
        {
            return _internalBuffer.Read(false);
        }

        /// <summary>
        /// Read data from the buffer not consuming it.
        /// </summary>
        /// <returns>
        /// The readed data.
        /// </returns>
        public string GetDataAsString()
        {
            return _internalBuffer.ReadAsString(false);
        }

        /// <summary>
        /// Initializes the context.
        /// </summary>
        public void Initialize()
        {
            Clear();
        }
    }
}