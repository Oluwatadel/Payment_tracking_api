using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

namespace PaymentTracker.Services
{
    public interface IAccountService
    {
        Task<Account?> GetAccountByUserIdAsync(int userId);
        Task<Account> CreateAccountAsync(int userId, CreateAccountRequest request);
        Task<Account?> UpdateAccountAsync(int userId, UpdateAccountRequest request);
    }

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;

        public AccountService(IAccountRepository accountRepository, IUserRepository userRepository)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
        }

        public async Task<Account?> GetAccountByUserIdAsync(int userId)
        {
            return await _accountRepository.GetByUserIdAsync(userId);
        }

        public async Task<Account> CreateAccountAsync(int userId, CreateAccountRequest request)
        {
            if (!await _userRepository.ExistsByIdAsync(userId))
                throw new InvalidOperationException("User not found");

            if (await _accountRepository.ExistsByUserIdAsync(userId))
                throw new InvalidOperationException("User already has an account");

            var account = new Account
            {
                UserId = userId,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                AccountHolder = request.AccountHolder ?? string.Empty
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();

            return account;
        }

        public async Task<Account?> UpdateAccountAsync(int userId, UpdateAccountRequest request)
        {
            var account = await _accountRepository.GetByUserIdAsync(userId, tracking: true);
            if (account == null)
                return null;

            if (!string.IsNullOrEmpty(request.BankName))
                account.BankName = request.BankName;

            if (!string.IsNullOrEmpty(request.AccountNumber))
                account.AccountNumber = request.AccountNumber;

            if (!string.IsNullOrEmpty(request.AccountHolder))
                account.AccountHolder = request.AccountHolder;

            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.SaveChangesAsync();

            return account;
        }
    }
}
