using System.Threading;
using System.Threading.Tasks;
using SIO.Infrastructure.Events;

namespace SIO.Domain
{
    public interface IEventManager
    {
        Task ProcessAsync<TEvent>(StreamId streamId, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    }
}
