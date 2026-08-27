using System.Data.Common;
namespace BankingApi.Api.Data;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
