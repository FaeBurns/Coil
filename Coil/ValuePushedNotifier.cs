using System;
using Coil.Connections;

namespace Coil
{
    public static class ValuePushedNotifier
    {
        public static EventHandler ValuePushed;

        internal static void NotifyValuePushed(SynchronizedValueSource changedSource)
        {
            ValuePushed?.Invoke(changedSource, EventArgs.Empty);
        }
    }
}