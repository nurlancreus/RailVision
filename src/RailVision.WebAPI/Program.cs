
using Microsoft.EntityFrameworkCore;
using RailVision.Infrastructure.Persistence;
using RailVision.WebAPI.Configurations;
using RailVision.WebAPI.Endpoints;

namespace RailVision.WebAPI
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.RegisterServices();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await OverpassDataSeeder.SeedAsync(services);
            }

            app.UseMiddlewares();

            app.RegisterRailwaysEndpoints()
               .RegisterStationsEndpoints()
               .RegisterTerrainsEndpoints()
               .RegisterRoutesEndpoints();

            app.Run();
        }
    }
}
