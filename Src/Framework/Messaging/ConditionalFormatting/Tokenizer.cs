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
using System.IO;

namespace Trx.Messaging.ConditionalFormatting
{
    /// <summary>
    /// This class implements a tokenizer to be used by SemanticParser.
    /// </summary>
    public class Tokenizer : yyInput
    {
        private readonly LexicalAnalyzer _lexer;
        private Yytoken _currentToken;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="reader">
        /// It's the source of the expression to be parsed.
        /// </param>
        public Tokenizer(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _lexer = new LexicalAnalyzer(reader);
        }

        #region yyInput Members
        /// <summary>
        /// It returns the index of the last parsed token.
        /// </summary>
        public int LastParsedTokenIndex
        {
            get { return _lexer.CurrentTokenIndex; }
        }

        /// <summary>
        /// It parses the next token.
        /// </summary>
        /// <returns>
        /// true if the parse was performed, otherwise false.
        /// </returns>
        public bool advance()
        {
            _currentToken = _lexer.yylex();

            if (_currentToken == null)
                return false;

            return true;
        }

        /// <summary>
        /// It return the last parsed token.
        /// </summary>
        /// <returns>
        /// The last parsed token.
        /// </returns>
        public int token()
        {
            if (_currentToken == null)
                throw new InvalidOperationException();

            return _currentToken.Sym;
        }

        /// <summary>
        /// It returns the las parsed object.
        /// </summary>
        /// <returns>
        /// The las parsed object.
        /// </returns>
        public Object value()
        {
            if (_currentToken == null)
                throw new InvalidOperationException();

            return _currentToken.Value;
        }
        #endregion
    }
}