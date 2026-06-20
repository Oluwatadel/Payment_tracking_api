using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentTracker.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentTracker.Services
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationCleanupService> _logger;

        public NotificationCleanupService(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        int deletedCount = await notificationService.CleanupExpiredNotificationsAsync();
                        
                        if (deletedCount > 0)
                        {
                            _logger.LogInformation("Successfully cleaned up {Count} expired notifications.", deletedCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired notifications.");
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Notification Cleanup Service is stopping.");
        }
    }
}
