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

namespace Trx.Communication.Channels.Tcp.Ssl
{
    public class FileBasedCertificateProvider : ICertificateProvider
    {
        private X509Certificate _certificate;

        /// <summary>
        /// The file name wich contains the certificate.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Password of the certificate container.
        /// </summary>
        public string Password { get; set; }

        public X509Certificate GetCertificate()
        {
            return _certificate ??
                (_certificate =
                    string.IsNullOrEmpty(Password)
                        ? new X509Certificate2(FileName)
                        : new X509Certificate2(FileName, Password));
        }
    }
}
