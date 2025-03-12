using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RailVision.Infrastructure.Persistence;

namespace RailVision.Infrastructure.Services.Background
{
    public class LogCleanupService(ILogger<LogCleanupService> logger, IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly ILogger<LogCleanupService> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LogCleanupService is starting.");
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

                var sql = "DELETE FROM Logs WHERE TimeStamp < DATEADD(day, -30, GETDATE())";
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
