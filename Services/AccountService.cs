using PaymentTracker.DTOs;
using PaymentTracker.Exceptions;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

namespace PaymentTracker.Services
{
    public interface IAccountService
    {
        Task<Account> GetAccountByUserIdAsync(Guid userId);
        Task<Account> GetAdminAccount(bool isTracking);
        Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request);
        Task<Account> UpdateAccountAsync(Guid userId, UpdateAccountRequest request);
        Task ReconcileAdminAccount(CancellationToken cancellationToken);
    }

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfwork;

        public AccountService(IAccountRepository accountRepository, IUserRepository userRepository, 
            ILogger<AccountService> logger, IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _logger = logger;
            _paymentRepository = paymentRepository;
            _unitOfwork = unitOfWork;
        }

        public async Task<Account> GetAccountByUserIdAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching account for user {UserId}", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId}", userId);
                throw new NotFoundException("Account not found");
            }

            _logger.LogInformation("Account found for user {UserId}", userId);
            return account;
        }

        public async Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Creating account for user {UserId}", userId);

            if (!await _userRepository.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("Cannot create account. User {UserId} was not found", userId);
                throw new NotFoundException("User not found");
            }

            if (await _accountRepository.ExistsByUserIdAsync(userId))
            {
                _logger.LogWarning("User {UserId} already has an account", userId);
                throw new InvalidOperationException("User already has an account");
            }

            var account = new Account
            {
                UserId = userId,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                AccountHolder = request.AccountHolder ?? string.Empty
            };

            await _accountRepository.AddAsync(account);
            var changes = await _unitOfwork.SaveChangesAsync();
            if (changes < 0)
            {
                _logger.LogError("Failed to create account for user {UserId}", userId);
                throw new SaveOperationException("Failed to create account");
            }

            _logger.LogInformation("Account created for user {UserId}", userId);

            return account;
        }

        public async Task<Account> UpdateAccountAsync(Guid userId, UpdateAccountRequest request)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Updating account for user {UserId}", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId, tracking: true);
            if (account == null)
            {
                _logger.LogWarning("Cannot update account. Account not found for user {UserId}", userId);
                throw new NotFoundException("Account not found");
            }

            if (!string.IsNullOrEmpty(request.BankName))
                account.BankName = request.BankName;

            if (!string.IsNullOrEmpty(request.AccountNumber))
                account.AccountNumber = request.AccountNumber;

            if (!string.IsNullOrEmpty(request.AccountHolder))
                account.AccountHolder = request.AccountHolder;

            account.UpdatedAt = DateTime.UtcNow;
           var changes = await _unitOfwork.SaveChangesAsync();
            if (changes < 0)
            {
                _logger.LogError("Failed to update account for user {UserId}", userId);
                throw new SaveOperationException("Failed to update account");
            }

            _logger.LogInformation("Account updated for user {UserId}", userId);

            return account;
        }

        public Task<Account> GetAdminAccount(bool tracking = true)
        {
            return _accountRepository.GetAdminAccount(tracking);
        }

        public async Task ReconcileAdminAccount(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");

            _logger.LogWarning("Admin account reconciliation skipped because one or more required environment variables are missing: ADMIN_USERNAME, ADMIN_PHONE.");

            var admin = await _accountRepository.GetAdminAccount(tracking: true);

            if (admin == null)
            {
                _logger.LogInformation("Admin account not found. Creating a new admin account.");
                admin = new Account
                {
                    BankName = "Default Bank",
                    AccountNumber = $"ACC-{Guid.NewGuid():N}".Substring(0, 9),
                    AccountHolder = "admin",
                };
                await _accountRepository.AddAsync(admin);
            }
            var totalPaymentAccruedOntheApp = (await _paymentRepository.GetAllAsync()).Sum(p => p.Amount);

            var deficit = totalPaymentAccruedOntheApp - admin.Balance;
            if (deficit > 0)
            {
                admin.AddPaymentToBalance(deficit);
                _logger.LogInformation($"Admin account balance reconciled. Added {deficit:C} to the balance.");
            }
            _accountRepository.Update(admin);
            var changes = await _unitOfwork.SaveChangesAsync();
            if (changes <= 0)
            {
                _logger.LogInformation("Admin account reconciled successfully.");
                throw new SaveOperationException("Admin account reconciled successfully.");
            }
            _logger.LogInformation("Admin account reconciliation successfully.");
        }
    }
}
