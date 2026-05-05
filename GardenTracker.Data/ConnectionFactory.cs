using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GardenTracker.Data;

public class ConnectionFactory(IConfiguration configuration) : IConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}

public interface IConnectionFactory
{
    IDbConnection CreateConnection();
}
