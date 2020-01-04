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
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Local
{
    public class LocalServerChannel : BaseServerChannel
    {
        private readonly string _address;
        private bool _isListening;
        private ChannelRequestCtrl _lastSuccessfulStartListening;

        /// <summary>
        /// Builds a server channel with childs intended to send messages only.
        /// </summary>
        /// <param name="address">
        /// Local address to accept connections.
        /// </param>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        public LocalServerChannel(string address, Pipeline pipeline, IPipelineFactory pipelineFactory)
            : base(pipeline, pipelineFactory)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _address = address;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="address">
        /// Local address to accept connections.
        /// </param>
        /// <param name="pipeline">
        /// The server pipeline.
        /// </param>
        /// <param name="pipelineFactory">
        /// The childs pipeline factory.
        /// </param>
        /// <param name="tupleSpace">
        /// Space used by childs to store received messages.
        /// </param>
        public LocalServerChannel(string address, Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, pipelineFactory, tupleSpace)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _address = address;
        }

        /// <summary>
        /// Builds a server channel with childs intended to send and receive messages.
        /// </summary>
        /// <param name="address">
        /// Local address to accept connections.
        /// </param>
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
        public LocalServerChannel(string address, Pipeline pipeline, IPipelineFactory pipelineFactory,
            ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, pipelineFactory, tupleSpace, messagesIdentifier)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _address = address;
        }

        /// <summary>
        /// True if the server channel is listening for new connections. Otherwise false.
        /// </summary>
        public override bool IsListening
        {
            get { return _isListening; }
        }

        /// <summary>
        /// Local address.
        /// </summary>
        public string Address
        {
            get { return _address; }
        }

        /// <summary>
        /// Ask the current channel to start listening connections.
        /// </summary>
        /// <returns>
        /// The operation request control.
        /// </returns>
        public override ChannelRequestCtrl StartListening()
        {
            if (_isListening)
                return _lastSuccessfulStartListening;

            if (!LocalServerRegistry.GetInstance().Register(_address, this))
            {
                Logger.Error(string.Format("{0}: address '{1}' already in use.", GetChannelTitle(), _address));
                return new ChannelRequestCtrl(false)
                           {
                               Message = "Address already in use."
                           };
            }

            Logger.Info(string.Format("{0}: accepting connections on address {1}.", GetChannelTitle(), _address));

            _isListening = true;
            _lastSuccessfulStartListening = new ChannelRequestCtrl(true);
            return _lastSuccessfulStartListening;
        }

        /// <summary>
        /// Ask the current channel to stop listening new connections.
        /// </summary>
        public override void StopListening()
        {
            LocalServerRegistry.GetInstance().Unregister(_address);
            _isListening = false;
        }

        protected string GetChannelTitle()
        {
            return Name ?? "Local server channel";
        }

        /// <summary>
        /// Called from <see cref="LocalServerRegistry.Connect"/> when a <see cref="LocalClientChannel"/>
        /// try to connect to a local address and a server is listening on it.
        /// </summary>
        /// <param name="client">
        /// Client trying to connect.
        /// </param>
        /// <returns>
        /// The server endpoint channel (the one connected with the <see cref="LocalClientChannel"/>).
        /// </returns>
        internal LocalServerChildChannel Accept(LocalClientChannel client)
        {
            if (!_isListening)
                return null;

            var evt = new ConnectionRequestChannelEvent();

            // Any exception raised in the pipeline or in this method is catched in the client connect call.
            Pipeline.ProcessChannelEvent(PipelineContext, evt, false, null);

            LocalServerChildChannel child = null;
            if (evt.Accept)
            {
                if (TupleSpace == null)
                    child = new LocalServerChildChannel(client, PipelineFactory.CreatePipeline(), this);
                else if (MessagesIdentifier == null)
                    child = new LocalServerChildChannel(client, PipelineFactory.CreatePipeline(), this, TupleSpace);
                else
                    child = new LocalServerChildChannel(client, PipelineFactory.CreatePipeline(), this, TupleSpace,
                        MessagesIdentifier);

                child.Name = string.Format("{0} child-{1}", GetChannelTitle(), NextChildId);

                if (!ReferenceEquals(ChannelAddress, ReferenceChannelAddress))
                    // We have an addressing mechanism wich isn't the default. Create an address of the same kind
                    // for the child.
                    child.SetChannelAddressWithoutFiringEvent(ChannelAddress.GetAddress(child));

                Logger.Info(string.Format("{0}: connection accepted to address '{1}'.", GetChannelTitle(), _address));

                ChildConnection(child);
            }
            else
                Logger.Info(string.Format(
                    "{0}: connection request to address '{1}', not accepted by the pipeline.", GetChannelTitle(),
                    _address));

            return child;
        }
    }
}