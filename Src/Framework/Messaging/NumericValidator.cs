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

using Trx.Utilities;

namespace Trx.Messaging
{
    /// <summary>
    /// It defines a class to validate numeric fields values.
    /// </summary>
    /// <remarks>
    /// This class implements the Singleton pattern, you must use
    /// <see cref="GetInstance()"/> or <see cref="GetInstance( bool )"/>
    /// to acquire the instance.
    /// </remarks>
    public class NumericValidator : IStringValidator
    {
        private static volatile NumericValidator _instanceDontAllowNulls;
        private static volatile NumericValidator _instanceAllowNulls;

        private readonly bool _allowNulls;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="allowNulls">
        /// true to accept null field values, otherwise false.
        /// </param>
        private NumericValidator(bool allowNulls)
        {
            _allowNulls = allowNulls;
        }

        #region IStringValidator Members
        /// <summary>
        /// It validates the field value.
        /// </summary>
        /// <param name="value">
        /// The value to validate.
        /// </param>
        /// <exception cref="StringValidationException">
        /// Thrown when the value isn't numeric.
        /// </exception>
        public void Validate(string value)
        {
            if (_allowNulls && string.IsNullOrEmpty(value))
                return;

            if (!StringUtilities.IsNumber(value))
                throw new StringValidationException(string.Format("The value '{0}' isn't a numeric value.", value));
        }
        #endregion

        /// <summary>
        /// It returns an instance of <see cref="NumericValidator"/>.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="NumericValidator"/> which doesn't support null fields.
        /// </returns>
        public static NumericValidator GetInstance()
        {
            return GetInstance(false);
        }

        /// <summary>
        /// It returns an instance of <see cref="NumericValidator"/>.
        /// </summary>
        /// <param name="allowNulls">
        /// true to accept null field values, otherwise false.
        /// </param>
        /// <returns>
        /// An instance of <see cref="NumericValidator"/>.
        /// </returns>
        public static NumericValidator GetInstance(bool allowNulls)
        {
            NumericValidator instance = allowNulls ? _instanceAllowNulls : _instanceDontAllowNulls;

            if (instance == null)
                lock (typeof (NumericValidator))
                {
                    if (allowNulls)
                    {
                        if (_instanceAllowNulls == null)
                            _instanceAllowNulls = new NumericValidator(true);
                        instance = _instanceAllowNulls;
                    }
                    else
                    {
                        if (_instanceDontAllowNulls == null)
                            _instanceDontAllowNulls = new NumericValidator(false);
                        instance = _instanceDontAllowNulls;
                    }
                }

            return instance;
        }
    }
}