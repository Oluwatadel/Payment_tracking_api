using PaymentTracker.Data;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace PaymentTracker.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId);
        Task<PaymentHistoryResponse> GetUserPaymentHistoryAsync(int userId);
        Task<List<PaymentResponse>> GetAllPaymentsAsync();
        Task<PaymentResponse> AddPaymentAsync(int userId, CreatePaymentRequest request);
        Task<PaymentResponse?> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<bool> ClearUserPaymentsAsync(int userId);
        Task UpdateUserBalanceAsync(int userId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return null;

            return MapToResponse(payment);
        }

        public async Task<PaymentHistoryResponse> GetUserPaymentHistoryAsync(int userId)
        {
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var totalPaid = payments.Sum(p => p.Amount);
            var paymentResponses = payments.Select(MapToResponse).ToList();

            return new PaymentHistoryResponse
            {
                PaymentCount = payments.Count,
                TotalPaid = totalPaid,
                Payments = paymentResponses
            };
        }

        public async Task<List<PaymentResponse>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(MapToResponse).ToList();
        }

        public async Task<PaymentResponse> AddPaymentAsync(int userId, CreatePaymentRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var payment = new Payment
            {
                UserId = userId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                BankName = request.BankName,
                ReferenceNumber = request.ReferenceNumber
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Update balance
            await UpdateUserBalanceAsync(userId);

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse?> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return null;

            if (request.Amount.HasValue)
                payment.Amount = request.Amount.Value;

            if (request.PaymentDate.HasValue)
                payment.PaymentDate = request.PaymentDate.Value;

            if (!string.IsNullOrEmpty(request.BankName))
                payment.BankName = request.BankName;

            if (!string.IsNullOrEmpty(request.ReferenceNumber))
                payment.ReferenceNumber = request.ReferenceNumber;

            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Update balance
            await UpdateUserBalanceAsync(payment.UserId);

            return MapToResponse(payment);
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return false;

            var userId = payment.UserId;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            // Update balance
            await UpdateUserBalanceAsync(userId);

            return true;
        }

        public async Task<bool> ClearUserPaymentsAsync(int userId)
        {
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (payments.Count == 0)
                return false;

            _context.Payments.RemoveRange(payments);
            await _context.SaveChangesAsync();

            // Reset balance
            await UpdateUserBalanceAsync(userId);

            return true;
        }

        public async Task UpdateUserBalanceAsync(int userId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                return;

            var totalPaid = await _context.Payments
                .Where(p => p.UserId == userId)
                .SumAsync(p => p.Amount);

            account.Balance = totalPaid;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                UserId = payment.UserId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                BankName = payment.BankName,
                ReferenceNumber = payment.ReferenceNumber,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
