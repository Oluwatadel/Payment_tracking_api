using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentTracker.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id, bool tracking = false);
        Task<List<Notification>> GetAllActiveAsync();
        Task<List<Notification>> GetAllAsync();
        Task<List<Notification>> GetExpiredAsync();
        Task AddAsync(Notification notification);
        void Remove(Notification notification);
        void RemoveRange(IEnumerable<Notification> notifications);
        Task<int> SaveChangesAsync();
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id, bool tracking = false)
        {
            var query = _context.Notifications.AsQueryable();
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<Notification>> GetAllActiveAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.IsActive && n.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetExpiredAsync()
        {
            return await _context.Notifications
                .Where(n => n.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public void Remove(Notification notification)
        {
            _context.Notifications.Remove(notification);
        }

        public void RemoveRange(IEnumerable<Notification> notifications)
        {
            _context.Notifications.RemoveRange(notifications);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
