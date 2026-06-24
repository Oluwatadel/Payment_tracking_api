namespace PaymentTracker.DTOs
{
    public class CreatePaymentRequest
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public required string BankName { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    public class UpdatePaymentRequest
    {
        public Guid UserId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? BankName { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public required string BankName { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentHistoryResponse
    {
        public int PaymentCount { get; set; }
        public decimal TotalPaid { get; set; }
        public List<PaymentResponse> Payments { get; set; } = new();
    }
}
