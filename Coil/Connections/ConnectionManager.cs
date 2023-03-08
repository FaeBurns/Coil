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
        /// <summary>
        /// A mapping of wires to all the wire's they are directly connected to.
        /// </summary>
        private readonly WireConnections _localWireConnections = new WireConnections();
        
        /// <summary>
        /// A mapping of wires to all the wires they are connected to globally
        /// </summary>
        private readonly WireConnections _globalWireConnections = new WireConnections();
        
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
            if (!_globalWireConnections.ContainsKey(wire1)) _globalWireConnections[wire1] = new HashSet<Wire>();
            if (!_globalWireConnections.ContainsKey(wire2)) _globalWireConnections[wire2] = new HashSet<Wire>();
            
            // create connection between the wires
            _globalWireConnections[wire1].Add(wire2);
            _globalWireConnections[wire2].Add(wire1);
            
            // add all of the connections the other wire has
            _globalWireConnections[wire1].UnionWith(_globalWireConnections[wire2]);
            _globalWireConnections[wire2].UnionWith(_globalWireConnections[wire1]);

            // remove connections to self
            _globalWireConnections[wire1].Remove(wire1);
            _globalWireConnections[wire2].Remove(wire2);

            // loop through all connected wires on the first wire
            // can use either here - they both contain the same values
            // this does mean that wire2 gets operated on twice - one above, one below
            // easier to read like this though
            foreach (Wire connectedWire in _globalWireConnections[wire1])
            {
                // for each of those connected wires
                // give them the same connections
                // also have to manually add the connection to the looping wire
                // as it does not contain itself in the connections
                _globalWireConnections[connectedWire].UnionWith(_globalWireConnections[wire1]);
                _globalWireConnections[connectedWire].Add(wire1);
                
                // make sure they don't contain themselves however
                _globalWireConnections[connectedWire].Remove(connectedWire);

                // set value provider token on connected wire
                connectedWire.ValueProvider = newSharedProvider;
            }
            
            // perform local connections
            
            // add wires to local connections if not already there
            if (!_localWireConnections.ContainsKey(wire1)) _localWireConnections.Add(wire1, new HashSet<Wire>());
            if (!_localWireConnections.ContainsKey(wire2)) _localWireConnections.Add(wire2, new HashSet<Wire>());
            
            // create connections
            _localWireConnections[wire1].Add(wire2);
            _localWireConnections[wire2].Add(wire1);
        }

        public void Disconnect(Wire wire1, Wire wire2)
        {
            // exit if neither of the wires have any local connections
            if ((!_localWireConnections.ContainsKey(wire1) || !_localWireConnections.ContainsKey(wire2)) 
                || (_localWireConnections[wire1].Count == 0 || _localWireConnections[wire2].Count == 0))
                return;

            // remove each other from their local connections
            _localWireConnections[wire1].Remove(wire2);
            _localWireConnections[wire2].Remove(wire1);

            HashSet<Wire> floodResult = new HashSet<Wire>();
            bool findResult = FloodFindRecursive(wire1, wire2, floodResult);
            
            // if there was a loop found, there is still a connection to the other wire and so no global edits need to be done
            if (findResult)
                return;

            HashSet<Wire> wire2FloodResult = new HashSet<Wire>();
            bool wire2FindResult = FloodFindRecursive(wire2, wire1, wire2FloodResult);
            
            // if wire2FindResult is true, but findResult is not, then there's a one-way local connection somewhere
            Debug.Assert(!wire2FindResult);
            
            foreach (Wire wire in floodResult)
            {
                // set global connections of wire to result from flood
                // create new hashset to avoid propagating unwanted changes
                _globalWireConnections[wire] = new HashSet<Wire>(floodResult);
                
                // remove self from connection
                _globalWireConnections[wire].Remove(wire);
                
                // set value provider to the one on wire1
                wire.ValueProvider = wire1.ValueProvider;
            }
            
            // give wire2 a new value provider
            wire2.ValueProvider = new SynchronizedValueSource();
            
            // do the same with wire2FloodResult
            foreach (Wire wire in wire2FloodResult)
            {
                // set global connections of wire to result from flood
                // create new hashset to avoid propagating unwanted changes
                _globalWireConnections[wire] = new HashSet<Wire>(wire2FloodResult);

                // remove self from connection
                _globalWireConnections[wire].Remove(wire);

                //set value provider to the one on value2
                wire.ValueProvider = wire2.ValueProvider;
            }
        }

        private bool FloodFindRecursive(Wire wire, Wire target, HashSet<Wire> found)
        {
            // add self to result
            found.Add(wire);
            foreach (Wire connectedWire in _localWireConnections[wire])
            {
                // ignore if already found - prevent loops
                if (found.Contains(connectedWire))
                    continue;

                // if the wire we're looking at is the target
                // we have a loop and can exit early
                if (connectedWire == target)
                    return true;

                // if not been found before, search through
                bool result = FloodFindRecursive(connectedWire, target, found);

                // only return on true value so the loop can go more than once
                if (result)
                    return true;
            }
            
            // will occur if all in _localWireConnections are in found
            return false;
        }

        public IReadOnlyCollection<Wire> GetConnections(Wire wire)
        {
            return _globalWireConnections[wire];
        }
    }
    
    /// <summary>
    /// A mapping of wires to their connected wires.
    /// </summary>
    internal class WireConnections : Dictionary<Wire, HashSet<Wire>> { }
}