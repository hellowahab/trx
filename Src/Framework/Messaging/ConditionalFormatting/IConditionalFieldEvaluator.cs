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

namespace Trx.Messaging.ConditionalFormatting
{
	/// <summary>
	/// It defines the interface needed by <see cref="ConditionalFieldFormatter"/>,
	/// to evaluate which field formatter to use.
	/// </summary>
	public interface IConditionalFieldEvaluator {

		/// <summary>
		/// Evaluates the field to format to decide the field formatter to be used.
		/// </summary>
		/// <param name="field">
		/// It's the field to format.
		/// </param>
		/// <param name="formatterContext">
		/// It's the context of formatting to be used by the method.
		/// </param>
		/// <returns>
		/// A logical value indicating the field formatter to be used.
		/// </returns>
		bool EvaluateFormat( Field field, ref FormatterContext formatterContext );

		/// <summary>
		/// Evaluates the parser context to decide the field formatter to be used.
		/// </summary>
		/// <param name="parserContext">
		/// It's the parser context.
		/// </param>
		/// <returns>
		/// A logical value indicating the field formatter to be used.
		/// </returns>
		bool EvaluateParse( ref ParserContext parserContext );
	}
}
