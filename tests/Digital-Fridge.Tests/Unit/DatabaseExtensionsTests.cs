using Digital_Fridge.Data;
using Microsoft.Extensions.Configuration;

namespace Digital_Fridge.Tests.Unit;

[Trait("Category", "Unit")]
public class DatabaseExtensionsTests
{
    [Fact]
    public void BuildConnectionString_PrefersDirectDefaultConnection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:DefaultConnection"] = "Host=custom-host;Database=custom-db",
                ["Database:Host"] = "ignored-host"
            })
            .Build();

        var connStr = DatabaseExtensions.BuildConnectionString(config);

        Assert.Equal("Host=custom-host;Database=custom-db", connStr);
    }

    [Fact]
    public void BuildConnectionString_BuildsFromPostgresKeysAndDefaults()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["Postgres:Host"] = "db.internal",
                ["Postgres:Port"] = "5433",
                ["Postgres:DbName"] = "fridge_prod",
                ["Postgres:User"] = "appuser",
                ["Database:Password"] = "secret"
            })
            .Build();

        var connStr = DatabaseExtensions.BuildConnectionString(config);

        Assert.Contains("Host=db.internal", connStr);
        Assert.Contains("Port=5433", connStr);
        Assert.Contains("Database=fridge_prod", connStr);
        Assert.Contains("Username=appuser", connStr);
    }
}
