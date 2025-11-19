using System.Data.Common;

namespace UserAccountMicroservice.Infrastructure.Connection;

public interface IDbConnectionFactory
{
    DbConnection  CreateConnection();

    string GetProviderName();
}