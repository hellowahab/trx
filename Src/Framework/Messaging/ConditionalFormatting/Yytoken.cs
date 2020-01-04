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
    /// It represents a token in an expression.
    /// </summary>
    public class Yytoken
    {
        private readonly int _position;
        public int Left;
        public int Right;
        public int Sym;
        public object Value;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="symNum">
        /// The number of the token.
        /// </param>
        /// <param name="position">
        /// The start position of the token in the expression.
        /// </param>
        public Yytoken(int symNum, int position) :
            this(symNum, position, -1, -1, null)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="symNum">
        /// The number of the token.
        /// </param>
        /// <param name="position">
        /// The start position of the token in the expression.
        /// </param>
        /// <param name="l">
        /// The left token.
        /// </param>
        /// <param name="r">
        /// The right token.
        /// </param>
        public Yytoken(int symNum, int position, int l, int r) :
            this(symNum, position, l, r, null)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="symNum">
        /// The number of the token.
        /// </param>
        /// <param name="position">
        /// The start position of the token in the expression.
        /// </param>
        /// <param name="o">
        /// The value of the token.
        /// </param>
        public Yytoken(int symNum, int position, object o) :
            this(symNum, position, -1, -1, o)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="symNum">
        /// The number of the token.
        /// </param>
        /// <param name="position">
        /// The start position of the token in the expression.
        /// </param>
        /// <param name="l">
        /// The left token.
        /// </param>
        /// <param name="r">
        /// The right token.
        /// </param>
        /// <param name="o">
        /// The value of the token.
        /// </param>
        public Yytoken(int symNum, int position, int l, int r, object o)
        {
            Sym = symNum;
            Left = l;
            Right = r;
            Value = o;
            _position = position;
        }

        /// <summary>
        /// It returns the start position in the expression of the token.
        /// </summary>
        public int Position
        {
            get { return _position; }
        }

        /// <summary>
        /// It returns a string representation of the token.
        /// </summary>
        /// <returns>
        /// A string representation of the token.
        /// </returns>
        public override string ToString()
        {
            return "#" + Sym;
        }
    }
}