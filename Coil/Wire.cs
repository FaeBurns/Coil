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
        public Wire(SynchronizedValueSource valueSource)
        {
            ValueProvider = valueSource;
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        [ExcludeFromCodeCoverage]
        public Wire()
        {
            ValueProvider = new SynchronizedValueSource();
#if DEBUG
            _wireIndex = _wireCount;
            _wireCount++;
#endif
        }

        /// <summary>
        /// Gets or Sets the value source that synchronizes the value across wires
        /// </summary>
        internal SynchronizedValueSource ValueProvider { get; set; }

        /// <summary>
        /// Pushes a new value onto the wire. The value will only be pushed if it is greater than the value currently on the wire.
        /// </summary>
        /// <param name="value">The value to push.</param>
        public void Push(BoolValue value)
        {
            // only push a true value
            if (value.Value)
            {
                ValueProvider.PushValue(this, value);
            }
        }

        /// <summary>
        /// Fetches the value on the wire.
        /// </summary>
        /// <returns>The resulting value.</returns>
        public BoolValue Peek()
        {
            return ValueProvider.SynchronizedValue;
        }

        /// <summary>
        /// Sets the wire to the default value.
        /// </summary>
        public void Clear()
        {
            ValueProvider.SynchronizedValue = new BoolValue(false);
            ValueProvider.PushSourceWires.Clear();
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