using System.Data;
using Microsoft.Data.SqlClient;
using GardenTracker.Data;

namespace GardenTracker.Tests.Integration;

/// <summary>
/// Lightweight IConnectionFactory for tests — takes a connection string directly
/// instead of requiring IConfiguration.
/// </summary>
public class TestConnectionFactory(string connectionString) : IConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
