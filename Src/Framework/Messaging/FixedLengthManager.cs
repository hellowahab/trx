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

namespace Trx.Messaging {

    /// <summary>
    /// Implementa la clase que permite administrar largos constantes
    /// de datos de componentes de mensajería.
    /// </summary>
    public class FixedLengthManager : LengthManager {

        #region Constructors
        /// <summary>
        /// Construye un nuevo administrador de largos constantes de datos.
        /// </summary>
        /// <param name="length">
        /// Es el largo de los datos.
        /// </param>
        public FixedLengthManager( int length )
            : base( length ) {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Escribe el largo de los datos del campo en el contexto de
        /// formateo del mensaje.
        /// </summary>
        /// <param name="component">
        /// Es el componente de mensajería del que se está escribiendo
        /// el largo de sus datos.
        /// </param>
        /// <param name="dataLength">
        /// Es el largo de los datos del componente (puede diferir del largo
        /// de los datos obtenidos de <paramref name="component"/>, pues pueden
        /// estar rellenados)
        /// </param>
        /// <param name="encodedLength">
        /// Es el largo de los datos codificados.
        /// </param>
        /// <param name="formatterContext">
        /// Es el contexto de formateo del mensaje.
        /// </param>
        /// <exception cref="MessagingException">
        /// El largo de los datos no concuerda con el largo conocido por
        /// este administrador.
        /// </exception>
        public override void WriteLength( MessagingComponent component,
            int dataLength, int encodedLength, ref FormatterContext formatterContext ) {

            if ( dataLength != MaximumLength )
                throw new ArgumentOutOfRangeException("dataLength", dataLength,
                    string.Format("Insufficient data, {0} required.", MaximumLength));
        }

        /// <summary>
        /// Indica si existen datos suficientes como para leer el largo
        /// de los datos desde el contexto de análisis y construcción de mensajes.
        /// </summary>
        /// <param name="parserContext">
        /// Es el contexto de análisis y construcción de mensajes.
        /// </param>
        /// <returns>
        /// <see langref="true"/> en caso de que existan datos suficientes
        /// como para leer el largo de los datos, <see langref="false"/> en caso
        /// contrario.
        /// </returns>
        public override bool EnoughData( ref ParserContext parserContext ) {

            return true;
        }

        /// <summary>
        /// Lee desde el contexto de análisis y construcción de mensajes,
        /// el largo de los datos.
        /// </summary>
        /// <param name="parserContext">
        /// Es el contexto de análisis y construcción de mensajes.
        /// </param>
        /// <returns>
        /// El largo de los datos.
        /// </returns>
        public override int ReadLength( ref ParserContext parserContext ) {

            return MaximumLength;
        }
        #endregion
    }
}
