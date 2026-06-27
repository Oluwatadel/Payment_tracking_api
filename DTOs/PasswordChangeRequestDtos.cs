namespace PaymentTracker.DTOs
{
    public class CreatePasswordChangeRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Username { get; set; }
    }

    public class PasswordChangeRequestResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsForgotPassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
