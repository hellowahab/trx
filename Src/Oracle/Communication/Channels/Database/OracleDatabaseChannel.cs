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
using System.Data;
using System.Data.OracleClient;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Database
{
    public class OracleDatabaseChannel : BaseDatabaseChannel
    {
        private string _readStoredProcedureCursorName = "readCursor";

        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        public OracleDatabaseChannel(Pipeline pipeline)
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
        public OracleDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
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
        public OracleDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        /// <summary>
        /// Stored procedure name to read data from database.
        /// </summary>
        public string ReadStoredProcedureCursorName
        {
            get { return _readStoredProcedureCursorName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _readStoredProcedureCursorName = value;
            }
        }

        private DbType GetDbType(string dataTypeName, out int oracleDbType)
        {
            switch (dataTypeName)
            {
                case "DATE":
                    oracleDbType = (int)OracleType.DateTime;
                    return DbType.Time;
                case "FLOAT":
                    oracleDbType = (int)OracleType.Float;
                    return DbType.Single;
                case "NUMBER":
                    oracleDbType = (int)OracleType.Number;
                    return DbType.Int64;
                case "CHAR":
                    oracleDbType = (int)OracleType.Char;
                    return DbType.AnsiStringFixedLength;
                case "NCHAR":
                    oracleDbType = (int)OracleType.NChar;
                    return DbType.StringFixedLength;
                case "INTERVAL YEAR TO MONTH":
                    oracleDbType = (int)OracleType.IntervalYearToMonth;
                    return DbType.Int32;
                case "INTEGER":
                    oracleDbType = (int)OracleType.Int32;
                    return DbType.Int32;
                case "UNSIGNED INTEGER":
                    oracleDbType = (int)OracleType.UInt32;
                    return DbType.UInt32;
                case "BFILE":
                    oracleDbType = (int)OracleType.BFile;
                    return DbType.Binary;
                case "BLOB":
                    oracleDbType = (int)OracleType.Blob;
                    return DbType.Binary;
                case "LONG RAW":
                    oracleDbType = (int)OracleType.LongRaw;
                    return DbType.Binary;
                case "RAW":
                    oracleDbType = (int)OracleType.Raw;
                    return DbType.Binary;
                case "LONG":
                    oracleDbType = (int)OracleType.LongVarChar;
                    return DbType.String;
                case "NCLOB":
                    oracleDbType = (int)OracleType.NClob;
                    return DbType.String;
                case "NVARCHAR2":
                    oracleDbType = (int)OracleType.NVarChar;
                    return DbType.String;
                case "CLOB":
                    oracleDbType = (int)OracleType.Clob;
                    return DbType.AnsiString;
                case "ROWID":
                    oracleDbType = (int)OracleType.RowId;
                    return DbType.AnsiString;
                case "VARCHAR2":
                    oracleDbType = (int)OracleType.VarChar;
                    return DbType.AnsiString;
                case "TIMESTAMP":
                    oracleDbType = (int)OracleType.Timestamp;
                    return DbType.DateTime;
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    oracleDbType = (int)OracleType.TimestampLocal;
                    return DbType.DateTime;
                case "TIMESTAMP WITH TIME ZONE":
                    oracleDbType = (int)OracleType.TimestampWithTZ;
                    return DbType.DateTime;
                case "REF CURSOR":
                    oracleDbType = (int)OracleType.Cursor;
                    return DbType.Object;
                case "INTERVAL DAY TO SECOND":
                    oracleDbType = (int)OracleType.IntervalDayToSecond;
                    return DbType.Object;
                default:
                    oracleDbType = (int)OracleType.LongRaw;
                    return DbType.Object;
            }
        }

#pragma warning disable 612,618

        /// <summary>
        /// Read a tuple from database.
        /// </summary>
        /// <returns>
        /// A list of stored procedure parameters or null if no tuple is available.
        /// </returns>
        protected override List<DatabaseChannelParameter> ReadFromDatabase()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                OracleTransaction trx = conn.BeginTransaction();
                List<DatabaseChannelParameter> list = null;
                using (OracleCommand command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = QueryTimeout;
                    command.Transaction = trx;
                    command.CommandText = ReadStoredProcedureName;

                    command.Parameters.Add(ReadStoredProcedureCursorName, OracleType.Cursor).Direction = ParameterDirection.Output;

                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                        if (reader.Read())
                        {
                            list = new List<DatabaseChannelParameter>(reader.FieldCount);
                            for (int i = 0; i < reader.FieldCount; i++)
                                if (!reader.IsDBNull(i))
                                {
                                    int engineDbType;
                                    DbType dbType = GetDbType(reader.GetDataTypeName(i), out engineDbType);
                                    list.Add(new DatabaseChannelParameter
                                    {
                                        DbType = dbType,
                                        EngineDbType = engineDbType,
                                        ParameterName = reader.GetName(i),
                                        Value = reader[i]
                                    });
                                }
                        }
                }

                trx.Commit();

                return list;
            }
        }

        /// <summary>
        /// Send a pipeline processed message to the database.
        /// </summary>
        /// <param name="message">
        /// The message to send.
        /// </param>
        protected override void SendToDatabase(List<DatabaseChannelParameter> message)
        {
            if (message == null || message.Count == 0)
                return;

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                OracleTransaction trx = conn.BeginTransaction();
                using (OracleCommand command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = QueryTimeout;
                    command.Transaction = trx;
                    command.CommandText = WriteStoredProcedureName;

                    foreach (DatabaseChannelParameter msgParam in message)
                    {
                        OracleParameter cmdParam = command.CreateParameter();
                        if (msgParam.EngineDbType.HasValue)
                            cmdParam.OracleType = (OracleType) msgParam.EngineDbType.Value;
                        else
                            cmdParam.DbType = msgParam.DbType;
                        cmdParam.ParameterName = msgParam.ParameterName;
                        cmdParam.Value = msgParam.Value;

                        command.Parameters.Add(cmdParam);
                    }
                    command.ExecuteNonQuery();
                }

                trx.Commit();
            }
        }
#pragma warning restore 612,618
    }
}