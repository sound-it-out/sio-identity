using System;
using System.Collections.Generic;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SIO.Migrations;

namespace SIO.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAdd(GetServices());

            return services;
        }

        public static IServiceCollection AddSIOIdentity(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var serviceProvder = services.BuildServiceProvider();
            var configuration = serviceProvder.GetRequiredService<IConfiguration>();
            var environment = serviceProvder.GetRequiredService<IHostingEnvironment>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var migrationsAssembly = typeof(SIOIdentityDbContext).Assembly.GetName().Name;

            services.AddDbContext<SIOIdentityDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure();
                    sql.MigrationsAssembly(migrationsAssembly);
                }));

            services.AddIdentity<SIOUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SIOIdentityDbContext>()
            .AddDefaultTokenProviders();

            var identityServer = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ErrorUrl = "/error";
                options.UserInteraction.ConsentUrl = "/consent";
                options.PublicOrigin = configuration.GetValue<string>("Identity:Authority");
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseSqlServer(connectionString, (sql) =>
                    {
                        sql.EnableRetryOnFailure();
                        sql.MigrationsAssembly(migrationsAssembly);
                    });
            })
            .AddAspNetIdentity<SIOUser>()
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseSqlServer(connectionString, (sql) =>
                    {
                        sql.EnableRetryOnFailure();
                        sql.MigrationsAssembly(migrationsAssembly);
                    });

                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            });

            services.AddAuthentication()
            .AddOpenIdConnect("oidc", "Sound It Out OpenID Connect", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                options.Authority = configuration.GetValue<string>("Identity:Authority");
                options.ClientId = configuration.GetValue<string>("Identity:ClientId");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.ClaimActions.Add(new MapAllClaimsAction());
#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
            });

            if (!environment.IsProduction())
                identityServer.AddDeveloperSigningCredential();
            else
                throw new Exception("need to configure key material");


            return services;
        }

        private static IEnumerable<ServiceDescriptor> GetServices()
        {
            yield return ServiceDescriptor.Scoped<ILoginDetailsResponseBuilder, LoginDetailsResponseBuilder>();
        }
    }
}
