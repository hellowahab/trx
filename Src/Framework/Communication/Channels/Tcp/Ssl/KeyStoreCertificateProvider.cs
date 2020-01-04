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

using System.Security.Cryptography.X509Certificates;
using Trx.Exceptions;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    public class KeyStoreCertificateProvider : ICertificateProvider
    {
        private X509Certificate _certificate;

        public KeyStoreCertificateProvider()
        {
            StoreName = StoreName.My;
            StoreLocation = StoreLocation.LocalMachine;
            OnlyValidCertificatesFromStore = true;
        }

        /// <summary>
        /// One of the enumeration values that specifies the name of the X.509 certificate store.
        /// </summary>
        /// <remarks>
        /// Used if <see ref="CustomStoreName"/> is not set.
        /// Default value is <see ref="StoreLocation.LocalMachine"/>.
        /// </remarks>
        public StoreName StoreName { get; set; }

        /// <summary>
        /// A custom store name.
        /// </summary>
        /// <remarks>
        /// If not set, <see ref="StoreName"/> will be used.
        /// </remarks>
        public string CustomStoreName { get; set; }

        /// <summary>
        /// One of the enumeration values that specifies the location of the X.509 certificate store.
        /// </summary>
        /// <remarks>
        /// Default value is <see ref="StoreName.My"/>.
        /// </remarks>
        public StoreLocation StoreLocation { get; set; }

        /// <summary>
        /// The certificate subject to search in the certificate store.
        /// </summary>
        public string CertificateSubject { get; set; }

        /// <summary>
        /// True if only valid certificates can be retrieved from the store, otherwise false.
        /// </summary>
        /// <remarks>
        /// Default value is true.
        /// </remarks>
        public bool OnlyValidCertificatesFromStore { get; set; }

        #region ICertificateProvider Members
        public X509Certificate GetCertificate()
        {
            if (CertificateSubject == null)
                throw new ConfigurationException("CertificateSubject property hasn't been set.");

            if (_certificate == null)
            {
                X509Store store = string.IsNullOrEmpty(CustomStoreName)
                    ? new X509Store(StoreName, StoreLocation)
                    : new X509Store(CustomStoreName, StoreLocation);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName,
                    CertificateSubject, OnlyValidCertificatesFromStore);
                if (certificates.Count > 0)
                    _certificate = certificates[0];
                store.Close();
            }

            if (_certificate == null)
                throw new ConfigurationException("Certificate not found.");

            return _certificate;
        }
        #endregion
    }
}