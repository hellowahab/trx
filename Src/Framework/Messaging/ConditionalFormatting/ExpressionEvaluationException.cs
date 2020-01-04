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
    /// evaluation produces an error.
    /// </summary>
    [Serializable]
    public class ExpressionEvaluationException : ApplicationException
    {
        private int _tokenIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationException" /> 
        /// class.
        /// </summary>
        public ExpressionEvaluationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationException" /> 
        /// class with a descriptive message.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        public ExpressionEvaluationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationException" /> 
        /// class with a descriptive message.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        /// <param name="tokenIndex">
        /// The index of the token where the error was produced.
        /// </param>
        public ExpressionEvaluationException(string message, int tokenIndex)
            : base(message)
        {
            _tokenIndex = tokenIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationException" /> 
        /// class with the specified descriptive message and inner exception.
        /// </summary>
        /// <param name="message">
        /// A descriptive message to include with the exception.
        /// </param>
        /// <param name="innerException">
        /// A nested exception that is the cause of the current exception.
        /// </param>
        public ExpressionEvaluationException(string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationException" /> 
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
        protected ExpressionEvaluationException(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// It returns or sets the index of the token where the error was produced.
        /// </summary>
        public int TokenIndex
        {
            get { return _tokenIndex; }

            set { _tokenIndex = value; }
        }
    }
}