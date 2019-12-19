using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SIO.Migrations;

namespace SIO.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            host.SeedDatabase();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((context) =>
            {
                context.ValidateScopes = false;
            })
            .UseStartup<Startup>();
    }
}
