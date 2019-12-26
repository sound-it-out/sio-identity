using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEventSourcing.Events;

namespace SIO.Identity.Tests
{
    internal class MockEventBusPublisher : IEventBusPublisher
    {
        private readonly List<IEvent> _events;

        public MockEventBusPublisher(List<IEvent> events)
        {
            _events = events;
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _events.Add(@event);
            return Task.CompletedTask;
        }

        public Task PublishAsync(IEnumerable<IEvent> events)
        {
            _events.AddRange(events);
            return Task.CompletedTask;
        }
    }
}
