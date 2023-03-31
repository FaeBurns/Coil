using System.Collections.Generic;

namespace Coil.Connections
{
    public class SynchronizedPowerSource
    {
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
            
            // notify of new value
            ValuePushedNotifier.NotifyValuePushed(this);
        }
        
        /// <summary>
        /// Mark that a specific wire is no longer providing power.
        /// </summary>
        /// <param name="pushingWire">The wire no longer providing power.</param>
        public void MarkUnpowered(Wire pushingWire)
        {
            PowerSourceWires.Remove(pushingWire);
        }

        /// <summary>
        /// Resets this source.
        /// </summary>
        public void Reset()
        {
            PowerSourceWires.Clear();
        }
    }
}