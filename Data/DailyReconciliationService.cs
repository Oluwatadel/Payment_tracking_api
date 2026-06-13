
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
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day).AddHours(5); // Next 5hours
                var delay = nextRunTime - now;
                _logger.LogInformation("Daily reconciliation will run at {NextRunTime} (in {Delay})", nextRunTime, delay);
                await Task.Delay(delay, stoppingToken);


                using var scope = _serviceScopeFactory.CreateScope();
                
                var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                var adminAccount = await accountService.GetAdminAccount(true);
                var getAllPaymentSummary = (await paymentService.GetAllPaymentsAsync()).Sum(p => p.Amount);
                if (adminAccount.Balance < 0 || getAllPaymentSummary > adminAccount.Balance)
                {
                    _logger.LogWarning("Admin account balance is negative: {Balance}. Reconciliation will proceed.", adminAccount.Balance);
                    await accountService.ReconcileAdminAccount(stoppingToken);
                    return;
                }
                await accountService.ReconcileAdminAccount(stoppingToken);
                _logger.LogInformation("Reconciliation completed at {Time}", DateTime.UtcNow);
            }
        }
    }
}
