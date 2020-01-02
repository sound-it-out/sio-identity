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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddOpenEventSourcing()
                .AddEntityFrameworkCoreSqlServer(options => {
                    options.MigrationsAssembly("SIO.Migrations");
                })
                .AddRabbitMq(options =>
                {
                    options.UseConnection(Configuration.GetValue<string>("RabbitMQ:Connection"))
                        .UseExchange(e =>
                        {
                            e.WithName(Configuration.GetValue<string>("RabbitMQ:Exchange:Name"));
                            e.UseExchangeType(Configuration.GetValue<string>("RabbitMQ:Exchange:Type"));
                        })
                        .UseManagementApi(m =>
                        {
                            m.WithEndpoint(Configuration.GetValue<string>("RabbitMQ:ManagementApi:Endpoint"));
                            m.WithCredentials(Configuration.GetValue<string>("RabbitMQ:ManagementApi:Username"), Configuration.GetValue<string>("RabbitMQ:ManagementApi:Password"));
                        });
                })
                .AddEvents()
                .AddJsonSerializers();
            
            services.AddSIOIdentity();

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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseIdentityServer();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
