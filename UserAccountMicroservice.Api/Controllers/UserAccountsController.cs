using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccountMicroservice.Api.DTOs;
using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.Entities;
using UserAccountMicroservice.Domain.Services.Validations;

namespace UserAccountMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController : ControllerBase
{
    private readonly UserAccountService _service;
    private readonly IValidator<UserAccount> _validator;

    public UserAccountsController(UserAccountService service, IValidator<UserAccount> validator)
    {
        _service = service;
        _validator = validator;
    }

    [Authorize(Roles = "CEO")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _service.GetAll();
        return Ok(accounts);
    }

    [Authorize(Roles = "CEO")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var  account = await _service.GetById(id);
        if (account is null)
            return NotFound();
        return Ok(account);
    }

    [Authorize(Roles = "CEO")]
    [HttpPost("create")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserAccount account)
    {
        var validationResult = _validator.Validate(account);
        
        if (validationResult.IsFailure)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validación fallida",
                Errors = validationResult.Errors
            });
        }

        var success = await _service.Create(account);
        
        if (!success)
        {
            return BadRequest(new { message = "No se pudo crear el usuario. Posiblemente el nombre de usuario ya existe." });
        }
        
        return Ok(new SuccessResponse
        {
            Message = "Usuario creado exitosamente",
            Id = account.Id
        });
    }

    [Authorize(Roles = "CEO")]
    [HttpPut()]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UserAccount account, [FromHeader] int userId)
    {
        var existingAccount = await _service.GetById(account.Id);
        if (existingAccount == null)
        {
            return NotFound(new { message = $"Usuario con ID {account.Id} no encontrado" });
        }

        var validationResult = _validator.Validate(account);
        
        if (validationResult.IsFailure)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validación fallida",
                Errors = validationResult.Errors
            });
        }

        var success = await _service.Update(account, userId);
        
        if (!success)
        {
            return StatusCode(500, new { message = "Error al actualizar el usuario" });
        }
        
        return Ok(new SuccessResponse
        {
            Message = "Usuario actualizado exitosamente",
            Id = account.Id
        });
    }

    [Authorize(Roles = "CEO")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteById(int id, [FromHeader] int userId)
    {
        var success = await _service.DeleteById(id, userId);
        if (!success)
            return NotFound();
        return Ok();
    }
    
}