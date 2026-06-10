namespace PaymentTracker.DTOs
{
    public class CreateAccountRequest
    {
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
    }

    public class UpdateAccountRequest
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
    }

    public class AccountResponse
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public required string BankName { get; set; }
        public required string AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        public decimal Balance { get; set; }
    }
}
