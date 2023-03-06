using System.Runtime.Remoting.Messaging;
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
        public void StoresValue()
        {
            Wire wire = new Wire();

            wire.Push(new BoolValue(false));
            Assert.IsFalse(wire.Peek().Value);
            
            wire.Push(new BoolValue(true));
            Assert.IsTrue(wire.Peek().Value);
            
            wire.Push(new BoolValue(false));
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
    }
}