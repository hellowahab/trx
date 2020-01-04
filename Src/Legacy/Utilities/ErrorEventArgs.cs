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

namespace Trx.Utilities
{
    /// <summary>
    /// This class defines the arguments for events that notify an error.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        private readonly Exception _exception;

        /// <summary>
        /// Creates and initializes a new instance of class <see cref="ErrorEventArgs"/>.
        /// </summary>
        /// <param name="exception">
        /// It is the exception that produced the error that has been received.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="exception"/> it's null.
        /// </exception>
        public ErrorEventArgs(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            _exception = exception;
        }

        /// <summary>
        /// It returns the exception that has produced the error.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }
    }
}