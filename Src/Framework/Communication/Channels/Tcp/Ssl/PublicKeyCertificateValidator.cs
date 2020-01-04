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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Trx.Exceptions;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    /// <summary>
    /// Validates to true when a remote provided certificate has the same public key of
    /// a given local certificate.
    /// </summary>
    public class PublicKeyCertificateValidator : ICertificateValidator
    {
        /// <summary>
        /// The local certificate provider.
        /// </summary>
        public ICertificateProvider CertificateProvider { get; set; }

        #region ICertificateValidator Members
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
        public bool ValidateCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            if (CertificateProvider == null)
                throw new ConfigurationException("CertificateProvider property hasn't been set.");

            X509Certificate localCertificate = CertificateProvider.GetCertificate();
            if (localCertificate == null)
                throw new ConfigurationException("Local certificate not found.");

            byte[] certPubKey = certificate.GetPublicKey();
            byte[] localCertPubKey = localCertificate.GetPublicKey();

            if (certPubKey.Length != localCertPubKey.Length)
                return false;

            for (int i = certPubKey.Length - 1; i >= 0; i--)
                if (certPubKey[i] != localCertPubKey[i])
                    return false;

            return true;
        }
        #endregion
    }
}