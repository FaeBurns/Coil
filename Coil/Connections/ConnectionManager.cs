using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
        private readonly LocalWireConnections _localWireConnections = new LocalWireConnections();

        /// <summary>
        /// A mapping of wires to all the wires they are connected to globally
        /// </summary>
        private readonly WireConnections _globalWireConnections = new WireConnections();

        public void Connect(Wire wire1, Wire wire2)
        {
            if (wire1 == wire2)
                throw new ArgumentException("Cannot connect a wire to itself");

            // get value source to use from wire1
            SynchronizedPowerSource newSharedProvider = wire1.PowerSource;

            // merge source wire collections
            newSharedProvider.PowerSourceWires.AddRange(wire2.PowerSource.PowerSourceWires);

            // share to wire2
            wire2.PowerSource = newSharedProvider;

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
                connectedWire.PowerSource = newSharedProvider;
            }

            // perform local connections
            AddLocalConnection(wire1, wire2);
            AddLocalConnection(wire2, wire1);

            // notify of potential state change
            newSharedProvider.NotifyStateChanged();
        }

        public void Disconnect(Wire wire1, Wire wire2)
        {
            if (wire1 == wire2)
                throw new ArgumentException("Cannot disconnect wire from itself");

            // exit if neither of the wires have any local connections
            if (!HasLocalConnections(wire1) && !HasLocalConnections(wire2))
                return;

            // remove each other from their local connections
            bool removed1 = RemoveLocalConnection(wire1, wire2);
            bool removed2 = RemoveLocalConnection(wire2, wire1);

            // if they don't share the same value, something went wrong somewhere
            Debug.Assert(removed1 == removed2);

            // don't need to check multiple - only check one
            // if no removal occured, the connection still exists
            if (!removed1)
                return;

            HashSet<Wire> floodResult = new HashSet<Wire>();
            bool findResult = FloodFindRecursive(wire1, wire2, floodResult);

            // if there was a loop found, there is still a connection to the other wire and so no global edits need to be done
            if (findResult)
                return;

            HashSet<Wire> wire2FloodResult = new HashSet<Wire>();
            bool wire2FindResult = FloodFindRecursive(wire2, wire1, wire2FloodResult);

            // if wire2FindResult is true, but findResult is not, then there's a one-way local connection somewhere
            Debug.Assert(!wire2FindResult);

            List<Wire> pushingWiresInFlood1 = new List<Wire>();
            List<Wire> pushingWiresInFlood2 = new List<Wire>();

            foreach (Wire pushingWire in wire1.PowerSource.PowerSourceWires)
            {
                if (floodResult.Contains(pushingWire))
                    pushingWiresInFlood1.Add(pushingWire);

                else if (wire2FloodResult.Contains(pushingWire))
                    pushingWiresInFlood2.Add(pushingWire);
            }

            // set the source wires on wire1 to the flood find result - removes all no longer connected
            wire1.PowerSource.PowerSourceWires = pushingWiresInFlood1;

            foreach (Wire wire in floodResult)
            {
                // set global connections of wire to result from flood
                // create new hashset to avoid propagating unwanted changes
                _globalWireConnections[wire] = new HashSet<Wire>(floodResult);

                // remove self from connection
                _globalWireConnections[wire].Remove(wire);

                // set value provider to the one on wire1
                wire.PowerSource = wire1.PowerSource;
            }

            // give wire2 a new value provider
            wire2.PowerSource = new SynchronizedPowerSource
            {
                PowerSourceWires = pushingWiresInFlood2,
            };

            // do the same with wire2FloodResult
            foreach (Wire wire in wire2FloodResult)
            {
                // set global connections of wire to result from flood
                // create new hashset to avoid propagating unwanted changes
                _globalWireConnections[wire] = new HashSet<Wire>(wire2FloodResult);

                // remove self from connection
                _globalWireConnections[wire].Remove(wire);

                //set value provider to the one on value2
                wire.PowerSource = wire2.PowerSource;
            }

            // remove wires from connection mappings if they have no connections
            if (_localWireConnections[wire1].Count == 0)
                _localWireConnections.Remove(wire1);

            if (_localWireConnections[wire2].Count == 0)
                _localWireConnections.Remove(wire2);

            if (_globalWireConnections[wire1].Count == 0)
                _globalWireConnections.Remove(wire1);

            if (_globalWireConnections[wire2].Count == 0)
                _globalWireConnections.Remove(wire2);

            // notify of potential state change
            wire1.PowerSource.NotifyStateChanged();
            wire2.PowerSource.NotifyStateChanged();
        }

        private bool FloodFindRecursive(Wire wire, Wire target, HashSet<Wire> found)
        {
            // add self to result
            found.Add(wire);
            foreach (Wire connectedWire in _localWireConnections[wire].Keys)
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

        public IReadOnlyCollection<Wire> GetGlobalConnections(Wire wire)
        {
            if (!HasGlobalConnections(wire))
                return Array.Empty<Wire>();

            return _globalWireConnections[wire];
        }

        public IReadOnlyCollection<Wire> GetLocalConnections(Wire wire)
        {
            if (!HasLocalConnections(wire))
                return Array.Empty<Wire>();

            return _localWireConnections[wire].Keys.ToArray();
        }

        public bool HasLocalConnections(Wire wire)
        {
            if (_localWireConnections.TryGetValue(wire, out LocalMapping value))
                return value.Count > 0;
            return false;
        }

        public bool HasGlobalConnections(Wire wire)
        {
            if (_globalWireConnections.TryGetValue(wire, out HashSet<Wire> value))
                return value.Count > 0;
            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool HasLocalConnectionTo(Wire wire1, Wire wire2)
        {
            if (_localWireConnections.TryGetValue(wire1, out LocalMapping value))
                return value.ContainsKey(wire2);
            return false;
        }

        [ExcludeFromCodeCoverage]
        public bool HasGlobalConnectionTo(Wire wire1, Wire wire2)
        {
            if (_globalWireConnections.TryGetValue(wire1, out HashSet<Wire> value))
                return value.Contains(wire2);
            return false;
        }

        /// <summary>
        /// Creates a one-way local connection from wire <paramref name="from"/> to wire <paramref name="to"/>.
        /// </summary>
        /// <param name="from">The first connecting wire</param>
        /// <param name="to">The second connecting wire</param>
        private void AddLocalConnection(Wire from, Wire to)
        {
            // add wires to local connections if not already there
            if (!_localWireConnections.ContainsKey(from)) _localWireConnections.Add(from, new LocalMapping());

            // either create entry for the to wire
            // or increment entry
            if (!_localWireConnections[from].ContainsKey(to))
            {
                _localWireConnections[from].Add(to, 1);
            }
            else
            {
                _localWireConnections[from][to]++;
            }
        }

        /// <summary>
        /// Removes the local connection between two wires.
        /// <param name="from">The wire that contains the connection.</param>
        /// <param name="to">The wire being removed.</param>
        /// </summary>
        /// <returns><see langword="true"/> if there are no more connections left to that wire; otherwise <see langword="false"/>.</returns>
        private bool RemoveLocalConnection(Wire from, Wire to)
        {
            // decrement connection count
            _localWireConnections[from][to]--;

            // if there is at least one connection, return false
            if (_localWireConnections[from][to] > 0)
                return false;

            // otherwise remove the local connection and return true
            _localWireConnections[from].Remove(to);
            return true;
        }
    }

    /// <summary>
    /// A mapping of wires to their connected wires.
    /// </summary>
    internal class WireConnections : Dictionary<Wire, HashSet<Wire>> { }

    /// <summary>
    /// A mapping of wires to their connected wires, including a count for those connections
    /// </summary>
    internal class LocalWireConnections : Dictionary<Wire, LocalMapping> { }

    /// <summary>
    /// A container class that holds a set of connections and a counting of how many connections they hold
    /// </summary>
    internal class LocalMapping : Dictionary<Wire, int> { }
}