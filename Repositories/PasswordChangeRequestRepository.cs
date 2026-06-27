using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;

namespace PaymentTracker.Repositories
{
    public interface IPasswordChangeRequestRepository
    {
        Task<List<PasswordChangeRequest>> GetAllPendingAsync();
        Task<List<PasswordChangeRequest>> GetByUserIdAsync(Guid userId);
        Task<PasswordChangeRequest?> GetByIdAsync(Guid id, bool tracking = false);
        Task AddAsync(PasswordChangeRequest request);
        Task<int> SaveChangesAsync();
    }

    public class PasswordChangeRequestRepository : IPasswordChangeRequestRepository
    {
        private readonly AppDbContext _context;

        public PasswordChangeRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PasswordChangeRequest>> GetAllPendingAsync()
        {
            return await _context.PasswordChangeRequests
                .Include(r => r.User)
                .Where(r => r.Status == PasswordChangeRequestStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PasswordChangeRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PasswordChangeRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<PasswordChangeRequest?> GetByIdAsync(Guid id, bool tracking = false)
        {
            var query = _context.PasswordChangeRequests.Include(r => r.User).AsQueryable();
            if (!tracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(PasswordChangeRequest request)
        {
            await _context.PasswordChangeRequests.AddAsync(request);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
