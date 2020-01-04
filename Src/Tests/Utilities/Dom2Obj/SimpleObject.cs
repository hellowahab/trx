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

namespace Tests.Trx.Utilities.Dom2Obj
{
    public class SimpleObject
    {
        private static volatile SimpleObject _instance;

        public SimpleObject()
        {
        }

        public SimpleObject(string value1)
        {
            Value1 = value1;
        }

        public SimpleObject(string value1, string value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        public SimpleObject(string value1, string value2, int value3)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public SimpleObject(string value1, string value2, int value3, SimpleObject value4)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        public string Value1 { get; set; }

        public string Value2 { get; set; }

        public int Value3 { get; set; }

        public SimpleObject Value4 { get; set; }

        public bool InvokeFlag { get; set; }

        public void TurnOnInvokeFlag()
        {
            InvokeFlag = true;
        }

        public void SetValue1(string value)
        {
            Value1 = value;
        }

        public static SimpleObject GetInstance()
        {
            if (_instance == null)
                lock (typeof(SimpleObject))
                {
                    if (_instance == null)
                        _instance = new SimpleObject();
                }

            return _instance;
        }
    }
}