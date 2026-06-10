using Microsoft.EntityFrameworkCore;
using PaymentTracker.Data;
using PaymentTracker.Models;

namespace PaymentTracker.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int paymentId, bool tracking = false);
        Task<List<Payment>> SearchPaymentsAsync(DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount);
        Task<List<Payment>> GetByUserIdAsync(int userId);
        Task<List<Payment>> GetAllAsync();
        Task<decimal> SumByUserIdAsync(int userId);
        Task<bool> ExistsByIdAsync(int paymentId);
        Task AddAsync(Payment payment);
        void Remove(Payment payment);
        void RemoveRange(IEnumerable<Payment> payments);
        Task<int> RemoveByUserIdAsync(int userId);
        Task<int> SaveChangesAsync();
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int paymentId, bool tracking = false)
        {
            var query = _context.Payments.AsQueryable();
            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<List<Payment>> GetByUserIdAsync(int userId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> SumByUserIdAsync(int userId)
        {
            var total = await _context.Payments
                .Where(p => p.UserId == userId)
                .Select(p => (decimal?)p.Amount)
                .SumAsync();

            return total ?? 0m;
        }

        public Task<bool> ExistsByIdAsync(int paymentId)
        {
            return _context.Payments.AnyAsync(p => p.Id == paymentId);
        }

        public Task AddAsync(Payment payment)
        {
            return _context.Payments.AddAsync(payment).AsTask();
        }

        public void Remove(Payment payment)
        {
            _context.Payments.Remove(payment);
        }

        public void RemoveRange(IEnumerable<Payment> payments)
        {
            _context.Payments.RemoveRange(payments);
        }

        public Task<int> RemoveByUserIdAsync(int userId)
        {
            return _context.Payments
                .Where(p => p.UserId == userId)
                .ExecuteDeleteAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<List<Payment>> SearchPaymentsAsync(DateTime? startDate, DateTime? endDate, decimal? minAmount, decimal? maxAmount)
        {
            var query = _context.Payments.AsNoTracking().AsQueryable();
            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value);
            if (minAmount.HasValue)
                query = query.Where(p => p.Amount >= minAmount.Value);
            if (maxAmount.HasValue)
                query = query.Where(p => p.Amount <= maxAmount.Value);

            return query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        }

    }
}
