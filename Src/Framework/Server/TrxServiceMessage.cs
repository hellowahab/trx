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

namespace Trx.Server
{
    /// <summary>
    /// Defines a message interchanged between Trx Server services using tuple spaces.
    /// </summary>
    [Serializable]
    public class TrxServiceMessage
    {
        private readonly string _inputContext;
        private readonly string _outputContext;
        private readonly object _message;

        public TrxServiceMessage(string inputContext, object message)
        {
            if (inputContext == null)
                throw new ArgumentNullException("inputContext");

            if (message == null)
                throw new ArgumentNullException("message");

            _inputContext = inputContext;
            _message = message;
        }

        public TrxServiceMessage(string inputContext, string outputContext, object message) : this(inputContext, message)
        {
            if (outputContext == null)
                throw new ArgumentNullException("outputContext");

            _outputContext = outputContext;
        }

        public object Message
        {
            get { return _message; }
        }

        /// <summary>
        /// The context the consumer can respond the message, if applies, to the producer.
        /// </summary>
        public string OutputContext
        {
            get { return _outputContext; }
        }

        /// <summary>
        /// The context name the producer left the message and the consumer got it.
        /// </summary>
        public string InputContext
        {
            get { return _inputContext; }
        }
    }
}
