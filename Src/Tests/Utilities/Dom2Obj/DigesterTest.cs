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
using Trx.Utilities.Dom2Obj;

namespace Tests.Trx.Utilities.Dom2Obj
{
    [TestFixture(Description = "Digester tests.")]
    public class DigesterTest
    {
        [Test(Description = "Simple instantiation test (no properties, declares & invokes).")]
        public void SimpleInstantiation()
        {
            // Default constructor (no args).
            var obj = Digester.DigestString("<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\" />") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNull(obj.Value4);
            Assert.IsFalse(obj.InvokeFlag);

            // SimpleObject(string)
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Constructor>" +
                        "<Parameter Value=\"Test value 1\" />" +
                    "</Constructor>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNull(obj.Value4);

            // SimpleObject(string, string)
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Constructor>" +
                        "<Parameter Value=\"Test value 1\" />" +
                        "<Parameter Value=\"Test value 2\" />" +
                    "</Constructor>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreEqual("Test value 2", obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNull(obj.Value4);

            // SimpleObject(string, string, int)
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Constructor>" +
                        "<Parameter Value=\"Test value 1\" />" +
                        "<Parameter Value=\"Test value 2\" />" +
                        "<Parameter Type=\"int\" Value=\"5\" />" +
                    "</Constructor>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreEqual("Test value 2", obj.Value2);
            Assert.AreEqual(5, obj.Value3);
            Assert.IsNull(obj.Value4);

            // SimpleObject(string, string, int, SimpleObject(no args))
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Constructor>" +
                        "<Parameter Value=\"Test value 1\" />" +
                        "<Parameter Value=\"Test value 2\" />" +
                        "<Parameter Type=\"int\" Value=\"5\" />" +
                        "<Parameter Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\" />" +
                    "</Constructor>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreEqual("Test value 2", obj.Value2);
            Assert.AreEqual(5, obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNull(obj.Value4.Value1);
            Assert.IsNull(obj.Value4.Value2);
            Assert.AreEqual(default(int), obj.Value4.Value3);
            Assert.IsNull(obj.Value4.Value4);

            // SimpleObject(string, string, int, SimpleObject(string))
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Constructor>" +
                        "<Parameter Value=\"Test value 1\" />" +
                        "<Parameter Value=\"Test value 2\" />" +
                        "<Parameter Type=\"int\" Value=\"5\" />" +
                        "<Parameter Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                            "<Constructor>" +
                                "<Parameter Value=\"Test child value 1\" />" +
                            "</Constructor>" +
                        "</Parameter>" +
                    "</Constructor>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreEqual("Test value 2", obj.Value2);
            Assert.AreEqual(5, obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreEqual("Test child value 1", obj.Value4.Value1);
            Assert.IsNull(obj.Value4.Value2);
            Assert.AreEqual(default(int), obj.Value4.Value3);
            Assert.IsNull(obj.Value4.Value4);
        }

        [Test(Description = "Test namespaces declaration.")]
        public void CheckNamespaces()
        {
            DigestContext context;
            // Use default namespace.
            var obj = Digester.DigestString("<SimpleObject Type=\"SimpleObject\" xmlns=\"Tests.Trx.Utilities.Dom2Obj, Tests.Trx\" />",
                out context) as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(context);
            Assert.IsNotNull(context.NamespaceList);
            Assert.AreEqual(1, context.NamespaceList.Count);
            Assert.IsTrue(context.NamespaceList.ContainsKey(string.Empty));
            var ns = context.NamespaceList[string.Empty];
            string expectedNs = "Tests.Trx.Utilities.Dom2Obj, Tests.Trx";
            Assert.IsTrue(ns.Length > expectedNs.Length);
            Assert.AreEqual(expectedNs, ns.Substring(0, expectedNs.Length));

            // Use declared namespace.
            obj = Digester.DigestString("<SimpleObject tests:Type=\"SimpleObject\" xmlns:tests=\"Tests.Trx.Utilities.Dom2Obj, Tests.Trx\" />",
                out context) as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(context);
            Assert.IsNotNull(context.NamespaceList);
            Assert.AreEqual(2, context.NamespaceList.Count);
            Assert.IsTrue(context.NamespaceList.ContainsKey("tests"));
            // tests namespace.
            ns = context.NamespaceList["tests"];
            Assert.IsTrue(ns.Length > expectedNs.Length);
            Assert.AreEqual(expectedNs, ns.Substring(0, expectedNs.Length));
            // Default namespace.
            Assert.IsTrue(context.NamespaceList.ContainsKey(string.Empty));
            ns = context.NamespaceList[string.Empty];
            expectedNs = Digester.DefaultNamespace;
            Assert.IsTrue(ns.Length > expectedNs.Length);
            Assert.AreEqual(expectedNs, ns.Substring(0, expectedNs.Length));
        }

        [Test(Description = "Declaration test.")]
        public void Declaration()
        {
            // Root object declaration.
            var obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\" Declare=\"rootObj\">" +
                    "<Property Name=\"Value4\" Reference=\"rootObj\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value4);
            Assert.AreSame(obj, obj.Value4);

            // Value object declaration.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Declare Name=\"declaredObj\" Value=\"Some value\" />" +
                    "<Property Name=\"Value1\" Reference=\"declaredObj\" />" +
                    "<Property Name=\"Value2\" Reference=\"declaredObj\" />" +
                    "<Property Name=\"Value4\" Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                        "<Constructor>" +
                            "<Parameter Reference=\"declaredObj\" />" +
                        "</Constructor>" +
                    "</Property>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Some value", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreSame(obj.Value1, obj.Value2);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreSame(obj.Value1, obj.Value4.Value1);

            // Object declaration.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Declare Name=\"declaredObj\" Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                        "<Constructor>" +
                            "<Parameter Value=\"Test value 1\" />" +
                        "</Constructor>" +
                    "</Declare>" +
                    "<Property Name=\"Value4\" Reference=\"declaredObj\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Value1);
            Assert.IsNotNull(obj.Value4);
            Assert.AreNotSame(obj, obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreEqual("Test value 1", obj.Value4.Value1);
        }

        [Test(Description = "Invoke test.")]
        public void Invoke()
        {
            // Invoke parameterless method.
            var obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Invoke Name=\"TurnOnInvokeFlag\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.InvokeFlag);

            // Invoke with parameter.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Invoke Name=\"SetValue1\">" +
                        "<Parameter Value=\"Set from invoke\" />" +
                    "</Invoke>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsFalse(obj.InvokeFlag);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Set from invoke", obj.Value1);

            // Child element Invoke.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Property Name=\"Value4\" Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                        "<Invoke Name=\"TurnOnInvokeFlag\" />" +
                        "<Invoke Name=\"SetValue1\">" +
                            "<Parameter Value=\"Set from invoke\" />" +
                        "</Invoke>" +
                    "</Property>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsFalse(obj.InvokeFlag);
            Assert.IsNotNull(obj.Value4);
            Assert.IsTrue(obj.Value4.InvokeFlag);
            Assert.AreEqual("Set from invoke", obj.Value4.Value1);

            // Instrumentate only.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Invoke Name=\"TurnOnInvokeFlag\" Invocation=\"instrumentate\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsFalse(obj.InvokeFlag);

            // Call on parse and instrumentate.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Invoke Name=\"TurnOnInvokeFlag\" Invocation=\"parse,instrumentate\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.InvokeFlag);
        }

        [Test(Description = "Property test.")]
        public void Property()
        {
            // Simple set value.
            var obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Property Name=\"Value1\" Value=\"Test value 1\" />" +
                    "<Property Name=\"Value2\" Value=\"Test value 2\" />" +
                    "<Property Name=\"Value3\" Type=\"int\" Value=\"5\" />" +
                    "<Property Name=\"Value4\" Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                        "<Constructor>" +
                            "<Parameter Value=\"Test child value 1\" />" +
                        "</Constructor>" +
                    "</Property>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNotNull(obj.Value2);
            Assert.AreEqual("Test value 2", obj.Value2);
            Assert.AreEqual(5, obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreEqual("Test child value 1", obj.Value4.Value1);
            Assert.IsNull(obj.Value4.Value2);
            Assert.AreEqual(default(int), obj.Value4.Value3);
            Assert.IsNull(obj.Value4.Value4);

            // Set null.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Property Name=\"Value1\" Value=\"Test value 1\" />" +
                    "<Property Name=\"Value1\" Options=\"setnull\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNull(obj.Value4);

            // Singleton.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"Tests.Trx.Utilities.Dom2Obj.SimpleObject, Tests.Trx\">" +
                    "<Property Name=\"Value4\" Options=\"singleton\" />" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNull(obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.AreSame(SimpleObject.GetInstance(), obj.Value4);

            // Using default namespace.
            obj = Digester.DigestString(
                "<SimpleObject Type=\"SimpleObject\" xmlns=\"Tests.Trx.Utilities.Dom2Obj, Tests.Trx\">" +
                    "<Property Name=\"Value1\" Value=\"Test value 1\" />" +
                    "<Property Name=\"Value4\" Type=\"SimpleObject\">" +
                        "<Constructor>" +
                            "<Parameter Value=\"Test child value 1\" />" +
                        "</Constructor>" +
                    "</Property>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreEqual("Test child value 1", obj.Value4.Value1);
            Assert.IsNull(obj.Value4.Value2);
            Assert.AreEqual(default(int), obj.Value4.Value3);
            Assert.IsNull(obj.Value4.Value4);

            // Using namespace declaration.
            obj = Digester.DigestString(
                "<SimpleObject tests1:Type=\"SimpleObject\" xmlns:tests1=\"Tests.Trx.Utilities.Dom2Obj, Tests.Trx\" "+
                    "xmlns:tests2=\"Tests.Trx.Utilities.Dom2Obj, Tests.Trx\">" +
                    "<Property Name=\"Value1\" Value=\"Test value 1\" />" +
                    "<Property Name=\"Value4\" tests2:Type=\"SimpleObject\">" +
                        "<Constructor>" +
                            "<Parameter Value=\"Test child value 1\" />" +
                        "</Constructor>" +
                    "</Property>" +
                "</SimpleObject>") as SimpleObject;
            Assert.IsNotNull(obj);
            Assert.IsNotNull(obj.Value1);
            Assert.AreEqual("Test value 1", obj.Value1);
            Assert.IsNull(obj.Value2);
            Assert.AreEqual(default(int), obj.Value3);
            Assert.IsNotNull(obj.Value4);
            Assert.IsNotNull(obj.Value4.Value1);
            Assert.AreEqual("Test child value 1", obj.Value4.Value1);
            Assert.IsNull(obj.Value4.Value2);
            Assert.AreEqual(default(int), obj.Value4.Value3);
            Assert.IsNull(obj.Value4.Value4);
        }
    }
}
