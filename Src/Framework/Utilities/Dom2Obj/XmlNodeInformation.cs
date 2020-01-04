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

using System.Reflection;
using System.Xml;

namespace Trx.Utilities.Dom2Obj
{
    public class XmlNodeInformation
    {
        private readonly int _lineNumber;
        private readonly int _linePosition;
        private readonly XmlNode _node;

        public XmlNodeInformation(XmlNode node, int lineNumber, int linePosition)
        {
            _node = node;
            _lineNumber = lineNumber;
            _linePosition = linePosition;
        }

        public XmlNode Node
        {
            get { return _node; }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int LinePosition
        {
            get { return _linePosition; }
        }

        public object Instance { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }
}