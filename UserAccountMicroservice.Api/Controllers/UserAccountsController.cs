using Microsoft.AspNetCore.Mvc;
using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.Entities;

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
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _service.GetAll();
        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var  account = await _service.GetById(id);
        if (account is null)
            return NotFound();
        return Ok(account);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] UserAccount account)
    {
        var success = await _service.Create(account);
        if(!success)
            return BadRequest();
        return Ok();
    }

    [HttpPut()]
    public async Task<IActionResult> Update([FromBody] UserAccount account, [FromHeader] int id)
    {
        var success = await _service.Update(account, id);
        if (!success)
            return  NotFound();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteById(int id, [FromHeader] int userId)
    {
        var success = await _service.DeleteById(id, userId);
        if (!success)
            return NotFound();
        return Ok();
    }
    
}