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

namespace Trx.Messaging.FlowControl
{
    /// <summary>
    /// This class defines the arguments of the event <see cref="IListener.ConnectionRequest"/>.
    /// </summary>
    public class ListenerConnectionRequestEventArgs : EventArgs
    {
        private readonly object _connectionInfo;
        private bool _accept = true;

        /// <summary>
        /// It creates and initializes a new instance of the
        /// type <see cref="ListenerConnectionRequestEventArgs"/>.
        /// </summary>
        /// <param name="connectionInfo">
        /// It's the associated information to the connection request.
        /// </param>
        public ListenerConnectionRequestEventArgs(object connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        /// <summary>
        /// It returns or sets the parameter which allows to accept or deny the
        /// incoming connection.
        /// </summary>
        public bool Accept
        {
            get { return _accept; }

            set { _accept = value; }
        }

        /// <summary>
        /// It returns the associated information to the connection request.
        /// </summary>
        /// <remarks>
        /// The type of this parameter if it holds a valid value, will depend
        /// of the class which implements the interface <see cref="IListener"/>.
        /// </remarks>
        public object ConnectionInfo
        {
            get { return _connectionInfo; }
        }
    }
}