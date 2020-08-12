using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEventSourcing.Events;
using SIO.Infrastructure.Events;

namespace SIO.Identity.Tests
{
    internal class MockEventPublisher : IEventPublisher
    {
        private readonly List<IEvent> _events;

        public MockEventPublisher()
        {
            _events = new List<IEvent>();
        }

        public MockEventPublisher(List<IEvent> events)
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
