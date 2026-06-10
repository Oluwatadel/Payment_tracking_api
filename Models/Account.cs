namespace PaymentTracker.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
    }
}
