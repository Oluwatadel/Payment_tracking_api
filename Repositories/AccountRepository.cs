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
        Task<Account> GetAdminAccount(bool tracking = true);
        Task<List<Account>> GetAllUserAccountsAsync(bool tracking = false);
        void Update(Account account);
        Task<int> RemoveByUserIdAsync(Guid userId);
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

        public async Task<Account> GetAdminAccount(bool tracking = false)
        {
            var query = from user in _context.Users
                        join acct in _context.Accounts on user.Id equals acct.UserId
                        where user.Role == UserRole.Admin
                        select acct;
            if (!tracking)
                query = query.AsNoTracking();

            return (await query.FirstOrDefaultAsync())!;
        }
        public async Task<List<Account>> GetAllUserAccountsAsync(bool tracking = false)
        {
            var query = from user in _context.Users
                        join acct in _context.Accounts on user.Id equals acct.UserId
                        where user.Role != UserRole.Admin
                        select acct;
            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public void Remove(Account account)
        {
            _context.Accounts.Remove(account);
        }

        public Task<int> RemoveByUserIdAsync(Guid userId)
        {
            return _context.Accounts
                .Where(a => a.UserId == userId)
                .ExecuteDeleteAsync();
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
