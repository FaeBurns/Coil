using Coil.Connections;
using NUnit.Framework;

namespace Coil.Tests
{
    [TestFixture]
    public class NotificationTests
    {
        [Test]
        public void NotificationFires()
        {
            Wire wire = new Wire();
            EventListener eventListener = new EventListener();
            eventListener.ListenTo(wire);

            Assert.IsFalse(eventListener.HasReceivedEvent);
            wire.Power();
            Assert.IsTrue(eventListener.HasReceivedEvent);
        }

        [Test]
        public void NotificationFiresOnConnect()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            EventListener eventListener = new EventListener();
            eventListener.ListenTo(wire2);

            Assert.IsFalse(eventListener.HasReceivedEvent);
            wire1.Power();
            Assert.IsFalse(eventListener.HasReceivedEvent);

            Assert.IsFalse(eventListener.HasReceivedEvent);
            connectionManager.Connect(wire1, wire2);
            Assert.IsTrue(eventListener.HasReceivedEvent);
            Assert.IsTrue(eventListener.LastReceivedState);
        }

        [Test]
        public void NotificationFiresOnDisconnect_Wire1State()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            wire1.Power();

            EventListener eventListener = new EventListener();
            eventListener.ListenTo(wire1);

            Assert.IsFalse(eventListener.HasReceivedEvent);
            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(eventListener.HasReceivedEvent);
            Assert.IsTrue(eventListener.LastReceivedState);
        }

        [Test]
        public void NotificationFiresOnDisconnect_Wire2State()
        {
            ConnectionManager connectionManager = new ConnectionManager();

            Wire wire1 = new Wire();
            Wire wire2 = new Wire();

            connectionManager.Connect(wire1, wire2);
            wire1.Power();

            EventListener eventListener = new EventListener();
            eventListener.ListenTo(wire2);

            Assert.IsFalse(eventListener.HasReceivedEvent);
            connectionManager.Disconnect(wire1, wire2);
            Assert.IsTrue(eventListener.HasReceivedEvent);
            Assert.IsFalse(eventListener.LastReceivedState);
        }
    }

    public class EventListener
    {
        public int TimesReceivedEvent { get; set; }
        public bool HasReceivedEvent => TimesReceivedEvent > 0;
        public bool LastReceivedState { get; private set; }

        public void ListenTo(Wire wire)
        {
            wire.StateChanged += OnWireStateChanged;
        }

        private void OnWireStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            TimesReceivedEvent++;
            LastReceivedState = e.State;
        }
    }
}