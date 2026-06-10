namespace PaymentTracker.Models
{
    /// <summary>
    /// Defines the roles available to a user.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Standard application user.
        /// </summary>
        User,

        /// <summary>
        /// Administrative user with elevated permissions.
        /// </summary>
        Admin
    }

    /// <summary>
    /// Represents an application user and their profile details.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user's identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's username.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public required string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Gets or sets the UTC timestamp when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC timestamp when the user was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the related account entity.
        /// </summary>
        public Account? Account { get; set; }

        /// <summary>
        /// Gets or sets the payments associated with the user.
        /// </summary>
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
