using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentTracker.DTOs;
using PaymentTracker.Services;

namespace PaymentTracker.Controllers
{
    /// <summary>
    /// Authentication endpoints for user login and profile retrieval
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        /// <param name="request">Login credentials (username and password)</param>
        /// <returns>JWT token and user information on successful login</returns>
        /// <response code="200">Login successful, returns JWT token</response>
        /// <response code="400">Invalid request body</response>
        /// <response code="401">Invalid username or password</response>
        /// <response code="500">Server error</response>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required");

            var response = await _userService.LoginAsync(request);
            if (response == null)
                return Unauthorized("Invalid username or password");

            return Ok(response);
        }

        /// <summary>
        /// Get current authenticated user's profile information
        /// </summary>
        /// <returns>User profile details including username, phone, and role</returns>
        /// <response code="200">Returns user profile</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Server error</response>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponse>> GetProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("User not authenticated");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            });
        }
    }
}