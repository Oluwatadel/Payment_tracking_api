using BC = BCrypt.Net.BCrypt;
using PaymentTracker.DTOs;
using PaymentTracker.Models;
using PaymentTracker.Repositories;
using PaymentTracker.Exceptions;

namespace PaymentTracker.Services
{
    public interface IPasswordChangeRequestService
    {
        Task<PasswordChangeRequest> CreateRequestAsync(Guid userId, CreatePasswordChangeRequest request);
        Task<PasswordChangeRequest> CreateForgotPasswordRequestAsync(string username);
        Task<List<PasswordChangeRequest>> GetPendingRequestsAsync();
        Task<List<PasswordChangeRequest>> GetUserRequestsAsync(Guid userId);
        Task ApproveRequestAsync(Guid requestId, Guid adminId);
        Task RejectRequestAsync(Guid requestId, Guid adminId);
    }

    public class PasswordChangeRequestService : IPasswordChangeRequestService
    {
        private readonly IPasswordChangeRequestRepository _repository;
        private readonly IUserRepository _userRepository;

        public PasswordChangeRequestService(
            IPasswordChangeRequestRepository repository,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<PasswordChangeRequest> CreateRequestAsync(Guid userId, CreatePasswordChangeRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId, tracking: true);
            if (user == null) throw new NotFoundException("User not found");

            if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Current password is incorrect");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new InvalidOperationException("New password must be at least 6 characters");

            var changeRequest = new PasswordChangeRequest
            {
                UserId = userId,
                NewPasswordHash = BC.HashPassword(request.NewPassword),
                Status = PasswordChangeRequestStatus.Pending,
            };

            await _repository.AddAsync(changeRequest);
            await _repository.SaveChangesAsync();
            return changeRequest;
        }

        public async Task<List<PasswordChangeRequest>> GetPendingRequestsAsync()
        {
            return await _repository.GetAllPendingAsync();
        }

        public async Task<List<PasswordChangeRequest>> GetUserRequestsAsync(Guid userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        public async Task<PasswordChangeRequest> CreateForgotPasswordRequestAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null) throw new NotFoundException("No account found with that username");

            var changeRequest = new PasswordChangeRequest
            {
                UserId = user.Id,
                NewPasswordHash = string.Empty,
                Status = PasswordChangeRequestStatus.Pending,
            };

            await _repository.AddAsync(changeRequest);
            await _repository.SaveChangesAsync();
            return changeRequest;
        }

        public async Task ApproveRequestAsync(Guid requestId, Guid adminId)
        {
            var changeRequest = await _repository.GetByIdAsync(requestId, tracking: true);
            if (changeRequest == null) throw new NotFoundException("Request not found");
            if (changeRequest.Status != PasswordChangeRequestStatus.Pending)
                throw new InvalidOperationException("Request is no longer pending");

            // For forgot-password requests the admin resets via adminResetPassword separately;
            // here we just mark it resolved without touching the password hash.
            if (!string.IsNullOrEmpty(changeRequest.NewPasswordHash))
            {
                var user = await _userRepository.GetByIdAsync(changeRequest.UserId, tracking: true);
                if (user == null) throw new NotFoundException("User not found");
                user.PasswordHash = changeRequest.NewPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
            }

            changeRequest.Status = PasswordChangeRequestStatus.Approved;
            changeRequest.ReviewedAt = DateTime.UtcNow;
            changeRequest.ReviewedBy = adminId;

            await _repository.SaveChangesAsync();
        }

        public async Task RejectRequestAsync(Guid requestId, Guid adminId)
        {
            var changeRequest = await _repository.GetByIdAsync(requestId, tracking: true);
            if (changeRequest == null) throw new NotFoundException("Request not found");
            if (changeRequest.Status != PasswordChangeRequestStatus.Pending)
                throw new InvalidOperationException("Request is no longer pending");

            changeRequest.Status = PasswordChangeRequestStatus.Rejected;
            changeRequest.ReviewedAt = DateTime.UtcNow;
            changeRequest.ReviewedBy = adminId;

            await _repository.SaveChangesAsync();
        }
    }
}
