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
            Assert.IsFalse(wire.Peek().Value);
        }

        [Test]
        public void ClearsValue()
        {
            Wire wire = new Wire();

            wire.Push(new BoolValue(false));
            Assert.IsFalse(wire.Peek().Value);

            wire.Push(new BoolValue(true));
            Assert.IsTrue(wire.Peek().Value);

            wire.Clear();
            Assert.IsFalse(wire.Peek().Value);
        }

        [Test]
        public void ConnectsValue_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire, wire2);

            // check that value on wire2 changes when pushed to wire1
            Assert.IsFalse(wire2.Peek().Value);
            wire.Push(new BoolValue(true));
            Assert.IsTrue(wire2.Peek().Value);
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
            Assert.IsFalse(wire3.Peek().Value);
            wire.Push(new BoolValue(true));
            Assert.IsTrue(wire3.Peek().Value);
        }

        [Test]
        public void PushPriority()
        {
            Wire wire = new Wire();

            wire.Push(new BoolValue(true));
            Assert.IsTrue(wire.Peek().Value);

            // check that value does not change to false when the value is already true
            wire.Push(new BoolValue(false));
            Assert.IsTrue(wire.Peek().Value);
        }

        [Test]
        public void Connect_Keeps_Value_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
        }

        [Test]
        public void Connect_Keeps_Value_4()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Push(new BoolValue(true));
            wire4.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsTrue(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);

            Assert.Contains(wire1, wire1.ValueProvider.PushSourceWires);
            Assert.Contains(wire4, wire1.ValueProvider.PushSourceWires);
        }

        [Test]
        public void Disconnect_Value_2()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire2.Peek().Value);

            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsFalse(wire2.Peek().Value);
        }

        [Test]
        public void Disconnect_Value_2_OppositeEnd()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            wire1.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(wire2.Peek().Value);

            connectionManager.Disconnect(wire2, wire1);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsFalse(wire2.Peek().Value);
        }

        [Test]
        public void Disconnect_Value_4()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsTrue(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);

            connectionManager.Disconnect(wire2, wire3);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsFalse(wire3.Peek().Value);
            Assert.IsFalse(wire4.Peek().Value);
        }

        [Test]
        public void Disconnect_Value_4_OppositeEnd()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsTrue(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);

            connectionManager.Disconnect(wire3, wire2);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsFalse(wire3.Peek().Value);
            Assert.IsFalse(wire4.Peek().Value);
        }

        [Test]
        public void Disconnect_Value_4_BothPushed()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Push(new BoolValue(true));
            wire4.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsTrue(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);

            connectionManager.Disconnect(wire2, wire3);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsTrue(wire2.Peek().Value);
            Assert.IsTrue(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);

            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(wire1.Peek().Value);
            Assert.IsFalse(wire2.Peek().Value);

            connectionManager.Disconnect(wire3, wire4);
            Assert.IsFalse(wire3.Peek().Value);
            Assert.IsTrue(wire4.Peek().Value);
        }

        [Test]
        public void Reset_Source()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);

            wire1.Push(new BoolValue(true));

            Assert.AreEqual(1, wire1.ValueProvider.PushSourceWires.Count);
            Assert.Contains(wire1, wire1.ValueProvider.PushSourceWires);

            wire1.ValueProvider.Reset();
            Assert.IsEmpty(wire1.ValueProvider.PushSourceWires);
            Assert.IsFalse(wire1.Peek().Value);
        }

        [Test]
        public void Merge_Source_Wires()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();
            Wire wire3 = new Wire();
            Wire wire4 = new Wire();

            wire1.Push(new BoolValue(true));
            wire4.Push(new BoolValue(true));
            connectionManager.Connect(wire1, wire2);
            connectionManager.Connect(wire2, wire3);
            connectionManager.Connect(wire3, wire4);

            Assert.Contains(wire1, wire1.ValueProvider.PushSourceWires);
            Assert.Contains(wire4, wire1.ValueProvider.PushSourceWires);
        }
    }
}