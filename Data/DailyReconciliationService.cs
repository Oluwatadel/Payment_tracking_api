
using Microsoft.IdentityModel.Tokens;
using PaymentTracker.Services;

namespace PaymentTracker.Data
{
    public class DailyReconciliationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DailyReconciliationService> _logger;
        private readonly TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;

        public DailyReconciliationService(IServiceScopeFactory serviceScopeFactory, ILogger<DailyReconciliationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(stoppingToken.IsCancellationRequested)
            {
                var now  = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo);
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day).AddDays(1); // Next midnight
                var delay = nextRunTime - now;
                _logger.LogInformation("Daily reconciliation will run at {NextRunTime} (in {Delay})", nextRunTime, delay);
                await Task.Delay(delay, stoppingToken);


                using var scope = _serviceScopeFactory.CreateScope();
                
                var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                await accountService.ReconcileAdminAccount(stoppingToken);
                _logger.LogInformation("Reconciliation completed at {Time}", DateTime.UtcNow);
            }
        }
    }
}
