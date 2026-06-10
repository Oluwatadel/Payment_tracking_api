using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;

namespace PaymentTracker.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, bool tracking = false);
        Task<User?> GetByUsernameAsync(string username, bool tracking = false);
        Task<List<User>> GetAllAsync();
        Task<bool> ExistsByIdAsync(Guid userId);
        Task<bool> ExistsByUsernameAsync(string username, Guid? excludedUserId = null);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedUserId = null);
        Task AddAsync(User user);
        void Remove(User user);
        Task<int> SaveChangesAsync();
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid userId, bool tracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByUsernameAsync(string username, bool tracking = false)
        {
            var query = _context.Users.AsQueryable();
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync();
        }

        public Task<bool> ExistsByIdAsync(Guid userId)
        {
            return _context.Users.AnyAsync(u => u.Id == userId);
        }

        public Task<bool> ExistsByUsernameAsync(string username, Guid? excludedUserId = null)
        {
            return _context.Users.AnyAsync(u =>
                u.Username == username &&
                (!excludedUserId.HasValue || u.Id != excludedUserId.Value));
        }

        public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedUserId = null)
        {
            return _context.Users.AnyAsync(u =>
                u.PhoneNumber == phoneNumber &&
                (!excludedUserId.HasValue || u.Id != excludedUserId.Value));
        }

        public Task AddAsync(User user)
        {
            return _context.Users.AddAsync(user).AsTask();
        }

        public void Remove(User user)
        {
            _context.Users.Remove(user);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
