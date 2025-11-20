using Microsoft.AspNetCore.Mvc;
using UserAccountMicroservice.Application.Facades;
using UserAccountMicroservice.Domain.DTOs;

namespace UserAccountMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthFacade _authFacade;

    public AuthController(AuthFacade authFacade)
    {
        _authFacade = authFacade;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        var response = await _authFacade.Login(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(response);
    }
}
