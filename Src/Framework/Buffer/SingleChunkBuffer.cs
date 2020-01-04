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
using System.Text;
using Trx.Utilities;

namespace Trx.Buffer
{
    /// <summary>
    /// Simple buffer using only one chunk.
    /// </summary>
    public class SingleChunkBuffer : IBuffer
    {
        public const int DefaultChunkSize = 4096;

        private readonly IChunkManager _chunkManager;
        private ArraySegment<byte> _chunk;
        private List<BufferSecureArea> _secureAreas; 
        private Encoding _encoding;
        private int _lowerDataBound;
        private int _upperDataBound;

        public SingleChunkBuffer(IChunkManager chunkManager)
        {
            if (chunkManager == null)
                throw new ArgumentNullException("chunkManager");

            _encoding = FrameworkEncoding.GetInstance().Encoding;
            _chunkManager = chunkManager;
            _chunk = _chunkManager.CheckOut();
        }

        public SingleChunkBuffer(int chunkSize)
        {
            _encoding = FrameworkEncoding.GetInstance().Encoding;
            _chunkManager = new SimpleChunkManager(chunkSize);
            _chunk = _chunkManager.CheckOut();
        }

        public SingleChunkBuffer() : this(DefaultChunkSize)
        {
        }

        #region IBuffer Members
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            Clear();
            _chunkManager.CheckIn(_chunk);

            IsDisposed = true;
        }

        /// <summary>
        /// Access the byte at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of the data.
        /// </param>
        /// <returns>
        /// The data at the given index.
        /// </returns>
        public byte this[int index]
        {
            get
            {
                CheckDisposed();
                return _chunk.Array[index];
            }
        }

        /// <summary>
        /// It returns true when the data is stored internally in more than one chunk.
        /// </summary>
        public bool MultiChunk
        {
            get { return false; }
        }

        /// <summary>
        /// Returns the lower offset within chunks where data starts.
        /// </summary>
        public int LowerDataBound
        {
            get { return _lowerDataBound; }
            set { _lowerDataBound = value; }
        }

        /// <summary>
        /// Returns the upper offset within chunks where data ends.
        /// </summary>
        public int UpperDataBound
        {
            get { return _upperDataBound; }
            set { _upperDataBound = value; }
        }

        /// <summary>
        /// Returns stored data length.
        /// </summary>
        public int DataLength
        {
            get { return _upperDataBound - _lowerDataBound; }
        }

        /// <summary>
        /// Returns total buffer capacity.
        /// </summary>
        public int Capacity
        {
            get { return _chunk.Array.Length; }
            set
            {
                CheckDisposed();

                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "Capacity must be grater than zero");

                if (value == _chunk.Array.Length)
                    return;

                if (value < DataLength)
                    throw new ArgumentOutOfRangeException("value",
                        "New capacity is smaller than current stored data length");

                ResizeBuffer(value, 0);
            }
        }

        /// <summary>
        /// Returns the free space at the beginning of the buffer.
        /// </summary>
        public int FreeSpaceAtTheBeginning
        {
            get { return _lowerDataBound; }
        }

        /// <summary>
        /// Returns the free space at the end of the buffer.
        /// </summary>
        public int FreeSpaceAtTheEnd
        {
            get { return _chunk.Array.Length - _upperDataBound; }
        }

        /// <summary>
        /// Returns true if data can be written in the buffer, otherwise false (read only buffer).
        /// </summary>
        public bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Get or sets the encoding used to transform bytes to/from string.
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _encoding = value;
            }
        }

        /// <summary>
        /// Returns true when the buffer is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Add a secure area.
        /// </summary>
        /// <param name="secureArea">
        /// The area to add.
        /// </param>
        public void AddSecureArea(BufferSecureArea secureArea)
        {
            if (_secureAreas == null)
                _secureAreas = new List<BufferSecureArea>();

            _secureAreas.Add(secureArea);
        }

        /// <summary>
        /// Check if a given index is in a secure area.
        /// </summary>
        /// <param name="index">
        /// The index to check.
        /// </param>
        /// <returns>
        /// True if the given index is in a secure are, otherwise false.
        /// </returns>
        public bool InSecureArea(int index)
        {
            if (_secureAreas == null)
                return false;

            foreach (var bufferSecureArea in _secureAreas)
                if (index >= bufferSecureArea.From && index <= bufferSecureArea.To)
                    return true;

            return false;
        }

        /// <summary>
        /// Forget the secure areas.
        /// </summary>
        public void ForgetSecureAreas()
        {
            if (_secureAreas != null)
                _secureAreas.Clear();
        }

        /// <summary>
        /// If the buffer is not chunked returns the underlying byte array holding the data.
        /// </summary>
        /// <returns>
        /// A byte array where data is stored.
        /// </returns>
        public virtual byte[] GetArray()
        {
            CheckDisposed();
            return _chunk.Array;
        }

        /// <summary>
        /// Returns a list with array segments of the free space at the end of the buffer.
        /// </summary>
        /// <returns>
        /// A list of <see ref="ArraySegment"/> with the free space of the buffer at the end of it.
        /// </returns>
        public IList<ArraySegment<byte>> GetFreeSegments()
        {
            CheckDisposed();
            return new List<ArraySegment<byte>>(1) {new ArraySegment<byte>(_chunk.Array, _upperDataBound, FreeSpaceAtTheEnd)};
        }
            
        /// <summary>
        /// Returns a list with array segments containing the data.
        /// </summary>
        /// <param name="dataLength">
        /// Maximum data length of the returned data. Cannot be greater than DataLength.
        /// </param>
        /// <returns>
        /// A list of <see ref="ArraySegment"/> with the data in the buffer.
        /// </returns>
        public virtual IList<ArraySegment<byte>> GetDataSegments(int dataLength)
        {
            CheckDisposed();
            if (dataLength > DataLength)
                dataLength = DataLength;
            return new List<ArraySegment<byte>>(1) { new ArraySegment<byte>(_chunk.Array, _lowerDataBound, dataLength) };
        }

        /// <summary>
        /// Clear the data in the buffer.
        /// </summary>
        public void Clear()
        {
            CheckDisposed();
            _lowerDataBound = _upperDataBound = 0;
            ForgetSecureAreas();
        }

        /// <summary>
        /// Expand capacity using a length computed by the buffer implementation.
        /// </summary>
        public void Expand()
        {
            ResizeBuffer(Capacity + DefaultChunkSize, 0);
        }

        /// <summary>
        /// Move data stored in the buffer to the beginning of it.
        /// </summary>
        public void Compact()
        {
            if (_lowerDataBound == 0)
                return;

            byte[] array = _chunk.Array;
            for (int i = _lowerDataBound; i < _upperDataBound; i++)
                array[i - _lowerDataBound] = array[i];

            if (_secureAreas != null)
                foreach (var bufferSecureArea in _secureAreas)
                {
                    bufferSecureArea.From -= _lowerDataBound;
                    bufferSecureArea.To -= _lowerDataBound;
                }

            _upperDataBound = DataLength;
            _lowerDataBound = 0;
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="prepend">
        /// If true, writes the data behind the data already stored in the buffer.
        /// </param>
        /// <param name="data">
        /// The data to write.
        /// </param>
        public void Write(bool prepend, string data)
        {
            byte[] bytes = _encoding.GetBytes(data);
            Write(prepend, bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="prepend">
        /// If true, writes the data behind the data already stored in the buffer.
        /// </param>
        /// <param name="data">
        /// The data to write.
        /// </param>
        public void Write(bool prepend, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Write(prepend, data, 0, data.Length);
        }

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="prepend">
        /// If true, writes the data behind the data already stored in the buffer.
        /// </param>
        /// <param name="data">
        /// The data to write.
        /// </param>
        /// <param name="offset">
        /// Offset within the given buffer.
        /// </param>
        /// <param name="count">
        /// Bytes to write.
        /// </param>
        public void Write(bool prepend, byte[] data, int offset, int count)
        {
            CheckDisposed();

            if (data == null)
                throw new ArgumentNullException("data");

            if (count <= 0)
                return;

            if (offset < 0 || offset >= data.Length)
                throw new ArgumentException("Offset is not within data", "offset");

            if ((offset + count) > data.Length)
                throw new ArgumentException("Not enough bytes in data to write", "count");

            if (FreeSpaceAtTheBeginning + FreeSpaceAtTheEnd < count)
                ResizeBuffer(Capacity + (((count % DefaultChunkSize) == 0)
                    ? count
                    : (((count / DefaultChunkSize) + 1) * DefaultChunkSize)), prepend ? count : 0);

            byte[] chunk = _chunk.Array;
            int writeOffset;
            if (prepend)
            {
                if (DataLength > 0)
                    if (_lowerDataBound < count)
                    {
                        // Shift stored data to prepend new one
                        int shift = count - _lowerDataBound;
                        for (int i = _upperDataBound - 1; i >= _lowerDataBound; i--)
                            chunk[i + shift] = chunk[i];
                        _lowerDataBound = 0;
                        _upperDataBound += shift;
                    }
                    else
                        _lowerDataBound -= count;
                else
                    _upperDataBound += count;
                writeOffset = _lowerDataBound;
            }
            else
            {
                if (FreeSpaceAtTheEnd < count)
                    Compact();
                writeOffset = _upperDataBound;
                _upperDataBound += count;
            }

            // Write data.
            for (int i = 0; i < count; i++)
                chunk[writeOffset + i] = data[offset + i];
        }

        /// <summary>
        /// Discard the given count of bytes from the buffer.
        /// </summary>
        /// <param name="count">
        /// Number of bytes to discard.
        /// </param>
        public virtual void Discard(int count)
        {
            CheckDisposed();

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "Cannot be negative");

            _lowerDataBound += count;
            if (_lowerDataBound > _upperDataBound)
            {
                _lowerDataBound -= count;
                throw new ArgumentOutOfRangeException("count", count, "Invalid consumed data");
            }

            if (_lowerDataBound == _upperDataBound)
                _lowerDataBound = _upperDataBound = 0;
        }

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        public byte[] Read(bool consume)
        {
            CheckDisposed();

            if (DataLength == 0)
                return null;

            var buffer = new byte[DataLength];
            GetData(consume, buffer, 0, DataLength);
            return buffer;
        }

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <param name="count">
        /// Bytes to read.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        public byte[] Read(bool consume, int count)
        {
            CheckDisposed();
            
            if (count == 0)
                return null;

            if (DataLength < count)
                throw new ArgumentException("Not enough data", "count");

            var buffer = new byte[count];
            GetData(consume, buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// Read data from the buffer into the given byte array.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <param name="buffer">
        /// Destination buffer.
        /// </param>
        /// <param name="offset">
        /// Offset within the given buffer.
        /// </param>
        /// <param name="count">
        /// Numberof bytes to read.
        /// </param>
        /// <returns>
        /// The number of bytes read.
        /// </returns>
        public int Read(bool consume, byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Can not be a negative number");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Can not be a negative number");

            if (buffer.Length - offset < count)
                throw new ArgumentException("Not enough data in destination buffer");

            if (DataLength < count)
                count = DataLength;

            GetData(consume, buffer, offset, count);

            return count;
        }

        private void GetData(bool consume, byte[] buffer, int offset, int count)
        {
            byte[] chunk = _chunk.Array;
            for (int i = (_lowerDataBound + count - 1); i >= _lowerDataBound; i--)
                buffer[i - _lowerDataBound + offset] = chunk[i];

            if (consume)
                Discard(count);
        }

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        public string ReadAsString(bool consume)
        {
            return GetDataAsString(consume, DataLength);
        }

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <param name="count">
        /// Bytes to read.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        public string ReadAsString(bool consume, int count)
        {
            return GetDataAsString(consume, count);
        }
        #endregion

        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Buffer is disposed");
        }

        private void ResizeBuffer(int newLength, int destinationLowerDataBound)
        {
            ArraySegment<byte> chunk = _chunkManager.CheckOut(newLength);
            int dataLength = DataLength;
            Array.Copy(_chunk.Array, _lowerDataBound, chunk.Array, destinationLowerDataBound, dataLength);
            if (_secureAreas != null && _lowerDataBound != destinationLowerDataBound)
                foreach (var bufferSecureArea in _secureAreas)
                {
                    bufferSecureArea.From -= _lowerDataBound;
                    bufferSecureArea.To -= _lowerDataBound;
                }
            _chunkManager.CheckIn(_chunk);
            _chunk = chunk;
            _lowerDataBound = destinationLowerDataBound;
            _upperDataBound = _lowerDataBound + dataLength;
        }

        private string GetDataAsString(bool consume, int count)
        {
            CheckDisposed();

            if (count == 0)
                return null;

            if (DataLength < count)
                throw new ArgumentException("Not enough data", "count");

            string data = _encoding.GetString(_chunk.Array, _lowerDataBound, count);
            if (consume)
                Discard(count);

            return data;
        }
    }
}