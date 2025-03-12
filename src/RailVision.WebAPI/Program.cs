
using Microsoft.EntityFrameworkCore;
using RailVision.Infrastructure.Persistence;
using RailVision.WebAPI.Configurations;
using RailVision.WebAPI.Endpoints;

namespace RailVision.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.RegisterServices();

            var app = builder.Build();

            app.UseMiddlewares(builder);

            app.RegisterRailwaysEndpoints()
               .RegisterStationsEndpoints()
               .RegisterRoutesEndpoints();

            app.Run();
        }
    }
}
