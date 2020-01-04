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
using System.Text;

namespace Trx.Logging
{
    public class LogUnit : IRenderable, IXmlRenderable
    {
        private DateTime _creationDateTime = DateTime.Now;
        private readonly List<object> _messages = new List<object>();

        public LogUnit()
        {
        }

        public LogUnit(object message)
        {
            if (message != null)
                _messages.Add(message);
        }

        public LogUnit(params object[] messages)
        {
            foreach (var message in messages)
                _messages.Add(message);
        }

        public List<object> Messages
        {
            get { return _messages; }
        }

        public void Render(StringBuilder sb, string prefix, string indentation)
        {
            var appendLine = false;
            foreach (var message in _messages)
            {
                if (appendLine)
                    sb.AppendLine();
                if (message is IRenderable)
                    ((IRenderable)message).Render(sb, prefix, indentation);
                else
                {
                    sb.Append(prefix);
                    if (appendLine)
                        sb.Append(indentation);
                    sb.Append(message.ToString());
                }
                appendLine = true;
            }
        }

        public void XmlRender(StringBuilder sb, string prefix, string indentation)
        {
            if (_messages.Count == 0)
                return;

            sb.AppendLine();
            sb.Append(prefix);
            sb.AppendLine(string.Format(
                "<LogUnit CreationDateTime=\"{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}.{6:000}\">",
                _creationDateTime.Year, _creationDateTime.Month, _creationDateTime.Day,
                _creationDateTime.Hour, _creationDateTime.Minute, _creationDateTime.Second,
                _creationDateTime.Millisecond));

            foreach (var message in _messages)
            {
                sb.Append(prefix);
                sb.Append(indentation);
                if (message is IXmlRenderable)
                {
                    sb.AppendLine("<LogMessage>");
                    ((IXmlRenderable) message).XmlRender(sb, prefix + indentation + indentation, indentation);
                    sb.Append(prefix);
                    sb.Append(indentation);
                    sb.AppendLine("</LogMessage>");
                }
                else
                {
                    sb.Append("<LogMessage>");
                    sb.Append(message.ToString());
                    sb.AppendLine("</LogMessage>");
                }
            }

            sb.Append(prefix);
            sb.AppendLine("</LogUnit>");
        }
    }
}
