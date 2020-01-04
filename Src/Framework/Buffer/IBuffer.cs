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

namespace Trx.Buffer
{
    /// <summary>
    /// Defines the interface for buffers used by Trx Framework.
    /// </summary>
    public interface IBuffer : IDisposable
    {
        /// <summary>
        /// Access the byte at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of the data.
        /// </param>
        /// <returns>
        /// The data at the given index.
        /// </returns>
        byte this[int index] { get; }

        /// <summary>
        /// It returns true when the data is stored internally in more than one chunk.
        /// </summary>
        bool MultiChunk { get; }

        /// <summary>
        /// Returns the lower offset within chunks where data starts.
        /// </summary>
        int LowerDataBound { get; set; }

        /// <summary>
        /// Returns the upper offset within chunks where data ends.
        /// </summary>
        int UpperDataBound { get; set; }

        /// <summary>
        /// Returns stored data length.
        /// </summary>
        int DataLength { get; }

        /// <summary>
        /// Returns total buffer capacity.
        /// </summary>
        int Capacity { get; set; }

        /// <summary>
        /// Returns the free space at the beginning of the buffer.
        /// </summary>
        int FreeSpaceAtTheBeginning { get; }

        /// <summary>
        /// Returns the free space at the end of the buffer.
        /// </summary>
        int FreeSpaceAtTheEnd { get; }

        /// <summary>
        /// Returns true if data can be written in the buffer, otherwise false (read only buffer).
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Get or sets the encoding used to transform bytes to/from string.
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// Returns true when the buffer is disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Add a secure area.
        /// </summary>
        /// <param name="secureArea">
        /// The area to add.
        /// </param>
        void AddSecureArea(BufferSecureArea secureArea);

        /// <summary>
        /// Check if a given index is in a secure area.
        /// </summary>
        /// <param name="index">
        /// The index to check.
        /// </param>
        /// <returns>
        /// True if the given index is in a secure are, otherwise false.
        /// </returns>
        bool InSecureArea(int index);

        /// <summary>
        /// Forget the secure areas.
        /// </summary>
        void ForgetSecureAreas();

        /// <summary>
        /// If the buffer is not chunked returns the underlying byte array holding the data.
        /// </summary>
        /// <returns>
        /// A byte array where data is stored.
        /// </returns>
        byte[] GetArray();

        /// <summary>
        /// Returns a list with array segments of the free space at the end of the buffer.
        /// </summary>
        /// <returns>
        /// A list of <see ref="ArraySegment"/> with the free space of the buffer at the end of it.
        /// </returns>
        IList<ArraySegment<byte>> GetFreeSegments();

        /// <summary>
        /// Returns a list with array segments containing the data.
        /// </summary>
        /// <param name="dataLength">
        /// Maximum data length of the returned data. Cannot be greater than DataLength.
        /// </param>
        /// <returns>
        /// A list of <see ref="ArraySegment"/> with the data in the buffer.
        /// </returns>
        IList<ArraySegment<byte>> GetDataSegments(int dataLength);

        /// <summary>
        /// Clear the data in the buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Expand capacity using a length computed by the buffer implementation.
        /// </summary>
        void Expand();

        /// <summary>
        /// Move data stored in the buffer to the beginning of it.
        /// </summary>
        void Compact();

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="prepend">
        /// If true, writes the data behind the data already stored in the buffer.
        /// </param>
        /// <param name="data">
        /// The data to write.
        /// </param>
        void Write(bool prepend, string data);

        /// <summary>
        /// Writes data in the buffer.
        /// </summary>
        /// <param name="prepend">
        /// If true, writes the data behind the data already stored in the buffer.
        /// </param>
        /// <param name="data">
        /// The data to write.
        /// </param>
        void Write(bool prepend, byte[] data);

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
        void Write(bool prepend, byte[] data, int offset, int count);

        /// <summary>
        /// Discard the given count of bytes from the buffer.
        /// </summary>
        /// <param name="count">
        /// Number of bytes to discard.
        /// </param>
        void Discard(int count);

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        byte[] Read(bool consume);

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
        byte[] Read(bool consume, int count);

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
        int Read(bool consume, byte[] buffer, int offset, int count);

        /// <summary>
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        string ReadAsString(bool consume);

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
        string ReadAsString(bool consume, int count);
    }
}