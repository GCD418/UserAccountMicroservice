using Microsoft.AspNetCore.Mvc;
using UserAccountMicroservice.Application.Services;

namespace UserAccountMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController : ControllerBase
{
    private readonly UserAccountService _service;

    public UserAccountsController(UserAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserAccounts()
    {
        var accounts = await _service.GetAll();
        return Ok(accounts);
    }
    
}