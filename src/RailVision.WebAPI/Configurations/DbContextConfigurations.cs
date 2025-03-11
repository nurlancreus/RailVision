using RailVision.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RailVision.WebAPI.Configurations
{
    public static class DbContextConfigurations
    {
        public static WebApplicationBuilder ConfigureDbContext(this WebApplicationBuilder builder)
        {

            if (!builder.Environment.IsTesting())
            {
                builder.Services.AddDbContext<AppDbContext>((sp, options) =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), sqlOptions => sqlOptions
                    .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);

                });
            }

            return builder;
        }
    }
}
