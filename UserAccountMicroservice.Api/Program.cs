using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserAccountMicroservice.Application.Facades;
using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.DTOs;
using UserAccountMicroservice.Domain.Entities;
using UserAccountMicroservice.Domain.Ports;
using UserAccountMicroservice.Domain.Services;
using UserAccountMicroservice.Domain.Services.Validations;
using UserAccountMicroservice.Infrastructure;
using UserAccountMicroservice.Infrastructure.Connection;
using UserAccountMicroservice.Infrastructure.Persistence;
using UserAccountMicroservice.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

#region DatabaseConnection

var connectionString = builder.Configuration.GetConnectionString("PostgreSql");
var connectionManager = DatabaseConnectionManager.GetInstance(connectionString!);
builder.Services.AddSingleton(connectionManager);
builder.Services.AddScoped<IDbConnectionFactory, PostgreSqlConnection>();

#endregion

#region UserAccount

builder.Services.AddScoped<UserAccountService>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IValidator<UserAccount>, UserAccountValidator>();

#endregion

#region Email

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IMailSender, SmtpEmailSender>();

#endregion
#region Authentication

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthFacade>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
    });

builder.Services.AddAuthorization();

#endregion

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
