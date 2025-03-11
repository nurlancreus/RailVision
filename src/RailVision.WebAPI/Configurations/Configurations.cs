using RailVision.Application.Abstractions;
using RailVision.Infrastructure.Services;

namespace RailVision.WebAPI.Configurations
{
    public static class Configurations
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Configuration.ConfigureEnvironments<Program>(builder.Environment);

            // Registering HttpClient with a custom timeout
            builder.Services.AddHttpClient("OverpassClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddControllers()
              .AddJsonOptions(options =>
              {
                  options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Enable case-insensitive deserialization
              });

            builder
                .ConfigureCORS()
                .ConfigureDbContext()
                .ConfigureRateLimiter();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Register App Services
            builder.Services.AddScoped<IRailwayService, RailwayService>();
            #endregion

            #region Exception Handling
            // Add ProblemDetails services
            builder.Services.AddProblemDetails();

            // Add Exception Handler
            builder.Services.AddExceptionHandler<CustomExceptionHandler>();
            #endregion
        }

        public static void UseMiddlewares(this WebApplication app)
        {
            app.UseExceptionHandler();

            app.UseStatusCodePages();

            app.UseCors(Constants.CorsPolicies.AllowAllPolicy);

            app.UseRateLimiter();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
        }
    }
}
