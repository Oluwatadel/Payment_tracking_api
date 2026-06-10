using PaymentTracker.Data;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

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
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IAccountRepository accountRepository)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return null;

            return MapToResponse(payment);
        }

        public async Task<PaymentHistoryResponse> GetUserPaymentHistoryAsync(int userId)
        {
            var payments = await _paymentRepository.GetByUserIdAsync(userId);

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
            var payments = await _paymentRepository.GetAllAsync();

            return payments.Select(MapToResponse).ToList();
        }

        public async Task<PaymentResponse> AddPaymentAsync(int userId, CreatePaymentRequest request)
        {
            if (!await _userRepository.ExistsByIdAsync(userId))
                throw new InvalidOperationException("User not found");

            var payment = new Payment
            {
                UserId = userId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                BankName = request.BankName,
                ReferenceNumber = request.ReferenceNumber
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            await UpdateUserBalanceAsync(userId);

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse?> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, tracking: true);
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
            await _paymentRepository.SaveChangesAsync();

            await UpdateUserBalanceAsync(payment.UserId);

            return MapToResponse(payment);
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, tracking: true);
            if (payment == null)
                return false;

            var userId = payment.UserId;
            _paymentRepository.Remove(payment);
            await _paymentRepository.SaveChangesAsync();

            await UpdateUserBalanceAsync(userId);

            return true;
        }

        public async Task<bool> ClearUserPaymentsAsync(int userId)
        {
            var removedCount = await _paymentRepository.RemoveByUserIdAsync(userId);
            if (removedCount == 0)
                return false;

            await UpdateUserBalanceAsync(userId);

            return true;
        }

        public async Task UpdateUserBalanceAsync(int userId)
        {
            var account = await _accountRepository.GetByUserIdAsync(userId, tracking: true);
            if (account == null)
                return;

            var totalPaid = await _paymentRepository.SumByUserIdAsync(userId);

            account.Balance = totalPaid;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.SaveChangesAsync();
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
