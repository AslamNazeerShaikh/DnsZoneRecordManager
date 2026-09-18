using DnsZoneRecordManager.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Migration reversibility + seeder tests against real SQLite.</summary>
    public class MigrationTests
    {
        [Fact]
        public async Task should_migrate_up_down_and_up_with_seed()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

            await using (var context = new AppDbContext(options))
            {
                await context.Database.MigrateAsync();
                await DbSeeder.SeedAsync(context, CancellationToken.None);
                (await context.Zones.CountAsync()).Should().Be(2);
                (await context.Records.CountAsync()).Should().Be(13);

                await DbSeeder.SeedAsync(context, CancellationToken.None);
                (await context.Zones.CountAsync()).Should().Be(2);
            }

            await using (var context = new AppDbContext(options))
            {
                await context.Database.GetService<IMigrator>().MigrateAsync("0");
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Zones', 'Records')";
                (await command.ExecuteScalarAsync()).Should().Be(0L);
            }

            await using (var context = new AppDbContext(options))
            {
                await context.Database.MigrateAsync();
                await DbSeeder.SeedAsync(context, CancellationToken.None);
                (await context.Zones.CountAsync()).Should().Be(2);
                context.Zones.Should().ContainSingle(z => z.Name == "demo.example");
            }
        }
    }
}
