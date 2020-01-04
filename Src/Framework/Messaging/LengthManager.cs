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

namespace Trx.Messaging {

	public abstract class LengthManager {

		private readonly int _maximumLength;

		protected LengthManager( int maximumLength ) {

			_maximumLength = maximumLength;
		}

		public int MaximumLength {

			get {

				return _maximumLength;
			}
		}

		public virtual void WriteLength( MessagingComponent component,
			int dataLength, int encodedLength, ref FormatterContext formatterContext) {

		}

		public virtual void WriteLengthTrailer( MessagingComponent component,
			int dataLength, int encodedLength, ref FormatterContext formatterContext) {

		}

		public abstract bool EnoughData( ref ParserContext parserContext);

		public abstract int ReadLength( ref ParserContext parserContext );

		public virtual void ReadLengthTrailer( ref ParserContext parserContext) {

		}
	}
}
