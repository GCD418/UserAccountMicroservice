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
    private readonly IMailSender _mailSender;

    public AuthFacade(
        UserAccountService userAccountService,
        IPasswordService passwordService,
        IJwtService jwtService,
        IConfiguration configuration,
        IMailSender mailSender)
    {
        _userAccountService = userAccountService;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _configuration = configuration;
        _mailSender = mailSender;
    }
    
    public async Task<bool> Create(UserAccount userAccount, int userId)
    {
        userAccount.UserName = _userAccountService.GenerateUserName(userAccount);
        if (await _userAccountService.IsUserNameUsed(userAccount.UserName))
        {
            return false;
        }

        var password = _passwordService.GenerateRandomPassword();
        userAccount.Password = _passwordService.HashPassword(password);
        
        await SendEmail(userAccount.Name, userAccount.UserName, userAccount.Email, password);

        return await _userAccountService.Create(userAccount, userId);

    }

    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        UserAccount? userAccount = await _userAccountService.GetByUserName(request.UserName);
        
        if (userAccount == null || !VerifyCredentials(userAccount, request.Password))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(userAccount);
        var expirationMinutes = int.Parse(_configuration.GetSection("JwtSettings")["ExpirationInMinutes"] ?? "60");

        return new LoginResponse
        {
            Token = token,
            ExpiresIn = expirationMinutes * 60, 
            UserName = userAccount.UserName,
            Role = userAccount.Role,
            FullName = userAccount.FullName,
            IsFirstLogin = userAccount.IsFirstLogin
        };
    }

    private bool VerifyCredentials(UserAccount userAccount, string password)
    {
        return _passwordService.VerifyPassword(password, userAccount.Password);
    }
    
    private async Task SendEmail(string name, string username, string email, string password)
    {
        string subject = "Bienvenido a FuerzaG";
        string body = $@"
            <h1>Hola {name}!</h1>
            <p>Tu nombre de usuario es: <strong>{username}</strong></p>
            <p>Tu contraseña es: <strong>{password}</strong></p>
            <p>Ya puedes iniciar sesión en el sistema. Recuerda cuidarla como las llaves de tu casa</p>
        ";
        await _mailSender.SendEmail(email, subject, body);
    }
}
