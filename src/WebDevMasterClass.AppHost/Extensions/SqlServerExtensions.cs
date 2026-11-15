using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebDevMasterClass.AppHost.Extensions;

public static class SqlServerExtensions
{
    public static IResourceBuilder<SqlServerDatabaseResource> WithSeedDataCommand(
        this IResourceBuilder<SqlServerDatabaseResource> builder
    )
    => builder.WithCommand(
        "seed-data", 
        "Add seed data",
        async ctx =>
        {
            var connstring = await builder.Resource.ConnectionStringExpression.GetValueAsync(ctx.CancellationToken);
            await using var conn = new SqlConnection(connstring);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = await File.ReadAllTextAsync("Data/SeedData.sql");
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
            return CommandResults.Success();
        },
        new CommandOptions
        {
            IconName = "ArchiveArrowBack",
            IconVariant = IconVariant.Filled,
            UpdateState = ctx => ctx.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
                    ? ResourceCommandState.Enabled
                    : ResourceCommandState.Disabled
        });
}