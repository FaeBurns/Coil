﻿using System;
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

            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire2);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire);
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

            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire, wire2);
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

            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire, wire2);
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

            Assert.AreSame(wire.PowerSource, wire2.PowerSource);
            Assert.AreSame(wire.PowerSource, wire3.PowerSource);
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

            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire2, wire3, wire4);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire, wire3, wire4);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire, wire2, wire4);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire4), wire, wire2, wire3);
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
            SynchronizedPowerSource powerSource = new SynchronizedPowerSource();
            Wire wire1 = new Wire(powerSource);
            Wire wire2 = new Wire(powerSource);

            Assert.AreSame(wire1.PowerSource, wire2.PowerSource);
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
            AssertContainsAll(connectionManager.GetGlobalConnections(wire));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2));
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
            AssertContainsAll(connectionManager.GetGlobalConnections(wire));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2));
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

            AssertContainsAll(connectionManager.GetGlobalConnections(wire));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire2);
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
            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire2, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire, wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire, wire2);
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
            AssertContainsAll(connectionManager.GetGlobalConnections(wire), wire3);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3), wire);
        }

        [Test]
        public void Disconnect_2_SourceChanges()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);

            Assert.AreSame(wire1.PowerSource, wire2.PowerSource);

            connectionManager.Disconnect(wire1, wire2);

            Assert.AreNotSame(wire1.PowerSource, wire2.PowerSource);
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

            Assert.AreSame(wire1.PowerSource, wire2.PowerSource);
            Assert.AreSame(wire1.PowerSource, wire3.PowerSource);
            Assert.AreSame(wire1.PowerSource, wire4.PowerSource);

            connectionManager.Disconnect(wire2, wire3);

            Assert.AreSame(wire1.PowerSource, wire2.PowerSource);
            Assert.AreNotSame(wire2.PowerSource, wire3.PowerSource);
            Assert.AreSame(wire3.PowerSource, wire4.PowerSource);
        }

        [Test]
        public void Disconnect_No_Connection()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Disconnect(wire1, wire2);
        }

        [Test]
        public void Connect_5_Local()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();
            Wire wire5 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire1, wire3);

            connectionManager.Connect(wire3, wire5);
            connectionManager.Connect(wire5, wire4);

            AssertContainsAll(connectionManager.GetLocalConnections(wire1), wire2, wire3);
            AssertContainsAll(connectionManager.GetLocalConnections(wire2), wire1);
            AssertContainsAll(connectionManager.GetLocalConnections(wire3), wire1, wire5);
            AssertContainsAll(connectionManager.GetLocalConnections(wire4), wire5);
            AssertContainsAll(connectionManager.GetLocalConnections(wire5), wire3, wire4);
        }

        [Test]
        public void Disconnect_5_Local()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();
            Wire wire5 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire1, wire3);

            connectionManager.Connect(wire3, wire5);
            connectionManager.Connect(wire5, wire4);

            connectionManager.Disconnect(wire1, wire2);
            connectionManager.Disconnect(wire1, wire3);

            connectionManager.Disconnect(wire3, wire5);
            connectionManager.Disconnect(wire5, wire4);

            AssertContainsAll(connectionManager.GetLocalConnections(wire1));
            AssertContainsAll(connectionManager.GetLocalConnections(wire2));
            AssertContainsAll(connectionManager.GetLocalConnections(wire3));
            AssertContainsAll(connectionManager.GetLocalConnections(wire4));
            AssertContainsAll(connectionManager.GetLocalConnections(wire5));

            AssertContainsAll(connectionManager.GetGlobalConnections(wire1));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire3));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire4));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire5));
        }

        [Test]
        public void DualConnection_Connect()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire1, wire2);

            AssertContainsAll(connectionManager.GetGlobalConnections(wire1), wire2);
            AssertContainsAll(connectionManager.GetLocalConnections(wire1), wire2);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire1);
            AssertContainsAll(connectionManager.GetLocalConnections(wire2), wire1);
        }

        [Test]
        public void DualConnection_Disconnect_Single()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire1, wire2);

            connectionManager.Disconnect(wire1, wire2);

            AssertContainsAll(connectionManager.GetGlobalConnections(wire1), wire2);
            AssertContainsAll(connectionManager.GetLocalConnections(wire1), wire2);
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2), wire1);
            AssertContainsAll(connectionManager.GetLocalConnections(wire2), wire1);
        }

        [Test]
        public void DualConnection_Disconnect_All()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire1, wire2);

            connectionManager.Disconnect(wire1, wire2);
            connectionManager.Disconnect(wire1, wire2);

            AssertContainsAll(connectionManager.GetGlobalConnections(wire1));
            AssertContainsAll(connectionManager.GetLocalConnections(wire1));
            AssertContainsAll(connectionManager.GetGlobalConnections(wire2));
            AssertContainsAll(connectionManager.GetLocalConnections(wire2));
        }

        [Test]
        public void Disconnect_Self()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();

            Assert.Throws<ArgumentException>(() => connectionManager.Disconnect(wire1, wire1), "Cannot disconnect wire from itself");
        }
    }
}