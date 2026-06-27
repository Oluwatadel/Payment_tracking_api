namespace PaymentTracker.DTOs
{
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public required string Token { get; set; }
        public required string Role { get; set; }
    }

    public class CreateUserRequest
    {
        public required string Username { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }
        public string? AccountNo { get; set; }
        public string? BankName { get; set; }
        public string AccountHolder { get; set; } = default!;
    }

    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public string? AccountNo { get; set; }
        public string? BankName { get; set; }
        public string? AccountHolder { get; set; }
    }

    public class FcmTokenUpdateRequest
    {
        public required string FcmToken { get; set; }
    }

    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    public class AdminResetPasswordRequest
    {
        public required string NewPassword { get; set; }
    }

    public class UserStatsResponse
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public decimal TotalEverProcessed { get; set; }
        public decimal ActiveBalance { get; set; }
    }
}

