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
using System.Reflection;
using System.Threading;
using Trx.Utilities;
using log4net;

namespace Trx.Messaging.FlowControl
{
    public class PeerRequest
    {
        private static readonly VolatileSequencer Sequencer = new VolatileSequencer();
        private readonly int _id = Sequencer.Increment();
        private readonly Peer _peer;

        private readonly DateTime _requestDateTime;
        private readonly Message _requestMessage;
        private bool _expired;
        private DateTime _expiredDateTime;
        private ILog _logger;
        private DateTime _responseDateTime;
        private Message _responseMessage;
        private bool _sent;
        private Timer _timer;
        private DateTime _transmissionDateTime;
        private bool _transmitted;

        #region Class constructors
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="PeerRequest"/>.
        /// </summary>
        /// <param name="peer">
        /// Es el punto por el que se procesa el requerimiento.
        /// </param>
        /// <param name="message">
        /// Es el mensaje que inicia el requerimiento.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Alguno de los parámetros es nulo.
        /// </exception>
        public PeerRequest(Peer peer, Message message)
        {
            if (peer == null)
                throw new ArgumentNullException("peer");

            if (message == null)
                throw new ArgumentNullException("message");

            if (peer.MessagesIdentifier == null)
                throw new InvalidOperationException(
                    "This Peer instance isn't configured to process requests.");

            _peer = peer;
            _requestMessage = message;
            _requestDateTime = DateTime.Now;
            _transmitted = false;
            _expired = false;
            _timer = null;
            _sent = false;
            Payload = null;
            _logger = null;
        }
        #endregion

        #region Class properties
        public DateTime TransmissionDateTime
        {
            get { return _transmissionDateTime; }
        }

        /// <summary>
        /// Informa si el requerimiento ha vencido, es decir,
        /// si no ha recibido respuesta desde el sistema remoto.
        /// </summary>
        public bool Expired
        {
            get { return _expired; }
        }

        /// <summary>
        /// Retorna la fecha y la hora en que el requerimiento
        /// expiró.
        /// </summary>
        public DateTime ExpiredDateTime
        {
            get { return _expiredDateTime; }
        }

        /// <summary>
        /// Indica si el mensaje de requerimiento ha sido enviado
        /// al sistema remoto.
        /// </summary>
        public bool Transmitted
        {
            get { return _transmitted; }
        }

        /// <summary>
        /// Retorna el mensaje de requerimiento.
        /// </summary>
        public Message RequestMessage
        {
            get { return _requestMessage; }
        }

        /// <summary>
        /// Retorna la fecha y la hora del requerimiento.
        /// </summary>
        public DateTime RequestDateTime
        {
            get { return _requestDateTime; }
        }

        /// <summary>
        /// Retorna el mensaje de respuesta.
        /// </summary>
        public Message ResponseMessage
        {
            get { return _responseMessage; }
        }

        /// <summary>
        /// Retorna la fecha y la hora de la respuesta.
        /// </summary>
        public DateTime ResponseDateTime
        {
            get { return _responseDateTime; }
        }

        /// <summary>
        /// Retorna el punto de conexión por donde
        /// se gestiona o gestionó el requerimiento.
        /// </summary>
        public Peer Peer
        {
            get { return _peer; }
        }

        /// <summary>
        /// Retorna o asigna la carga que transporta el requerimiento.
        /// </summary>
        /// <remarks>
        /// Esta propiedad no es empleada por el punto de conexión, se
        /// provee para que el usuario pueda salvar información de su
        /// interes.
        /// </remarks>
        public object Payload { get; set; }

        /// <summary>
        /// Retorna o asigna el logger empleado por la clase.
        /// </summary>
        public ILog Logger
        {
            get
            {
                return _logger ?? (_logger = LogManager.GetLogger(
                    MethodBase.GetCurrentMethod().DeclaringType));
            }

            set
            {
                if (value == null)
                    _logger = LogManager.GetLogger(
                        MethodBase.GetCurrentMethod().DeclaringType);
                else
                    _logger = value;
            }
        }

        /// <summary>
        /// Retorna o asigna el nombre del logger que se utiliza.
        /// </summary>
        public string LoggerName
        {
            set { Logger = string.IsNullOrEmpty(value) ? null : LogManager.GetLogger(value); }

            get { return Logger.Logger.Name; }
        }
        #endregion

        #region Class methods
        /// <summary>
        /// Elimina el timer de timeout.
        /// </summary>
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Marca el requerimiento como transmitido.
        /// </summary>
        internal void MarkAsTransmitted()
        {
            _transmitted = true;
            _transmissionDateTime = DateTime.Now;
        }

        /// <summary>
        /// Sets the response to the request if it's not an expired request.
        /// </summary>
        /// <param name="message">
        /// It's the response message.
        /// </param>
        /// <returns>
        /// true if the response was set, false if the response wasn't set because
        /// the request is expired.
        /// </returns>
        internal bool SetResponseMessage(Message message)
        {
            bool wasSet = false;

            if (!_expired)
                lock (this)
                {
                    if (!_expired)
                    {
                        _responseMessage = message;
                        _responseDateTime = DateTime.Now;
                        wasSet = true;
                        DisposeTimer();
                        Monitor.Pulse(this);
                    }
                }

            return wasSet;
        }

        /// <summary>
        /// Marca el requerimiento como vencido.
        /// </summary>
        public void MarkAsExpired()
        {
            if (!_expired)
                lock (this)
                {
                    if (!_expired)
                    {
                        if (_timer != null)
                        {
                            _timer.Change(Timeout.Infinite, Timeout.Infinite);
                            DisposeTimer();
                        }
                        _expired = true;
                        _expiredDateTime = DateTime.Now;
                        _peer.Cancel(this);
                        Monitor.Pulse(this);
                    }
                }
        }

        /// <summary>
        /// Espera el tiempo indicado, a que llegue el mensaje de respuesta
        /// al requerimiento, si es que aun no ha llegado.
        /// </summary>
        /// <param name="timeout">
        /// Es el tiempo máximo, expresado en milisegundos, que se espera
        /// por la respuesta.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// El tiempo máximo que se espera por la respuesta debe ser
        /// mayor que cero.
        /// </exception>
        public void WaitResponse(int timeout)
        {
            if (timeout < 1)
                throw new ArgumentOutOfRangeException(
                    "timeout", timeout, "Must be greater tha zero.");

            if (_responseMessage == null)
                lock (this)
                {
                    if (_responseMessage == null)
                        // Wait for response.
                        Monitor.Wait(this, timeout);
                }

            // Mark as expired if we don't have response yet.
            if (_responseMessage == null)
                MarkAsExpired();
        }

        /// <summary>
        /// Este método le pide al <see cref="Peer"/> que se le pasó al
        /// objeto en el momento de su creación, que envíe el mensaje
        /// al sistema remoto para iniciar el requerimiento.
        /// </summary>
        /// <remarks>
        /// Luego de enviar el mensaje, esta método devuelve el control
        /// sin efectuar otra operación.
        /// Para esperar la respuesta con un timeout, se debe invocar al
        /// método <see cref="WaitResponse"/>.
        /// </remarks>
        public void Send()
        {
            if (!_sent)
                lock (this)
                {
                    if (!_sent)
                    {
                        _peer.Send(this);
                        _sent = true;
                    }
                }
        }

        /// <summary>
        /// Este método recibe el evento del timer que controla
        /// el tiempo máximo por el que se espera respuesta desde
        /// el sistema remoto.
        /// </summary>
        /// <param name="state">
        /// Este parámetro no se utiliza.
        /// </param>
        private void OnTimerTick(object state)
        {
            if (Logger.IsDebugEnabled)
                Logger.Debug("Timer tick received in peer request.");

            if (!_expired && (_responseMessage == null))
                lock (this)
                {
                    if (!_expired && (_responseMessage == null))
                    {
                        DisposeTimer();

                        _expired = true;
                        _expiredDateTime = DateTime.Now;
                        _peer.Cancel(this);
                        Monitor.Pulse(this);
                    }
                }
        }

        /// <summary>
        /// Este método le pide al <see cref="Peer"/> que se le pasó al
        /// objeto en el momento de su creación, que envíe el mensaje
        /// al sistema remoto para iniciar el requerimiento, iniciando
        /// además un proceso de control que marca el requerimiento
        /// como expirado si la respuesta no llega antes de que transcurra
        /// el tiempo indicado por parámetro.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// Es el tiempo que se espera antes de marcar automáticamente el
        /// requerimiento como que ha expirado.
        /// </param>
        /// <remarks>
        /// Luego de enviar el mensaje, esta método devuelve el control
        /// sin efectuar otra operación, el control del tiempo se hace
        /// asincronicamente.
        /// </remarks>
        public void Send(int millisecondsTimeout)
        {
            if (millisecondsTimeout < 1)
                throw new ArgumentOutOfRangeException(
                    "millisecondsTimeout", millisecondsTimeout,
                    "Must be greater tha zero.");

            if (!_sent)
                lock (this)
                {
                    if (!_sent)
                    {
                        _peer.Send(this);
                        _sent = true;
                        _timer = new Timer(OnTimerTick,
                            null, millisecondsTimeout, Timeout.Infinite);

                        if (Logger.IsDebugEnabled)
                            Logger.Debug(string.Format(
                                "Timer tick for peer request {0} set to {1} ms.",
                                _id, millisecondsTimeout));
                    }
                }
        }

        /// <summary>
        /// Crea una cadena de caracteres que representa al requerimiento.
        /// </summary>
        /// <returns>
        /// La cadena de caracteres que representa al requerimiento.
        /// </returns>
        public override string ToString()
        {
            if (_responseMessage == null)
                return string.Format("{0} request, peer {1}",
                    (_expired ? "Expired" : "There's no response for this "), _id);

            return string.Format("{0} request for peer {1}, response received in {2}ms",
                (_expired ? "Expired" : "Successful"), _id,
                ((_responseDateTime - _requestDateTime)).TotalMilliseconds.ToString());
        }
        #endregion
    }
}