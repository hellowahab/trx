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
using System.Globalization;
using System.Reflection;
using Trx.Logging;
using Trx.Utilities;
using Trx.Utilities.Dom2Obj;

namespace Trx.Messaging
{
    public abstract class BaseMessageFormatter : IMessageFormatter
    {
        private readonly FieldFormatterCollection _fieldsFormatters;

        private int[] _bitmaps;
        private string _description;
        private IMessageHeaderFormatter _headerFormatter;
        private ILogger _logger;
        private string _name;
        private byte[] _packetHeader;

        /// <summary>
        /// It builds a new messages formatter.
        /// </summary>
        protected BaseMessageFormatter()
        {
            _description = string.Empty;
            _name = string.Empty;
            _headerFormatter = null;
            _bitmaps = new int[4];
            InitializeBitmapTable(_bitmaps);
            _fieldsFormatters = new FieldFormatterCollection();
            _fieldsFormatters.Added += OnFieldsFormatterAdded;
            _fieldsFormatters.Cleared += OnFieldsFormattersCleared;
            _fieldsFormatters.Removed += OnFieldsFormatterRemoved;
        }

        /// <summary>
        /// Creates a message formatter with the definition in file given Xml cofiguration file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        /// <param name="typeName">
        /// Expected formatter type contained in the definition.
        /// </param>
        protected BaseMessageFormatter(string xmlFileName, string typeName)
            : this()
        {
            ProcessFormatterDefinition(xmlFileName, typeName);
        }

        /// <summary>
        /// It returns or assigns the description of the messages formatter.
        /// </summary>
        public string Description
        {
            get { return _description; }

            set { _description = value; }
        }

        protected int[] Bitmaps
        {
            get { return _bitmaps; }
        }

        /// <summary>
        /// It returns or assigns the logger employed by the instance.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                return _logger ??
                    (_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString()));
            }
            set { _logger = value ?? LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString()); }
        }

        /// <summary>
        /// It returns or assigns the name of the logger that is utilized.
        /// </summary>
        public string LoggerName
        {
            set { Logger = string.IsNullOrEmpty(value) ? null : LogManager.GetLogger(value); }
            get { return Logger.Name; }
        }

        /// <summary>
        /// Returns the field formatter for the specified field number.  
        /// </summary>
        /// <remarks>
        /// If the field formatter does not exist, a null value is returned.
        /// </remarks>
        public FieldFormatter this[int fieldNumber]
        {
            get { return _fieldsFormatters[fieldNumber]; }
        }

        /// <summary>
        /// It returns or assigns the message header formatter. 
        /// </summary>
        public IMessageHeaderFormatter MessageHeaderFormatter
        {
            get { return _headerFormatter; }

            set { _headerFormatter = value; }
        }

        /// <summary>
        /// It returns or assigns the name of messages formatter instance. 
        /// </summary>
        public string Name
        {
            get { return _name; }

            set { _name = value; }
        }

        /// <summary>
        /// Get or set packet header.
        /// </summary>
        public string PacketHeader
        {
            get { return _packetHeader == null ? null : FrameworkEncoding.GetInstance().Encoding.GetString(_packetHeader); }
            set { _packetHeader = value == null ? null : FrameworkEncoding.GetInstance().Encoding.GetBytes(value); }
        }

        public MessageSecuritySchema MessageSecuritySchema { get; set; }

        /// <summary>
        /// Set packet header, but can be specified in hex (i.e. 840 = 383430).
        /// </summary>
        public string HexadecimalPacketHeader
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    _packetHeader = null;
                else
                {
                    _packetHeader = new byte[(value.Length + 1) >> 1];

                    // Initialize result bytes.
                    for (int i = _packetHeader.Length - 1; i >= 0; i--)
                        _packetHeader[i] = 0;

                    // Format data.
                    for (int i = 0; i < value.Length; i++)
                        if (value[i] < 0x40)
                            _packetHeader[(i >> 1)] |=
                                (byte) (((value[i]) - 0x30) << ((i & 1) == 1 ? 0 : 4));
                        else
                            _packetHeader[(i >> 1)] |=
                                (byte) (((value[i]) - 0x37) << ((i & 1) == 1 ? 0 : 4));
                }
            }
        }

        #region IMessageFormatter Members
        /// <summary>
        /// It returns the collection of field formatters known by this instance of messages formatter.
        /// </summary>
        public FieldFormatterCollection FieldFormatters
        {
            get { return _fieldsFormatters; }
        }

        public abstract object Clone();

        /// <summary>
        /// It formats a message.
        /// </summary>
        /// <param name="message">
        /// It's the message to be formatted.
        /// </param>
        /// <param name="formatterContext">
        /// It's the formatter context to be used in the format.
        /// </param>
        public abstract void Format(Message message, ref FormatterContext formatterContext);

        /// <summary>
        /// It parses the data contained in the parser context.
        /// </summary>
        /// <param name="parserContext">
        /// It's the context holding the information to produce a new message instance.
        /// </param>
        /// <returns>
        /// The parsed message, or a null reference if the data contained in the context
        /// is insufficient to produce a new message.
        /// </returns>
        public abstract Message Parse(ref ParserContext parserContext);
        #endregion

        public virtual void CopyTo(BaseMessageFormatter messageFormatter)
        {
            messageFormatter.Description = _description;
            messageFormatter.Logger = _logger;
            messageFormatter.Name = _name;
            messageFormatter.MessageHeaderFormatter = _headerFormatter;
            MessageSecuritySchema = MessageSecuritySchema;

            lock (_fieldsFormatters)
            {
                foreach (FieldFormatter fieldFormatter in _fieldsFormatters)
                    messageFormatter.FieldFormatters.Add(fieldFormatter);
            }
        }

        /// <summary>
        /// It initializes an array of bitmaps.
        /// </summary>
        /// <param name="bitmaps">
        /// It's the array to initialize.
        /// </param>
        private static void InitializeBitmapTable(int[] bitmaps)
        {
            for (int i = 0; i < bitmaps.Length; i++)
                bitmaps[i] = int.MinValue;
        }

        /// <summary>
        /// It handles the event fired when a field formatter is added to
        /// the collection of field formatters.
        /// </summary>
        /// <param name="sender">
        /// The object sending the event.
        /// </param>
        /// <param name="e">
        /// The event parameters.
        /// </param>
        private void OnFieldsFormatterAdded(object sender, FieldFormatterEventArgs e)
        {
            if (e.FieldFormatter is BitMapFieldFormatter)
            {
                // Check if is in our bitmap table.
                for (int i = _bitmaps.Length - 1; i >= 0; i--)
                    if (_bitmaps[i] == e.FieldFormatter.FieldNumber) // It's in, don't add.
                        return;

                if (_bitmaps[0] != int.MinValue)
                {
                    // It's full, expand it.
                    var bitmaps = new int[_bitmaps.Length*2];

                    InitializeBitmapTable(bitmaps);

                    // Copy previous data.
                    for (int i = _bitmaps.Length - 1; i >= 0; i--)
                        bitmaps[_bitmaps.Length + i] = _bitmaps[i];
                    _bitmaps = bitmaps;
                }

                _bitmaps[0] = e.FieldFormatter.FieldNumber;
                Array.Sort(_bitmaps);
            }
        }

        /// <summary>
        /// It handles the event fired by the field formatters collection when
        /// all the elements are removed.
        /// </summary>
        /// <param name="sender">
        /// The object sending the event.
        /// </param>
        /// <param name="e">
        /// The event parameters.
        /// </param>
        private void OnFieldsFormattersCleared(object sender, EventArgs e)
        {
            InitializeBitmapTable(_bitmaps);
        }

        /// <summary>
        /// It handles the event fired when a field formatter is removed from
        /// the collection of field formatters.
        /// </summary>
        /// <param name="sender">
        /// The object sending the event.
        /// </param>
        /// <param name="e">
        /// The event parameters.
        /// </param>
        private void OnFieldsFormatterRemoved(object sender, FieldFormatterEventArgs e)
        {
            if (e.FieldFormatter is BitMapFieldFormatter) // Check if is in our bitmap table.
                for (int i = _bitmaps.Length - 1; i >= 0; i--)
                    if (_bitmaps[i] == e.FieldFormatter.FieldNumber)
                    {
                        // Located, erase it.
                        _bitmaps[i] = int.MinValue;
                        Array.Sort(_bitmaps);
                        break;
                    }
        }

        protected FormatterDefinition ProcessFormatterDefinition(string xmlFileName, string typeName)
        {
            var formatterDefinition = Digester.DigestFile(xmlFileName) as FormatterDefinition;
            if (formatterDefinition == null)
                throw new MessagingException(string.Format("Invalid root object in config file '{0}', " +
                    "an object of type {1} was expected", xmlFileName, typeName));

            if (formatterDefinition.ClearDescription)
                Description = null;

            if (formatterDefinition.Description != null)
                Description = formatterDefinition.Description;

            if (formatterDefinition.ClearPacketHeader)
                PacketHeader = null;

            if (formatterDefinition.PacketHeader != null)
                PacketHeader = formatterDefinition.PacketHeader;

            if (formatterDefinition.HexadecimalPacketHeader != null)
                HexadecimalPacketHeader = formatterDefinition.HexadecimalPacketHeader;

            if (formatterDefinition.ClearMessageHeaderFormatter)
                MessageHeaderFormatter = null;

            if (formatterDefinition.MessageHeaderFormatter != null)
                MessageHeaderFormatter = formatterDefinition.MessageHeaderFormatter;

            if (formatterDefinition.ClearMessageSecuritySchema)
                MessageSecuritySchema = null;

            if (formatterDefinition.MessageSecuritySchema != null)
                MessageSecuritySchema = formatterDefinition.MessageSecuritySchema;

            if (formatterDefinition.ClearFieldsFormatters)
                _fieldsFormatters.Clear();

            foreach (FieldFormatterFactory fieldFormatter in formatterDefinition.FieldsFormattersFactories)
                _fieldsFormatters.Add(fieldFormatter.GetInstance());

            return formatterDefinition;
        }

        public virtual Message NewMessage()
        {
            return new Message();
        }

        public virtual bool FieldFormatterIsPresent(int fieldNumber)
        {
            return FieldFormatters.Contains(fieldNumber);
        }

        public virtual FieldFormatter GetFieldFormatter(int fieldNumber)
        {
            return FieldFormatters[fieldNumber];
        }

        public virtual int MaximumFieldFormatterNumber()
        {
            return FieldFormatters.MaximumFieldFormatterNumber;
        }

        /// <summary>
        /// It returns an array of integers containing the numbers of the bimatps formatters
        /// known by the message formatter instance. 
        /// </summary>
        /// <returns>
        /// An array of integers containing the numbers of the bitmaps formatters
        /// known by the message formatter instance. The numbers are orderer in
        /// descendent mode.
        /// If no bitmap formatters are found, a null value is returned.
        /// </returns>
        public int[] GetBitMapFieldNumbers()
        {
            int found = 0;
            int[] bitmaps = null;

            // Count bitmaps.
            for (int i = Bitmaps.Length - 1; (i >= 0) && (Bitmaps[i] >= 0); i--)
                found++;

            if (found > 0)
            {
                bitmaps = new int[found];

                // Copy bitmaps.
                for (int i = Bitmaps.Length - 1; (i >= 0) && (Bitmaps[i] >= 0); i--)
                    bitmaps[bitmaps.Length - (Bitmaps.Length - i)] = Bitmaps[i];
            }

            return bitmaps;
        }

        private static FieldAttribute GetFieldAttribute(ICustomAttributeProvider propertyInfo)
        {
            object[] attributes = propertyInfo.GetCustomAttributes(
                typeof (FieldAttribute), false);

            if (attributes.Length == 1)
                return (FieldAttribute) attributes[0];

            return null;
        }

        public void AssignFields(Message message, object fieldsContainer)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (fieldsContainer == null)
                throw new ArgumentNullException("fieldsContainer");

            foreach (PropertyInfo propertyInfo in
                fieldsContainer.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance)) // Property must be readable and not indexed.
                if ((propertyInfo.CanRead) &&
                    (propertyInfo.GetIndexParameters().Length == 0))
                {
                    // Get FieldAttribute.
                    FieldAttribute fieldAttribute =
                        GetFieldAttribute(propertyInfo);

                    if (fieldAttribute != null)
                        if (FieldFormatterIsPresent(
                            fieldAttribute.FieldNumber))
                        {
                            FieldFormatter fieldFormatter =
                                GetFieldFormatter(fieldAttribute.FieldNumber);

                            if (fieldFormatter is StringFieldFormatter)
                                if (((StringFieldFormatter)
                                    (fieldFormatter)).ValueFormatter == null)
                                    // Field formatter doesn't provide a valid value
                                    // formatter, default is ToString method.
                                    message.Fields.Add(fieldAttribute.FieldNumber,
                                        propertyInfo.GetValue(fieldsContainer,
                                            null).ToString());
                                else // Format property value with value formatter.
                                    message.Fields.Add(fieldAttribute.FieldNumber,
                                        ((StringFieldFormatter)
                                            (fieldFormatter)).ValueFormatter.Format(
                                                propertyInfo.GetValue(fieldsContainer,
                                                    null)));
                            else // Field formatter isn't a StringFieldFormatter,
                                // default is ToString method.
                                message.Fields.Add(fieldAttribute.FieldNumber,
                                    propertyInfo.GetValue(fieldsContainer,
                                        null).ToString());
                        }
                        else // Field formatter doesn't include a valid field
                            // value formatter, default is ToString method.
                            message.Fields.Add(fieldAttribute.FieldNumber,
                                propertyInfo.GetValue(fieldsContainer,
                                    null).ToString());
                }
        }

        private static void ApplyDefaultConvertion(object fieldsContainer, PropertyInfo propertyInfo,
            FieldAttribute fieldAttribute, object valueToConvert)
        {
            object convertedValue = null;

            try
            {
                // These are our default supported types.
                switch (propertyInfo.PropertyType.Name)
                {
                    case "Object":
                        convertedValue = valueToConvert;
                        break;
                    case "Boolean":
                        convertedValue = Convert.ToBoolean(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Byte":
                        convertedValue = Convert.ToByte(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Char":
                        convertedValue = Convert.ToChar(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "DateTime":
                        convertedValue = Convert.ToDateTime(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Decimal":
                        convertedValue = Convert.ToDecimal(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Double":
                        convertedValue = Convert.ToDouble(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Int16":
                        convertedValue = Convert.ToInt16(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Int32":
                        convertedValue = Convert.ToInt32(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Int64":
                        convertedValue = Convert.ToInt64(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "SByte":
                        convertedValue = Convert.ToSByte(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "Single":
                        convertedValue = Convert.ToSingle(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "String":
                        convertedValue = Convert.ToString(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "UInt16":
                        convertedValue = Convert.ToUInt16(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "UInt32":
                        convertedValue = Convert.ToUInt32(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                    case "UInt64":
                        convertedValue = Convert.ToUInt64(valueToConvert, CultureInfo.InvariantCulture);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new MessagingException(string.Format("Can't convert field '{1}' value to property '{0}'.",
                    fieldAttribute.FieldNumber, propertyInfo.Name), e);
            }

            if (convertedValue != null) // Set property value.
                propertyInfo.SetValue(fieldsContainer, convertedValue, null);
        }

        public void RetrieveFields(Message message, object fieldsContainer)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (fieldsContainer == null)
                throw new ArgumentNullException("fieldsContainer");

            foreach (PropertyInfo propertyInfo in
                fieldsContainer.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance)) // Property must be writable and not indexed.
                if ((propertyInfo.CanWrite) &&
                    (propertyInfo.GetIndexParameters().Length == 0))
                {
                    // Get FieldAttribute.
                    FieldAttribute fieldAttribute =
                        GetFieldAttribute(propertyInfo);

                    if (fieldAttribute != null)
                        if (message.Fields.Contains(fieldAttribute.FieldNumber) &&
                            FieldFormatterIsPresent(fieldAttribute.FieldNumber))
                        {
                            FieldFormatter fieldFormatter =
                                GetFieldFormatter(fieldAttribute.FieldNumber);

                            if (fieldFormatter is StringFieldFormatter)
                                if (((StringFieldFormatter)
                                    (fieldFormatter)).ValueFormatter == null)
                                    // Field formatter doesn't provide a valid value
                                    // formatter, try default convertion.
                                    ApplyDefaultConvertion(fieldsContainer,
                                        propertyInfo, fieldAttribute,
                                        message[fieldAttribute.FieldNumber].Value);
                                else
                                    propertyInfo.SetValue(fieldsContainer,
                                        ((StringFieldFormatter)
                                            (fieldFormatter)).ValueFormatter.Parse(
                                                propertyInfo.PropertyType,
                                                message[fieldAttribute.FieldNumber].ToString()), null);
                            else // Field formatter isn't a StringFieldFormatter,
                                // try default convertion.
                                ApplyDefaultConvertion(fieldsContainer,
                                    propertyInfo, fieldAttribute,
                                    message[fieldAttribute.FieldNumber].Value);
                        }
                        else // Field formatter doesn't include a valid field
                            // value formatter, try default convertion.
                            ApplyDefaultConvertion(fieldsContainer,
                                propertyInfo, fieldAttribute,
                                message[fieldAttribute.FieldNumber].Value);
                }
        }
    }
}