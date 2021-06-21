using System;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SIO.Migrations;

namespace SIO.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSIOIdentity(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var serviceProvder = services.BuildServiceProvider();
            var configuration = serviceProvder.GetRequiredService<IConfiguration>();
            var environment = serviceProvder.GetRequiredService<IWebHostEnvironment>();

            var migrationsAssembly = typeof(SIOIdentityDbContext).Assembly.GetName().Name;

            services.AddIdentityConfiguration();

            services.AddDbContext<SIOIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Identity"), sql =>
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
                options.Cors.CorsPaths.Add("/v1/client");
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseSqlServer(configuration.GetConnectionString("IdentityServer"), (sql) =>
                    {
                        sql.EnableRetryOnFailure();
                        sql.MigrationsAssembly(migrationsAssembly);
                    });
            })
            .AddAspNetIdentity<SIOUser>()
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseSqlServer(configuration.GetConnectionString("IdentityServer"), (sql) =>
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

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
            services.AddDataProtection()
                .SetApplicationName("sio-identity");

            if (!environment.IsProduction())
                identityServer.AddDeveloperSigningCredential();
            else
                throw new Exception("need to configure key material");


            return services;
        }
    }
}
