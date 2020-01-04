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

using System.Xml;
using NUnit.Framework;
using Trx.Buffer;
using Trx.Messaging;

namespace Tests.Trx.Messaging
{
    /// <summary>
    /// Test fixture for XmlMessageFormatter.
    /// </summary>
    [TestFixture(Description = "Xml formatter tests.")]
    public class XmlMessageFormatterTest
    {
        [Test(Description = "Format messages.")]
        public void Format()
        {
            var msg = new Message();
            var fmt = new XmlMessageFormatter { XmlRenderConfig = { Indent = false } };
            var fc = new FormatterContext(new SingleChunkBuffer());
            fmt.Format(msg, ref fc);
            var xml = fc.GetDataAsString();
            Assert.AreEqual("<Message></Message>", xml);

            msg.Header = new StringMessageHeader("HEADER");
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<Message><Header Type=\"string\" Value=\"HEADER\" /></Message>", xml);

            msg.Fields.Add(41, "FLD00041");
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<Message><Header Type=\"string\" Value=\"HEADER\" /><Field Number=\"41\" Type=\"string\" Value=\"FLD00041\" /></Message>", xml);

            msg.Fields.Add(52, new byte[] {0x30, 0x31, 0x32});
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<Message><Header Type=\"string\" Value=\"HEADER\" /><Field Number=\"41\" Type=\"string\" Value=\"FLD00041\" />" +
                "<Field Number=\"52\" Type=\"binary\" Value=\"0x303132\" /></Message>", xml);

            var inner = new Message();
            inner.Fields.Add(42, "MERCHANT");
            msg.Fields.Add(120, inner);
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<Message><Header Type=\"string\" Value=\"HEADER\" /><Field Number=\"41\" Type=\"string\" Value=\"FLD00041\" />" +
                "<Field Number=\"52\" Type=\"binary\" Value=\"0x303132\" /><Message Number=\"120\">" +
                "<Field Number=\"42\" Type=\"string\" Value=\"MERCHANT\" /></Message></Message>", xml);

            // Test customization.
            fmt.XmlRenderConfig.MessageTag = "isomsg";
            fmt.XmlRenderConfig.HeaderTag = "header";
            fmt.XmlRenderConfig.FieldTag = "field";
            fmt.XmlRenderConfig.NumberAttr = "id";
            fmt.XmlRenderConfig.TypeAttr = "type";
            fmt.XmlRenderConfig.ValueAttr = "value";
            fmt.XmlRenderConfig.Iso8583MessageTag = "isomsg";
            fmt.XmlRenderConfig.Iso8583MtiAttr = "id";
            fmt.XmlRenderConfig.IncludeTypeForStringField = false;
            fmt.XmlRenderConfig.PrefixHexString = false;
            fmt.XmlRenderConfig.HeaderValueInContent = true;
            fmt.XmlRenderConfig.IncludeTypeForStringHeader = false;
            fmt.XmlRenderConfig.HeaderValueInHex = true;
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<isomsg><header>484541444552</header><field id=\"41\" value=\"FLD00041\" />" +
                "<field id=\"52\" type=\"binary\" value=\"303132\" /><isomsg id=\"120\">" +
                "<field id=\"42\" value=\"MERCHANT\" /></isomsg></isomsg>", xml);

            // Test Xml normalization.
            msg = new Message();
            msg.Fields.Add(41, "\"<>");
            fc.Clear();
            fmt.Format(msg, ref fc);
            xml = fc.GetDataAsString();
            Assert.AreEqual("<isomsg><field id=\"41\" value=\"&quot;&lt;&gt;\" /></isomsg>", xml);
        }

        [Test(Description = "Parse messages.")]
        public void Parse()
        {
            var fmt = new XmlMessageFormatter { XmlRenderConfig = { Indent = false } };
            
            var pc2 = new ParserContext(new SingleChunkBuffer());
            pc2.Write("invalid data");
            Assert.Throws<XmlException>(() => fmt.Parse(ref pc2));
            Assert.AreEqual(12, pc2.DataLength);

            var pc = new ParserContext(new SingleChunkBuffer());
            // Not enough data to parse a message.
            var msg = fmt.Parse(ref pc);
            Assert.IsNull(msg);

            pc.Write("<");
            msg = fmt.Parse(ref pc);
            Assert.IsNull(msg);
            Assert.AreEqual(1, pc.DataLength);

            pc.Write("Message><Header Type=\"string\" Value=\"HEADER\" /><!-- Comments --><Field Number=\"41\" " +
                "Type=\"string\" Value=\"FLD00041\" /><Field Number=\"52\" Type=\"binary\" Value=\"0x303132\" /><Messa");
            msg = fmt.Parse(ref pc);
            Assert.IsNull(msg);
            Assert.AreEqual(175, pc.DataLength);

            pc.Write("ge Number=\"120\"><Field Number=\"42\" Type=\"string\" Value=\"MERCHANT\" /></Message></Message>");
            msg = fmt.Parse(ref pc);
            Assert.IsNotNull(msg);
            Assert.AreEqual(0, pc.DataLength);

            Assert.IsNotNull(msg.Header);
            Assert.AreEqual("HEADER", msg.Header.ToString());

            Assert.AreEqual(3, msg.Fields.Count);
            Assert.IsTrue(msg.Fields.Contains(41));
            Assert.IsTrue(msg.Fields.Contains(52));
            Assert.IsTrue(msg.Fields.Contains(120));

            Assert.AreEqual("FLD00041", msg.Fields[41].ToString());
            Assert.AreEqual(new byte[] { 0x30, 0x31, 0x32 }, msg.Fields[52].GetBytes());
            var inner = msg.Fields[120] as InnerMessageField;
            Assert.IsNotNull(inner);
            var innerMsg = inner.Value as Message;
            Assert.IsNotNull(innerMsg);

            Assert.AreEqual(1, innerMsg.Fields.Count);
            Assert.IsTrue(innerMsg.Fields.Contains(42));
            Assert.AreEqual("MERCHANT", innerMsg.Fields[42].ToString());

            // Parse with known frame size.
            var pc3 = new ParserContext(new SingleChunkBuffer());
            pc3.Write("<Message />");
            pc3.FrameSize = 10;
            Assert.Throws<XmlException>(() => fmt.Parse(ref pc3));

            var pc4 = new ParserContext(new SingleChunkBuffer());
            pc4.Write("<Message />");
            pc4.FrameSize = 11;
            msg = fmt.Parse(ref pc4);
            Assert.IsNotNull(msg);
            Assert.AreEqual(0, pc4.DataLength);
        }
    }
}