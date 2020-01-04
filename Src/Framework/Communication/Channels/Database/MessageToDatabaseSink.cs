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
using System.ComponentModel;
using System.Data;
using Trx.Messaging;

namespace Trx.Communication.Channels.Database
{
    /// <summary>
    /// Maps a message to and from database.
    /// </summary>
    public class MessageToDatabaseSink : ISink
    {
        private readonly Dictionary<int, string> _mapFieldNumberToParameter = new Dictionary<int, string>();
        private readonly Dictionary<string, int> _mapParameterToFieldNumber = new Dictionary<string, int>();
        private readonly Dictionary<int, InternalWriteType> _writeTypes = new Dictionary<int, InternalWriteType>();

        #region ISink Members
        /// <summary>
        /// Crea un nuevo objeto copiado de la instancia actual.
        /// </summary>
        /// <returns>
        /// Nuevo objeto que es una copia de esta instancia.
        /// </returns>
        public virtual object Clone()
        {
            return new MessageToDatabaseSink();
        }

        /// <summary>
        /// Called when a significant event (other than send or receive) was caught in the channel.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <param name="channelEvent">
        /// The event.
        /// </param>
        /// <returns>
        /// True if the event can be informed to the next sink in the pipeline, otherwise false.
        /// </returns>
        public bool OnEvent(PipelineContext context, ChannelEvent channelEvent)
        {
            return true;
        }

        /// <summary>
        /// Process the message to be sent.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <remarks>
        /// The message to be sent is stored in the <see cref="PipelineContext.MessageToSend"/>. If
        /// null is set by the sink the message is consumed and it stop going through the pipeline,
        /// whereby the channel doesn't send it.
        /// </remarks>
        public virtual void Send(PipelineContext context)
        {
            var message = context.MessageToSend as Message;
            if (message == null)
                throw new ChannelException("This sink implementation only support to send messages of type Message.");

            var list = new List<DatabaseChannelParameter>();
            foreach (Field field in message.Fields)
            {
                if (!_mapFieldNumberToParameter.ContainsKey(field.FieldNumber))
                    continue;

                int? engineDbType = null;
                DbType type = DbType.Object;
                object fldValue = field.Value;
                string paramName = _mapFieldNumberToParameter[field.FieldNumber];
                if (_writeTypes.ContainsKey(field.FieldNumber))
                {
                    Type dstType = _writeTypes[field.FieldNumber].Type;
                    if (fldValue.GetType() != dstType)
                        if (!TryToConvert(field.Value, dstType, ref fldValue))
                            new ChannelException(string.Format("Field {0} (destination parameter name {1}) " +
                                "cannot be converted from type {2} to type {3}.",
                                field.FieldNumber, paramName, field.Value.GetType(), dstType));
                    type = _writeTypes[field.FieldNumber].DbType;
                    engineDbType = _writeTypes[field.FieldNumber].EngineDbType;
                    string aux = _writeTypes[field.FieldNumber].WriteParameterName;
                    if (!string.IsNullOrEmpty(aux))
                        paramName = _writeTypes[field.FieldNumber].WriteParameterName;
                }
                else
                {
                    var sf = field as StringField;
                    if (sf == null)
                    {
                        var bf = field as BinaryField;
                        if (bf != null)
                            type = DbType.Binary;
                    }
                    else
                        type = DbType.String;
                }

                if (type != DbType.Object)
                    list.Add(new DatabaseChannelParameter
                                 {
                                     DbType = type,
                                     EngineDbType = engineDbType,
                                     ParameterName = paramName,
                                     Value = fldValue
                                 });
            }

            context.MessageToSend = list;
        }

        /// <summary>
        /// Process the received message.
        /// </summary>
        /// <param name="context">
        /// Pipeline context.
        /// </param>
        /// <returns>
        /// True if the pipeline can continue with the next sink, otherwise false.
        /// </returns>
        /// <remarks>
        /// The received message (if the sink creates one) is be stored in the
        /// <see cref="PipelineContext.ReceivedMessage"/> property.  If null is set by the sink the
        /// message is consumed and it stop going through the pipeline, whereby the channel doesn't 
        /// put in the receive tuple space.
        /// </remarks>
        public virtual bool Receive(PipelineContext context)
        {
            var list = context.ReceivedMessage as List<DatabaseChannelParameter>;
            if (list == null)
                throw new ChannelException(
                    "This sink implementation only support to receive messages of type List<DatabaseChannelParameter>.");

            Message message = NewMessage();
            foreach (DatabaseChannelParameter dbParam in list)
                if (_mapParameterToFieldNumber.ContainsKey(dbParam.ParameterName))
                    if (dbParam.DbType == DbType.Binary)
                        message.Fields.Add(new BinaryField(_mapParameterToFieldNumber[dbParam.ParameterName],
                            (byte[]) dbParam.Value));
                    else
                        message.Fields.Add(new StringField(_mapParameterToFieldNumber[dbParam.ParameterName],
                            dbParam.Value.ToString()));

            context.ReceivedMessage = message;

            return true;
        }
        #endregion

        private static bool TryToConvert(object data, Type type, ref object obj)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter == null)
                return false;

            if (!converter.CanConvertFrom(data.GetType()))
                return false;

            obj = converter.ConvertFrom(data);
            return true;
        }

        protected virtual Message NewMessage()
        {
            return new Message();
        }

        /// <summary>
        /// Map a field number to a database stored procedure paramenter name or table column name.
        /// </summary>
        /// <param name="fieldNumber">
        /// The message field number.
        /// </param>
        /// <param name="parameterName">
        /// The parameter/column name.
        /// </param>
        public void MapFieldNumberToDatabaseParameter(int fieldNumber, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException("parameterName");

            if (_mapFieldNumberToParameter.ContainsKey(fieldNumber))
            {
                _mapFieldNumberToParameter.Remove(fieldNumber);
                _mapParameterToFieldNumber.Remove(parameterName);
            }

            if (_writeTypes.ContainsKey(fieldNumber))
                _writeTypes.Remove(fieldNumber);

            _mapFieldNumberToParameter.Add(fieldNumber, parameterName);
            _mapParameterToFieldNumber.Add(parameterName, fieldNumber);
        }

        /// <summary>
        /// Map a field number to a database stored procedure paramenter name or table column name.
        /// </summary>
        /// <param name="fieldNumber">
        /// The message field number.
        /// </param>
        /// <param name="parameterName">
        /// The parameter/column name.
        /// </param>
        /// <param name="dbType">
        /// Type name to use on write.
        /// </param>
        /// <param name="typeName">
        /// Type name used to locate it's converter to get the object value to use on write.
        /// </param>
        public void MapFieldNumberToDatabaseParameter(int fieldNumber, string parameterName, DbType dbType,
            string typeName)
        {
            MapFieldNumberToDatabaseParameter(fieldNumber, parameterName);
            _writeTypes.Add(fieldNumber, new InternalWriteType {DbType = dbType, Type = Type.GetType(typeName)});
        }

        /// <summary>
        /// Map a field number to a database stored procedure paramenter name or table column name.
        /// </summary>
        /// <param name="fieldNumber">
        /// The message field number.
        /// </param>
        /// <param name="parameterName">
        /// The parameter/column name.
        /// </param>
        /// <param name="dbType">
        /// Type name to use on write.
        /// </param>
        /// <param name="typeName">
        /// Type name used to locate it's converter to get the object value to use on write.
        /// </param>
        /// <param name="writeParameterName">
        /// Name of the write parameter, if null or empty <paramref name="parameterName"/> is used.
        /// </param>
        public void MapFieldNumberToDatabaseParameter(int fieldNumber, string parameterName, DbType dbType,
            string typeName, string writeParameterName)
        {
            MapFieldNumberToDatabaseParameter(fieldNumber, parameterName);
            _writeTypes.Add(fieldNumber,
                new InternalWriteType
                    {
                        DbType = dbType,
                        Type = Type.GetType(typeName),
                        WriteParameterName = writeParameterName
                    });
        }

        /// <summary>
        /// Map a field number to a database stored procedure paramenter name or table column name.
        /// </summary>
        /// <param name="fieldNumber">
        /// The message field number.
        /// </param>
        /// <param name="parameterName">
        /// The parameter/column name.
        /// </param>
        /// <param name="engineDbType">
        /// Type name to use on write.
        /// </param>
        /// <param name="typeName">
        /// Type name used to locate it's converter to get the object value to use on write.
        /// </param>
        public void MapFieldNumberToDatabaseParameter(int fieldNumber, string parameterName, int engineDbType,
            string typeName)
        {
            MapFieldNumberToDatabaseParameter(fieldNumber, parameterName);
            _writeTypes.Add(fieldNumber,
                new InternalWriteType {EngineDbType = engineDbType, Type = Type.GetType(typeName)});
        }

        /// <summary>
        /// Map a field number to a database stored procedure paramenter name or table column name.
        /// </summary>
        /// <param name="fieldNumber">
        /// The message field number.
        /// </param>
        /// <param name="parameterName">
        /// The parameter/column name.
        /// </param>
        /// <param name="engineDbType">
        /// Type name to use on write.
        /// </param>
        /// <param name="typeName">
        /// Type name used to locate it's converter to get the object value to use on write.
        /// </param>
        /// <param name="writeParameterName">
        /// Name of the write parameter, if null or empty <paramref name="parameterName"/> is used.
        /// </param>
        public void MapFieldNumberToDatabaseParameter(int fieldNumber, string parameterName, int engineDbType,
            string typeName, string writeParameterName)
        {
            MapFieldNumberToDatabaseParameter(fieldNumber, parameterName);
            _writeTypes.Add(fieldNumber,
                new InternalWriteType
                    {
                        EngineDbType = engineDbType,
                        Type = Type.GetType(typeName),
                        WriteParameterName = writeParameterName
                    });
        }

        #region Nested type: InternalWriteType
        private struct InternalWriteType
        {
            public DbType DbType;
            public int? EngineDbType;
            public Type Type;
            public string WriteParameterName;
        }
        #endregion
    }
}