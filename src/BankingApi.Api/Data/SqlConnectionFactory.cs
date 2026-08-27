using System.Data.Common;
using Microsoft.Data.SqlClient;
namespace BankingApi.Api.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("BankingDatabase")
        ?? throw new InvalidOperationException("Connection string 'BankingDatabase' is missing.");
    public DbConnection CreateConnection() => new SqlConnection(_connectionString);
}
