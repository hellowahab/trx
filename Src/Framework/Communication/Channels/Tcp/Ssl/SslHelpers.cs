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
using Trx.Logging;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    public static class SslHelpers
    {
        public static LogUnit GetSslStreamInfo(SslStream sslStream, string channelName)
        {
            var lu = new LogUnit(string.Format("{0} SSL connection detail:", channelName));
            lu.Messages.Add(string.Format("SSL protocol: {0}", sslStream.SslProtocol));
            lu.Messages.Add(string.Format("Authenticated: {0}", sslStream.IsAuthenticated));
            lu.Messages.Add(string.Format("Encrypted: {0}", sslStream.IsEncrypted));
            lu.Messages.Add(string.Format("Mutually authenticated: {0}", sslStream.IsMutuallyAuthenticated));
            lu.Messages.Add(string.Format("Signed: {0}", sslStream.IsSigned));
            lu.Messages.Add(string.Format("Cipher algorithm: {0}", sslStream.CipherAlgorithm));
            lu.Messages.Add(string.Format("Cipher strength: {0}", sslStream.CipherStrength));
            lu.Messages.Add(string.Format("Hash algorithm: {0}", sslStream.HashAlgorithm));
            lu.Messages.Add(string.Format("Hash strength: {0}", sslStream.HashStrength));
            lu.Messages.Add(string.Format("Key exchange algorithm: {0}", sslStream.KeyExchangeAlgorithm));
            lu.Messages.Add(string.Format("Key exchange strength: {0}", sslStream.KeyExchangeStrength));
            if (sslStream.RemoteCertificate != null)
            {
                var certLu = new LogUnit("Remote certificate information:");
                certLu.Messages.Add(string.Format("Subject: {0}", sslStream.RemoteCertificate.Subject));
                certLu.Messages.Add(string.Format("Effective date: {0}",
                    sslStream.RemoteCertificate.GetEffectiveDateString()));
                certLu.Messages.Add(string.Format("Expiration date: {0}",
                    sslStream.RemoteCertificate.GetExpirationDateString()));
                certLu.Messages.Add(string.Format("Format: {0}", sslStream.RemoteCertificate.GetFormat()));
                certLu.Messages.Add(string.Format("Issuer: {0}", sslStream.RemoteCertificate.Issuer));
                lu.Messages.Add(certLu);
            }

            return lu;
        }
    }
}