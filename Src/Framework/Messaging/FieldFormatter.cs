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

namespace Trx.Messaging
{

	/// <summary>
    /// This class implements a field formatter.
	/// </summary>
	public abstract class FieldFormatter {

		private readonly int _fieldNumber;

		/// <summary>
        /// It builds a new field formatter.
		/// </summary>
		/// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
		/// </param>
		protected FieldFormatter( int fieldNumber) :
			this( fieldNumber, string.Empty) {

		}

		/// <summary>
        /// It builds a new field formatter.
		/// </summary>
		/// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
		/// </param>
		/// <param name="description">
		/// It's the description of the field formatter.
		/// </param>
		protected FieldFormatter( int fieldNumber, string description) {

			_fieldNumber = fieldNumber;
			Description = description;
		}

		/// <summary>
        /// It returns the number of the field this formatter formats/parse.
		/// </summary>
		public int FieldNumber {

			get {

				return _fieldNumber;
			}
		}

	    /// <summary>
	    /// It returns the description of the field formatter.
	    /// </summary>
	    public string Description { get; set; }

		/// <summary>
		/// Formats the specified field.
		/// </summary>
		/// <param name="field">
		/// It's the field to format.
		/// </param>
		/// <param name="formatterContext">
		/// It's the context of formatting to be used by the method.
		/// </param>
		public abstract void Format( Field field, ref FormatterContext formatterContext);

		/// <summary>
        /// It parses the information in the parser context and builds the field.
		/// </summary>
		/// <param name="parserContext">
		/// It's the parser context.
		/// </param>
		/// <returns>
        /// The new field built with the information found in the parser context.
		/// </returns>
		public abstract Field Parse( ref ParserContext parserContext);
	}
}
