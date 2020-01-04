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
using Trx.Utilities.Dom2Obj;

namespace Trx.Messaging
{
    [Serializable]
    public class MessageSecuritySchema
    {
        /// <summary>
        /// We can't log values for these fields.
        /// </summary>
        private readonly List<RestrictedLogField> _restrictedLogFields = new List<RestrictedLogField>();

        public List<RestrictedLogField> RestrictedLogFields
        {
            get { return _restrictedLogFields; }
        }

        /// <summary>
        /// Get message security schema from a Xml formatter definition file.
        /// </summary>
        /// <param name="xmlFileName">
        /// The formatter definition.
        /// </param>
        public static MessageSecuritySchema GetFromFormatterXmlConfigFile(string xmlFileName)
        {
            var formatterDefinition = Digester.DigestFile(xmlFileName) as FormatterDefinition;
            return formatterDefinition == null ? null : formatterDefinition.MessageSecuritySchema;
        }

        /// <summary>
        /// It indicates if the specified field number can be logged.
        /// </summary>
        /// <param name="fieldNumber">
        /// The field number to known if can logged.
        /// </param>
        /// <returns>
        /// true if the field can be logged, otherwise false.
        /// </returns>
        public bool FieldCanBeLogged(int fieldNumber)
        {
            foreach (RestrictedLogField restrictedField in RestrictedLogFields)
                if (fieldNumber == restrictedField.FieldNumber)
                    return false;

            return true;
        }

        /// <summary>
        /// It returns the obfuscated field value.
        /// </summary>
        /// <param name="field">
        /// The field to be logged.
        /// </param>
        /// <returns>
        /// The data to be logged representing the obfuscated field value.
        /// </returns>
        public virtual string ObfuscateFieldData(Field field)
        {
            foreach (RestrictedLogField restrictedField in RestrictedLogFields)
                if (restrictedField.Obfuscator != null && field.FieldNumber == restrictedField.FieldNumber)
                    return restrictedField.Obfuscator.Obfuscate(field);

            return "__obfuscated__";
        }
    }
}
