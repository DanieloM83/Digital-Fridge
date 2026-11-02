using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Digital_Fridge.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        services.AddDbContextFactory<AppDbContext>(options => {
            options.UseNpgsql(connectionString);
        });

        return services;
    }

    public static string BuildConnectionString(IConfiguration configuration)
    {
        var directConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(directConnectionString))
        {
            return directConnectionString;
        }

        // Support Database:Host / Postgres:Host / flat keys flexibly
        var host = configuration["Database:Host"]
            ?? configuration["Database:host"]
            ?? configuration["Postgres:Host"]
            ?? configuration["Host"]
            ?? "localhost";

        var portStr = configuration["Database:Port"]
            ?? configuration["Database:port"]
            ?? configuration["Postgres:Port"]
            ?? configuration["Port"]
            ?? "5432";

        int.TryParse(portStr, out var port);
        if (port <= 0) port = 5432;

        var database = configuration["Database:DbName"]
            ?? configuration["Database:Database"]
            ?? configuration["Database:dbname"]
            ?? configuration["Database:Name"]
            ?? configuration["Postgres:DbName"]
            ?? "digitalfridge";

        var username = configuration["Database:User"]
            ?? configuration["Database:Username"]
            ?? configuration["Database:user"]
            ?? configuration["Database:username"]
            ?? configuration["Postgres:User"]
            ?? "postgres";

        var password = configuration["Database:Password"]
            ?? configuration["Database:password"]
            ?? configuration["Database:Pass"]
            ?? configuration["Postgres:Password"]
            ?? string.Empty;

        var builder = new NpgsqlConnectionStringBuilder {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            Pooling = true
        };

        return builder.ConnectionString;
    }
}

