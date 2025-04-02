using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RailVision.Infrastructure.Persistence;

namespace RailVision.Infrastructure.Services.Background
{
    public class LogCleanupService(ILogger<LogCleanupService> logger, IServiceProvider serviceProvider, IConfiguration configuration) : BackgroundService
    {
        private readonly ILogger<LogCleanupService> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IConfiguration _configuration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LogCleanupService is starting.");

            var isDbAvailable = AppDbContext.CheckDatabaseAvailability(_configuration);

            if (!isDbAvailable)
            {
                _logger.LogError("Database is not available. LogCleanupService will not run.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("LogCleanupService is working.");
                await DeleteOldLogsAsync(stoppingToken);
                // Wait for 24 hours before the next cleanup
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task DeleteOldLogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var sql = "DELETE FROM Logs WHERE TimeStamp < DATEADD(day, -1, GETDATE())";
                var affectedRows = await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);

                _logger.LogInformation($"{affectedRows} log entries deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning logs.");
            }
        }
    }
}
