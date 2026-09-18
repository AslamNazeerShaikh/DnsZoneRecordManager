using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DnsZoneRecordManager.Data
{
    /// <summary>Design-time context factory for the <c>dotnet ef</c> tooling only (never used at runtime or in tests).</summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>Creates a context pointing at the default SQLite file for tooling commands.</summary>
        /// <param name="args">Tooling arguments (ignored).</param>
        /// <returns>Context for migrations tooling.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=dnsmanager.db").Options;
            return new AppDbContext(options);
        }
    }
}
