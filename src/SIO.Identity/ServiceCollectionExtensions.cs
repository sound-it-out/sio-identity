using System;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                options.IssuerUri = configuration.GetValue<string>("Identity:Authority");
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

        public static IServiceCollection AddSameSiteHandling(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            return services;
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (DisallowsSameSiteNone(userAgent))
                {
                    // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }
        private static bool DisallowsSameSiteNone(string userAgent)
        {
            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the iOS networking stack
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions, 
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
    }
}
