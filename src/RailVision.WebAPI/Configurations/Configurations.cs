using Microsoft.AspNetCore.ResponseCompression;
using RailVision.Application.Abstractions;
using RailVision.Application.Abstractions.OverpassAPI;
using RailVision.Infrastructure.Persistence;
using RailVision.Infrastructure.Services;
using RailVision.Infrastructure.Services.Background;
using Serilog;

namespace RailVision.WebAPI.Configurations
{
    public static class Configurations
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Configuration.ConfigureEnvironments<Program>(builder.Environment);

            // Registering HttpClient with a custom timeout and Automatic GZIP Handling
            builder.Services.AddHttpClient("OverpassClient", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(1);
            })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // Compress HTTPS responses
                options.Providers.Add<GzipCompressionProvider>();
                // You can also add Brotli: options.Providers.Add<BrotliCompressionProvider>();

                options.MimeTypes =
                [
                    "application/json",
                ];
            });

            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
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
                .ConfigureSeriLog()
                .ConfigureRateLimiter();

            //Registering the Background service
            builder.Services.AddHostedService<LogCleanupService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Register App Services
            builder.Services.AddScoped<IStationsOverpassApiService, OverpassApiService>();
            builder.Services.AddScoped<IRailwaysOverpassApiService, OverpassApiService>();
            builder.Services.AddScoped<ITerrainsOverpassApiService, OverpassApiService>();
            builder.Services.AddScoped<IOverpassApiService, OverpassApiService>();

            builder.Services.AddScoped<IRailwayService, RailwayService>();
            builder.Services.AddScoped<IStationService, StationService>();
            builder.Services.AddScoped<ITerrainService, TerrainService>();
            builder.Services.AddScoped<IRouteService, RouteService>();
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

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseResponseCompression();

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "Unknown");
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };

                options.MessageTemplate = "Handled {RequestPath}";
            });

            app.UseHttpLogging();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
        }

    }
}
