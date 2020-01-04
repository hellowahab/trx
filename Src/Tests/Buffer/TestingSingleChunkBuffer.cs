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
using Trx.Buffer;

namespace Tests.Trx.Buffer
{
    /// <summary>
    /// A derived <see cref="SingleChunkBuffer"/> with modifications intended to do internal tests of components
    /// using it.
    /// </summary>
    public class TestingSingleChunkBuffer : SingleChunkBuffer
    {
        public int DiscardCallCnt;
        public bool RaiseExceptionInDispose;
        public bool RaiseExceptionInDiscard;
        public bool RaiseExceptionInGetArray;
        public bool RaiseExceptionInGetDataSegments;

        public void Reset()
        {
            DiscardCallCnt = 0;
            RaiseExceptionInDispose = false;
            RaiseExceptionInDiscard = false;
            RaiseExceptionInGetArray = false;
            RaiseExceptionInGetDataSegments = false;
        }

        public override void Dispose()
        {
            if (RaiseExceptionInDispose)
                throw new ApplicationException("Raising exception as requested.");

            base.Dispose();
        }

        public override void Discard(int count)
        {
            if (RaiseExceptionInDiscard)
                throw new ApplicationException("Raising exception as requested.");

            DiscardCallCnt++;

            base.Discard(count);
        }

        public override byte[] GetArray()
        {
            if (RaiseExceptionInGetArray)
                throw new ApplicationException("Raising exception as requested."); 

            return base.GetArray();
        }

        public override IList<ArraySegment<byte>> GetDataSegments(int dataLength)
        {
            if (RaiseExceptionInGetDataSegments)
                throw new ApplicationException("Raising exception as requested.");

            return base.GetDataSegments(dataLength);
        }
    }
}
