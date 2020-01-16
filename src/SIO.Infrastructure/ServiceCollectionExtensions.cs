using Microsoft.Extensions.DependencyInjection;
using SIO.Infrastructure.Events;

namespace SIO.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSIOInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}
