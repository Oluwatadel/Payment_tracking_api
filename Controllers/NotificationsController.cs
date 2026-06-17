using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentTracker.DTOs;
using PaymentTracker.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets all active notifications.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<NotificationResponse>>> GetActiveNotifications()
        {
            var notifications = await _notificationService.GetActiveNotificationsAsync();
            return Ok(notifications);
        }

        /// <summary>
        /// Gets all notifications (Admin only).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<NotificationResponse>>> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotificationsAsync();
            return Ok(notifications);
        }

        /// <summary>
        /// Creates a new notification (Admin only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] NotificationCreateRequest request)
        {
            var response = await _notificationService.CreateNotificationAsync(request);
            return CreatedAtAction(nameof(GetActiveNotifications), new { id = response.Id }, response);
        }

        /// <summary>
        /// Deletes a notification (Admin only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }
    }
}
