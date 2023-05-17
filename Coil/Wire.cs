using System;
using System.Diagnostics.CodeAnalysis;
using Coil.Connections;

namespace Coil
{
    /// <summary>
    /// <para>A Wire can be connected to other wires via the <see cref="ConnectionManager"/>.</para>
    /// <para>Values pushed onto a wire will be found on all other wires - highest value wins.</para>
    /// </summary>
    public class Wire
    {
        private SynchronizedPowerSource _powerSource;

#if DEBUG
        private readonly int _wireIndex;
        private static int _wireCount;
#endif

        /// <summary>
        /// Event fired when the wire's power or connection state changes.
        /// </summary>
        public event EventHandler<PowerStateChangedEventArgs> StateChanged;

        [ExcludeFromCodeCoverage]
        internal Wire(SynchronizedPowerSource powerSource)
        {
            PowerSource = powerSource;
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        [ExcludeFromCodeCoverage]
        public Wire()
        {
            PowerSource = new SynchronizedPowerSource();
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        /// <summary>
        /// Gets or Sets the value source that synchronizes the value across wires
        /// </summary>
        internal SynchronizedPowerSource PowerSource
        {
            get => _powerSource;
            set
            {
                // remove listener from old power source and add it to the new one
                // old one won't exist if setting for the first time
                if (_powerSource != null)
                    _powerSource.StateChanged -= StateChangedPassthrough;
                _powerSource = value;
                _powerSource.StateChanged += StateChangedPassthrough;
            }
        }

        /// <summary>
        /// Powers this wire.
        /// </summary>
        public void Power()
        {
            PowerSource.MarkPowered(this);
        }

        /// <summary>
        /// UnPowers this wire.
        /// </summary>
        public void UnPower()
        {
            PowerSource.MarkUnpowered(this);
        }

        /// <summary>
        /// Fetches the value on the wire.
        /// </summary>
        /// <returns>The resulting value.</returns>
        public bool Peek()
        {
            return PowerSource.IsPowered;
        }

        /// <summary>
        /// Sets the wire to the default value.
        /// </summary>
        public void Clear()
        {
            PowerSource.Reset();
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return base.ToString() + (_wireIndex + 1);
        }
#endif
        private void StateChangedPassthrough(object _, PowerStateChangedEventArgs e)
        {
            Console.WriteLine("State changed event received");
            StateChanged?.Invoke(this, e);
        }
    }
}