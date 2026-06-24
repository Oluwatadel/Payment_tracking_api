using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;

namespace PaymentTracker.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, bool tracking = false);
        Task<User?> GetByUsernameAsync(string username, bool tracking = false);
        Task<List<User>> GetAllAsync(bool? isActive = null, string? search = null);
        Task<bool> ExistsByIdAsync(Guid userId);
        Task<bool> ExistsByUsernameAsync(string username, Guid? excludedUserId = null);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, Guid? excludedUserId = null);
        Task AddAsync(User user);
        void Deactivate(User user);
        void Reactivate(User user);
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

        public async Task<List<User>> GetAllAsync(bool? isActive = null, string? search = null)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.PhoneNumber.Contains(search));

            return await query.OrderBy(u => u.Username).ToListAsync();
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

        public void Deactivate(User user)
        {
            user.IsActive = false;
            user.DeactivatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
        }

        public void Reactivate(User user)
        {
            user.IsActive = true;
            user.DeactivatedAt = null;
            _context.Users.Update(user);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
