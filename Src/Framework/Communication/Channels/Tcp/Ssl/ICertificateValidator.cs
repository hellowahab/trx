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

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    /// <summary>
    /// Certificate validator used by SSL channels.
    /// </summary>
    /// <remarks>
    /// The certificate validator is used by the channel if Secure Socket Layer (SSL) policy
    /// errors were caugth in the validation, giving the user the option to customize the
    /// process.
    /// 
    /// Every implementation must be state less.
    /// </remarks>
    public interface ICertificateValidator
    {
        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <param name="certificate">
        /// The certificate used to authenticate the remote party.
        /// </param>
        /// <param name="chain">
        /// The chain of certificate authorities associated with the remote certificate.
        /// </param>
        /// <param name="sslPolicyErrors">
        /// One or more errors associated with the remote certificate.
        /// </param>
        /// <returns>
        /// A boolean value that determines whether the specified certificate is accepted for authentication.
        /// </returns>
        bool ValidateCertificate(X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors);
    }
}
