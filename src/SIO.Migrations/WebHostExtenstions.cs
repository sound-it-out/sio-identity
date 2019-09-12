
using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SIO.Migrations
{
    public static class WebHostExtenstions
    {
        public static void SeedDatabase(this IWebHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var identityContext = scope.ServiceProvider.GetRequiredService<SIOIdentityDbContext>();
                var config = scope.ServiceProvider.GetRequiredService<IOptions<IdentityConfig>>().Value;

                persistedGrantContext.Database.Migrate();
                configurationDbContext.Database.Migrate();
                identityContext.Database.Migrate();

                var identityResources = new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                };

                SeedClient(configurationDbContext, config);
                SeedIdentiyResources(configurationDbContext, identityResources);
                SeedApiResources(configurationDbContext, config);
            }
        }

        private static void SeedClient(ConfigurationDbContext context, IdentityConfig config)
        {
            var newClients = config.Clients.Where(client => !context.Clients.Any(c => c.ClientId == client.ClientId)).ToList();
            var existingClients = config.Clients.Where(client => context.Clients.Any(c => c.ClientId == client.ClientId)).ToList();

            context.Clients.AddRange(newClients.Select(c => c.ToEntity()));
            context.SaveChanges();

            var toRemove = context.Clients.Where(client => existingClients.Any(c => c.ClientId == client.ClientId)).ToList();
            context.Clients.RemoveRange(toRemove);

            context.SaveChanges();

            context.Clients.AddRange(existingClients.Select(client => client.ToEntity()));
            context.SaveChanges();
        }

        private static void SeedIdentiyResources(ConfigurationDbContext context, IEnumerable<IdentityResource> resources)
        {
            var newResources = resources.Where(resource => !context.IdentityResources.Any(r => r.Name == resource.Name)).ToList();
            var existingResources = resources.Where(resource => context.IdentityResources.Any(r => r.Name == resource.Name)).ToList();

            context.IdentityResources.AddRange(newResources.Select(resource => resource.ToEntity()));
            context.SaveChanges();

            var toRemove = context.IdentityResources.Where(resource => existingResources.Any(c => c.Name == resource.Name)).ToList();
            context.IdentityResources.RemoveRange(toRemove);
            context.SaveChanges();

            context.IdentityResources.AddRange(existingResources.Select(resource => resource.ToEntity()));
            context.SaveChanges();
        }

        private static void SeedApiResources(ConfigurationDbContext context, IdentityConfig config)
        {
            var newResources = config.ApiResources.Where(resource => !context.ApiResources.Any(r => r.Name == resource.Name)).ToList();
            var existingResources = config.ApiResources.Where(resource => context.ApiResources.Any(r => r.Name == resource.Name)).ToList();

            context.ApiResources.AddRange(newResources.Select(resource => resource.ToEntity()));
            context.SaveChanges();

            var toRemove = context.ApiResources.Where(resource => existingResources.Any(c => c.Name == resource.Name)).ToList();
            context.ApiResources.RemoveRange(toRemove);
            context.SaveChanges();

            context.ApiResources.AddRange(existingResources.Select(resource => resource.ToEntity()));
            context.SaveChanges();
        }
    }
}
