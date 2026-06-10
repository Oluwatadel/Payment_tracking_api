using PaymentTracker.Data;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using Microsoft.EntityFrameworkCore;

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
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetAccountByUserIdAsync(int userId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Account> CreateAccountAsync(int userId, CreateAccountRequest request)
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (existingAccount != null)
                throw new InvalidOperationException("User already has an account");

            var account = new Account
            {
                UserId = userId,
                BankName = request.BankName,
                AccountNumber = request.AccountNumber,
                AccountHolder = request.AccountHolder
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<Account?> UpdateAccountAsync(int userId, UpdateAccountRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return null;

            if (!string.IsNullOrEmpty(request.BankName))
                account.BankName = request.BankName;

            if (!string.IsNullOrEmpty(request.AccountNumber))
                account.AccountNumber = request.AccountNumber;

            if (!string.IsNullOrEmpty(request.AccountHolder))
                account.AccountHolder = request.AccountHolder;

            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return account;
        }
    }
}
