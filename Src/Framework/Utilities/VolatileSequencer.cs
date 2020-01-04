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

namespace Trx.Utilities
{
    /// <summary>
    /// Implements a numerical sequencer.
    /// </summary>
    /// <remarks>
    /// The minimum and maximum default values are defined by
    /// <see cref="VolatileSequencerMinimumValue"/> and <see cref="int.MaxValue"/> respectively.
    /// </remarks>
    [Serializable]
    public class VolatileSequencer : ISequencer
    {
        /// <summary>
        /// The minimum default value for the sequencer.
        /// </summary>
        public const int VolatileSequencerMinimumValue = 1;

        private readonly int _maximumValue = Int32.MaxValue;
        private readonly int _minimumValue = VolatileSequencerMinimumValue;
        private int _traceSeq;

        /// <summary>
        /// Initializes a new instance of the class <see cref="VolatileSequencer"/>.
        /// </summary>
        public VolatileSequencer()
        {
            _traceSeq = _minimumValue;
        }

        /// <summary>
        /// Initializes a new instance of the class <see cref="VolatileSequencer"/>.
        /// </summary>
        /// <param name="minimumValue">
        /// The minimum value of the sequencer.
        /// </param>
        public VolatileSequencer(int minimumValue)
        {
            _traceSeq = _minimumValue = minimumValue;
        }

        /// <summary>
        /// Initializes a new instance of the class <see cref="VolatileSequencer"/>.
        /// </summary>
        /// <param name="minimumValue">
        /// The minimum value of the sequencer.
        /// </param>
        /// <param name="maximumValue">
        /// The maximum value of the sequencer.
        /// </param>
        public VolatileSequencer(int minimumValue, int maximumValue) :
            this(minimumValue)
        {
            if (maximumValue <= minimumValue)
                throw new ArgumentOutOfRangeException("maximumValue", maximumValue,
                    "Must be greater than minimumValue.");

            _maximumValue = maximumValue;
        }

        #region ISequencer Members
        /// <summary>
        /// It's the value of the sequencer.
        /// </summary>
        /// <returns>
        /// The actual value of the sequencer.
        /// </returns>
        public int CurrentValue()
        {
            lock (this)
            {
                return _traceSeq;
            }
        }

        /// <summary>
        /// It increases in one the present value of the sequencer.
        /// </summary>
        /// <returns>
        /// It returns the value of the sequencer before being increased.
        /// </returns>
        /// <remarks>
        /// If the value increased of the sequencer surpasses the maximum value
        /// permitted by <see cref="Maximum"/>, <see cref="Minimum"/>  it is assigned
        /// to present value.  
        /// </remarks>
        public int Increment()
        {
            int valueToReturn;

            lock (this)
            {
                valueToReturn = _traceSeq;

                _traceSeq++;
                if (_traceSeq > _maximumValue)
                    _traceSeq = _minimumValue;
            }

            return valueToReturn;
        }

        /// <summary>
        /// Is the minimum value that can be worth the sequencer.
        /// </summary>
        /// <returns>
        /// The minimum value that can be worth the sequencer.
        /// </returns>
        public int Maximum()
        {
            return _maximumValue;
        }

        /// <summary>
        /// Is the maximum value that can be worth the sequencer.
        /// </summary>
        /// <returns>
        /// The maximum value that can be worth the sequencer.
        /// </returns>
        public int Minimum()
        {
            return _minimumValue;
        }
        #endregion
    }
}