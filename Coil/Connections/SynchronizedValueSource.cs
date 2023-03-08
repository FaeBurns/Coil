using System;
using System.Collections.Generic;

namespace Coil.Connections
{
    public class SynchronizedValueSource : IDisposable
    {
        private bool _disposed;

        public BoolValue SynchronizedValue { get; set; } = new BoolValue(false);

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
        }
    }
}