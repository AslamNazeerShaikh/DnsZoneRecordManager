using DnsZoneRecordManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Data
{
    /// <summary>EF Core context for zones and records (SQLite file store, created by migrations).</summary>
    public class AppDbContext : DbContext
    {
        /// <summary>Creates a context with the given options.</summary>
        /// <param name="options">EF Core options (SQLite connection comes from configuration).</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>DNS zones.</summary>
        public DbSet<DnsZone> Zones => Set<DnsZone>();

        /// <summary>DNS records.</summary>
        public DbSet<DnsRecord> Records => Set<DnsRecord>();

        /// <summary>Configures keys, lengths, the duplicate-guard index (A6), and cascade delete.</summary>
        /// <param name="modelBuilder">EF Core model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DnsZone>(zone =>
            {
                zone.HasKey(z => z.Id);
                zone.Property(z => z.Name).IsRequired().HasMaxLength(253);
                zone.HasIndex(z => z.Name).IsUnique();
                zone.HasMany(z => z.Records).WithOne(r => r.Zone).HasForeignKey(r => r.ZoneId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DnsRecord>(record =>
            {
                record.HasKey(r => r.Id);
                record.Property(r => r.Name).IsRequired().HasMaxLength(63);
                record.Property(r => r.Data).IsRequired().HasMaxLength(1000);
                record.Property(r => r.Type).HasConversion<string>().HasMaxLength(8);
                record.HasIndex(r => new { r.ZoneId, r.Name, r.Type, r.Data }).IsUnique();
            });
        }
    }
}
