using System;

namespace PaymentTracker.Models
{
    /// <summary>
    /// Represents a notification created by an admin.
    /// </summary>
    public class Notification : Auditable
    {
        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets the message content of the notification.
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the notification expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets whether the notification is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
