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
using NUnit.Framework;
using Trx.Buffer;
using Trx.Utilities;

namespace Tests.Trx.Buffer
{
    public class SingleChunkBufferTest
    {
        [Test(Description = "Test single chunk buffer constructors and initialization.")]
        public void ConstructorAndProperties()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => new SingleChunkBuffer(null));
            Assert.That(ex1.ParamName, Is.EqualTo("chunkManager"));

            var buffer = new SingleChunkBuffer();

            Assert.IsFalse(buffer.MultiChunk);
            Assert.AreEqual(0, buffer.LowerDataBound);
            Assert.AreEqual(0, buffer.UpperDataBound);
            Assert.AreEqual(0, buffer.DataLength);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, buffer.Capacity);
            Assert.IsTrue(buffer.CanWrite);
            Assert.AreEqual(FrameworkEncoding.GetInstance().Encoding, buffer.Encoding);
            Assert.AreEqual(0, buffer.FreeSpaceAtTheBeginning);
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize, buffer.FreeSpaceAtTheEnd);
            buffer.LowerDataBound = 5;
            Assert.AreEqual(5, buffer.FreeSpaceAtTheBeginning);
            buffer.UpperDataBound = 10;
            Assert.AreEqual(SingleChunkBuffer.DefaultChunkSize - 10, buffer.FreeSpaceAtTheEnd);

            var chunkManager = new SimpleChunkManager(1024);
            buffer = new SingleChunkBuffer(chunkManager);
            Assert.AreEqual(1024, buffer.Capacity);

            buffer = new SingleChunkBuffer(2048);
            Assert.AreEqual(2048, buffer.Capacity);

            buffer.LowerDataBound = 10;
            Assert.AreEqual(10, buffer.LowerDataBound);
            buffer.UpperDataBound = 20;
            Assert.AreEqual(20, buffer.UpperDataBound);

            buffer.Encoding = Encoding.ASCII;
            Assert.AreEqual(Encoding.ASCII, buffer.Encoding);

            ex1 = Assert.Throws<ArgumentNullException>(() => buffer.Encoding = null);
            Assert.That(ex1.ParamName, Is.EqualTo("value"));

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Capacity = 0);
            Assert.That(ex2.ParamName, Is.EqualTo("value"));
            Assert.That(ex2.Message.Substring(0, 33), Is.EqualTo("Capacity must be grater than zero"));

            var bytes = buffer.GetArray();
            buffer.Capacity = buffer.Capacity;
            Assert.AreSame(bytes, buffer.GetArray());

            buffer.Write(false, new byte[] { 0, 1, 2 });
            ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Capacity = 2);
            Assert.That(ex2.ParamName, Is.EqualTo("value"));
            Assert.That(ex2.Message.Substring(0, 55), Is.EqualTo("New capacity is smaller than current stored data length"));

            int newCapacity = buffer.Capacity*2;
            buffer.Capacity = newCapacity;
            Assert.AreEqual(newCapacity, buffer.Capacity);
            Assert.AreNotSame(bytes, buffer.GetArray());

        }

        [Test(Description = "GetArray/GetFreeSegments test")]
        public void GetArray()
        {
            var buffer = new SingleChunkBuffer();
            byte[] bytes = buffer.GetArray();
            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length == buffer.Capacity);
        }

        [Test(Description = "GetFreeSegments/GetDataSegments test")]
        public void GetSegments()
        {
            var buffer = new SingleChunkBuffer();
            byte[] bytes = buffer.GetArray();
            IList<ArraySegment<byte>> chunks = buffer.GetFreeSegments();
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.Capacity, chunks[0].Count);

            buffer.UpperDataBound = 20;
            chunks = buffer.GetFreeSegments();
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.Capacity - 20, chunks[0].Count);
            Assert.AreEqual(buffer.UpperDataBound, chunks[0].Offset);

            chunks = buffer.GetDataSegments(buffer.DataLength);
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.LowerDataBound, chunks[0].Offset);
            Assert.AreEqual(buffer.DataLength, chunks[0].Count);

            chunks = buffer.GetDataSegments(buffer.DataLength * 10);
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.LowerDataBound, chunks[0].Offset);
            Assert.AreEqual(buffer.DataLength, chunks[0].Count);

            chunks = buffer.GetDataSegments(1);
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.LowerDataBound, chunks[0].Offset);
            Assert.AreEqual(1, chunks[0].Count);

            buffer.LowerDataBound = 5;
            chunks = buffer.GetDataSegments(buffer.DataLength);
            Assert.IsNotNull(chunks);
            Assert.AreEqual(1, chunks.Count);
            Assert.AreSame(chunks[0].Array, bytes);
            Assert.AreEqual(buffer.LowerDataBound, chunks[0].Offset);
            Assert.AreEqual(buffer.DataLength, chunks[0].Count);
        }

        public bool ArrayEquals(byte[] arr1, int arr1Offset, byte[] arr2, int arr2Offset, int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            for (int i = arr1Offset - 1 + count; i >= 0; i--)
                if (arr1[i] != arr2[arr2Offset + i])
                    return false;

            return true;
        }

        [Test(Description = "Discard test")]
        public void Discard()
        {
            // Test parameters.
            var buffer = new SingleChunkBuffer();
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Discard(-1));
            Assert.That(ex.ParamName, Is.EqualTo("count"));
            Assert.That(ex.Message.Substring(0, 18), Is.EqualTo("Cannot be negative"));

            buffer.Write(false, "012345");

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Discard(7));
            Assert.That(ex.ParamName, Is.EqualTo("count"));
            Assert.That(ex.Message.Substring(0, 21), Is.EqualTo("Invalid consumed data"));

            buffer.Discard(4);
            Assert.AreEqual(4, buffer.LowerDataBound);
            Assert.AreEqual(6, buffer.UpperDataBound);

            buffer.Discard(2);
            Assert.AreEqual(0, buffer.LowerDataBound);
            Assert.AreEqual(0, buffer.UpperDataBound);
        }

        [Test(Description = "Dispose test")]
        public void Dispose()
        {
            var buffer = new SingleChunkBuffer();
            buffer.Dispose();
            buffer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => buffer.Capacity = 10);
            Assert.Throws<ObjectDisposedException>(() => buffer.GetArray());
            Assert.Throws<ObjectDisposedException>(() => buffer.GetFreeSegments());
            Assert.Throws<ObjectDisposedException>(() => buffer.GetDataSegments(buffer.DataLength));
            Assert.Throws<ObjectDisposedException>(buffer.Clear);
            Assert.Throws<ObjectDisposedException>(() => buffer.Write(false, "123"));
            Assert.Throws<ObjectDisposedException>(() => buffer.Write(false, new byte[] {0x31, 0x32, 0x33}));
            Assert.Throws<ObjectDisposedException>(() => buffer.Write(false, new byte[] {0x31, 0x32, 0x33}, 0, 3));
            Assert.Throws<ObjectDisposedException>(() => buffer.Discard(3));
            Assert.Throws<ObjectDisposedException>(() => buffer.Read(true));
            Assert.Throws<ObjectDisposedException>(() => buffer.Read(true, 3));
            Assert.Throws<ObjectDisposedException>(() => buffer.Read(true, new byte[5], 0, 5));
            Assert.Throws<ObjectDisposedException>(() => buffer.ReadAsString(true));
            Assert.Throws<ObjectDisposedException>(() => buffer.ReadAsString(true, 3));
        }

        [Test(Description = "Write test")]
        public void Write()
        {
            var buffer = new SingleChunkBuffer();
            var data1 = new byte[] {0, 1, 2, 3, 4, 5};
            var data2 = new byte[] {6, 7, 8, 9, 10, 11, 12};

            // Test parameters.
            var ex1 = Assert.Throws<ArgumentNullException>(() => new SingleChunkBuffer().Write(false, (byte[])null));
            Assert.That(ex1.ParamName, Is.EqualTo("data"));

            ex1 = Assert.Throws<ArgumentNullException>(() => new SingleChunkBuffer().Write(false, null, 0, 0));
            Assert.That(ex1.ParamName, Is.EqualTo("data"));

            buffer.Write(false, data1, 0, 0);
            Assert.AreEqual(0, buffer.DataLength);

            var ex2 = Assert.Throws<ArgumentException>(() => new SingleChunkBuffer().Write(false, data1, -1, data1.Length));
            Assert.That(ex2.ParamName, Is.EqualTo("offset"));
            Assert.That(ex2.Message.Substring(0, 25), Is.EqualTo("Offset is not within data"));

            ex2 = Assert.Throws<ArgumentException>(() => new SingleChunkBuffer().Write(false, data1, data1.Length, data1.Length));
            Assert.That(ex2.ParamName, Is.EqualTo("offset"));
            Assert.That(ex2.Message.Substring(0, 25), Is.EqualTo("Offset is not within data"));

            ex2 = Assert.Throws<ArgumentException>(() => new SingleChunkBuffer().Write(false, data1, 1, data1.Length));
            Assert.That(ex2.ParamName, Is.EqualTo("count"));
            Assert.That(ex2.Message.Substring(0, 33), Is.EqualTo("Not enough bytes in data to write"));

            // Simple write with enough space in buffer
            buffer.Write(false, data1);
            Assert.AreEqual(data1.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), 0, data1.Length));

            buffer.Write(false, data2);
            Assert.AreEqual( data1.Length + data2.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data2, 0, buffer.GetArray(), data1.Length, data2.Length));

            buffer.Clear();
            Assert.AreEqual(0, buffer.DataLength);

            // Write in a buffer with no enough space, resize expected
            buffer = new SingleChunkBuffer(data1.Length);
            Assert.AreEqual(data1.Length, buffer.Capacity);

            buffer.Write(false, data1);
            Assert.AreEqual(data1.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), 0, data1.Length));

            buffer.Write(false, data2);
            Assert.AreEqual(data1.Length + data2.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data2, 0, buffer.GetArray(), data1.Length, data2.Length));

            // Prepend a write with no data
            buffer = new SingleChunkBuffer(data1.Length);
            Assert.AreEqual(data1.Length, buffer.Capacity);

            buffer.Write(true, data1);
            Assert.AreEqual(data1.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), 0, data1.Length));

            // Prepend a write in a buffer with no enough space, resize expected
            buffer = new SingleChunkBuffer(data1.Length);
            Assert.AreEqual(data1.Length, buffer.Capacity);

            buffer.Write(false, data1);
            Assert.AreEqual(data1.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), 0, data1.Length));

            buffer.Write(true, data2);
            Assert.AreEqual(data1.Length + data2.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data2, 0, buffer.GetArray(), 0, data2.Length));
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), data2.Length, data1.Length));

            // Write in a buffer with no enough space, resize expected
            buffer = new SingleChunkBuffer(data1.Length);
            Assert.AreEqual(data1.Length, buffer.Capacity);

            buffer.Write(false, data1);
            buffer.Write(false, data1);
            Assert.AreEqual(data1.Length * 2, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), 0, data1.Length));
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), data1.Length, data1.Length));

            buffer.Discard(data1.Length);
            Assert.AreEqual(data1.Length, buffer.DataLength);
            Assert.AreEqual(data1.Length, buffer.LowerDataBound);

            buffer.Write(true, data2);
            Assert.AreEqual(data1.Length + data2.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data2, 0, buffer.GetArray(), 0, data2.Length));
            Assert.IsTrue(ArrayEquals(data1, 0, buffer.GetArray(), data2.Length, data1.Length));

            // Test string writing
            var data3 = new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35 };
            buffer = new SingleChunkBuffer();

            buffer.Write(false, "012345");
            Assert.AreEqual(data3.Length, buffer.DataLength);
            Assert.IsTrue(ArrayEquals(data3, 0, buffer.GetArray(), 0, data3.Length));
        }

        [Test(Description = "Read test")]
        public void Read()
        {
            var buffer = new SingleChunkBuffer();

            Assert.IsNull(buffer.Read(true));
            Assert.IsNull(buffer.Read(true, 0));
            Assert.IsNull(buffer.ReadAsString(true));
            Assert.IsNull(buffer.ReadAsString(true, 0));

            var ex = Assert.Throws<ArgumentException>(() => buffer.Read(true, 1));
            Assert.That(ex.ParamName, Is.EqualTo("count"));
            Assert.That(ex.Message.Substring(0, 15), Is.EqualTo("Not enough data"));

            ex = Assert.Throws<ArgumentException>(() => buffer.ReadAsString(true, 1));
            Assert.That(ex.ParamName, Is.EqualTo("count"));
            Assert.That(ex.Message.Substring(0, 15), Is.EqualTo("Not enough data"));

            var data = new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35 };

            buffer.Write(false, data);

            // Read all without discarding data
            Assert.IsTrue(ArrayEquals(data, 0, buffer.Read(false), 0, data.Length));
            Assert.IsTrue(ArrayEquals(data, 0, buffer.GetArray(), 0, data.Length));
            Assert.AreEqual(data.Length, buffer.DataLength);

            Assert.AreEqual("012345", buffer.ReadAsString(false));
            Assert.IsTrue(ArrayEquals(data, 0, buffer.GetArray(), 0, data.Length));
            Assert.AreEqual(data.Length, buffer.DataLength);

            // Read all discarding data
            Assert.IsTrue(ArrayEquals(data, 0, buffer.Read(true), 0, data.Length));
            Assert.AreEqual(0, buffer.DataLength);

            buffer.Write(false, data);

            Assert.AreEqual("012345", buffer.ReadAsString(true));
            Assert.AreEqual(0, buffer.DataLength);

            // Read count discarding data
            buffer.Write(false, data);

            Assert.IsTrue(ArrayEquals(data, 0, buffer.Read(true, 2), 0, 2));
            Assert.AreEqual(4, buffer.DataLength);

            Assert.AreEqual("234", buffer.ReadAsString(true, 3));
            Assert.AreEqual(1, buffer.DataLength);

            // Read in a destination buffer.
            buffer.Clear();
            buffer.Write(false, data);
            var dstBuffer = new byte[6];
            Assert.AreEqual(6, buffer.Read(true, dstBuffer, 0, 6));
            Assert.AreEqual("012345", Encoding.Default.GetString(dstBuffer));

            // Read with offset.
            dstBuffer = new byte[7];
            dstBuffer[0] = (byte)'!';
            buffer.Write(false, data);
            Assert.AreEqual(6, buffer.Read(true, dstBuffer, 1, 6));
            Assert.AreEqual("!012345", Encoding.Default.GetString(dstBuffer));

            ex = Assert.Throws<ArgumentException>(() => buffer.Read(false, dstBuffer, 2, 6));
            Assert.That(ex.Message.Substring(0, 37), Is.EqualTo("Not enough data in destination buffer"));

            var exr = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Read(false, dstBuffer, -1, 6));
            Assert.That(exr.ParamName, Is.EqualTo("offset"));
            Assert.That(exr.Message.Substring(0, 28), Is.EqualTo("Can not be a negative number"));

            exr = Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Read(false, dstBuffer, 2, -1));
            Assert.That(exr.ParamName, Is.EqualTo("count"));
            Assert.That(exr.Message.Substring(0, 28), Is.EqualTo("Can not be a negative number"));
        }
    }
}