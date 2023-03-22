using System;
using System.Collections.Generic;

namespace Coil.Connections
{
    public class SynchronizedValueSource
    {
        public SynchronizedValueSource()
        {
            _synchronizedValue = new BoolValue(false);
        }

        public SynchronizedValueSource(BoolValue initialValue)
        {
            _synchronizedValue = initialValue;
        }

        private BoolValue _synchronizedValue;

        public List<Wire> PushSourceWires { get; internal set; } = new List<Wire>();

        public BoolValue SynchronizedValue
        {
            get => _synchronizedValue;
            internal set
            {
                ValuePushedNotifier.NotifyValuePushed(this);
                _synchronizedValue = value;
            }
        }

        public void PushValue(Wire pushingWire, BoolValue value)
        {
            PushSourceWires.Add(pushingWire);
            SynchronizedValue = value;
        }

        public void Reset()
        {
            PushSourceWires.Clear();
            SynchronizedValue = new BoolValue(false);
        }
    }
}