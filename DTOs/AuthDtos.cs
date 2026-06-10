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
    }

    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
    }

    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Role { get; set; }
    }
}
