using System;

namespace PaymentTracker.DTOs
{
    public class NotificationCreateRequest
    {
        public required string Title { get; set; }
        public required string Message { get; set; }
        public int DurationInMinutes { get; set; }
    }

    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
