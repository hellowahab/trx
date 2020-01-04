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

namespace Trx.Server
{
    public sealed class TrxServiceState
    {
        public static string InitializingEvent = "initializing";
        public static string InitializedEvent = "initialized";
        public static string StartingEvent = "starting";
        public static string StartedEvent = "started";
        public static string StoppingEvent = "stopping";
        public static string StoppedEvent = "stopped";
        public static string DisposingEvent = "disposing";
        public static string DisposedEvent = "disposed";

        public static readonly TrxServiceState Created = new TrxServiceState("Created", null);
        public static readonly TrxServiceState Failed = new TrxServiceState("Failed", null);
        public static readonly TrxServiceState Initializing = new TrxServiceState("Initializing", InitializingEvent);
        public static readonly TrxServiceState Initialized = new TrxServiceState("Initialized", InitializedEvent);
        public static readonly TrxServiceState Starting = new TrxServiceState("Starting", StartingEvent);
        public static readonly TrxServiceState Started = new TrxServiceState("Started", StartedEvent);
        public static readonly TrxServiceState Stopping = new TrxServiceState("Stopping", StoppingEvent);
        public static readonly TrxServiceState Stopped = new TrxServiceState("Stopped", StoppedEvent);
        public static readonly TrxServiceState Destroying = new TrxServiceState("Destroying", DisposingEvent);
        public static readonly TrxServiceState Destroyed = new TrxServiceState("Destroyed", DisposedEvent);

        private readonly string _eventToFire;
        private readonly string _name;

        private TrxServiceState(string name, string eventToFire)
        {
            _name = name;
            _eventToFire = eventToFire;
        }

        public string Name
        {
            get { return _name; }
        }

        public string EventToFire
        {
            get { return _eventToFire; }
        }

        public override string ToString()
        {
            return _name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is TrxServiceState && Equals((TrxServiceState) obj);
        }

        public bool Equals(TrxServiceState other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(other._name, _name) && Equals(other._eventToFire, _eventToFire);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^
                    (_eventToFire != null ? _eventToFire.GetHashCode() : 0);
            }
        }
    }
}