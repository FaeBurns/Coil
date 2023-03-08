using System;
using System.Collections.Generic;

namespace Coil.Connections
{
    public class SynchronizedValueSource
    {
        public BoolValue SynchronizedValue { get; set; } = new BoolValue(false);
    }
}