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

#if DEBUG
        private readonly int _wireIndex;
        private static int _wireCount;
#endif

        [ExcludeFromCodeCoverage]
        public Wire(SynchronizedPowerSource powerSource)
        {
            PowerProvider = powerSource;
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        [ExcludeFromCodeCoverage]
        public Wire()
        {
            PowerProvider = new SynchronizedPowerSource();
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        /// <summary>
        /// Gets or Sets the value source that synchronizes the value across wires
        /// </summary>
        internal SynchronizedPowerSource PowerProvider { get; set; }

        /// <summary>
        /// Powers this wire.
        /// </summary>
        public void Power()
        {
            PowerProvider.MarkPowered(this);
        }

        /// <summary>
        /// UnPowers this wire.
        /// </summary>
        public void UnPower()
        {
            PowerProvider.MarkUnpowered(this);
        }

        /// <summary>
        /// Fetches the value on the wire.
        /// </summary>
        /// <returns>The resulting value.</returns>
        public bool Peek()
        {
            return PowerProvider.IsPowered;
        }

        /// <summary>
        /// Sets the wire to the default value.
        /// </summary>
        public void Clear()
        {
            PowerProvider.Reset();
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return base.ToString() + (_wireIndex + 1);
        }
#endif
    }
}