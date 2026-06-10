using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

namespace PaymentTracker.Services
{
    public interface IAccountService
    {
        Task<Account> GetAccountByUserIdAsync(Guid userId);
        Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request);
        Task<Account> UpdateAccountAsync(Guid userId, UpdateAccountRequest request);
    }

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository, IUserRepository userRepository, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Account> GetAccountByUserIdAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching account for user {UserId}", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId}", userId);
                throw new KeyNotFoundException("Account not found");
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
                throw new KeyNotFoundException("User not found");
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
            await _accountRepository.SaveChangesAsync();

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
                throw new KeyNotFoundException("Account not found");
            }

            if (!string.IsNullOrEmpty(request.BankName))
                account.BankName = request.BankName;

            if (!string.IsNullOrEmpty(request.AccountNumber))
                account.AccountNumber = request.AccountNumber;

            if (!string.IsNullOrEmpty(request.AccountHolder))
                account.AccountHolder = request.AccountHolder;

            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.SaveChangesAsync();

            _logger.LogInformation("Account updated for user {UserId}", userId);

            return account;
        }
    }
}
