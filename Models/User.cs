namespace PaymentTracker.Models
{
    public enum UserRole
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PhoneNumber { get; set; }
        public required string PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Account? Account { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
