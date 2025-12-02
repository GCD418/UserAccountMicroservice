using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccountMicroservice.Api.DTOs;
using UserAccountMicroservice.Application.Facades;
using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.DTOs;
using UserAccountMicroservice.Domain.Entities;
using UserAccountMicroservice.Domain.Ports;
using UserAccountMicroservice.Domain.Services.Validations;

namespace UserAccountMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAccountsController : ControllerBase
{
    private readonly UserAccountService _service;
    private readonly IValidator<UserAccount> _validator;
    private readonly AuthFacade _authFacade;
    private readonly IPasswordService _passwordService;

    public UserAccountsController(
        UserAccountService service, 
        IValidator<UserAccount> validator, 
        AuthFacade authFacade,
        IPasswordService passwordService)
    {
        _service = service;
        _validator = validator;
        _authFacade = authFacade;
        _passwordService = passwordService;
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
    public async Task<IActionResult> Create([FromBody] UserAccount account, [FromHeader] int userId)
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

        var success = await _authFacade.Create(account, userId);
        
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

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, [FromHeader] int userId)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Las contraseñas no coinciden",
                Errors = new List<string> { "La nueva contraseña y la confirmación deben ser iguales" }
            });
        }

        if (request.NewPassword.Length < 8)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Contraseña muy corta",
                Errors = new List<string> { "La contraseña debe tener al menos 8 caracteres" }
            });
        }

        var user = await _service.GetById(userId);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        if (!string.IsNullOrEmpty(request.CurrentPassword))
        {
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.Password))
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Contraseña actual incorrecta",
                    Errors = new List<string> { "La contraseña actual no es válida" }
                });
            }
        }
        else if (!user.IsFirstLogin)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Se requiere contraseña actual",
                Errors = new List<string> { "Debe proporcionar su contraseña actual para cambiarla" }
            });
        }

        var hashedPassword = _passwordService.HashPassword(request.NewPassword);

        var success = await _service.ChangePassword(userId, hashedPassword);

        if (!success)
        {
            return StatusCode(500, new { message = "Error al cambiar la contraseña" });
        }

        return Ok(new SuccessResponse
        {
            Message = "Contraseña cambiada exitosamente",
            Id = userId
        });
    }
    
}