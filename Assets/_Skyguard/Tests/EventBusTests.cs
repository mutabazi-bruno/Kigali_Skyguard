using NUnit.Framework;
using Skyguard.Core;

namespace Skyguard.Tests
{
    public class EventBusTests
    {
        struct Ping { public int Value; }

        int received;

        void OnPing(Ping p) => received += p.Value;

        [SetUp]
        public void SetUp() => received = 0;

        [TearDown]
        public void TearDown() => EventBus.Unsubscribe<Ping>(OnPing);

        [Test]
        public void SubscriberGetsPublishedEvents()
        {
            EventBus.Subscribe<Ping>(OnPing);
            EventBus.Publish(new Ping { Value = 3 });
            Assert.AreEqual(3, received);
        }

        [Test]
        public void NothingArrivesAfterUnsubscribing()
        {
            EventBus.Subscribe<Ping>(OnPing);
            EventBus.Unsubscribe<Ping>(OnPing);
            EventBus.Publish(new Ping { Value = 3 });
            Assert.AreEqual(0, received);
        }

        [Test]
        public void PublishingWithNoListenersIsFine()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new Ping { Value = 1 }));
        }
    }
}
