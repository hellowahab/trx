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
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    /// <remarks>
    /// See http://support.microsoft.com/kb/245030/en-us: How to Restrict the Use of Certain
    /// Cryptographic Algorithms and Protocols in Schannel.dll
    /// </remarks>
    public class SslClientChannel : TcpClientChannel
    {
        private X509Certificate _clientCertificate;
        private SslProtocols _sslProtocols = SslProtocols.Default;
        private SslStream _sslStream;
        private Timer _negotiationTimer;
        private int _negotiationTimeout = 30000;

        /// <summary>
        ///   Builds a channel to send messages.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        public SslClientChannel(Pipeline pipeline)
            : base(pipeline)
        {
        }

        /// <summary>
        ///   Buils a channel to send and receive messages.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        /// <param name = "tupleSpace">
        ///   Tuple space to store received messages.
        /// </param>
        public SslClientChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
        }

        /// <summary>
        ///   Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name = "pipeline">
        ///   Messages pipeline.
        /// </param>
        /// <param name = "tupleSpace">
        ///   Tuple space to store received messages.
        /// </param>
        /// <param name = "messagesIdentifier">
        ///   Messages identifier to compute keys to match requests with responses.
        /// </param>
        public SslClientChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </summary>
        public ICertificateValidator CertificateValidator { get; set; }

        /// <summary>
        /// The server certificate provider wich is given to the clients.
        /// </summary>
        public ICertificateProvider CertificateProvider { get; set; }

        /// <summary>
        /// Defines the possible versions of SslProtocols, default value <see ref="SslProtocols.Default"/>.
        /// </summary>
        public SslProtocols SslProtocols
        {
            get { return _sslProtocols; }
            set { _sslProtocols = value; }
        }

        /// <summary>
        /// A boolean value that specifies whether the certificate revocation list is checked
        /// during authentication.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }

        /// <summary>
        /// If null <see ref="RemoteInterface"/> will be used.
        /// </summary>
        /// <remarks>
        /// In general, if <see ref="CertificateValidator"/> doesn't override this default behavior, 
        /// this value or <see ref="RemoteInterface"/> must mach the server certificate subject.
        /// </remarks>
        public string TargetHost { get; set; }

        /// <summary>
        /// True if client certificate is required for mutual authentication.
        /// </summary>
        public bool ClientCertificateRequired { get; set; }

        /// <summary>
        /// Time in milliseconds the SSL/TLS negotiation is timedout (closing the underlying socket).
        /// 
        /// Default value is 30000 (30 seconds).
        /// </summary>
        public int NegotiationTimeout
        {
            get { return _negotiationTimeout; }
            set { _negotiationTimeout = value; }
        }

        protected override void OnDisconnection()
        {
            base.OnDisconnection();

            if (_sslStream != null)
            {
                _sslStream.Close();
                _sslStream.Dispose();
                _sslStream = null;
                _clientCertificate = null;
            }
        }

        protected override void OnSocketConnection()
        {
            _sslStream = new SslStream(new NetworkStream(Socket), false, ServerCertificateValidationCallback,
                CertificateSelectionCallback);
            _negotiationTimer = new Timer(NegotiationTimeoutCallback, Socket, NegotiationTimeout, Timeout.Infinite);
            _sslStream.BeginAuthenticateAsClient(TargetHost ?? RemoteInterface, null, SslProtocols,
                CheckCertificateRevocation, AsyncAuthenticateAsClient, Socket);
        }

        private void NegotiationTimeoutCallback(object state)
        {
            var socket = state as Socket;
            if (socket == null)
                return;

            try
            {
                socket.Close();
            }
            catch
            {
            }
        }

        protected override void OnSuccesfulSocketConnection()
        {
            // Do nothing here because we don't want the parent class start to receive
            // on the socket, we need to authenticate the SSL stream.
        }

        private void AsyncAuthenticateAsClient(IAsyncResult asyncResult)
        {
            lock (SyncRoot)
            {
                var socket = asyncResult.AsyncState as Socket;

                if (socket == null || !ReferenceEquals(socket, Socket))
                    // Someone called Close over socket.
                    return;

                try
                {
                    _negotiationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _sslStream.EndAuthenticateAsClient(asyncResult);

                    // EndAuthenticateAsClient returns OK when server asks client certificate and we don't 
                    // provide one in CertificateSelectionCallback. This is a drawback because
                    // OnSocketConnection below will be called notifing the user code we are ready to send
                    // messages and the server maybe will close the connection (detected on the first read).
                    // That's why ClientCertificateRequired = true in conjunction with IsMutuallyAuthenticated
                    // can be handy to prevent this situation.
                    //
                    // The brief story is: we cannot automatically detect the server is requesting mutual
                    // authentication during SSL negotiation, EndAuthenticateAsClient doesn't raise an exception.

                    if (ClientCertificateRequired && !_sslStream.IsMutuallyAuthenticated)
                        throw new ChannelException(
                            string.Format("{0}: mutual authentication is required (client certificate).",
                                GetChannelTitle()));
                }
                catch (IOException ex)
                {
                    Logger.Info(string.Format("{0}: connection closed on TLS/SSL negotiation (timeout or network error).", GetChannelTitle()));
                    OnSocketConnectionException(ex);
                    return;
                }
                catch (Exception ex)
                {
                    OnSocketConnectionException(ex);
                    return;
                }

                try
                {
                    base.OnSocketConnection();

                    Logger.Info(SslHelpers.GetSslStreamInfo(_sslStream, GetChannelTitle()));
                }
                catch (Exception ex)
                {
                    OnSocketConnectionException(ex);
                    return;
                }

                base.OnSuccesfulSocketConnection();
            }
        }

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                // If there's an error and a certificate validator isn't available, fail.
                if (sslPolicyErrors != SslPolicyErrors.None && CertificateValidator == null)
                    return false;

                // If no certificate validator is provided return true, else call the validator to decide.
                return CertificateValidator == null ||
                    CertificateValidator.ValidateCertificate(certificate, chain, sslPolicyErrors);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    string.Format("{0}: exception caught on server certificate validation callback.", GetChannelTitle()),
                    ex);
                return false;
            }
        }

        private X509Certificate CertificateSelectionCallback(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            try
            {
                // Not called two times when server requires client certificate :(

                return _clientCertificate ??
                    (_clientCertificate = CertificateProvider != null ? CertificateProvider.GetCertificate() : null);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    string.Format("{0}: exception caught on certificate selection callback.", GetChannelTitle()),
                    ex);
                return null;
            }
        }

        protected override int EndReceiveOrRead(IAsyncResult asyncResult)
        {
            return _sslStream.EndRead(asyncResult);
        }

        protected override void OnStartAsyncReceive()
        {
            if (InputBuffer.MultiChunk)
            {
                // Use the first free segment because SslStream doesn't support array segments.
                ArraySegment<byte> buffer = InputBuffer.GetFreeSegments()[0];
                _sslStream.BeginRead(buffer.Array, buffer.Offset, buffer.Count, AsyncReceiveOrReadRequestHandler,
                    _sslStream);
            }
            else
                _sslStream.BeginRead(InputBuffer.GetArray(), InputBuffer.UpperDataBound,
                    InputBuffer.FreeSpaceAtTheEnd, AsyncReceiveOrReadRequestHandler, Socket);
        }

        protected override int EndAsyncSendOrWrite(IAsyncResult asyncResult, AsyncSendInfo sendInfo)
        {
            _sslStream.EndWrite(asyncResult);
            return sendInfo.RequestedBytes;
        }

        protected override void OnStartAsyncSend(AsyncSendInfo sendInfo, int dataToSend)
        {
            if (sendInfo.Buffer.MultiChunk)
            {
                // Use the first free segment because SslStream doesn't support array segments.
                ArraySegment<byte> buffer = sendInfo.Buffer.GetDataSegments(dataToSend)[0];
                sendInfo.RequestedBytes = buffer.Count;
                _sslStream.BeginWrite(buffer.Array, buffer.Offset, buffer.Count,
                    AsyncSendOrWriteRequestHandler, sendInfo);
            }
            else
            {
                sendInfo.RequestedBytes = dataToSend;
                _sslStream.BeginWrite(sendInfo.Buffer.GetArray(), sendInfo.Buffer.LowerDataBound, dataToSend,
                    AsyncSendOrWriteRequestHandler, sendInfo);
            }
        }
    }
}