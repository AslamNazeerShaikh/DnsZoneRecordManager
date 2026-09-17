using DnsZoneRecordManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DnsZoneRecordManager.Data
{
    /// <summary>Seeds the sample zones on first run: nahuexolab.com (assessment sample)
    /// plus demo.example (one record of every allowed type for filtering demos).</summary>
    public static class DbSeeder
    {
        /// <summary>Inserts the seed zones when the store is empty; otherwise does nothing.</summary>
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

            var demo = new DnsZone
            {
                Name = "demo.example",
                CreatedUtc = now,
                UpdatedUtc = now,
            };
            foreach (
                var ns in new[]
                {
                    "ns1.demo.example.",
                    "ns2.demo.example.",
                    "ns3.demo.example.",
                    "ns4.demo.example.",
                }
            )
            {
                demo.Records.Add(
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

            demo.Records.Add(
                new DnsRecord
                {
                    Name = "www",
                    Type = RecordType.A,
                    Ttl = 300,
                    Data = "192.0.2.1",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
            demo.Records.Add(
                new DnsRecord
                {
                    Name = "ipv6",
                    Type = RecordType.AAAA,
                    Ttl = 300,
                    Data = "2001:db8::1",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
            demo.Records.Add(
                new DnsRecord
                {
                    Name = "alias",
                    Type = RecordType.CNAME,
                    Ttl = 3600,
                    Data = "www.demo.example.",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
            demo.Records.Add(
                new DnsRecord
                {
                    Name = "info",
                    Type = RecordType.TXT,
                    Ttl = 3600,
                    Data = "demo zone for filtering",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
            await context.Zones.AddAsync(demo);

            await context.SaveChangesAsync();
        }
    }
}
