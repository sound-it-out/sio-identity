
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenEventSourcing.EntityFrameworkCore.DbContexts;

namespace SIO.Migrations
{
    public static class WebHostExtenstions
    {
        public static async Task SeedDatabaseAsync(this IWebHost host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var identityContext = scope.ServiceProvider.GetRequiredService<SIOIdentityDbContext>();
                var config = scope.ServiceProvider.GetRequiredService<IOptions<IdentityConfig>>().Value;

                await persistedGrantContext.Database.MigrateAsync();
                await configurationDbContext.Database.MigrateAsync();
                await identityContext.Database.MigrateAsync();

                var identityResources = new IdentityResource[]
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                };

                var clients = config.Clients.Select(c => new Client
                {
                    ClientId = c.ClientId,
                    ClientName = c.ClientName,
                    Enabled = true,
                    RequireConsent = c.RequiresConsent,
                    AllowedGrantTypes = c.AllowedGrantTypes,
                    AllowAccessTokensViaBrowser = c.AllowAccessTokensViaBrowser,
                    RequireClientSecret = c.RequireClientSecret,
                    RequirePkce = c.RequirePkce,
                    ClientSecrets = c.ClientSecrets.Select(secret => new Secret(secret.Sha256())).ToList(),
                    RedirectUris = c.RedirectUris,
                    PostLogoutRedirectUris = c.PostLogoutRedirectUris,
                    AllowedCorsOrigins = c.AllowedCorsOrigins,
                    AllowedScopes = c.AllowedScopes,
                    AllowOfflineAccess = c.AllowOfflineAccess
                });

                var apiResources = config.ApiResources.Select(resource => new ApiResource(resource.Name, resource.DisplayName, new[] { "role" }));

                await SeedClientAsync(configurationDbContext, clients);
                await SeedIdentiyResourcesAsync(configurationDbContext, identityResources);
                await SeedApiResourcesAsync(configurationDbContext, apiResources);
            }
        }

        private static async Task SeedClientAsync(ConfigurationDbContext context, IEnumerable<Client> clients)
        {
            var newClients = clients.Where(client => !context.Clients.Any(c => c.ClientId == client.ClientId)).ToList();
            var existingClients = clients.Where(client => context.Clients.Any(c => c.ClientId == client.ClientId)).ToList();

            context.Clients.AddRange(newClients.Select(c => c.ToEntity()));
            await context.SaveChangesAsync();

            var toRemove = context.Clients.Where(client => existingClients.Any(c => c.ClientId == client.ClientId)).ToList();
            context.Clients.RemoveRange(toRemove);

            await context.SaveChangesAsync();

            context.Clients.AddRange(existingClients.Select(client => client.ToEntity()));
            await context.SaveChangesAsync();
        }

        private static async Task SeedIdentiyResourcesAsync(ConfigurationDbContext context, IEnumerable<IdentityResource> resources)
        {
            var newResources = resources.Where(resource => !context.IdentityResources.Any(r => r.Name == resource.Name)).ToList();
            var existingResources = resources.Where(resource => context.IdentityResources.Any(r => r.Name == resource.Name)).ToList();

            context.IdentityResources.AddRange(newResources.Select(resource => resource.ToEntity()));
            await context.SaveChangesAsync();

            var toRemove = context.IdentityResources.Where(resource => existingResources.Any(c => c.Name == resource.Name)).ToList();
            context.IdentityResources.RemoveRange(toRemove);
            await context.SaveChangesAsync();

            context.IdentityResources.AddRange(existingResources.Select(resource => resource.ToEntity()));
            await context.SaveChangesAsync();
        }

        private static async Task SeedApiResourcesAsync(ConfigurationDbContext context, IEnumerable<ApiResource> apiResources)
        {
            var newResources = apiResources.Where(resource => !context.ApiResources.Any(r => r.Name == resource.Name)).ToList();
            var existingResources = apiResources.Where(resource => context.ApiResources.Any(r => r.Name == resource.Name)).ToList();

            context.ApiResources.AddRange(newResources.Select(resource => resource.ToEntity()));
            await context.SaveChangesAsync();

            var toRemove = context.ApiResources.Where(resource => existingResources.Any(c => c.Name == resource.Name)).ToList();
            context.ApiResources.RemoveRange(toRemove);
            await context.SaveChangesAsync();

            context.ApiResources.AddRange(existingResources.Select(resource => resource.ToEntity()));
            await context.SaveChangesAsync();
        }
    }
}
