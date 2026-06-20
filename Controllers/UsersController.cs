using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentTracker.DTOs;
using PaymentTracker.Services;
using System.Security.Claims;

namespace PaymentTracker.Controllers
{
    /// <summary>
    /// User management endpoints for profiles, accounts, and payments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IPaymentService _paymentService;

        public UsersController(IUserService userService, IAccountService accountService, IPaymentService paymentService)
        {
            _userService = userService;
            _accountService = accountService;
            _paymentService = paymentService;
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Get current authenticated user's profile
        /// </summary>
        /// <returns>Current user's profile information</returns>
        /// <response code="200">Returns user profile</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">User not found</response>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponse>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var user = await _userService.GetUserByIdAsync(userId.Value);
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


        [HttpPost("me/fcm-token")]
        [Authorize]
        public IActionResult UpdateFcmToken([FromBody] FcmTokenUpdateRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            // Token persistence can be added with a UserDevice table later.
            // For now this endpoint prevents mobile token registration from failing.
            return NoContent();
        }
        // User: Get own account
        [HttpGet("me/account")]
        public async Task<ActionResult<AccountResponse>> GetCurrentUserAccount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var account = await _accountService.GetAccountByUserIdAsync(userId.Value);
            if (account == null)
                return NotFound("Account not found");

            return Ok(new AccountResponse
            {
                Id = account.Id,
                UserId = account.UserId,
                BankName = account.BankName,
                AccountNumber = account.AccountNumber,
                AccountHolder = account.AccountHolder,
                Balance = account.Balance
            });
        }

        // User: Create own account
        //[HttpPost("me/account")]
        //public async Task<ActionResult<AccountResponse>> CreateCurrentUserAccount([FromBody] CreateAccountRequest request)
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        if (!userId.HasValue)
        //            return Unauthorized("User not authenticated");

        //        var account = await _accountService.CreateAccountAsync(userId.Value, request);
        //        return CreatedAtAction(nameof(GetCurrentUserAccount), new AccountResponse
        //        {
        //            Id = account.Id,
        //            UserId = account.UserId,
        //            BankName = account.BankName,
        //            AccountNumber = account.AccountNumber,
        //            AccountHolder = account.AccountHolder,
        //            Balance = account.Balance
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}

        // User: Update own account
        //[HttpPut("me/account/update")]
        //public async Task<ActionResult<AccountResponse>> UpdateCurrentUserAccount([FromBody] UpdateAccountRequest request)
        //{
        //    var userId = GetCurrentUserId();
        //    if (!userId.HasValue)
        //        return Unauthorized("User not authenticated");

        //    var account = await _accountService.UpdateAccountAsync(userId.Value, request);
        //    if (account == null)
        //        return NotFound("Account not found");

        //    return Ok(new AccountResponse
        //    {
        //        Id = account.Id,
        //        UserId = account.UserId,
        //        BankName = account.BankName,
        //        AccountNumber = account.AccountNumber,
        //        AccountHolder = account.AccountHolder,
        //        Balance = account.Balance
        //    });
        //}

        // User: Get own payment history
        [HttpGet("me/payments")]
        public async Task<ActionResult<PaymentHistoryResponse>> GetCurrentUserPayments()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var history = await _paymentService.GetUserPaymentHistoryAsync(userId.Value);
            return Ok(history);
        }

        // User: Add own payment
        [HttpPost("me/payments")]
        public async Task<ActionResult<PaymentResponse>> AddCurrentUserPayment([FromBody] CreatePaymentRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized("User not authenticated");

            var payment = await _paymentService.AddPaymentAsync(userId.Value, request);
            return CreatedAtAction(nameof(AddCurrentUserPayment), payment);
        }

        // Admin: Get all users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserProfileResponse>>> GetAllUsers()
        {
            var role = GetCurrentUserRole();
            if (role != "Admin")
                return Forbid();

            var users = await _userService.GetAllUsersAsync();
            var responses = users.Select(u => new UserProfileResponse
            {
                Id = u.Id,
                Username = u.Username,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.ToString()
            }).ToList();

            return Ok(responses);
        }

        // Admin: Create user
        [HttpPost]
        public async Task<ActionResult<UserProfileResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            var role = GetCurrentUserRole();
            if (role != "Admin")
                return Forbid();

            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            });
        }

        // Admin: Get specific user
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileResponse>> GetUser(Guid id)
        {
            var role = GetCurrentUserRole();
            if (role != "Admin")
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
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

        // Admin: Get specific user's account
        //[HttpGet("{id}/account")]
        //public async Task<ActionResult<AccountResponse>> GetUserAccount(Guid id)
        //{
        //    var role = GetCurrentUserRole();
        //    if (role != "Admin")
        //        return Forbid();

        //    // Verify user exists
        //    var user = await _userService.GetUserByIdAsync(id);
        //    if (user == null)
        //        return NotFound("User not found");

        //    var account = await _accountService.GetAccountByUserIdAsync(id);
        //    if (account == null)
        //        return NotFound("Account not found for this user");

        //    return Ok(new AccountResponse
        //    {
        //        Id = account.Id,
        //        UserId = account.UserId,
        //        BankName = account.BankName,
        //        AccountNumber = account.AccountNumber,
        //        AccountHolder = account.AccountHolder,
        //        Balance = account.Balance
        //    });
        //}

        // Admin: Update user
        [HttpPut("{id}")]
        public async Task<ActionResult<UserProfileResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var role = GetCurrentUserRole();
                if (role != "Admin")
                    return Forbid();

                var user = await _userService.UpdateUserAsync(id, request);
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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Admin: Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var role = GetCurrentUserRole();
            if (role != "Admin")
                return Forbid();

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound("User not found");

            return NoContent();
        }
    }
}

