using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using RailVision.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpLogging;
using Serilog.Sinks.SystemConsole.Themes;

namespace RailVision.WebAPI.Configurations
{
    public static class LoggerConfigurations
    {
        public static WebApplicationBuilder ConfigureSeriLog(this WebApplicationBuilder builder)
        {
            #region Custom loggin theme
            var customTheme = new SystemConsoleTheme(new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red },
                [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
                [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
                [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Magenta },
                [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red },
                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red }
            });
            #endregion

            bool isDbAvailable = AppDbContext.CheckDatabaseAvailability(builder.Configuration);
            builder.Host.UseSerilog((context, services, configuration) =>
            {

                // Read Serilog configuration from appsettings.json if needed
                var logConfig = configuration
                    .MinimumLevel.Information() // Default minimum log level (Information)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("System", LogEventLevel.Error)

                    // Global properties (Application and Server)
                    .Enrich.WithProperty("Application", "RailVision")
                    .Enrich.WithProperty("Server", "Server-125.08.13.1")

                    // Write to multiple sinks (Console, File, MSSqlServer)
                    .WriteTo.Async(a => a
                        .Console(
                            theme: customTheme,
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

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("sec-ch-ua");
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
            });

            return builder;
        }
    }
}
