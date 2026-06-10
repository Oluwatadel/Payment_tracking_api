namespace PaymentTracker.Models
{
    /// <summary>
    /// Represents a payment made by a user.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the payment identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the owning user's identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the date the payment was made.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the originating bank name.
        /// </summary>
        public required string BankName { get; set; }

        /// <summary>
        /// Gets or sets the optional payment reference number.
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the payment was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp when the payment was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Gets or sets the related user entity.
        /// </summary>
        public User? User { get; set; }
    }
}
