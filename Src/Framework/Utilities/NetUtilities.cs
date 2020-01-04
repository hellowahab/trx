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

using System.Net;

namespace Trx.Utilities {

	/// <summary>
	/// Network utilities.
	/// </summary>
	public static class NetUtilities {

		/// <summary>
        /// It verifies that the number of port indicated is a valid TCP port. 
		/// </summary>
		/// <param name="port">
		/// It's the port number to validate.
		/// </param>
		/// <returns>
		/// true if the port is valid, otherwise false.
		/// </returns>
		public static bool IsValidTcpPort( int port) {

			if ( port >= IPEndPoint.MinPort) {
				return ( port <= IPEndPoint.MaxPort);
			}

			return false;
		}
	}
}
