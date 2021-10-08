using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using OpenEventSourcing.Commands;
using OpenEventSourcing.EntityFrameworkCore.InMemory;
using OpenEventSourcing.Events;
using OpenEventSourcing.Extensions;
using OpenEventSourcing.Queries;
using OpenEventSourcing.Serialization.Json.Extensions;
using SIO.Infrastructure;
using SIO.Infrastructure.Events;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests
{
    public abstract class ControllerSpecification<TController, TResult> : IAsyncLifetime
        where TController : Controller
    {
        protected readonly TController _controller;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly List<IEvent> _events = new List<IEvent>();
        protected readonly Mock<IConfiguration> _configuration;

        protected TResult Result { get; private set; }

        protected abstract Task<TResult> Given();
        protected abstract Task When();

        public ControllerSpecification()
        {
            var services = new ServiceCollection();

            services.AddOpenEventSourcing()
                .AddEntityFrameworkCoreInMemory()
                .AddCommands()
                .AddQueries()
                .AddEvents()
                .AddJsonSerializers();

            var migrationsAssembly = typeof(SIOIdentityDbContext).Assembly.GetName().Name;
            var dbName = $"{GetType().Name}_{Guid.NewGuid()}";
            //services.AddIdentityConfiguration();

            services.AddDbContext<SIOIdentityDbContext>(options =>
                options.UseInMemoryDatabase(dbName)
            );

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
                //options.PublicOrigin = configuration.GetValue<string>("Identity:Authority");
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseInMemoryDatabase(dbName);
            })
            .AddAspNetIdentity<SIOUser>()
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = (builder) =>
                    builder.UseInMemoryDatabase(dbName);

                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            });

            services.AddAuthentication()
            .AddOpenIdConnect("oidc", "Sound It Out OpenID Connect", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                options.GetClaimsFromUserInfoEndpoint = true;
                options.ClaimActions.Add(new MapAllClaimsAction());
#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
            });

            identityServer.AddDeveloperSigningCredential();

            _configuration = new Mock<IConfiguration>();

            services.AddSingleton(_configuration.Object);
            services.AddTransient<TController>();
            services.RemoveAll<SignInManager<SIOUser>>();
            services.RemoveAll<IIdentityServerInteractionService>();
            services.AddSingleton<SignInManager<SIOUser>, MockSignInManager>();
            services.AddSingleton<UserManager<SIOUser>, MockUserManager>();
            services.AddSingleton<IIdentityServerInteractionService, MockIdentityServerInteraction>();

            var mockEventBusPublisher = new MockEventPublisher(_events);

            services.AddSingleton<IEventManager, MockEventPublisher>(sp => mockEventBusPublisher);
            _serviceProvider = services.BuildServiceProvider();
            _controller = _serviceProvider.GetRequiredService<TController>();
        }

        public async Task InitializeAsync()
        {
            await When();
            Result = await Given();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
