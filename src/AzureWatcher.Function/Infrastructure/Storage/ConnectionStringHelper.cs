using System;
using System.Text;

namespace AzureWatcher.Function.Infrastructure.Storage;

public static class ConnectionStringHelper
{
    public static string ConvertToNpgsqlFormat(string? rawConnectionString)
    {
        if (string.IsNullOrEmpty(rawConnectionString))
            return string.Empty;

        if (!rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) && 
            !rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return rawConnectionString;

        try
        {
            var uri = new Uri(rawConnectionString);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
            var host = uri.Host;
            var port = uri.Port <= 0 ? 5432 : uri.Port;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch
        {
            return rawConnectionString;
        }
    }
}
