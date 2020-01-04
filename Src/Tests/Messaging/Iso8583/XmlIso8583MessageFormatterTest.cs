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

using NUnit.Framework;
using Trx.Buffer;
using Trx.Messaging;
using Trx.Messaging.Iso8583;

namespace Tests.Trx.Messaging.Iso8583
{
    /// <summary>
    /// Test fixture for XmlMessageFormatter.
    /// </summary>
    [TestFixture(Description = "ISO 8583 Xml formatter tests.")]
    public class XmlIso8583MessageFormatterTest
    {
        [Test(Description = "Format messages.")]
        public void Format()
        {
            var msg = new Iso8583Message(1100);
            var fmt = new XmlIso8583MessageFormatter { XmlRenderConfig = { Indent = false } };
            var fc = new FormatterContext(new SingleChunkBuffer());
            fmt.Format(msg, ref fc);
            var xml = fc.GetDataAsString();
            Assert.AreEqual("<Iso8583Message Mti=\"1100\"></Iso8583Message>", xml);
        }

        [Test(Description = "Parse messages.")]
        public void Parse()
        {
            var pc = new ParserContext(new SingleChunkBuffer());
            pc.Write("<Iso8583Message Mti=\"1200\"><Header Type=\"string\" Value=\"HEADER\" /><!-- Comments --><Field Number=\"41\" " +
                "Type=\"string\" Value=\"FLD00041\" /><Field Number=\"52\" Type=\"binary\" Value=\"0x303132\" />" +
                "<Message Number=\"120\"><Field Number=\"42\" Type=\"string\" Value=\"MERCHANT\" /></Message></Iso8583Message>");
            var fmt = new XmlIso8583MessageFormatter { XmlRenderConfig = { Indent = false } };
            var msg = fmt.Parse(ref pc);
            Assert.AreEqual(0, pc.DataLength);
            Assert.IsNotNull(msg);
            var isoMsg = msg as Iso8583Message;
            Assert.IsNotNull(isoMsg);

            Assert.AreEqual(1200, isoMsg.MessageTypeIdentifier);
            var inner = msg.Fields[120] as InnerMessageField;
            Assert.IsNotNull(inner);
            Assert.IsNotNull(inner.Value as Message);
            Assert.IsNull(inner.Value as Iso8583Message);

            // Parse with customized format.
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

            pc.Write("<isomsg id=\"1200\"><isomsg id=\"120\"><field id=\"42\" value=\"MERCHANT\" /></isomsg></isomsg>");

            msg = fmt.Parse(ref pc);
            Assert.AreEqual(0, pc.DataLength);
            Assert.IsNotNull(msg);
            isoMsg = msg as Iso8583Message;
            Assert.IsNotNull(isoMsg);

            Assert.AreEqual(1200, isoMsg.MessageTypeIdentifier);
            inner = msg.Fields[120] as InnerMessageField;
            Assert.IsNotNull(inner);
            Assert.IsNotNull(inner.Value as Message);
            Assert.IsNotNull(inner.Value as Iso8583Message);
        }
    }
}
