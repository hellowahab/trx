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
using System.Text;
using Trx.Logging;

namespace Trx.Messaging
{
    [Serializable]
    public abstract class MessagingComponent : ICloneable, IDisposable, IXmlRenderable
    {
        private static MessagingComponentXmlRenderConfig _xmlRenderConfig = new MessagingComponentXmlRenderConfig();

        public static MessagingComponentXmlRenderConfig XmlRenderConfig
        {
            get { return _xmlRenderConfig; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _xmlRenderConfig = value;
            }
        }

        #region ICloneable Members
        public abstract object Clone();
        #endregion

        #region IDisposable Members
        public virtual void Dispose()
        {
        }
        #endregion

        #region IXmlRenderable Members
        public void XmlRender(StringBuilder sb, string prefix, string indentation)
        {
            XmlRender(sb, prefix, indentation, XmlRenderConfig);
        }
        #endregion

        public abstract void XmlRender(StringBuilder sb, string prefix, string indentation,
            MessagingComponentXmlRenderConfig xmlRenderConfig);

        public abstract byte[] GetBytes();

        public override string ToString()
        {
            return string.Empty;
        }

        public abstract MessagingComponent NewComponent();
    }
}