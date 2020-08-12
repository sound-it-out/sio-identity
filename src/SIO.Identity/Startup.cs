using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenEventSourcing.Extensions;
using OpenEventSourcing.Serialization.Json.Extensions;
using OpenEventSourcing.EntityFrameworkCore.SqlServer;
using SIO.Migrations;
using OpenEventSourcing.RabbitMQ.Extensions;
using SIO.Domain.User.Events;
using SIO.Infrastructure;
using OpenEventSourcing.Azure.ServiceBus.Extensions;
using Microsoft.Extensions.Hosting;

namespace SIO.Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddApiExplorer();

            services.AddMvc()
                .AddRazorRuntimeCompilation()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddOpenEventSourcing()
                .AddEntityFrameworkCoreSqlServer(options => {
                    options.MigrationsAssembly("SIO.Migrations");
                })
                .AddAzureServiceBus(options =>
                {
                    options.UseConnection(Configuration.GetValue<string>("Azure:ServiceBus:ConnectionString"))
                    .UseTopic(e =>
                    {
                        e.WithName(Configuration.GetValue<string>("Azure:ServiceBus:Topic"));
                    });
                })
                .AddCommands()
                .AddEvents()
                .AddJsonSerializers();
            
            services.AddSIOIdentity()
                .AddSIOInfrastructure();


            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add($"/{{1}}/Views/{{0}}{RazorViewEngine.ViewExtension}");
            });

            services.AddCors(options =>
                     options.AddPolicy("cors", builder =>
                     {
                         builder.WithOrigins(Configuration.GetValue<string>("DefaultAppUrl"))
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .AllowAnyHeader();
                     }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("cors");
            app.UseRouting();
            app.UseIdentityServer();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
