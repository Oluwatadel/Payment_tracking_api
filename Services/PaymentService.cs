using PaymentTracker.Data;
using PaymentTracker.DTOs;
using PaymentTracker.Exceptions;
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
        //Task UpdateUserBalanceAsync(Guid userId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<PaymentService> _logger;
        private readonly IUnitOfWork _uniOfWork;


        public PaymentService(
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _uniOfWork = unitOfWork;
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
            _logger.LogInformation("===Creating payment for user {UserId}===", userId);

            if(await _paymentRepository.ExistsByReferenceAsync(request.ReferenceNumber!))
            {
                _logger.LogInformation($"Payment with reference {request.ReferenceNumber} already exist");
                throw new AlreadyExistException($"Payment with reference {request.ReferenceNumber} already exist");
            }

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

            var userAccount = await _accountRepository.GetByUserIdAsync(userId, tracking: false)
                ?? throw new NotFoundException("User account not found");

            _logger.LogInformation("Adding payment amount to user balance");

            userAccount.AddPaymentToBalance(payment.Amount);

            _accountRepository.Update(userAccount);

            await _paymentRepository.AddAsync(payment);            

            //await UpdateUserBalanceAsync(userId);
            _logger.LogInformation("Adding payment amount to admin balance");
            var adminAccount = await _accountRepository.GetAdminAccount(tracking: false)
                ?? throw new NotFoundException("Admin account not found");

            adminAccount.AddPaymentToBalance(payment.Amount);

            _accountRepository.Update(adminAccount);


            var changes = await _uniOfWork.SaveChangesAsync();
            if (changes <= 0)
            {
                _logger.LogError("Error saving payment");
                throw new SaveOperationException($"Payment for user {userId} not saved..Error!!!");
            }

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

            var previousPaymentAmount = payment.Amount;

            payment.UpdatePaymentDetails(
                amount: request.Amount,
                paymentDate: request.PaymentDate,
                bankName: request.BankName,
                referenceNumber: request.ReferenceNumber
            );

            var adminAccount = await _accountRepository.GetAdminAccount(tracking: true)
                ?? throw new NotFoundException("Admin account not found");

            adminAccount.AddPaymentToBalance(previousPaymentAmount - payment.Amount);
            payment.UpdatedAt = DateTime.UtcNow;
            var changes = await _uniOfWork.SaveChangesAsync();

            if (changes <= 0)
            {
                _logger.LogInformation("Error updating payment");
                throw new SaveOperationException($"Payment {paymentId} not updated..Error!!!");
            }

            //await UpdateUserBalanceAsync(payment.UserId);

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

            var user = await _userRepository.GetByIdAsync(payment.UserId);
            if (user == null)
            {
                _logger.LogInformation("user cannot be found");
                throw new NotFoundException($"user for this payment was not found.");
            }

            _paymentRepository.Remove(payment);
            var userAccount = await _accountRepository.GetByUserIdAsync(user.Id)
                ?? throw new NotFoundException($"Account for user {user.Username} not found");
            userAccount.DeductPaymentFromBalance(payment.Amount);

            var adminAccount = await _accountRepository.GetAdminAccount(tracking: true);
            if (adminAccount != null)
            {
                _logger.LogInformation("=======================================================");
                _logger.LogInformation("Admin account issue");
                throw new NotFoundException("admin account issure");
            }

            adminAccount!.DeductPaymentFromBalance(payment.Amount);
            _accountRepository.Update(adminAccount);
            _accountRepository.Update(userAccount);

            await _uniOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} deleted", paymentId);

            return true;
        }

        public async Task<bool> ClearUserPaymentsAsync(Guid userId)
        {
            _logger.LogInformation($"+++++++++++++++++++++++{DateTime.UtcNow.ToShortDateString()}+++++++++++++++++++++++++++++++++++++++++");
            _logger.LogInformation("Clearing payments for user {UserId}", userId);

            var userPayment = await _paymentRepository.GetByUserIdAsync(userId);
            if (!userPayment.Any())
            {
                _logger.LogInformation("No payment found");
                throw new NotFoundException("No payment found");
            }


            var totalAmount = await _paymentRepository.SumByUserIdAsync(userId);
            //_paymentRepository.RemoveRange(userPayment);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogInformation("user cannot be found");
                throw new NotFoundException($"user for this payment was not found.");
            }

            var userAccount = await _accountRepository.GetByUserIdAsync(userId)
                ?? throw new NotFoundException($"Account for user {user.Username} does not exist");

            userAccount.DeductPaymentFromBalance(totalAmount);

            var adminAccount = await _accountRepository.GetAdminAccount(tracking: false);
            if (adminAccount == null)
            {
                _logger.LogInformation("===========================================================================");
                _logger.LogInformation("Admin account not found");
                throw new NotFoundException("admin account issue");
            }

            var removedCount = await _paymentRepository.RemoveByUserIdAsync(userId);
            if (removedCount == 0)
            {
                _logger.LogWarning("No payments found to clear for user {UserId}", userId);
                return false;
            }
                       


            adminAccount!.DeductPaymentFromBalance(totalAmount);

            _accountRepository.Update(adminAccount);
            _accountRepository.Update(userAccount);

            var changes = await _uniOfWork.SaveChangesAsync();
            if(changes <= 0)
            {
                _logger.LogError("Error saving deduction ater clearing user payment");
                throw new SaveOperationException("Error saving deduction ater clearing user payment");
            }

            _logger.LogInformation("Cleared {Count} payments for user {UserId}", removedCount, userId);

            return true;
        }

        private async Task UpdateUserBalanceAsync(Guid userId)
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
