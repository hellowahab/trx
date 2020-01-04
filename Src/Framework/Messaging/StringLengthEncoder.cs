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

namespace Trx.Messaging {

	public class StringLengthEncoder : ILengthEncoder {

		// One for each supported size, if more are required only
		// enlarge the instances array.
		private static volatile StringLengthEncoder[] _instances = {
			null, null, null, null, null, null
		};
		private static readonly int[] Lengths = { 9, 99, 999, 9999, 99999, 999999};

		private readonly int _lengthsIndex;

		private StringLengthEncoder( int lengthsIndex) {

			_lengthsIndex = lengthsIndex;
		}

		public int MaximumLength {

			get {

				return Lengths[_lengthsIndex];
			}
		}

		public static StringLengthEncoder GetInstance( int maximumLength) {

			if ( maximumLength < 0) {
				throw new ArgumentOutOfRangeException( "maximumLength", maximumLength,
					"Can't be lower than zero.");
			}

			if ( maximumLength > Lengths[Lengths.Length - 1]) {
				throw new ArgumentOutOfRangeException( "maximumLength", maximumLength,
					string.Format("Only 0 to {0} is allowed.", Lengths[Lengths.Length - 1]));
			}

			int index = 0;
			for ( ; index < Lengths.Length; index++) {
				if ( maximumLength <= Lengths[index]) {
					break;
				}
			}

			if ( _instances[index] == null) {
				lock ( typeof( StringLengthEncoder)) {
					if ( _instances[index] == null) {
						_instances[index] = new StringLengthEncoder( index);
					}
				}
			}

			return _instances[index];
		}

		#region ILengthEncoder Members
		public int EncodedLength {

			get {

				return _lengthsIndex + 1;
			}
		}

		public void Encode( int length, ref FormatterContext formatterContext) {

			if ( length > Lengths[_lengthsIndex]) {
				throw new ArgumentOutOfRangeException( "length", length,
					string.Format("Must be less or equal than {0}.", Lengths[_lengthsIndex]));
			}

			// Check if we must resize our buffer.
			if ( formatterContext.FreeBufferSpace < ( _lengthsIndex + 1)) {
				formatterContext.ResizeBuffer( _lengthsIndex + 1);
			}

			byte[] buffer = formatterContext.GetBuffer();

			// Write encoded length.
			for ( int i = formatterContext.UpperDataBound + _lengthsIndex;
				i >= formatterContext.UpperDataBound; i--) {
				buffer[i] = ( byte)( length % 10 + 0x30);
				length /= 10;
			}

			// Update formatter context upper data bound.
			formatterContext.UpperDataBound += _lengthsIndex + 1;
		}

		public int Decode( ref ParserContext parserContext) {

			// Check available data.
			if ( parserContext.DataLength < ( _lengthsIndex + 1)) {
				throw new ArgumentException( "Insufficient data.", "parserContext");
			}

			int length = 0;
			byte[] buffer = parserContext.GetBuffer();
			int offset = parserContext.LowerDataBound;

			// Decode length.
			for ( int i = offset; i < ( offset + _lengthsIndex + 1); i++) {

				if ( ( buffer[i] < 0x30) || ( buffer[i] > 0x39)) {
					throw new MessagingException( string.Format("Invalid length detected, expecting a digit but {0} ASCII code was found.", buffer[i]));
				}

				length = length * 10 + buffer[i] - 0x30;
			}

			// Consume parser context data.
			parserContext.Consumed( _lengthsIndex + 1);

			return length;
		}
		#endregion
	}
}
