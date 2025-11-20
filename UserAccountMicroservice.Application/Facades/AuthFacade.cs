using Microsoft.Extensions.Configuration;
using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.DTOs;
using UserAccountMicroservice.Domain.Entities;
using UserAccountMicroservice.Domain.Ports;

namespace UserAccountMicroservice.Application.Facades;

public class AuthFacade
{
    private readonly UserAccountService _userAccountService;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthFacade(
        UserAccountService userAccountService,
        IPasswordService passwordService,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userAccountService = userAccountService;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        // Buscar usuario por nombre de usuario
        UserAccount? userAccount = await _userAccountService.GetByUserName(request.UserName);
        
        if (userAccount == null || !VerifyCredentials(userAccount, request.Password))
        {
            return null;
        }

        // Generar token JWT
        var token = _jwtService.GenerateToken(userAccount);
        var expirationMinutes = int.Parse(_configuration.GetSection("JwtSettings")["ExpirationInMinutes"] ?? "60");

        return new LoginResponse
        {
            Token = token,
            ExpiresIn = expirationMinutes * 60, // En segundos
            UserName = userAccount.UserName,
            Role = userAccount.Role,
            FullName = userAccount.FullName
        };
    }

    private bool VerifyCredentials(UserAccount userAccount, string password)
    {
        return _passwordService.VerifyPassword(password, userAccount.Password);
    }
}
