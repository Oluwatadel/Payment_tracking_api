using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentTracker.Services;

namespace PaymentTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAccountByUserId([FromRoute] Guid id)
        {
            var account = await _accountService.GetAccountByUserIdAsync(id);
             return Ok(account);
        }
    }
}
