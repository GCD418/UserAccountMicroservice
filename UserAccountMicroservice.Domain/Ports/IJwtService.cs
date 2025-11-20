using UserAccountMicroservice.Domain.Entities;

namespace UserAccountMicroservice.Domain.Ports;

public interface IJwtService
{
    string GenerateToken(UserAccount userAccount);
}
