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
using System.Threading;

namespace Trx.Logging
{
    public class XmlRenderer : IRenderer
    {
        #region IRenderer Members
        public string Render(DateTime dateTime, LogLevel level, object message)
        {
            return DoRender(dateTime, level, message, null);
        }

        public string Render(DateTime dateTime, LogLevel level, object message, Exception cause)
        {
            return DoRender(dateTime, level, message, cause);
        }
        #endregion

        private static void RenderException(Exception ex, StringBuilder sb, string elementName, string prefix,
            string indentation)
        {
            sb.Append(prefix);
            sb.Append('<');
            sb.Append(elementName);
            sb.AppendLine(">");
            sb.Append(prefix);
            sb.Append(indentation);
            sb.Append("<Message>");
            sb.Append(ex.Message);
            sb.AppendLine("</Message>");
            sb.Append(prefix);
            sb.Append(indentation);
            sb.AppendLine("<StackTrace>");
            sb.AppendLine(ex.StackTrace.Replace("   ", prefix + indentation + indentation));
            sb.Append(prefix);
            sb.Append(indentation);
            sb.AppendLine("</StackTrace>");
            if (ex.InnerException != null)
                RenderException(ex.InnerException, sb, "InnerException", prefix + indentation, indentation);
            sb.Append(prefix);
            sb.Append("</");
            sb.Append(elementName);
            sb.AppendLine(">");
        }

        private static string DoRender(DateTime dateTime, LogLevel level, object message, Exception cause)
        {
            var sb = new StringBuilder("<LogEntry ");
            if (LogManager.Domain != null)
            {
                sb.Append("Domain=\"");
                sb.Append(LogManager.Domain);
                sb.Append("\" ");
            }
            sb.Append(string.Format("DateTime=\"{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}.{6:000}\" " +
                "Level=\"{7}\" " +
                    "ManagedThreadId=\"{8}\">",
                dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond,
                level.ToString().ToUpper(), Thread.CurrentThread.ManagedThreadId));
            sb.AppendLine();

            if (message != null)
            {
                sb.Append("   <LogMessage>");
                if (message is IXmlRenderable)
                {
                    ((IXmlRenderable) message).XmlRender(sb, "      ", "   ");
                    sb.Append("   ");
                }
                else if (message is IRenderable)
                    ((IRenderable)message).Render(sb, "      ", "   ");
                else
                    sb.Append(message.ToString());
                sb.AppendLine("</LogMessage>");
            }

            if (cause != null)
                RenderException(cause, sb, "Exception", "   ", "   ");

            sb.AppendLine("</LogEntry>");

            return sb.ToString();
        }
    }
}