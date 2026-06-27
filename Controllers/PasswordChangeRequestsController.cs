using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentTracker.DTOs;
using PaymentTracker.Services;
using System.Security.Claims;

namespace PaymentTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PasswordChangeRequestsController : ControllerBase
    {
        private readonly IPasswordChangeRequestService _service;

        public PasswordChangeRequestsController(IPasswordChangeRequestService service)
        {
            _service = service;
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }

        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _service.CreateForgotPasswordRequestAsync(request.Username);
            return Created(string.Empty, new PasswordChangeRequestResponse
            {
                Id = result.Id,
                UserId = result.UserId,
                Username = request.Username,
                Status = result.Status.ToString(),
                IsForgotPassword = true,
                CreatedAt = result.CreatedAt,
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreatePasswordChangeRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _service.CreateRequestAsync(userId.Value, request);
            return Created(string.Empty, new PasswordChangeRequestResponse
            {
                Id = result.Id,
                UserId = result.UserId,
                Username = string.Empty,
                Status = result.Status.ToString(),
                CreatedAt = result.CreatedAt,
                ReviewedAt = result.ReviewedAt,
            });
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<PasswordChangeRequestResponse>>> GetMyRequests()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var requests = await _service.GetUserRequestsAsync(userId.Value);
            return Ok(requests.Select(r => new PasswordChangeRequestResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = string.Empty,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.ReviewedAt,
            }));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<PasswordChangeRequestResponse>>> GetPendingRequests()
        {
            var requests = await _service.GetPendingRequestsAsync();
            return Ok(requests.Select(r => new PasswordChangeRequestResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = r.User?.Username ?? string.Empty,
                Status = r.Status.ToString(),
                IsForgotPassword = string.IsNullOrEmpty(r.NewPasswordHash),
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.ReviewedAt,
            }));
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(Guid id)
        {
            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Unauthorized();
            await _service.ApproveRequestAsync(id, adminId.Value);
            return NoContent();
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest(Guid id)
        {
            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Unauthorized();
            await _service.RejectRequestAsync(id, adminId.Value);
            return NoContent();
        }
    }
}
