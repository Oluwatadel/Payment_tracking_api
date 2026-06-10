namespace PaymentTracker.Models
{
    /// <summary>
    /// Represents a user's bank account details and balance.
    /// </summary>
    public class Account : Auditable
    {
        /// <summary>
        /// Gets or sets the owning user's identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the bank name.
        /// </summary>
        public required string BankName { get; set; }

        /// <summary>
        /// Gets or sets the bank account number.
        /// </summary>
        public required string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the account holder's name.
        /// </summary>
        public required string AccountHolder { get; set; } = default!;

        /// <summary>
        /// Gets or sets the current account balance.
        /// </summary>
        public decimal Balance { get; set; } = 0;

        // Navigation properties
        /// <summary>
        /// Gets or sets the related user entity.
        /// </summary>
        public User? User { get; set; }
    }
}
