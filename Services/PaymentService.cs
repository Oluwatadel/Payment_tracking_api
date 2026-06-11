using PaymentTracker.Exceptions;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;

namespace PaymentTracker.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> GetPaymentByIdAsync(Guid paymentId);
        Task<PaymentHistoryResponse> GetUserPaymentHistoryAsync(Guid userId);
        Task<List<PaymentResponse>> GetAllPaymentsAsync();
        Task<List<PaymentResponse>> GetUserPaymentsAsync(Guid userId);
        Task<PaymentResponse> AddPaymentAsync(Guid userId, CreatePaymentRequest request);
        Task<PaymentResponse> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequest request);
        Task<bool> DeletePaymentAsync(Guid paymentId);
        Task<bool> ClearUserPaymentsAsync(Guid userId);
        Task UpdateUserBalanceAsync(Guid userId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(Guid paymentId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");

            _logger.LogInformation("Fetching payment {PaymentId}", paymentId);

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} was not found", paymentId);
                throw new NotFoundException("Payment not found");
            }

            _logger.LogInformation("Payment {PaymentId} found", paymentId);
            return MapToResponse(payment);
        }

        public async Task<PaymentHistoryResponse> GetUserPaymentHistoryAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching payment history for user {UserId}", userId);
            var payments = await _paymentRepository.GetByUserIdAsync(userId);

            var totalPaid = payments.Sum(p => p.Amount);
            var paymentResponses = payments.Select(MapToResponse).ToList();

            _logger.LogInformation("Payment history fetched for user {UserId}. Total payments: {Count}, Total paid: {TotalPaid}", userId, payments.Count, totalPaid);
            return new PaymentHistoryResponse
            {
                PaymentCount = payments.Count,
                TotalPaid = totalPaid,
                Payments = paymentResponses
            };
        }

        public async Task<List<PaymentResponse>> GetAllPaymentsAsync()
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching all payments");
            var payments = await _paymentRepository.GetAllAsync();
            _logger.LogInformation($"Total payments found: {payments.Count} with sum {payments.Sum(p => p.Amount)}");
            return payments.Select(MapToResponse).ToList();
        }

        public async Task<List<PaymentResponse>> GetUserPaymentsAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Fetching all payments for user {UserId}", userId);
            var payments = await _paymentRepository.GetByUserIdAsync(userId);
            _logger.LogInformation($"Total payments found for user {userId}: {payments.Count} with sum {payments.Sum(p => p.Amount)}");
            return payments.Select(MapToResponse).ToList();
        }

        public async Task<PaymentResponse> AddPaymentAsync(Guid userId, CreatePaymentRequest request)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Creating payment for user {UserId}", userId);

            if (!await _userRepository.ExistsByIdAsync(userId))
            {
                _logger.LogWarning("Cannot create payment. User {UserId} was not found", userId);
                throw new NotFoundException("User not found");
            }

            var payment = new Payment
            {
                UserId = userId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                BankName = request.BankName,
                ReferenceNumber = request.ReferenceNumber
            };

            await _paymentRepository.AddAsync(payment);
            await UpdateUserBalanceAsync(userId);

            await _paymentRepository.SaveChangesAsync();


            _logger.LogInformation("Payment created for user {UserId}", userId);

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(Guid paymentId, UpdatePaymentRequest request)
        {
            _logger.LogInformation("Updating payment {PaymentId}", paymentId);

            var payment = await _paymentRepository.GetByIdAsync(paymentId, tracking: true);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} was not found for update", paymentId);
                throw new KeyNotFoundException("Payment not found");
            }

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

            _logger.LogInformation("Payment {PaymentId} updated", paymentId);

            return MapToResponse(payment);
        }

        public async Task<bool> DeletePaymentAsync(Guid paymentId)
        {
            _logger.LogInformation("Deleting payment {PaymentId}", paymentId);

            var payment = await _paymentRepository.GetByIdAsync(paymentId, tracking: true);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} was not found for deletion", paymentId);
                throw new NotFoundException("Payment not found");
            }

            var userId = payment.UserId;
            _paymentRepository.Remove(payment);
            await _paymentRepository.SaveChangesAsync();

            await UpdateUserBalanceAsync(userId);

            _logger.LogInformation("Payment {PaymentId} deleted", paymentId);

            return true;
        }

        public async Task<bool> ClearUserPaymentsAsync(Guid userId)
        {
            _logger.LogInformation("Clearing payments for user {UserId}", userId);

            var removedCount = await _paymentRepository.RemoveByUserIdAsync(userId);
            if (removedCount == 0)
            {
                _logger.LogWarning("No payments found to clear for user {UserId}", userId);
                return false;
            }

            await UpdateUserBalanceAsync(userId);

            _logger.LogInformation("Cleared {Count} payments for user {UserId}", removedCount, userId);

            return true;
        }

        public async Task UpdateUserBalanceAsync(Guid userId)
        {
            _logger.LogInformation($"=========================={DateTime.Now:dd-MM-yyyy, HH:mm:ss}===============================");
            _logger.LogInformation("Updating balance for user {UserId}", userId);

            var account = await _accountRepository.GetByUserIdAsync(userId, tracking: true)
                ?? throw new NotFoundException("User account not found");

            var totalPaid = await _paymentRepository.SumByUserIdAsync(userId);

            if (totalPaid == account.Balance)
            {
                _logger.LogInformation("Balance for user {UserId} is already up to date", userId);
                throw new InvalidOperationException("Balance is already up to date");
            }

            account.AddPaymentToBalance(totalPaid - account.Balance);
            _logger.LogInformation("Balance updated for user {UserId}. New balance: {Balance}", userId, account.Balance);

            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.SaveChangesAsync();
            _logger.LogInformation("Balance updated for user {UserId}", userId);
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
