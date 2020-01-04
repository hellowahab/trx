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

using Trx.Messaging;

namespace Tests.Trx.Messaging.Iso8583 {

	/// <summary>
	/// It defines necessary data to perform ISO 8583 message formatter test.
	/// </summary>
	public struct Iso8583MessageFormatterTestConfig {

		/// <summary>
		/// Son los campos a emplear en el test.
		/// </summary>
		public int[] Fields;

		/// <summary>
		/// Es el formateador del header.
		/// </summary>
		public StringMessageHeaderFormatter HeaderFormatter;

		/// <summary>
		/// Es el header a emplear.
		/// </summary>
		public MessageHeader Header;

		/// <summary>
		/// Es el formateador del identificador de tipo de mensaje.
		/// </summary>
		public StringFieldFormatter MessageTypeIdentifierFormatter;

		/// <summary>
		/// Es el identificador de tipo de mensaje a emplear.
		/// </summary>
		public int MessageTypeIdentifier;

		/// <summary>
		/// Es el resultado esperado del formateo.
		/// </summary>
		public string ExpectedFormattedData;

		#region Constructors
		/// <summary>
		/// Construye una nueva instancia de <see cref="Iso8583MessageFormatterTestConfig"/>.
		/// </summary>
		/// <param name="fields">
		/// Son los campos a emplear en el test.
		/// </param>
		/// <param name="headerFormatter">
		/// Es el formateador del header.
		/// </param>
		/// <param name="header">
		/// Es el header a emplear.
		/// </param>
		/// <param name="messageTypeIdentifierFormatter">
		/// Es el formateador del identificador de tipo de mensaje.
		/// </param>
		/// <param name="messageTypeIdentifier">
		/// Es el identificador de tipo de mensaje a emplear.
		/// </param>
		/// <param name="expectedFormattedData">
		/// Es el resultado esperado del formateo.
		/// </param>
		public Iso8583MessageFormatterTestConfig( int[] fields,
			StringMessageHeaderFormatter headerFormatter,
			MessageHeader header, StringFieldFormatter messageTypeIdentifierFormatter,
			int messageTypeIdentifier, string expectedFormattedData) {

			Fields = fields;
			HeaderFormatter = headerFormatter;
			Header = header;
			MessageTypeIdentifierFormatter = messageTypeIdentifierFormatter;
			MessageTypeIdentifier = messageTypeIdentifier;
			ExpectedFormattedData = expectedFormattedData;
		}
		#endregion
	}
}
