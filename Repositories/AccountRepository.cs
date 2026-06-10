using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;

namespace PaymentTracker.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByUserIdAsync(Guid userId, bool tracking = false);
        Task<bool> ExistsByUserIdAsync(Guid userId);
        Task AddAsync(Account account);
        void Remove(Account account);
        Task<int> SaveChangesAsync();
        Task<Account> GetAdminAccount();
        void Update(Account account);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByUserIdAsync(Guid userId, bool tracking = false)
        {
            var query = _context.Accounts.AsQueryable();
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public Task<bool> ExistsByUserIdAsync(Guid userId)
        {
            return _context.Accounts.AnyAsync(a => a.UserId == userId);
        }

        public Task AddAsync(Account account)
        {
            return _context.Accounts.AddAsync(account).AsTask();
        }

        public async Task<Account> GetAdminAccount()
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.User!.Role == UserRole.Admin);
            return account!;
        }
        public void Remove(Account account)
        {
            _context.Accounts.Remove(account);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }
    }
}
