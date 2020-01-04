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
using System.Collections.Generic;
using System.Threading;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Database
{
    /// <summary>
    /// Base implementation of database channel wich is a channel reading and writing to and from a
    /// database using stored procedures.
    /// 
    /// The channel sends and receives a list of <see cref="DatabaseChannelParameter"/>. You can send the list
    /// directly or use a sink to handle the conversion (see <see cref="MessageToDatabaseSink"/> and
    /// <see cref="MessageToDatabaseSink"/>).
    /// 
    /// In a send request the channel calls <see cref="WriteStoredProcedureName"/> stored procedure 
    /// passing to it the parameters defined in the list.
    /// 
    /// In a receive the channel maps to the list the first row resulting of calling 
    /// <see cref="ReadStoredProcedureName"/>. Receive is done in a channel worker thread that pool 
    /// the database in intervals of <see cref="DatabaseReadPollInMs"/> milliseconds.
    /// </summary>
    public abstract class BaseDatabaseChannel : BaseSenderReceiverChannel, IClientChannel
    {
        private string _connectionString;
        private int _databaseReadPollInMs = 5000; // Default value to 5 seconds.
        private ChannelRequestCtrl _lastConnectRequest;
        private int _queryTimeout = 300; // Default to 5 minutes.
        private string _readStoredProcedureName = "DbChannelRead";
        private bool _receive;
        private Thread _worker;
        private string _writeStoredProcedureName = "DbChannelWrite";

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        protected BaseDatabaseChannel(Pipeline pipeline)
            : base(pipeline)
        {
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
        protected BaseDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
            : base(pipeline, tupleSpace)
        {
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
        protected BaseDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier) : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        /// Database read poll intervale in milliseconds. Default value is 5000 (5 seconds).
        /// </summary>
        public int DatabaseReadPollInMs
        {
            get { return _databaseReadPollInMs; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", value, "The poll interval must be grater than zero.");

                _databaseReadPollInMs = value;
            }
        }

        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _connectionString = value;
            }
        }

        public int QueryTimeout
        {
            get { return _queryTimeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", value,
                        "Query timeout must be equal or greater than zero.");

                _queryTimeout = value;
            }
        }

        /// <summary>
        /// Stored procedure name to read data from database.
        /// </summary>
        public string ReadStoredProcedureName
        {
            get { return _readStoredProcedureName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _readStoredProcedureName = value;
            }
        }

        /// <summary>
        /// Stored procedure name to write data to database.
        /// </summary>
        public string WriteStoredProcedureName
        {
            get { return _writeStoredProcedureName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _writeStoredProcedureName = value;
            }
        }

        #region IClientChannel Members
        /// <summary>
        /// Tells if the channel is connected.
        /// </summary>
        public override bool IsConnected
        {
            get { return _worker != null; }
            protected set { }
        }

        /// <summary>
        /// Close the connection, the channel is reusable.
        /// </summary>
        public override void Disconnect()
        {
            if (!IsConnected)
                return;

            lock (SyncRoot)
            {
                if (!IsConnected)
                    return;

                _receive = false;
                _worker.Interrupt();
                _worker.Join(30000);
                if (_worker.IsAlive)
                    _worker.Abort();

                _worker = null;
            }
        }

        /// <summary>
        /// Close channel and release all the allocated resources. In most cases the channel cannot be used again.
        /// </summary>
        public override void Close()
        {
            Disconnect();
        }

        /// <summary>
        /// Starts the connection of the channel.
        /// </summary>
        /// <returns>
        /// A connection request handler.
        /// </returns>
        public ChannelRequestCtrl Connect()
        {
            lock (SyncRoot)
            {
                if (!IsConnected)
                    try
                    {
                        _worker = new Thread(Reader);
                        _receive = true;
                        _worker.Start();

                        _lastConnectRequest = new ChannelRequestCtrl(true);
                    }
                    catch (Exception ex)
                    {
                        return new ChannelRequestCtrl(false)
                                   {
                                       Error = ex
                                   };
                    }

                return _lastConnectRequest;
            }
        }
        #endregion

        /// <summary>
        /// Read a tuple from database.
        /// </summary>
        /// <returns>
        /// A list of stored procedure parameters or null if no tuple is available.
        /// </returns>
        protected abstract List<DatabaseChannelParameter> ReadFromDatabase();

        /// <summary>
        /// Read worker thread.
        /// </summary>
        /// <param name="state">
        /// Not used.
        /// </param>
        private void Reader(object state)
        {
            bool exceptionOnPrevCall = false;
            bool firstTime = true;
            while (_receive)
            {
                int sleepInterval;
                try
                {
                    object message = ReadFromDatabase();

                    if (firstTime && !exceptionOnPrevCall)
                    {
                        Logger.Info(string.Format("{0}: is now polling database every {1} ms.", GetChannelTitle(),
                            DatabaseReadPollInMs));
                        firstTime = false;
                    }

                    if (message == null)
                        sleepInterval = _databaseReadPollInMs;
                    else
                    {
                        PipelineContext.ReceivedMessage = message;
                        Pipeline.Receive(PipelineContext);
                        BaseReceive(PipelineContext.ReceivedMessage);

                        sleepInterval = 0;
                    }

                    exceptionOnPrevCall = false;
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught reading from database.",
                        GetChannelTitle()), ex);
                    sleepInterval = exceptionOnPrevCall
                        ? 60000 // Wait 60 seconds before try again.
                        : 0;    // Try again ASAP.

                    exceptionOnPrevCall = true;
                }

                if (sleepInterval > 0)
                    try
                    {
                        Thread.Sleep(sleepInterval);
                    }
                    catch (ThreadInterruptedException)
                    {
                    }
            }
        }

        /// <summary>
        /// Send a pipeline processed message to the database.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        protected abstract void SendToDatabase(List<DatabaseChannelParameter> message);

        private static List<DatabaseChannelParameter> CastMessageToList(object message)
        {
            var list = message as List<DatabaseChannelParameter>;
            if (list == null)
                throw new NotSupportedException(
                    "This channel implementation only support to send messages of type List<DatabaseChannelParameter>.");

            return list;
        }

        /// <summary>
        /// It sends the specified message asynchronously.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        public override ChannelRequestCtrl Send(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            lock (SyncRoot)
            {
                if (!IsConnected)
                    return new ChannelRequestCtrl(false)
                               {
                                   Message = "The channel is not connected"
                               };

                try
                {
                    PipelineContext.MessageToSend = message;
                    Pipeline.Send(PipelineContext);
                    Logger.Info(string.Format("{0} message to send: {1}", GetChannelTitle(), message));
                    if (PipelineContext.MessageToSend == null)
                    {
                        Logger.Info(string.Format("{0}: the message has been consumed by the pipeline",
                            GetChannelTitle()));
                        return new ChannelRequestCtrl(false)
                                   {
                                       Message = "The message has been consumed by the pipeline"
                                   };
                    }

                    List<DatabaseChannelParameter> list = CastMessageToList(PipelineContext.MessageToSend);
                    SendToDatabase(list);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new ChannelRequestCtrl(false)
                               {
                                   Error = ex
                               };
                }

                return new ChannelRequestCtrl(true);
            }
        }

        /// <summary>
        /// Send a request message expecting a response from remote peer.
        /// </summary>
        /// <param name="message">
        /// It's the message to send.
        /// </param>
        /// <param name="timeout">
        /// The timeout in milliseconds for a response.
        /// </param>
        /// <param name="sendToTupleSpace">
        /// If true, the request is sent to the channel tuple space on completion or time out.
        /// </param>
        /// <param name="key">
        /// Request key, can be null.
        /// </param>
        /// <returns>
        /// A request control handler to manage the send request.
        /// </returns>
        /// <remarks>
        /// Completed or timed out requests are stored in the receive tuple space if <paramref name="sendToTupleSpace"/>
        /// was set to true. If false synchronous wait of the response from the calling thread is assumed via the 
        /// <see cref="Request.WaitResponse"/> method.
        /// </remarks>
        public override SendRequestHandlerCtrl SendExpectingResponse(object message, int timeout, bool sendToTupleSpace,
            object key)
        {
            SendRequestParametersChecks(message, timeout);

            lock (SyncRoot)
            {
                if (!IsConnected)
                    return new SendRequestHandlerCtrl(false, null)
                               {
                                   Message = "The channel is not connected"
                               };

                Request request;
                SendRequestHandlerCtrl ctrl;
                try
                {
                    PipelineContext.MessageToSend = message;
                    Pipeline.Send(PipelineContext);
                    Logger.Info(string.Format("{0} message to send: {1}", GetChannelTitle(), message));
                    if (PipelineContext.MessageToSend == null)
                    {
                        Logger.Info(string.Format("{0}: the message has been consumed by the pipeline",
                            GetChannelTitle()));
                        return new SendRequestHandlerCtrl(false, null)
                                   {
                                       Message = "The message has been consumed by the pipeline"
                                   };
                    }

                    List<DatabaseChannelParameter> list = CastMessageToList(PipelineContext.MessageToSend);

                    if (!BaseSendRequest(message, timeout, sendToTupleSpace, out request, out ctrl))
                        return ctrl;

                    SendToDatabase(list);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("{0}: exception caught sending data.", GetChannelTitle()), ex);
                    return new SendRequestHandlerCtrl(false, null)
                               {
                                   Error = ex
                               };
                }

                request.Key = key;
                request.StartTimer();

                return ctrl;
            }
        }
    }
}