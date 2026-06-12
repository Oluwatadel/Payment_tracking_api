namespace PaymentTracker.Models
{
    /// <summary>
    /// Represents a payment made by a user.
    /// </summary>
    public class Payment : Auditable
    {
        

        /// <summary>
        /// Gets or sets the owning user's identifier.
        /// </summary>
        public Guid UserId { get; set; }

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

        // Navigation properties
        /// <summary>
        /// Gets or sets the related user entity.
        /// </summary>
        public User? User { get; set; }

        public void UpdatePaymentDetails(decimal? amount, DateTime? paymentDate, string? bankName, string? referenceNumber)
        {
            if(amount.HasValue)
                Amount = amount.Value;
            if(paymentDate.HasValue)
                PaymentDate = paymentDate.Value;
            if(!string.IsNullOrEmpty(bankName))
                BankName = bankName;
            if(!string.IsNullOrEmpty(referenceNumber))
                ReferenceNumber = referenceNumber;
        }
    }
}
