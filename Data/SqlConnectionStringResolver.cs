using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Data;

public static class SqlConnectionStringResolver
{
    public static string Resolve(IConfiguration config)
    {
        var host = Environment.GetEnvironmentVariable("MSSQL_HOST");
        if (!string.IsNullOrWhiteSpace(host))
        {
            var port = Environment.GetEnvironmentVariable("MSSQL_PORT") ?? "1433";
            var database = Environment.GetEnvironmentVariable("MSSQL_DATABASE") ?? "PosDemoDb";
            var user = Environment.GetEnvironmentVariable("MSSQL_USER") ?? "sa";
            var password = Environment.GetEnvironmentVariable("MSSQL_PASSWORD") ?? "";

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = $"{host.Trim()},{port.Trim()}",
                InitialCatalog = database.Trim(),
                UserID = user.Trim(),
                Password = password,
                TrustServerCertificate = true,
                MultipleActiveResultSets = true,
            };

            return builder.ConnectionString;
        }

        var configured = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException(
                "SQL bağlantısı yok: ortamda MSSQL_HOST (ve MSSQL_PASSWORD) ayarlayın veya ConnectionStrings:DefaultConnection tanımlayın.");
        }

        return configured;
    }
}
