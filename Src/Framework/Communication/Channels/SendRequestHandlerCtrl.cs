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

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Send channel control handler of a request with access to the underling request
    /// descriptor intended to give the calling thread the option to wait synchronously
    /// on it. See <see ref="Request.WaitResponse"/> method.
    /// </summary>
    public class SendRequestHandlerCtrl : ChannelRequestCtrl
    {
        private readonly Request _request;

        #region Constructors
        internal SendRequestHandlerCtrl(Request request)
        {
            _request = request;
        }

        internal SendRequestHandlerCtrl(bool successful, Request request)
            : base(successful)
        {
            _request = request;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the channel request.
        /// </summary>
        public Request Request
        {
            get
            {
                return _request;
            }
        }
        #endregion
    }
}
