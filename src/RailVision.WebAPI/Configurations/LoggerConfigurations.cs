using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Microsoft.Extensions.Configuration;
using RailVision.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpLogging;

namespace RailVision.WebAPI.Configurations
{
    public static class LoggerConfigurations
    {
        public static WebApplicationBuilder ConfigureSeriLog(this WebApplicationBuilder builder)
        {
            bool isDbAvailable = AppDbContext.CheckDatabaseAvailability(builder.Configuration);
            // Set up Serilog programmatically using fluent configuration
            builder.Host.UseSerilog((context, services, configuration) =>
            {

                // Read Serilog configuration from appsettings.json if needed
                var logConfig = configuration
                    .MinimumLevel.Information() // Default minimum log level (Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Override Microsoft libraries to Warning
                    .MinimumLevel.Override("System", LogEventLevel.Error) // Override System namespaces to Error

                    // Global properties (Application and Server)
                    .Enrich.WithProperty("Application", "RailVision")
                    .Enrich.WithProperty("Server", "Server-125.08.13.1")

                    // Write to multiple sinks (Console, File, MSSqlServer)
                    .WriteTo.Async(a => a
                        .Console(
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss } [{Level:u3}] [{Application}/{Server}] {Message:lj}{NewLine}{Exception}"
                        )
                        .WriteTo.File(
                            "logs/MyAppLog-.txt",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 30,
                            fileSizeLimitBytes: 10485760,
                            rollOnFileSizeLimit: true,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                        )

                    );

                if (isDbAvailable)
                {
                    logConfig.WriteTo.MSSqlServer(connectionString: builder.Configuration.GetConnectionString("Default"),
                      sinkOptions: new MSSqlServerSinkOptions
                      {
                          TableName = "Logs",
                          AutoCreateSqlTable = true
                      });
                }
            });

            if(isDbAvailable)
            {
                builder.Services.AddHttpLogging(logging =>
                {
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestHeaders.Add("sec-ch-ua");
                    logging.MediaTypeOptions.AddText("application/javascript");
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;
                });
            }

            return builder;
        }
    }
}
