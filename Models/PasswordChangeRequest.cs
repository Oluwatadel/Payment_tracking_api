namespace PaymentTracker.Models
{
    public enum PasswordChangeRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class PasswordChangeRequest : Auditable
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string NewPasswordHash { get; set; } = string.Empty;
        public PasswordChangeRequestStatus Status { get; set; } = PasswordChangeRequestStatus.Pending;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
    }
}
