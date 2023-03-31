using System;
using System.Diagnostics.CodeAnalysis;
using Coil.Connections;

namespace Coil
{
    [ExcludeFromCodeCoverage]
    public static class ValuePushedNotifier
    {
        public static EventHandler ValuePushed;

        internal static void NotifyValuePushed(SynchronizedPowerSource changedSource)
        {
            ValuePushed?.Invoke(changedSource, EventArgs.Empty);
        }
    }
}