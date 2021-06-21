using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SIO.Infrastructure;
using SIO.Infrastructure.Events;

namespace SIO.Domain
{
    internal sealed class DefaultEventManager : IEventManager
    {
        private readonly IEventStore _eventStore;
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly ILogger<DefaultEventManager> _logger;

        public DefaultEventManager(IEventStore eventStore,
            IEventBusPublisher eventBusPublisher,
            ILogger<DefaultEventManager> logger)
        {
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _eventStore = eventStore;
            _eventBusPublisher = eventBusPublisher;
            _logger = logger;
        }

        public async Task ProcessAsync<TEvent>(StreamId streamId, TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{nameof(DefaultEventManager)}.{nameof(ProcessAsync)} was cancelled before execution");
                cancellationToken.ThrowIfCancellationRequested();
            }

            var context = new EventContext<IEvent>(streamId: streamId, @event: @event, correlationId: null, causationId: null, @event.Timestamp, actor: Actor.From("unknown"));
            var notification = new EventNotification<TEvent>(streamId: StreamId.New(), @event: @event, correlationId: null, causationId: null, timestamp: DateTimeOffset.UtcNow, userId: null);
            await _eventStore.SaveAsync(streamId, new IEventContext<IEvent>[] { context }, cancellationToken);
            await _eventBusPublisher.PublishAsync(notification, cancellationToken);
        }
    }
}
