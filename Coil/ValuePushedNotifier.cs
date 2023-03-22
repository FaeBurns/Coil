using System;
using System.Diagnostics.CodeAnalysis;
using Coil.Connections;

namespace Coil
{
    [ExcludeFromCodeCoverage]
    public static class ValuePushedNotifier
    {
        public static EventHandler ValuePushed;

        internal static void NotifyValuePushed(SynchronizedValueSource changedSource)
        {
            ValuePushed?.Invoke(changedSource, EventArgs.Empty);
        }
    }
}