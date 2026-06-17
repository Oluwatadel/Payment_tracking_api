using Microsoft.Extensions.Logging;
using PaymentTracker.DTOs;
using PaymentTracker.Exceptions;
using PaymentTracker.Models;
using PaymentTracker.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentTracker.Services
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(NotificationCreateRequest request);
        Task<List<NotificationResponse>> GetActiveNotificationsAsync();
        Task<List<NotificationResponse>> GetAllNotificationsAsync();
        Task<bool> DeleteNotificationAsync(Guid id);
        Task<int> CleanupExpiredNotificationsAsync();
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<NotificationResponse> CreateNotificationAsync(NotificationCreateRequest request)
        {
            _logger.LogInformation("Creating new notification: {Title}", request.Title);

            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationInMinutes),
                IsActive = true
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification created with ID: {Id}", notification.Id);

            return MapToResponse(notification);
        }

        public async Task<List<NotificationResponse>> GetActiveNotificationsAsync()
        {
            var notifications = await _notificationRepository.GetAllActiveAsync();
            return notifications.Select(MapToResponse).ToList();
        }

        public async Task<List<NotificationResponse>> GetAllNotificationsAsync()
        {
            var notifications = await _notificationRepository.GetAllAsync();
            return notifications.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeleteNotificationAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id, tracking: true);
            if (notification == null)
            {
                throw new NotFoundException("Notification not found");
            }

            _notificationRepository.Remove(notification);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> CleanupExpiredNotificationsAsync()
        {
            _logger.LogInformation("Cleaning up expired notifications...");
            var expired = await _notificationRepository.GetExpiredAsync();
            
            if (expired.Any())
            {
                _logger.LogInformation("Found {Count} expired notifications to delete", expired.Count);
                _notificationRepository.RemoveRange(expired);
                await _unitOfWork.SaveChangesAsync();
            }

            return expired.Count;
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                ExpiresAt = notification.ExpiresAt,
                IsActive = notification.IsActive
            };
        }
    }
}
