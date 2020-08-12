using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEventSourcing.Events;

namespace SIO.Infrastructure.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        Task PublishAsync(IEnumerable<IEvent> events);
    }
}
