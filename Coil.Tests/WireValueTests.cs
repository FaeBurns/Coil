using Coil.Connections;
using NUnit.Framework;

namespace Coil.Tests
{
    [TestFixture]
    public class WireValueTests
    {
        [Test]
        public void InitialValue()
        {
            Wire wire = new Wire();
            Assert.IsFalse(wire.Peek());
        }

        [Test]
        public void ClearsValue()
        {
            Wire wire = new Wire();

            wire.UnPower();
            Assert.IsFalse(wire.Peek());

            wire.Power();
            Assert.IsTrue(wire.Peek());

            wire.Clear();
            Assert.IsFalse(wire.Peek());
        }

        [Test]
        public void ConnectsValue_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire, wire2);

            // check that value on wire2 changes when pushed to wire1
            Assert.IsFalse(wire2.Peek());
            wire.Power();
            Assert.IsTrue(wire2.Peek());
        }

        [Test]
        public void ConnectsValue_3()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();

            connectionManager.Connect(wire, wire2);
            connectionManager.Connect(wire2, wire3);

            // check that value on wire3 changes when pushed to wire1
            Assert.IsFalse(wire3.Peek());
            wire.Power();
            Assert.IsTrue(wire3.Peek());
        }

        [Test]
        public void PushPriority()
        {
            Wire wire = new Wire();

            wire.Power();
            Assert.IsTrue(wire.Peek());

            // check that value does not change to false when the value is already true
            wire.Power();
            Assert.IsTrue(wire.Peek());
        }

        [Test]
        public void Connect_Keeps_Value_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Power();
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
        }

        [Test]
        public void Connect_Keeps_Value_4()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Power();
            wire4.Power();
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsTrue(wire3.Peek());
            Assert.IsTrue(wire4.Peek());

            Assert.Contains(wire1, wire1.PowerSource.PowerSourceWires);
            Assert.Contains(wire4, wire1.PowerSource.PowerSourceWires);
        }

        [Test]
        public void Disconnect_Value_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Power();
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire2.Peek());

            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(wire1.Peek());
            Assert.IsFalse(wire2.Peek());
        }

        [Test]
        public void Disconnect_Value_2_OppositeEnd()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Power();
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire2.Peek());

            connectionManager.Disconnect(wire2, wire1);
            Assert.IsTrue(wire1.Peek());
            Assert.IsFalse(wire2.Peek());
        }

        [Test]
        public void Disconnect_Value_4()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Power();
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsTrue(wire3.Peek());
            Assert.IsTrue(wire4.Peek());

            connectionManager.Disconnect(wire2, wire3);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsFalse(wire3.Peek());
            Assert.IsFalse(wire4.Peek());
        }

        [Test]
        public void Disconnect_Value_4_OppositeEnd()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Power();
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsTrue(wire3.Peek());
            Assert.IsTrue(wire4.Peek());

            connectionManager.Disconnect(wire3, wire2);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsFalse(wire3.Peek());
            Assert.IsFalse(wire4.Peek());
        }

        [Test]
        public void Disconnect_Value_4_BothPushed()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Power();
            wire4.Power();
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsTrue(wire3.Peek());
            Assert.IsTrue(wire4.Peek());

            connectionManager.Disconnect(wire2, wire3);
            Assert.IsTrue(wire1.Peek());
            Assert.IsTrue(wire2.Peek());
            Assert.IsTrue(wire3.Peek());
            Assert.IsTrue(wire4.Peek());

            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(wire1.Peek());
            Assert.IsFalse(wire2.Peek());

            connectionManager.Disconnect(wire3, wire4);
            Assert.IsFalse(wire3.Peek());
            Assert.IsTrue(wire4.Peek());
        }

        [Test]
        public void Reset_Source()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);

            wire1.Power();

            Assert.AreEqual(1, wire1.PowerSource.PowerSourceWires.Count);
            Assert.Contains(wire1, wire1.PowerSource.PowerSourceWires);

            wire1.PowerSource.Reset();
            Assert.IsEmpty(wire1.PowerSource.PowerSourceWires);
            Assert.IsFalse(wire1.Peek());
        }

        [Test]
        public void Merge_Source_Wires()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Power();
            wire4.Power();
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);

            Assert.Contains(wire1, wire1.PowerSource.PowerSourceWires);
            Assert.Contains(wire4, wire1.PowerSource.PowerSourceWires);
        }
    }
}