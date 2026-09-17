using DnsZoneRecordManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Data
{
    /// <summary>Seeds the sample zone (nahuexolab.com + 5 records) on first run.</summary>
    public static class DbSeeder
    {
        /// <summary>Inserts the seed zone when the store is empty; otherwise does nothing.</summary>
        /// <param name="context">EF Core context to seed.</param>
        /// <returns>A task for the operation.</returns>
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Zones.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;
            var zone = new DnsZone
            {
                Name = "nahuexolab.com",
                CreatedUtc = now,
                UpdatedUtc = now,
            };
            foreach (
                var ns in new[]
                {
                    "ns1-33.azure-dns.com.",
                    "ns2-33.azure-dns.net.",
                    "ns3-33.azure-dns.org.",
                    "ns4-33.azure-dns.info.",
                }
            )
            {
                zone.Records.Add(
                    new DnsRecord
                    {
                        Name = "@",
                        Type = RecordType.NS,
                        Ttl = 172800,
                        Data = ns,
                        CreatedUtc = now,
                        UpdatedUtc = now,
                    }
                );
            }

            zone.Records.Add(
                new DnsRecord
                {
                    Name = "_dmarc",
                    Type = RecordType.TXT,
                    Ttl = 3600,
                    Data =
                        "v=DMARC1; p=reject; pct=100; rua=mailto:rua@dmarc.microsoft; ruf=mailto:ruf@dmarc.microsoft; fo=1",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );

            await context.Zones.AddAsync(zone);
            await context.SaveChangesAsync();
        }
    }
}
