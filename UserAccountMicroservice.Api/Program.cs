using UserAccountMicroservice.Application.Services;
using UserAccountMicroservice.Domain.Ports;
using UserAccountMicroservice.Infrastructure.Connection;
using UserAccountMicroservice.Infrastructure.Persistence;

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
// builder.Services.AddScoped<IValidator<UserAccount>, UserAccountValidator>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
