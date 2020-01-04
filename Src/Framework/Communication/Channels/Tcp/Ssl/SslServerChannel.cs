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
using System.Net.Sockets;
using System.Security.Authentication;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Tcp.Ssl
{
    public class SslServerChannel : TcpServerChannel
    {
        private readonly ICertificateProvider _certificateProvider;
        private readonly ICertificateValidator _certificateValidator;
        private SslProtocols _sslProtocols = SslProtocols.Default;
        private int _negotiationTimeout = 30000;

        /// <summary>
        /// Builds a server channel with childs intended to send messages only.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ICertificateProvider certificateProvider)
            : base(pipeline, pipelineFactory)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            _certificateProvider = certificateProvider;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send messages only.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object to validate it.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ICertificateProvider certificateProvider, ICertificateValidator certificateValidator)
            : base(pipeline, pipelineFactory)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            if (certificateValidator == null)
                throw new ArgumentNullException("certificateValidator");

            _certificateProvider = certificateProvider;
            _certificateValidator = certificateValidator;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, ICertificateProvider certificateProvider)
            : base(pipeline, pipelineFactory, tupleSpace)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            _certificateProvider = certificateProvider;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object to validate it.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, ICertificateProvider certificateProvider,
            ICertificateValidator certificateValidator)
            : base(pipeline, pipelineFactory, tupleSpace)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            if (certificateValidator == null)
                throw new ArgumentNullException("certificateValidator");

            _certificateProvider = certificateProvider;
            _certificateValidator = certificateValidator;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier used by childs to compute keys to match requests with responses.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier,
            ICertificateProvider certificateProvider)
            : base(pipeline, pipelineFactory, tupleSpace, messagesIdentifier)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            _certificateProvider = certificateProvider;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        /// <param name="messagesIdentifier">
        /// Messages identifier used by childs to compute keys to match requests with responses.
        /// </param>
        /// <param name="certificateProvider">
        /// The server certificate provider wich is given to the clients.
        /// </param>
        /// <param name="certificateValidator">
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object to validate it.
        /// </param>
        public SslServerChannel(Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace, IMessagesIdentifier messagesIdentifier,
            ICertificateProvider certificateProvider, ICertificateValidator certificateValidator)
            : base(pipeline, pipelineFactory, tupleSpace, messagesIdentifier)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException("certificateProvider");

            if (certificateValidator == null)
                throw new ArgumentNullException("certificateValidator");

            _certificateProvider = certificateProvider;
            _certificateValidator = certificateValidator;
        }

        /// <summary>
        /// The server certificate provider wich is given to the clients.
        /// </summary>
        public ICertificateProvider CertificateProvider
        {
            get { return _certificateProvider; }
        }

        /// <summary>
        /// The client certificate validator. The server will ask a certificate to the
        /// client and use the given object instance to validate it.
        /// </summary>
        public ICertificateValidator CertificateValidator
        {
            get { return _certificateValidator; }
        }

        /// <summary>
        /// True if client certificate is required for mutual authentication.
        /// </summary>
        public bool ClientCertificateRequired { get; set; }

        /// <summary>
        /// Defines the possible versions of SslProtocols, default value <see ref="SslProtocols.Default"/>.
        /// </summary>
        public SslProtocols SslProtocols
        {
            get { return _sslProtocols; }
            set { _sslProtocols = value; }
        }

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

        /// <summary>
        /// A boolean value that specifies whether the certificate revocation list is checked 
        /// during authentication.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }

        protected override IServerChildChannel CreateChild(Socket acceptedSocket)
        {
            SslServerChildChannel child;
            string childName = string.Format("{0} child-{1}", GetChannelTitle(), NextChildId);
            if (TupleSpace == null)
                child = new SslServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName, CertificateProvider, CertificateValidator, ClientCertificateRequired,
                    SslProtocols, CheckCertificateRevocation, _negotiationTimeout)
                {
                    SendMaxRequestSize = SendMaxRequestSize
                };
            else if (MessagesIdentifier == null)
                child = new SslServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName, TupleSpace, CertificateProvider, CertificateValidator, ClientCertificateRequired,
                    SslProtocols, CheckCertificateRevocation, _negotiationTimeout)
                {
                    SendMaxRequestSize = SendMaxRequestSize
                };
            else
                child = new SslServerChildChannel(PipelineFactory.CreatePipeline(), this, acceptedSocket,
                    childName, TupleSpace, MessagesIdentifier, CertificateProvider, CertificateValidator,
                    ClientCertificateRequired, SslProtocols, CheckCertificateRevocation, _negotiationTimeout)
                {
                    SendMaxRequestSize = SendMaxRequestSize
                };

            if (!ReferenceEquals(ChannelAddress, ReferenceChannelAddress))
                // We have an addressing mechanism wich isn't the default. Create an address of the same kind
                // for the child.
                child.SetChannelAddressWithoutFiringEvent(ChannelAddress.GetAddress(child));

            child.LoggerName = LoggerName;
            child.Logger = Logger;

            return child;
        }
    }
}
