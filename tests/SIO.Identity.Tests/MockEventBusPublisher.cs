using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEventSourcing.Events;

namespace SIO.Identity.Tests
{
    internal class MockEventBusPublisher : IEventBusPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync(IEnumerable<IEvent> events)
        {
            return Task.CompletedTask;
        }
    }
}
