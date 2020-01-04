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
using NUnit.Framework;
using Trx.Utilities;

namespace Tests.Trx.Utilities
{
    /// <summary>
    /// Test fixture for SpacePaddingRight.
    /// </summary>
    [TestFixture(Description = "Space padding right tests.")]
    public class SpacePaddingRightTest
    {
        #region Setup/Teardown
        /// <summary>
        /// This method will be called by NUnit for test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _paddingWithTruncate = SpacePaddingRight.GetInstance(true);
            Assert.IsNotNull(_paddingWithTruncate);

            _paddingWithoutTruncate = SpacePaddingRight.GetInstance(false);
            Assert.IsNotNull(_paddingWithoutTruncate);
        }
        #endregion

        private SpacePaddingRight _paddingWithTruncate;
        private SpacePaddingRight _paddingWithoutTruncate;

        /// <summary>
        /// Test GetInstance method.
        /// </summary>
        [Test(Description = "Test GetInstance method.")]
        public void GetInstance()
        {
            Assert.IsTrue(_paddingWithTruncate == SpacePaddingRight.GetInstance(true));
            Assert.IsTrue(_paddingWithoutTruncate == SpacePaddingRight.GetInstance(false));
        }

        /// <summary>
        /// Test Pad method.
        /// </summary>
        [Test(Description = "Test Pad method.")]
        public void Pad()
        {
            string data = "Test data";
            string paddedData = "Test data           ";
            string result;

            // Test padding with truncate.
            result = _paddingWithTruncate.Pad(data, 20);
            Assert.IsTrue(result.Length == 20);
            Assert.IsTrue(paddedData.Equals(result));

            // Test padding with truncate.
            result = _paddingWithTruncate.Pad(data, data.Length);
            Assert.IsTrue(result.Length == data.Length);
            Assert.IsTrue(data.Equals(result));

            // Test padding with truncate.
            result = _paddingWithTruncate.Pad(null, 5);
            Assert.IsTrue(result.Length == 5);
            Assert.IsTrue(result.Equals("     "));

            // Test padding with truncate.
            result = _paddingWithTruncate.Pad(string.Empty, 3);
            Assert.IsTrue(result.Length == 3);
            Assert.IsTrue(result.Equals("   "));

            // Test truncate.
            result = _paddingWithTruncate.Pad(data, 4);
            Assert.IsTrue(result.Length == 4);
            Assert.IsTrue(data.Substring(0, 4).Equals(result));

            // Test width.
            try
            {
                result = _paddingWithTruncate.Pad(data, 0);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.ParamName == "totalWidth");
            }

            // Test padding without truncate.
            result = _paddingWithoutTruncate.Pad(data, 20);
            Assert.IsTrue(result.Length == 20);
            Assert.IsTrue(paddedData.Equals(result));

            // Test padding without truncate.
            result = _paddingWithoutTruncate.Pad(data, data.Length);
            Assert.IsTrue(result.Length == data.Length);
            Assert.IsTrue(data.Equals(result));

            // Test padding without truncate.
            result = _paddingWithoutTruncate.Pad(null, 5);
            Assert.IsTrue(result.Length == 5);
            Assert.IsTrue(result.Equals("     "));

            // Test padding without truncate.
            result = _paddingWithoutTruncate.Pad(string.Empty, 3);
            Assert.IsTrue(result.Length == 3);
            Assert.IsTrue(result.Equals("   "));

            // Test truncate (must fail).
            try
            {
                result = _paddingWithoutTruncate.Pad(data, 4);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.ParamName == "data");
            }

            // Test width.
            try
            {
                result = _paddingWithoutTruncate.Pad(data, 0);
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.ParamName == "totalWidth");
            }
        }

        /// <summary>
        /// Test RemovePad method.
        /// </summary>
        [Test(Description = "Test RemovePad method.")]
        public void RemovePad()
        {
            string data = "My test data";
            string paddedData = "My test data        ";
            string result;

            result = _paddingWithTruncate.RemovePad(paddedData);
            Assert.IsTrue(data.Equals(result));

            result = _paddingWithTruncate.RemovePad(data);
            Assert.IsTrue(data.Equals(result));

            result = _paddingWithTruncate.RemovePad(null);
            Assert.IsNull(result);

            result = _paddingWithTruncate.RemovePad(string.Empty);
            Assert.IsTrue(string.Empty.Equals(result));

            result = _paddingWithoutTruncate.RemovePad(paddedData);
            Assert.IsTrue(data.Equals(result));

            result = _paddingWithoutTruncate.RemovePad(data);
            Assert.IsTrue(data.Equals(result));

            result = _paddingWithoutTruncate.RemovePad(null);
            Assert.IsNull(result);

            result = _paddingWithoutTruncate.RemovePad(string.Empty);
            Assert.IsTrue(string.Empty.Equals(result));
        }

        /// <summary>
        /// Test Truncate property.
        /// </summary>
        [Test(Description = "Test Truncate property.")]
        public void Truncate()
        {
            Assert.IsTrue(_paddingWithTruncate.Truncate);
            Assert.IsTrue(_paddingWithoutTruncate.Truncate == false);
        }
    }
}