using RailVision.Infrastructure.Persistence;
using RailVision.WebAPI.Configurations;
using RailVision.WebAPI.Endpoints;
using RailVision.WebAPI.Endpoints.Cache;

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

            app.MapGet("/", () => "Hello");

            app.RegisterRailwaysEndpoints()
               .RegisterPopulationCentersEndpoints()
               .RegisterStationsEndpoints()
               .RegisterTerrainsEndpoints()
               .RegisterRoutesEndpoints()
               .RegisterRedisCacheManagementEndpoints()
               .RegisterInMemoryCacheManagementEndpoints();

            app.Run();
        }
    }
}
