using System;
using System.Collections.Generic;

namespace Coil.Connections
{
    public class SynchronizedValueSource
    {
        private BoolValue _synchronizedValue = new BoolValue(false);

        public BoolValue SynchronizedValue
        {
            get => _synchronizedValue;
            set
            {
                ValuePushedNotifier.NotifyValuePushed(this);
                _synchronizedValue = value;
            }
        }
    }
}