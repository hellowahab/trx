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

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Trx.Coordination.TupleSpace;

namespace Trx.Communication.Channels.Database
{
    public class SqlDatabaseChannel : BaseDatabaseChannel
    {
        /// <summary>
        /// Builds a channel to send messages.
        /// </summary>
        /// <param name="pipeline">
        /// Messages pipeline.
        /// </param>
        public SqlDatabaseChannel(Pipeline pipeline)
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
        public SqlDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace)
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
        public SqlDatabaseChannel(Pipeline pipeline, ITupleSpace<ReceiveDescriptor> tupleSpace,
            IMessagesIdentifier messagesIdentifier)
            : base(pipeline, tupleSpace, messagesIdentifier)
        {
        }

        private DbType GetDbType(string dataTypeName, out int sqlDbType)
        {
            switch (dataTypeName)
            {
                case "smallint":
                    sqlDbType = (int)SqlDbType.SmallInt;
                    return DbType.Int16;
                case "int":
                    sqlDbType = (int)SqlDbType.Int;
                    return DbType.Int32;
                case "bigint":
                    sqlDbType = (int)SqlDbType.BigInt;
                    return DbType.Int64;
                case "binary":
                    sqlDbType = (int)SqlDbType.Binary;
                    return DbType.Binary;
                case "bit":
                    sqlDbType = (int)SqlDbType.Bit;
                    return DbType.Boolean;
                case "char":
                    sqlDbType = (int)SqlDbType.Char;
                    return DbType.String;
                case "ntext":
                    sqlDbType = (int)SqlDbType.NText;
                    return DbType.String;
                case "nvarchar":
                    sqlDbType = (int)SqlDbType.NVarChar;
                    return DbType.String;
                case "text":
                    sqlDbType = (int)SqlDbType.Text;
                    return DbType.String;
                case "varchar":
                    sqlDbType = (int)SqlDbType.VarChar;
                    return DbType.String;
                case "date":
                    sqlDbType = (int)SqlDbType.Date;
                    return DbType.Date;
                case "datetime":
                    sqlDbType = (int)SqlDbType.DateTime;
                    return DbType.DateTime;
                case "smalldatetime":
                    sqlDbType = (int)SqlDbType.SmallDateTime;
                    return DbType.DateTime;
                case "datetimeoffset":
                    sqlDbType = (int)SqlDbType.DateTimeOffset;
                    return DbType.DateTimeOffset;
                case "decimal":
                case "numeric":
                    sqlDbType = (int)SqlDbType.Decimal;
                    return DbType.Decimal;
                case "money":
                    sqlDbType = (int)SqlDbType.Money;
                    return DbType.Decimal;
                case "smallmoney":
                    sqlDbType = (int)SqlDbType.SmallMoney;
                    return DbType.Decimal;
                case "FILESTREAM attribute (varbinary(max)":
                case "rowversion":
                    sqlDbType = (int)SqlDbType.Binary;
                    return DbType.Binary;
                case "image":
                    sqlDbType = (int)SqlDbType.Image;
                    return DbType.Binary;
                case "timestamp":
                    sqlDbType = (int)SqlDbType.Timestamp;
                    return DbType.Binary;
                case "varbinary":
                    sqlDbType = (int)SqlDbType.VarBinary;
                    return DbType.Binary;
                case "float":
                    sqlDbType = (int)SqlDbType.Float;
                    return DbType.Double;
                case "nchar":
                    sqlDbType = (int)SqlDbType.NChar;
                    return DbType.StringFixedLength;
                case "real":
                    sqlDbType = (int)SqlDbType.Real;
                    return DbType.Single;
                case "time":
                    sqlDbType = (int)SqlDbType.Time;
                    return DbType.Time;
                case "tinyint":
                    sqlDbType = (int)SqlDbType.TinyInt;
                    return DbType.Byte;
                case "uniqueidentifier":
                    sqlDbType = (int)SqlDbType.UniqueIdentifier;
                    return DbType.Guid;
                case "xml":
                    sqlDbType = (int)SqlDbType.Xml;
                    return DbType.Xml;
                //case "sql_variant":
                default:
                    sqlDbType = (int)SqlDbType.Variant;
                    return DbType.Object;
            }
        }

        /// <summary>
        /// Read a tuple from database.
        /// </summary>
        /// <returns>
        /// A list of stored procedure parameters or null if no tuple is available.
        /// </returns>
        protected override List<DatabaseChannelParameter> ReadFromDatabase()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction trx = conn.BeginTransaction();
                List<DatabaseChannelParameter> list = null;
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = QueryTimeout;
                    command.Transaction = trx;
                    command.CommandText = ReadStoredProcedureName;

                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
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

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction trx = conn.BeginTransaction();
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = QueryTimeout;
                    command.Transaction = trx;
                    command.CommandText = WriteStoredProcedureName;

                    foreach (DatabaseChannelParameter msgParam in message)
                    {
                        SqlParameter cmdParam = command.CreateParameter();
                        if (msgParam.EngineDbType.HasValue)
                            cmdParam.SqlDbType = (SqlDbType)msgParam.EngineDbType.Value;
                        else
                            cmdParam.DbType = msgParam.DbType;
                        cmdParam.DbType = msgParam.DbType;
                        cmdParam.ParameterName = "@" + msgParam.ParameterName;
                        cmdParam.Value = msgParam.Value;

                        command.Parameters.Add(cmdParam);
                    }

                    command.ExecuteNonQuery();
                }

                trx.Commit();
            }
        }
    }
}