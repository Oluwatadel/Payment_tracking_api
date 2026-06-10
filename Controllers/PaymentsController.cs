using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentTracker.DTOs;
using PaymentTracker.Services;
using System.Security.Claims;

namespace PaymentTracker.Controllers
{
    /// <summary>
    /// Payment management endpoints for viewing and managing payment records
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Get all payments (admin only) or user's own payments (user)
        /// </summary>
        /// <returns>List of payment records</returns>
        /// <response code="200">Returns list of payments</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">Forbidden - insufficient permissions</response>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<PaymentResponse>>> GetAllPayments()
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Get specific payment
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(Guid id)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                    return NotFound("Payment not found");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Add payment for a user
        [HttpPost("user/{userId}")]
        public async Task<ActionResult<PaymentResponse>> AddPaymentForUser(Guid userId, [FromBody] CreatePaymentRequest request)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var payment = await _paymentService.AddPaymentAsync(userId, request);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Update payment
        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentResponse>> UpdatePayment(Guid id, [FromBody] UpdatePaymentRequest request)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var payment = await _paymentService.UpdatePaymentAsync(id, request);
                if (payment == null)
                    return NotFound("Payment not found");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Delete payment
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var success = await _paymentService.DeletePaymentAsync(id);
                if (!success)
                    return NotFound("Payment not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Clear user payments
        [HttpPost("user/{userId}/clear")]
        public async Task<IActionResult> ClearUserPayments(Guid userId)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var success = await _paymentService.ClearUserPaymentsAsync(userId);
                if (!success)
                    return NotFound("No payments found for user");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
