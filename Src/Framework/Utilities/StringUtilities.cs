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
using System.Text;
using Trx.Buffer;

namespace Trx.Utilities
{
    /// <summary>
    /// String handling utilities.
    /// </summary>
    public static class StringUtilities
    {
        public static readonly byte[] HexadecimalAsciiDigits = {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
            0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46
        };

        public static char[] NewLineChars = {'\r', '\n'};

        public const char XmlApos = '\'';
        public const char XmlQuot = '"';

        public static int Count(string source, char find)
        {
            int ret = 0;

            foreach (char s in source)
                if (s == find)
                    ++ret;

            return ret;
        }

        public static bool IsNumber(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;

            bool isNumber = true;

            for (int i = 0; i < data.Length; i++)
                if (!Char.IsDigit(data, i))
                {
                    isNumber = false;
                    break;
                }

            return isNumber;
        }

        public static string LeftOf(string source, char c)
        {
            int idx = source.IndexOf(c);

            if (idx == -1)
                return source;

            return source.Substring(0, idx);
        }

        public static string RightOf(string source, char c)
        {
            int idx = source.IndexOf(c);

            if (idx == -1)
                return string.Empty;

            return source.Substring(idx + 1);
        }

        public static string ToHexString(byte[] data, int offset, int length)
        {
            var buffer = new byte[length << 1];
            for (int i = 0; i < length; i++)
            {
                buffer[offset + (i << 1)] = HexadecimalAsciiDigits[(data[i] & 0xF0) >> 4];
                buffer[offset + (i << 1) + 1] = HexadecimalAsciiDigits[data[i] & 0x0F];
            }

            return FrameworkEncoding.GetInstance().Encoding.GetString(buffer);
        }

        public static string ToHexString(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return ToHexString(data, 0, data.Length);
        }

        public static string ToHexString(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return ToHexString(Encoding.Default.GetBytes(data), 0, data.Length);
        }

        public static byte[] FromHexStringToByte(string data)
        {
            if (data.Length%2 != 0)
                throw new ArgumentException("Insufficient data (length must be multiple of two).", "data");

            var result = new byte[data.Length/2];
            for (int i = data.Length - 1; i >= 0; i--)
            {
                int right = data[i] > 0x40
                    ? 10 + (data[i] > 0x60
                        ? data[i] - 0x61
                        : data[i] - 0x41)
                    : data[i] - 0x30;
                int left = data[--i] > 0x40
                    ? 10 + (data[i] > 0x60
                        ? data[i] - 0x61
                        : data[i] - 0x41)
                    : data[i] - 0x30;

                result[i >> 1] = (byte) ((left << 4) | right);
            }

            return result;
        }

        public static string FromHexStringToString(string data)
        {
            return Encoding.Default.GetString(FromHexStringToByte(data));
        }

        private static void EncodeXmlChar(ref StringBuilder sb, string text, int idx, char chr)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
                if (idx > 0)
                    sb.Append(text.Substring(0, idx));
            }

            sb.Append("&#x");
            sb.Append(((int)chr).ToString("X", NumberFormatInfo.InvariantInfo));
            sb.Append(';');
        }

        private static void EncodeXmlEntityRef(ref StringBuilder sb, string text, int idx, string name)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
                if (idx > 0)
                    sb.Append(text.Substring(0, idx));
            }

            sb.Append('&');
            sb.Append(name);
            sb.Append(';');
        }

        public static string EncodeElementValueXml(string text)
        {
            return EncodeXml(text, false, XmlQuot);
        }

        public static string EncodeAttrValueXml(string text, char attrQuoteChar)
        {
            if (attrQuoteChar != XmlQuot && attrQuoteChar != XmlApos)
                throw new ArgumentException("Only \" or ' are valid as attribute quote chars.", "attrQuoteChar");

            return EncodeXml(text, true, XmlQuot);
        }

        private static string EncodeXml(string text, bool isAttribute, char attrQuoteChar)
        {
            if (text == null)
                return string.Empty;

            StringBuilder sb = null;
            for (int i = 0; i < text.Length; i++)
            {
                var chr = text[i];
                switch (chr)
                {
                    case '<':
                        EncodeXmlEntityRef(ref sb, text, i, "lt");
                        break;
                    case '>':
                        EncodeXmlEntityRef(ref sb, text, i, "gt");
                        break;
                    case XmlQuot:
                        if (isAttribute && attrQuoteChar == XmlQuot)
                            EncodeXmlEntityRef(ref sb, text, i, "quot");
                        else if (sb != null)
                            sb.Append(chr);
                        break;
                    case XmlApos:
                        if (isAttribute && attrQuoteChar == XmlApos)
                            EncodeXmlEntityRef(ref sb, text, i, "apos");
                        else if (sb != null)
                            sb.Append(chr);
                        break;
                    case '&':
                        EncodeXmlEntityRef(ref sb, text, i, "amp");
                        break;
                    case '\r':
                    case '\n':
                        if (isAttribute)
                            EncodeXmlChar(ref sb, text, i, chr);
                        else if (sb != null)
                            sb.Append(chr);
                        break;
                    default:
                        if (char.IsHighSurrogate(chr))
                        {
                            if (++i == text.Length || !char.IsLowSurrogate(text[i]))
                                throw new ArgumentException(
                                    string.Format(
                                        "Hi surrogate char found at position {0} but low surrogate is missing.",
                                        i - 1), "text");
                            if (sb != null)
                            {
                                sb.Append(chr);
                                sb.Append(text[i]);
                            }
                        }
                        else if (char.IsLowSurrogate(chr))
                            throw new ArgumentException(
                                string.Format("Low surrogate char found at position {0} but hi surrogate is missing.",
                                    i), "text");
                        else if (char.IsControl(chr))
                            EncodeXmlChar(ref sb, text, i, chr);
                        else if (sb != null)
                            sb.Append(chr);
                        break;
                }
            }

            return sb == null ? text : sb.ToString();
        }

        /// <summary>
        /// Returns a printable string containing a human readable representation of the specified data.
        /// </summary>
        /// <param name="prefix">
        /// Prefix for the string to be returned.
        /// </param>
        /// <param name="data">
        /// Data to be represented.
        /// </param>
        /// <param name="offset">
        /// Start offset within data bounds.
        /// </param>
        /// <param name="len">
        /// Length of the data to be represented.
        /// </param>
        /// <returns>
        /// A printable string with a human readable representation of
        /// the specified data.
        /// </returns>
        public static string GetPrintableBuffer(string prefix, byte[] data, int offset, int len)
        {
            if ((data == null) || (data.Length == 0) || (len == 0) || ((offset + len) > data.Length))
                return string.Empty;

            var s = new StringBuilder(len*4);
            if (!string.IsNullOrEmpty(prefix))
            {
                s.Append(prefix);
                s.Append(Environment.NewLine);
            }

            char c;
            int i, j;
            const int charsPerLine = 20;
            s.Append("     1 |");
            for (i = 0; i < len; i++)
            {
                if ((i > 0) &&
                    ((i%charsPerLine) == 0))
                {
                    s.Append("| ");
                    for (j = i - charsPerLine; j < i; j++)
                    {
                        c = (char) data[j + offset];
                        c = (char) ((c >> 4) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                        c = (char) (((char) data[j + offset]) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                        if ((j + 1) < i)
                            s.Append(' ');
                    }
                    s.Append(Environment.NewLine);
                    s.Append(string.Format("{0,6} |", i + 1));
                }

                c = (char) data[i + offset];
                if ((c == ' ') || char.IsLetterOrDigit(c) || char.IsPunctuation(c) ||
                    char.IsSeparator(c) || char.IsSymbol(c))
                    s.Append(c);
                else
                    s.Append(".");
            }

            s.Append('|');
            if ((i%charsPerLine) == 0)
                j = 0;
            else
                j = charsPerLine - (i%charsPerLine);
            for (; j > 0; j--)
                s.Append(' ');
            s.Append(' ');
            if ((i%charsPerLine) == 0)
                j = i - charsPerLine;
            else
                j = i - (i%charsPerLine);
            for (; j < i; j++)
            {
                c = (char) data[j + offset];
                c = (char) ((c >> 4) & 0x0F);
                if (c < 10)
                    c += '0';
                else
                    c += '7';
                s.Append(c);
                c = (char) (((char) data[j + offset]) & 0x0F);
                if (c < 10)
                    c += '0';
                else
                    c += '7';
                s.Append(c);
                if ((j + 1) < i)
                    s.Append(' ');
            }

            return s.ToString();
        }

        /// <summary>
        /// Returns a printable string containing a human readable
        /// representation of the specified data.
        /// </summary>
        /// <param name="prefix">
        /// Prefix for the string to be returned.
        /// </param>
        /// <param name="data">
        /// Data to be represented.
        /// </param>
        /// <returns>
        /// A printable string with a human readable representation of
        /// the specified data.
        /// </returns>
        public static string GetPrintableBuffer(string prefix, byte[] data)
        {
            if ((data == null) || (data.Length == 0))
                return string.Empty;

            return GetPrintableBuffer(prefix, data, 0, data.Length);
        }

        /// <summary>
        /// Returns a printable string containing a human readable representation of the specified data.
        /// </summary>
        /// <param name="prefix">
        /// Prefix for the string to be returned.
        /// </param>
        /// <param name="buffer">
        /// The buffer to dump.
        /// </param>
        /// <param name="offset">
        /// Start offset within data bounds.
        /// </param>
        /// <param name="count">
        /// Length of the data to be represented.
        /// </param>
        /// <param name="rightHex">
        /// True if right hexadecimal dump must be printed.
        /// </param>
        /// <param name="twoLinesBytes">
        /// True if two lines with hexa bytes must be printed.
        /// </param>
        /// <returns>
        /// A printable string with a human readable representation of
        /// the specified data.
        /// </returns>
        public static string GetPrintableBuffer(string prefix, IBuffer buffer, int offset, int count, bool rightHex,
            bool twoLinesBytes)
        {
            if ((buffer == null) || (buffer.Capacity == 0) || (count == 0) || ((offset + count) > buffer.Capacity))
                return string.Empty;

            var s = new StringBuilder(count*4);
            if (!string.IsNullOrEmpty(prefix))
            {
                s.Append(prefix);
                s.Append(Environment.NewLine);
            }

            char c;
            int i, j;
            int charsPerLine = BufferDumpConfig.GetInstance().BytesPerLine;
            s.Append("     1 |");
            for (i = 0; i < count; i++)
            {
                if ((i > 0) &&
                    ((i%charsPerLine) == 0))
                {
                    s.Append('|');
                    if (rightHex)
                    {
                        s.Append(' ');
                        for (j = i - charsPerLine; j < i; j++)
                        {
                            if (buffer.InSecureArea(j + offset))
                                s.Append("**");
                            else
                            {
                                c = (char) buffer[j + offset];
                                c = (char) ((c >> 4) & 0x0F);
                                if (c < 10)
                                    c += '0';
                                else
                                    c += '7';
                                s.Append(c);
                                c = (char) (((char) buffer[j + offset]) & 0x0F);
                                if (c < 10)
                                    c += '0';
                                else
                                    c += '7';
                                s.Append(c);
                            }
                            if ((j + 1) < i)
                                s.Append(' ');
                        }
                    }

                    if (twoLinesBytes)
                    {
                        s.Append(Environment.NewLine);
                        s.Append("        ");
                        for (j = i - charsPerLine; j < i; j++)
                            if (buffer.InSecureArea(j + offset))
                                s.Append('*');
                            else
                            {
                                c = (char) buffer[j + offset];
                                c = (char) ((c >> 4) & 0x0F);
                                if (c < 10)
                                    c += '0';
                                else
                                    c += '7';
                                s.Append(c);
                            }

                        s.Append(Environment.NewLine);
                        s.Append("        ");
                        for (j = i - charsPerLine; j < i; j++)
                            if (buffer.InSecureArea(j + offset))
                                s.Append('*');
                            else
                            {
                                c = (char) (((char) buffer[j + offset]) & 0x0F);
                                if (c < 10)
                                    c += '0';
                                else
                                    c += '7';
                                s.Append(c);
                            }
                    }

                    s.Append(Environment.NewLine);
                    s.Append(string.Format("{0,6} |", i + 1));
                }

                if (buffer.InSecureArea(i + offset))
                    s.Append(".");
                else
                {
                    c = (char) buffer[i + offset];
                    if ((c == ' ') || char.IsLetterOrDigit(c) || char.IsPunctuation(c) ||
                        char.IsSeparator(c) || char.IsSymbol(c))
                        s.Append(c);
                    else
                        s.Append(".");
                }
            }

            s.Append('|');
            if (rightHex)
            {
                if ((i%charsPerLine) == 0)
                    j = 0;
                else
                    j = charsPerLine - (i%charsPerLine);
                for (; j > 0; j--)
                    s.Append(' ');
                s.Append(' ');
                if ((i%charsPerLine) == 0)
                    j = i - charsPerLine;
                else
                    j = i - (i%charsPerLine);
                for (; j < i; j++)
                {
                    if (buffer.InSecureArea(j + offset))
                        s.Append("**");
                    else
                    {
                        c = (char) buffer[j + offset];
                        c = (char) ((c >> 4) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                        c = (char) (((char) buffer[j + offset]) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                    }
                    if ((j + 1) < i)
                        s.Append(' ');
                }
            }

            if (twoLinesBytes)
            {
                s.Append(Environment.NewLine);
                s.Append("        ");
                for (j = i - (i%charsPerLine == 0 ? charsPerLine : i%charsPerLine); j < i; j++)
                    if (buffer.InSecureArea(j + offset))
                        s.Append('*');
                    else
                    {
                        c = (char) buffer[j + offset];
                        c = (char) ((c >> 4) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                    }

                s.Append(Environment.NewLine);
                s.Append("        ");
                for (j = i - (i%charsPerLine == 0 ? charsPerLine : i%charsPerLine); j < i; j++)
                    if (buffer.InSecureArea(j + offset))
                        s.Append('*');
                    else
                    {
                        c = (char) (((char) buffer[j + offset]) & 0x0F);
                        if (c < 10)
                            c += '0';
                        else
                            c += '7';
                        s.Append(c);
                    }
            }

            return s.ToString();
        }

        /// <summary>
        /// Returns a printable string containing a human readable representation of the specified data.
        /// </summary>
        /// <param name="prefix">
        /// Prefix for the string to be returned.
        /// </param>
        /// <param name="buffer">
        /// The buffer to dump.
        /// </param>
        /// <param name="offset">
        /// Start offset within data bounds.
        /// </param>
        /// <param name="count">
        /// Length of the data to be represented.
        /// </param>
        /// <returns>
        /// A printable string with a human readable representation of the data in the buffer.
        /// </returns>
        public static string DumpBufferData(string prefix, IBuffer buffer, int offset, int count)
        {
            BufferDumpFormat format = BufferDumpConfig.GetInstance().DumpFormat;
            return GetPrintableBuffer(prefix, buffer, offset, count,
                format == BufferDumpFormat.Both || format == BufferDumpFormat.TwoColumnsBufferAndHex,
                format == BufferDumpFormat.Both || format == BufferDumpFormat.BufferAndHexInNextTwoLines);
        }
    }
}