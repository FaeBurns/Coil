using System;
using System.Collections.Generic;

namespace Coil.Connections
{
    internal class SynchronizedPowerSource
    {
        private bool _previousStateChangeCheckState = false;

        /// <summary>
        /// Event fired when the wire's power state changes.
        /// </summary>
        public event EventHandler<PowerStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets a value indicating whether there is power in this source.
        /// </summary>
        public bool IsPowered => PowerSourceWires.Count > 0;

        /// <summary>
        /// Gets a list of all wires that are providing power.
        /// </summary>
        public List<Wire> PowerSourceWires { get; internal set; } = new List<Wire>();

        /// <summary>
        /// Mark that a specific wire is now providing power.
        /// </summary>
        /// <param name="pushingWire">The wire providing the power.</param>
        public void MarkPowered(Wire pushingWire)
        {
            // skip if wire is already marked powered
            if (PowerSourceWires.Contains(pushingWire))
                return;

            // mark where the power is coming from
            PowerSourceWires.Add(pushingWire);

            // notify of potential state change
            CheckStateChanged();
        }

        /// <summary>
        /// Mark that a specific wire is no longer providing power.
        /// </summary>
        /// <param name="pushingWire">The wire no longer providing power.</param>
        public void MarkUnpowered(Wire pushingWire)
        {
            PowerSourceWires.Remove(pushingWire);

            // notify of potential state change
            CheckStateChanged();
        }

        /// <summary>
        /// Resets this source.
        /// </summary>
        public void Reset()
        {
            PowerSourceWires.Clear();

            // notify of potential state change
            CheckStateChanged();
        }

        public void CheckStateChanged()
        {
            if (_previousStateChangeCheckState != IsPowered)
                NotifyStateChanged();

            _previousStateChangeCheckState = IsPowered;
        }

        public void NotifyStateChanged()
        {
            StateChanged?.Invoke(this, new PowerStateChangedEventArgs(IsPowered));
            _previousStateChangeCheckState = IsPowered;
        }
    }
}