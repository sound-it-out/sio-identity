using System;
using System.Collections.Generic;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SIO.Migrations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<IdentityConfig>(config => {
                var clientOptions = new ClientOptions();
                configuration.Bind(clientOptions);
                config.Clients = clientOptions.Clients;

                var apiResourceOptions = new ApiResourceOptions();
                configuration.Bind(apiResourceOptions);
                config.ApiResources = apiResourceOptions.ApiResources;

                var apiScopeOptions = new ApiScopeOptions();
                configuration.Bind(apiScopeOptions);

                config.ApiScopes = apiScopeOptions.ApiScopes;
            });

            return services;
        }
    }
}
