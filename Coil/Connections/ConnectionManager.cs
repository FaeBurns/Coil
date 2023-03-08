using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Coil.Connections
{
    /// <summary>
    /// Manages the connections between the provided wires
    /// </summary>
    public class ConnectionManager
    {
        private readonly WireConnections _wireConnections = new WireConnections();
        
        public void Connect(Wire wire1, Wire wire2)
        {
            if (wire1 == wire2)
                throw new ArgumentException("Cannot connect a wire to itself");

            // get value source to use from wire1
            SynchronizedValueSource newSharedProvider = wire1.ValueProvider;
            
            // share to wire2
            wire2.ValueProvider = newSharedProvider;
            
            // if the wires do not have any recorded connections
            // add them to the dictionary
            if (!_wireConnections.ContainsKey(wire1)) _wireConnections[wire1] = new HashSet<Wire>();
            if (!_wireConnections.ContainsKey(wire2)) _wireConnections[wire2] = new HashSet<Wire>();
            
            // create connection between the wires
            _wireConnections[wire1].Add(wire2);
            _wireConnections[wire2].Add(wire1);
            
            // add all of the connections the other wire has
            _wireConnections[wire1].UnionWith(_wireConnections[wire2]);
            _wireConnections[wire2].UnionWith(_wireConnections[wire1]);

            // remove connections to self
            _wireConnections[wire1].Remove(wire1);
            _wireConnections[wire2].Remove(wire2);

            // loop through all connected wires on the first wire
            // can use either here - they both contain the same values
            // this does mean that wire2 gets operated on twice - one above, one below
            // easier to read like this though
            foreach (Wire connectedWire in _wireConnections[wire1])
            {
                // for each of those connected wires
                // give them the same connections
                // also have to manually add the connection to the looping wire
                // as it does not contain itself in the connections
                _wireConnections[connectedWire].UnionWith(_wireConnections[wire1]);
                _wireConnections[connectedWire].Add(wire1);
                
                // make sure they don't contain themselves however
                _wireConnections[connectedWire].Remove(connectedWire);

                // set value provider token on connected wire
                connectedWire.ValueProvider = newSharedProvider;
            }

            newSharedProvider.SynchronizedValue = new BoolValue(false);
        }

        public void Disconnect(Wire wire1, Wire wire2)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Wire> GetConnections(Wire wire)
        {
            return _wireConnections[wire];
        }
    }
    
    /// <summary>
    /// A mapping of wires to their connected wires.
    /// </summary>
    internal class WireConnections : Dictionary<Wire, HashSet<Wire>> { }
}