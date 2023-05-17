namespace Coil
{
    /// <summary>
    /// Event args for a <see cref="Wire.StateChanged"/> event.
    /// </summary>
    public class PowerStateChangedEventArgs
    {
        /// <summary>
        /// Gets the new value on the wire.
        /// </summary>
        public bool State { get; }

        /// <summary>
        /// Gets the old value on the wire.
        /// </summary>
        public bool OldState => !State;

        public PowerStateChangedEventArgs(bool state)
        {
            State = state;
        }
    }
}