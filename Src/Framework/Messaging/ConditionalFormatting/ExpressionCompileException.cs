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
using System.Runtime.Serialization;

namespace Trx.Messaging.ConditionalFormatting
{
    /// <summary>
    /// This class implements the exception raised when a expression
    /// parser locates an error.
    /// </summary>
    [Serializable]
    public class ExpressionCompileException : ApplicationException
    {
        private int _lastParsedTokenIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileException" /> 
        /// class.
        /// </summary>
        public ExpressionCompileException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileException" /> 
        /// class with a descriptive message.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        public ExpressionCompileException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileException" /> 
        /// class with a descriptive message.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        /// <param name="lastParsedTokenIndex">
        /// The index of the last parsed token.
        /// </param>
        public ExpressionCompileException(string message, int lastParsedTokenIndex)
            : base(message)
        {
            _lastParsedTokenIndex = lastParsedTokenIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileException" /> 
        /// class with the specified descriptive message and inner exception.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        /// <param name="innerException">
        /// A nested exception that is the cause of the current exception.
        /// </param>
        public ExpressionCompileException(string message,
            Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCompileException" /> 
        /// class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data
        /// about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information
        /// about the source or destination.
        /// </param>
        protected ExpressionCompileException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// It returns or sets the index of the last parsed token.
        /// </summary>
        public int LastParsedTokenIndex
        {
            get { return _lastParsedTokenIndex; }

            set { _lastParsedTokenIndex = value; }
        }
    }
}