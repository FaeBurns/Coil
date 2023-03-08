using System;
using System.Collections.Generic;
using System.Linq;
using Coil.Connections;
using NUnit.Framework;

namespace Coil.Tests
{
    [TestFixture]
    public class ConnectivityTests
    {
        private void AssertContainsAll(IReadOnlyCollection<Wire> collection, params Wire[] wires)
        {
            Assert.AreEqual(wires.Length, collection.Count);
            
            foreach (Wire wire in wires)
            {
                bool contains = collection.Contains(wire);
                
                Assert.IsTrue(contains);
            }
        }
        
        [Test]
        public void Connect_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire, wire2);

            AssertContainsAll(connectionManager.GetConnections(wire), wire2);
            AssertContainsAll(connectionManager.GetConnections(wire2), wire);
        }

        [Test]
        public void Connect_3()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);

            AssertContainsAll(connectionManager.GetConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire3), wire, wire2);
        }
        
        [Test]
        public void Connect_3_NonLinear()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire, wire3);

            AssertContainsAll(connectionManager.GetConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire3), wire, wire2);
        }

        [Test]
        public void Connect_3_SameSource()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            
            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);
            
            Assert.AreSame(wire.ValueProvider, wire2.ValueProvider);
            Assert.AreSame(wire.ValueProvider, wire3.ValueProvider);
        }

        [Test]
        public void Connect_4_Merge()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            // connect wires 1 and 2/3 and 4
            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire3, wire4);

            // connect the two networks
            connectionManager.Connect(wire2, wire3);
            
            AssertContainsAll(connectionManager.GetConnections(wire), wire2, wire3, wire4);
            AssertContainsAll(connectionManager.GetConnections(wire2), wire, wire3, wire4);
            AssertContainsAll(connectionManager.GetConnections(wire3), wire, wire2, wire4);
            AssertContainsAll(connectionManager.GetConnections(wire4), wire, wire2, wire3);
        }
        
        [Test]
        public void Connect_Self()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();

            ArgumentException exception = Assert.Throws<ArgumentException>(() => connectionManager.Connect(wire, wire));
            Assert.That(exception.Message, Is.EqualTo("Cannot connect a wire to itself"));
        }

        [Test]
        public void StartConnected()
        {
            SynchronizedValueSource valueSource = new SynchronizedValueSource();
            Wire wire1 = new Wire(valueSource);
            Wire wire2 = new Wire(valueSource);
            
            Assert.AreSame(wire1.ValueProvider, wire2.ValueProvider);
        }

        [Test]
        public void Disconnect_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Disconnect(wire, wire2);

            // check that there are no connections
            AssertContainsAll(connectionManager.GetConnections(wire));
            AssertContainsAll(connectionManager.GetConnections(wire2));
        }
        
        [Test]
        public void Disconnect_2_Reverse()
        {
            ConnectionManager connectionManager = new ConnectionManager();
            
            Wire wire = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire, wire2);
            
            // disconnect in reverse order 
            connectionManager.Disconnect(wire2, wire);

            // check that there are no connections
            AssertContainsAll(connectionManager.GetConnections(wire));
            AssertContainsAll(connectionManager.GetConnections(wire2));
        }

        [Test]
        public void Disconnect_3()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);

            // disconnect wires 1 and 2
            connectionManager.Disconnect(wire, wire2);
            
            AssertContainsAll(connectionManager.GetConnections(wire));
            AssertContainsAll(connectionManager.GetConnections(wire2), wire3);
            AssertContainsAll(connectionManager.GetConnections(wire3), wire2);
        }

        [Test]
        public void Disconnect_Circular_NonBreak()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire);

            // disconnect wires 1 and 2
            connectionManager.Disconnect(wire, wire2);
            
            // everything should still be connected
            AssertContainsAll(connectionManager.GetConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetConnections(wire3), wire, wire2);
        }
        
        [Test]
        public void Disconnect_Circular_Break()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire);

            // disconnect wires 1 and 2
            connectionManager.Disconnect(wire, wire2);
            connectionManager.Disconnect(wire2, wire3);
            
            // only wires 2 and 3 should be connected
            AssertContainsAll(connectionManager.GetConnections(wire), wire3);
            AssertContainsAll(connectionManager.GetConnections(wire2));
            AssertContainsAll(connectionManager.GetConnections(wire3), wire);
        }

        [Test]
        public void Disconnect_2_SourceChanges()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            
            Assert.AreSame(wire1.ValueProvider, wire2.ValueProvider);

            connectionManager.Disconnect(wire1, wire2);
            
            Assert.AreNotSame(wire1.ValueProvider, wire2.ValueProvider);
        }

        [Test]
        public void Disconnect_4_SourceChanges()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            
            Assert.AreSame(wire1.ValueProvider, wire2.ValueProvider);
            Assert.AreSame(wire1.ValueProvider, wire3.ValueProvider);
            Assert.AreSame(wire1.ValueProvider, wire4.ValueProvider);

            connectionManager.Disconnect(wire2, wire3);
            
            Assert.AreSame(wire1.ValueProvider, wire2.ValueProvider);
            Assert.AreNotSame(wire2.ValueProvider, wire3.ValueProvider);
            Assert.AreSame(wire3.ValueProvider, wire4.ValueProvider);
        }

        [Test]
        public void Disconnect_No_Connection()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Disconnect(wire1, wire2);
        }
    }
}