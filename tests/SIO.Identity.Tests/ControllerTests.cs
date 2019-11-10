using System;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenEventSourcing.EntityFrameworkCore.InMemory;
using OpenEventSourcing.Events;
using OpenEventSourcing.Extensions;
using OpenEventSourcing.Serialization.Json.Extensions;
using SIO.Migrations;

namespace SIO.Identity.Tests
{
    public class ControllerTests<TController>
        where TController : Controller
    {
        protected TController BuildController(out IServiceProvider serviceProvider) 
        {
            var services = new ServiceCollection();

            services.AddOpenEventSourcing()
                .AddEntityFrameworkCoreInMemory()
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

                //options.Authority = configuration.GetValue<string>("Identity:Authority");
                //options.ClientId = configuration.GetValue<string>("Identity:ClientId");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.ClaimActions.Add(new MapAllClaimsAction());
#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
            });

            identityServer.AddDeveloperSigningCredential();

            var configuration = new Mock<IConfiguration>();
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns("DefaultAppUrl");
            configuration.Setup(c => c.GetSection(It.Is<string>((s) => s == "DefaultAppUrl"))).Returns(section.Object);

            services.AddSingleton(configuration.Object);
            services.AddTransient<TController>();
            services.RemoveAll<SignInManager<SIOUser>>();
            services.RemoveAll<IIdentityServerInteractionService>();
            services.AddSingleton<SignInManager<SIOUser>, MockSignInManager>();
            services.AddSingleton<UserManager<SIOUser>, MockUserManager>();
            services.AddSingleton<IIdentityServerInteractionService, MockIdentityServerInteraction>();
            services.AddTransient<IEventBusPublisher, MockEventBusPublisher>();
            serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<TController>();
        }
    }
}
