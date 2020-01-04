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
    public class SslServerChildChannel : TcpServerChildChannel
    {
        private Timer _negotiationTimer;
        private SslStream _sslStream;

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </param>
        /// <param name="clientCertificateRequired">
        /// True if client certificate is required for mutual authentication.
        /// </param>
        /// <param name="sslProtocols">
        /// Defines the possible versions of SslProtocols.
        /// </param>
        /// <param name="checkCertificateRevocation">
        /// A boolean value that specifies whether the certificate revocation list is checked 
        /// during authentication.
        /// </param>
        /// <param name="negotiationTimeout">
        /// Time in milliseconds the SSL/TLS negotiation is timedout (closing the underlying socket).
        /// </param>
        internal SslServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel,
            Socket socket, string name, ICertificateProvider certificateProvider,
            ICertificateValidator certificateValidator, bool clientCertificateRequired,
            SslProtocols sslProtocols, bool checkCertificateRevocation, int negotiationTimeout)
            : base(pipeline, parentChannel, socket, name, false)
        {
            ConstructorHelper(certificateProvider, certificateValidator, clientCertificateRequired,
                sslProtocols, checkCertificateRevocation, negotiationTimeout);
        }

        /// <summary>
        /// Buils a channel to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </param>
        /// <param name="clientCertificateRequired">
        /// True if client certificate is required for mutual authentication.
        /// </param>
        /// <param name="sslProtocols">
        /// Defines the possible versions of SslProtocols.
        /// </param>
        /// <param name="checkCertificateRevocation">
        /// A boolean value that specifies whether the certificate revocation list is checked 
        /// during authentication.
        /// </param>
        /// <param name="negotiationTimeout">
        /// Time in milliseconds the SSL/TLS negotiation is timedout (closing the underlying socket).
        /// </param>
        internal SslServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel,
            Socket socket, string name, ITupleSpace<ReceiveDescriptor> tupleSpace,
            ICertificateProvider certificateProvider,
            ICertificateValidator certificateValidator, bool clientCertificateRequired,
            SslProtocols sslProtocols, bool checkCertificateRevocation, int negotiationTimeout)
            : base(pipeline, parentChannel, socket, name, tupleSpace, false)
        {
            ConstructorHelper(certificateProvider, certificateValidator, clientCertificateRequired,
                sslProtocols, checkCertificateRevocation, negotiationTimeout);
        }

        /// <summary>
        /// Buils a channel to send and receive messages and request responses.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        /// <param name="tupleSpace">
        /// Tuple space to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier to compute keys to match requests with responses.
        /// </param>
        /// <param name="parentChannel">
        /// The parent channel (the one which accepted the connection).
        /// </param>
        /// <param name="socket">
        /// The accepted socket.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </param>
        /// <param name="clientCertificateRequired">
        /// True if client certificate is required for mutual authentication.
        /// </param>
        /// <param name="sslProtocols">
        /// Defines the possible versions of SslProtocols.
        /// </param>
        /// <param name="checkCertificateRevocation">
        /// A boolean value that specifies whether the certificate revocation list is checked 
        /// during authentication.
        /// </param>
        /// <param name="negotiationTimeout">
        /// Time in milliseconds the SSL/TLS negotiation is timedout (closing the underlying socket).
        /// </param>
        internal SslServerChildChannel(Pipeline pipeline, TcpServerChannel parentChannel,
            Socket socket, string name, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier, ICertificateProvider certificateProvider,
            ICertificateValidator certificateValidator, bool clientCertificateRequired,
            SslProtocols sslProtocols, bool checkCertificateRevocation, int negotiationTimeout)
            : base(pipeline, parentChannel, socket, name, tupleSpace, messagesIdentifier, false)
        {
            ConstructorHelper(certificateProvider, certificateValidator, clientCertificateRequired,
                sslProtocols, checkCertificateRevocation, negotiationTimeout);
        }

        /// <summary>
        /// The server certificate provider wich is given to the clients.
        /// </summary>
        public ICertificateProvider CertificateProvider { get; internal set; }

        /// <summary>
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </summary>
        public ICertificateValidator CertificateValidator { get; internal set; }

        /// <summary>
        /// True if client certificate is required for mutual authentication.
        /// </summary>
        public bool ClientCertificateRequired { get; internal set; }

        /// <summary>
        /// Defines the possible versions of SslProtocols.
        /// </summary>
        public SslProtocols SslProtocols { get; internal set; }

        /// <summary>
        /// A boolean value that specifies whether the certificate revocation list is checked 
        /// during authentication.
        /// </summary>
        public bool CheckCertificateRevocation { get; internal set; }

        private void ConstructorHelper(ICertificateProvider certificateProvider,
            ICertificateValidator certificateValidator, bool clientCertificateRequired,
            SslProtocols sslProtocols, bool checkCertificateRevocation, int negotiationTimeout)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            CertificateProvider = certificateProvider;
            CertificateValidator = certificateValidator;
            ClientCertificateRequired = clientCertificateRequired;
            SslProtocols = sslProtocols;
            CheckCertificateRevocation = checkCertificateRevocation;

            _sslStream = new SslStream(new NetworkStream(Socket), false, ClientCertificateValidationCallback);
            _negotiationTimer = new Timer(NegotiationTimeoutCallback, Socket, negotiationTimeout, Timeout.Infinite);
            _sslStream.BeginAuthenticateAsServer(certificateProvider.GetCertificate(), clientCertificateRequired,
                SslProtocols, checkCertificateRevocation, AsyncAuthenticateAsServer, Socket);
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

        protected override void OnDisconnection()
        {
            base.OnDisconnection();

            if (_sslStream != null)
            {
                _sslStream.Close();
                _sslStream.Dispose();
                _sslStream = null;
            }
        }

        private void AsyncAuthenticateAsServer(IAsyncResult asyncResult)
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
                    _sslStream.EndAuthenticateAsServer(asyncResult);
                }
                catch (IOException)
                {
                    Logger.Error(string.Format("{0}: connection closed on TLS/SSL negotiation (timeout or network error).", GetChannelTitle()));
                    OnDisconnection();
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Info(string.Format("{0}: exception caught handling asynchronous authenticate as server.",
                        GetChannelTitle()), ex);
                    OnDisconnection();
                    return;
                }

                try
                {
                    Pipeline.ProcessChannelEvent(PipelineContext, new ChannelEvent(ChannelEventType.Connected), true,
                        Logger);
                    Logger.Info(SslHelpers.GetSslStreamInfo(_sslStream, string.Format("{0} ({1})",
                        GetChannelTitle(), socket.RemoteEndPoint)));
                    StartAsyncReceive();
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught handling asynchronous authenticate as server.",
                        GetChannelTitle()), ex);
                    OnDisconnection();
                    return;
                }
            }
        }

        private bool ClientCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                // If we don't require client certificate and isn't provided, ok.
                if (!ClientCertificateRequired && sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
                    return true;

                // If we require client certificate and isn't provided, fail.
                if (ClientCertificateRequired && sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
                    return false;

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
                    string.Format("{0}: exception caught on client certificate validation callback.", GetChannelTitle()),
                    ex);
                return false;
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