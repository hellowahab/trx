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
using System.Text;
using Trx.Utilities;
using Trx.Buffer;

namespace Trx.Messaging
{
    /// <summary>
    /// Parser context used by the <see cref="IMessageFormatter"/>.
    /// </summary>
    public class ParserContext
    {
        public const int DefaultBufferSize = 2048;

        private readonly IBuffer _internalBuffer;

        /// <summary>
        /// Creates a new parser context using the given <see cref="IBuffer"/>.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to use.
        /// </param>
        public ParserContext(IBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            _internalBuffer = buffer;

            DecodedLength = int.MinValue;
            AnnouncementHasBeenConsumed = false;
            MessageHasBeenConsumed();
        }

        /// <summary>
        /// Creates a new parser context using an internal buffer with at least 
        /// <paramref name="bufferSize"/> <see cref="IBuffer.Capacity"/>.
        /// </summary>
        /// <param name="bufferSize">
        /// The minimum <see cref="IBuffer.Capacity"/>.
        /// </param>
        public ParserContext(int bufferSize)
            : this(new SingleChunkBuffer(bufferSize))
        {
        }

        /// <summary>
        /// Returns or sets the current message.
        /// </summary>
        public Message CurrentMessage { get; set; }

        /// <summary>
        /// Returns or sets the current parsed field.
        /// </summary>
        public int CurrentField { get; set; }

        /// <summary>
        /// Returns or sets the current parsed field length.
        /// </summary>
        public int DecodedLength { get; set; }

        /// <summary>
        /// Returns or sets the current bitmap.
        /// </summary>
        public BitMapField CurrentBitMap { get; set; }

        /// <summary>
        /// Returns or sets an utility flag.
        /// </summary>
        /// <remarks>
        /// <see cref="BasicMessageFormatter"/> uses this flag to ckeck if
        /// subclasses have been notified about the message header analysis.
        /// By default this property has a false value and is reset each time
        /// <see cref="MessageHasBeenConsumed"/> is called.
        /// </remarks>
        public bool Signaled { get; set; }

        /// <summary>
        /// Returns or sets a flag indicating the packet header has been removed.
        /// </summary>
        /// <remarks>
        /// By default this property has a false value and is reset each time
        /// <see cref="MessageHasBeenConsumed"/> is called.
        /// </remarks>
        public bool PacketHeaderDataStripped { get; set; }

        /// <summary>
        /// It returns or sets a payload.
        /// </summary>
        /// <remarks>
        /// By default this property has a null value and is reset each time
        /// <see cref="MessageHasBeenConsumed"/> is called.
        /// </remarks>
        public object Payload { get; set; }

        /// <summary>
        /// Returns the upper offset within chunks where data ends.
        /// </summary>
        public int UpperDataBound
        {
            get { return _internalBuffer.UpperDataBound; }
            set { _internalBuffer.UpperDataBound = value; }
        }

        /// <summary>
        /// Returns the lower offset within chunks where data starts.
        /// </summary>
        public int LowerDataBound
        {
            get { return _internalBuffer.LowerDataBound; }
            set { _internalBuffer.LowerDataBound = value; }
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
        /// It returns or sets the flag which indicate if the field announcement has 
        /// been consumed.
        /// </summary>
        public bool AnnouncementHasBeenConsumed { get; set; }

        /// <summary>
        /// It returns or sets the upper bound in the buffer indicating the last byte of
        /// the last received frame.
        /// </summary>
        public int FrontierUpperBound { get; set; }

        /// <summary>
        /// Returns the internal buffer used to store data.
        /// </summary>
        public IBuffer Buffer
        {
            get { return _internalBuffer; }
        }

        /// <summary>
        /// Set by the <see cref="Trx.Communication.Channels.Sinks.MessageFormatterSink"/> to
        /// notify the <see cref="IMessageFormatter"/> the frame size detected by a frame delimiter
        /// sink.
        /// </summary>
        public int FrameSize { get; set; }

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

            _internalBuffer.Capacity += (count % DefaultBufferSize) == 0
                ? count
                : ((count / DefaultBufferSize) + 1) * DefaultBufferSize;
        }

        /// <summary>
        /// Clear the data in the buffer.
        /// </summary>
        public void Clear()
        {
            _internalBuffer.Clear();
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
        /// Read data from the buffer.
        /// </summary>
        /// <param name="consume">
        /// If true, the data is removed from the buffer.
        /// </param>
        /// <returns>
        /// The readed data.
        /// </returns>
        public byte[] GetData(bool consume)
        {
            return _internalBuffer.Read(consume);
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
        public byte[] GetData(bool consume, int count)
        {
            return _internalBuffer.Read(consume, count);
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
        public string GetDataAsString(bool consume)
        {
            return _internalBuffer.ReadAsString(consume);
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
        public string GetDataAsString(bool consume, int count)
        {
            return _internalBuffer.ReadAsString(consume, count);
        }

        /// <summary>
        /// Called to inform the context a deconded length has been parsed.
        /// </summary>
        public void ResetDecodedLength()
        {
            DecodedLength = int.MinValue;
            AnnouncementHasBeenConsumed = false;
        }

        /// <summary>
        /// Called to inform the context a message has been parsed.
        /// </summary>
        public void MessageHasBeenConsumed()
        {
            CurrentMessage = null;
            CurrentField = 0;
            CurrentBitMap = null;
            Signaled = false;
            PacketHeaderDataStripped = false;
            Payload = null;
            FrontierUpperBound = int.MinValue;
            FrameSize = 0;
        }

        /// <summary>
        /// Discard the given count of bytes from the buffer.
        /// </summary>
        /// <param name="count">
        /// Number of bytes to discard.
        /// </param>
        public void Consumed(int count)
        {
            _internalBuffer.Discard(count);
        }

        /// <summary>
        /// Initializes the parser.
        /// </summary>
        public void Initialize()
        {
            if (_internalBuffer.IsDisposed)
                return;

            Clear();
            ResetDecodedLength();
            MessageHasBeenConsumed();
        }

        /// <summary>
        /// Returns the string representation of the parser.
        /// </summary>
        /// <returns>
        /// An string containing internal information about the parser.
        /// </returns>
        public override string ToString()
        {
            var res = new StringBuilder();

            res.Append(string.Format(
                "ParserContext( buffer size = {0}, lower data = {1}, upper data = {2}, ",
                _internalBuffer.Capacity, _internalBuffer.LowerDataBound, _internalBuffer.UpperDataBound));

            res.Append(string.Format(
                "current message = {0}, current field = {1}, decoded length = {2}, ",
                CurrentMessage == null ? "null" : CurrentMessage.GetType().ToString(),
                CurrentField, DecodedLength));

            res.Append(string.Format(
                "current bitmap = {0}, signaled = {1}, ",
                CurrentBitMap == null ? "null" : CurrentBitMap.GetType().ToString(),
                Signaled));

            res.Append(string.Format(
                "payload = {0}, packet header data stripped = {1})",
                Payload == null ? "null" : Payload.GetType().ToString(),
                PacketHeaderDataStripped));

            if (_internalBuffer.DataLength > 0)
            {
                res.Append(Environment.NewLine);
                res.Append(StringUtilities.DumpBufferData("Data in buffer:",
                    _internalBuffer, _internalBuffer.LowerDataBound, _internalBuffer.DataLength));
            }

            return res.ToString();
        }
    }
}