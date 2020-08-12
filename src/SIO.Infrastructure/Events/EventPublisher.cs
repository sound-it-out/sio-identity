using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenEventSourcing.EntityFrameworkCore;
using OpenEventSourcing.EntityFrameworkCore.DbContexts;
using OpenEventSourcing.Events;

namespace SIO.Infrastructure.Events
{
    internal class EventPublisher : IEventPublisher
    {
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly IEventModelFactory _eventModelFactory;
        private readonly IDbContextFactory _dbContextFactory;

        public EventPublisher(IEventBusPublisher eventBusPublisher, IEventModelFactory eventModelFactory, IDbContextFactory dbContextFactory)
        {
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));
            if (eventModelFactory == null)
                throw new ArgumentNullException(nameof(eventModelFactory));
            if (dbContextFactory == null)
                throw new ArgumentNullException(nameof(dbContextFactory));

            _eventBusPublisher = eventBusPublisher;
            _eventModelFactory = eventModelFactory;
            _dbContextFactory = dbContextFactory;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {            
            using (var context = _dbContextFactory.Create())
            {
                await context.Events.AddAsync(_eventModelFactory.Create(@event));
                await context.SaveChangesAsync();
            }

            await _eventBusPublisher.PublishAsync(@event);
        }

        public async Task PublishAsync(IEnumerable<IEvent> events)
        {
            using (var context = _dbContextFactory.Create())
            {
                foreach(var @event in events)
                    await context.Events.AddAsync(_eventModelFactory.Create(@event));

                await context.SaveChangesAsync();
            }

            await _eventBusPublisher.PublishAsync(events);
        }
    }
}
