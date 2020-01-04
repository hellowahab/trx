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

	/// <summary>
	/// Implementa la clase que permite administrar largos variables de
	/// datos.
	/// </summary>
	public class VariableLengthManager : LengthManager {

		private readonly ILengthEncoder _lengthEncoder;
		private readonly int _minimumLength;

		public VariableLengthManager( int minimumLength, int maximumLength,
			ILengthEncoder lengthEncoder) : base( maximumLength) {

			if ( minimumLength < 0) {
				throw new ArgumentOutOfRangeException( "minimumLength",
					minimumLength, "Can't be lower than zero.");
			}

			if ( minimumLength > maximumLength) {
				throw new ArgumentOutOfRangeException( "minimumLength",
                    minimumLength, "Can't be greater than maximumLength.");
			}

			if ( lengthEncoder == null) {
				throw new ArgumentNullException( "lengthEncoder",
                    "Must specify a valid length encoder.");
			}

			_lengthEncoder = lengthEncoder;
			_minimumLength = minimumLength;
		}

		public ILengthEncoder LengthEncoder {

			get {

				return _lengthEncoder;
			}
		}

		public int MinimumLength {

			get {

				return _minimumLength;
			}
		}

		public override void WriteLength( MessagingComponent component,
			int dataLength, int encodedLength, ref FormatterContext formatterContext) {

			if ( ( dataLength < _minimumLength) ||
				( dataLength > MaximumLength)) {
				throw new ArgumentOutOfRangeException( "dataLength", dataLength,
					string.Format("Must be between {0} and {1}.", _minimumLength, MaximumLength));
			}

			_lengthEncoder.Encode( dataLength, ref formatterContext);
		}

		public override bool EnoughData( ref ParserContext parserContext) {

			return ( parserContext.DataLength >=
				_lengthEncoder.EncodedLength);
		}

		public override int ReadLength( ref ParserContext parserContext) {

			int length = _lengthEncoder.Decode( ref parserContext);

			if ( ( length < _minimumLength) || ( length > MaximumLength)) {
				throw new MessagingException( string.Format("Length must be between {0} and {1}, {2} was received.",
					_minimumLength, MaximumLength, length));
			}

			return length;
		}
	}
}
