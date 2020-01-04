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
using System.Reflection;
using Trx.Logging;

namespace Trx.Communication.Channels
{
    public abstract class BaseChannel : IChannel
    {
        private readonly Pipeline _pipeline;
        private readonly PipelineContext _pipelineContext;
        private ILogger _logger;
        private object _syncRoot = new object();
        private IChannelAddress _channelAddress;
        private readonly ReferenceChannelAddress _referenceChannelAddress;

        #region Constructors
        protected BaseChannel(Pipeline pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException("pipeline");

            _pipeline = pipeline;
            _pipelineContext = new PipelineContext(this);
            _referenceChannelAddress = new ReferenceChannelAddress(this);
            _channelAddress = _referenceChannelAddress;
        }
        #endregion

        #region Properties
        internal PipelineContext PipelineContext
        {
            get { return _pipelineContext; }
        }

        /// <summary>
        /// It returns or sets the channel name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current channel address, by default is a direct reference to the channel instance.
        /// </summary>
        public IChannelAddress ChannelAddress
        {
            get { return _channelAddress; }
            set
            {
                if (_channelAddress == value)
                    return;

                OnChannelAddressChange(value);
                _channelAddress = value;
            }
        }

        protected IChannelAddress ReferenceChannelAddress
        {
            get { return _referenceChannelAddress; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the channel.
        /// </summary>
        public object SyncRoot { internal set { _syncRoot = value; } get { return _syncRoot; } }

        /// <summary>
        /// Return the server pipeline used to process events.
        /// </summary>
        public Pipeline Pipeline
        {
            get { return _pipeline; }
        }

        /// <summary>
        /// Tells if the channel is connected.
        /// </summary>
        public abstract bool IsConnected { get; protected set; }

        public string LoggerName { get; set; }

        public ILogger Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    LoggerName ?? MethodBase.GetCurrentMethod().DeclaringType.ToString()));
            }
            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        LoggerName ?? MethodBase.GetCurrentMethod().DeclaringType.ToString());
                else
                    _logger = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Called when the channel address will be changed (before the change occurs).
        /// </summary>
        /// <param name="newAddress">
        /// The channel new address.
        /// </param>
        /// <remarks>
        /// Used by child channels to notify parent of the change.
        /// </remarks>
        protected virtual void OnChannelAddressChange(IChannelAddress newAddress)
        {
        }

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Close channel and release all the allocated resources. In most cases the channel cannot be used again.
        /// </summary>
        public abstract void Close();

        internal void SetChannelAddressWithoutFiringEvent(IChannelAddress channelAddress)
        {
            if (_channelAddress == channelAddress)
                return;
            _channelAddress = channelAddress;
        }
        #endregion
    }
}