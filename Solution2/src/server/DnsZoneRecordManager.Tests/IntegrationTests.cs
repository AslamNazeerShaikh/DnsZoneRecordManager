using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DnsZoneRecordManager.Cqrs;
using DnsZoneRecordManager.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>JSON options matching the server (camelCase + string enums).</summary>
    public static class ApiJson
    {
        /// <summary>Shared deserialization options.</summary>
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        };
    }

    /// <summary>Unregistered request for dispatcher failure tests.</summary>
    public sealed record UnknownQuery : IRequest<int>;
    /// <summary>Test host with an isolated SQLite file database per fixture instance.</summary>
    public class DnsWebFactory : WebApplicationFactory<DnsZoneRecordManager.Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"dnstest-{Guid.NewGuid():N}.db");

        /// <summary>Replaces the file store with an isolated database.</summary>
        /// <param name="builder">Web host builder.</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
            });
        }

        /// <summary>Deletes the isolated database file.</summary>
        /// <param name="disposing">Dispose flag.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            File.Delete(_dbPath);
            File.Delete(_dbPath + "-shm");
            File.Delete(_dbPath + "-wal");
        }
    }

    /// <summary>Zone endpoint flows: CRUD, search, statuses, typed errors.</summary>
    public class ZonesApiTests : IClassFixture<DnsWebFactory>
    {
        private readonly DnsWebFactory _factory;

        /// <summary>Creates zone API tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public ZonesApiTests(DnsWebFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task should_list_and_search_zones()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            zones.Should().ContainSingle(z => z.Name == "nahuexolab.com");
            zones.Should().ContainSingle(z => z.Name == "demo.example");

            var hit = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones?search=NAHU");
            hit.Should().ContainSingle().Which.Name.Should().Be("nahuexolab.com");
            var miss = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones?search=zzz-no-match");
            miss.Should().BeEmpty();
        }

        [Fact]
        public async Task should_get_zone_or_404()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            var id = zones!.First(z => z.Name == "nahuexolab.com").Id;

            var zone = await client.GetFromJsonAsync<ZoneDto>($"/api/zones/{id}");
            zone!.RecordCount.Should().Be(5);
            zone.NsCount.Should().Be(4);

            var missing = await client.GetAsync("/api/zones/99999");
            missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await missing.Content.ReadAsStringAsync()).Should().Contain("errors");
        }

        [Fact]
        public async Task should_create_rename_and_delete_zone()
        {
            var client = _factory.CreateClient();
            var name = $"api{Guid.NewGuid():N}.example";

            var created = await client.PostAsJsonAsync("/api/zones", new { Name = name });
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            created.Headers.Location.Should().NotBeNull();

            var invalid = await client.PostAsJsonAsync("/api/zones", new { Name = "" });
            invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var duplicate = await client.PostAsJsonAsync("/api/zones", new { Name = "nahuexolab.com" });
            duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);

            var id = int.Parse(created.Headers.Location!.Segments.Last());
            var renamed = await client.PutAsJsonAsync($"/api/zones/{id}", new { Name = $"r{name}" });
            renamed.StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.PutAsJsonAsync("/api/zones/99999", new { Name = name })).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await client.PutAsJsonAsync($"/api/zones/{id}", new { Name = "nahuexolab.com" })).StatusCode.Should().Be(HttpStatusCode.Conflict);
            (await client.PutAsJsonAsync($"/api/zones/{id}", new { Name = "" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

            (await client.DeleteAsync($"/api/zones/{id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await client.GetAsync($"/api/zones/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await client.DeleteAsync("/api/zones/99999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>Record endpoint flows: CRUD, filters, CSV, NS floor, typed errors.</summary>
    public class RecordsApiTests : IClassFixture<DnsWebFactory>
    {
        private readonly DnsWebFactory _factory;

        /// <summary>Creates record API tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public RecordsApiTests(DnsWebFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task should_list_filter_and_shape_records()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;

            var all = await client.GetStringAsync("/api/records");
            all.Should().Contain("_dmarc.nahuexolab.com").And.Contain("\"type\":\"NS\"");

            var zoned = await client.GetStringAsync($"/api/records?zoneId={zoneId}");
            zoned.Should().Contain("nahuexolab.com").And.NotContain("demo.example");
            var typed = await client.GetStringAsync("/api/records?type=A");
            typed.Should().NotContain("ns1-33.azure-dns.com.");
            var found = await client.GetStringAsync("/api/records?search=_dmarc");
            found.Should().Contain("_dmarc.nahuexolab.com");
            var none = await client.GetStringAsync("/api/records?search=zzz-no-match");
            none.Should().Be("[]");

            (await client.GetAsync("/api/records?zoneId=99999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_get_record_or_404()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;
            var rows = await client.GetFromJsonAsync<List<RecordDto>>($"/api/records?zoneId={zoneId}&type=TXT", ApiJson.Options);
            var id = rows!.Single().Id;

            var record = await client.GetFromJsonAsync<RecordDto>($"/api/records/{id}", ApiJson.Options);
            record!.Fqdn.Should().Be("_dmarc.nahuexolab.com");

            (await client.GetAsync("/api/records/99999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_create_update_and_delete_record()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;
            var name = $"j{Guid.NewGuid():N}";

            var created = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = zoneId,
                    Name = name,
                    Type = "A",
                    Ttl = 300,
                    Data = "10.3.3.3",
                }
            );
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = int.Parse(created.Headers.Location!.Segments.Last());

            var badZone = await client.PostAsJsonAsync("/api/records", new { ZoneId = 99999, Name = name, Type = "A", Ttl = 300, Data = "10.3.3.3" });
            badZone.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var badData = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = zoneId,
                    Name = name,
                    Type = "A",
                    Ttl = 300,
                    Data = "not-an-ip",
                }
            );
            badData.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var duplicate = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = zoneId,
                    Name = "@",
                    Type = "NS",
                    Ttl = 172800,
                    Data = "ns1-33.azure-dns.com.",
                }
            );
            duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var clash = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = zoneId,
                    Name = "@",
                    Type = "CNAME",
                    Ttl = 300,
                    Data = "target.example.com.",
                }
            );
            clash.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var updated = await client.PutAsJsonAsync($"/api/records/{id}", new { Name = name, Type = "A", Ttl = 600, Data = "10.3.3.4" });
            updated.StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.PutAsJsonAsync("/api/records/99999", new { Name = name, Type = "A", Ttl = 600, Data = "10.3.3.4" })).StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            (await client.DeleteAsync($"/api/records/{id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await client.GetAsync($"/api/records/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);

            var nsRows = await client.GetFromJsonAsync<List<RecordDto>>($"/api/records?zoneId={zoneId}&type=NS", ApiJson.Options);
            (await client.DeleteAsync($"/api/records/{nsRows!.First().Id}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await client.DeleteAsync("/api/records/99999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_export_csv_and_reject_unknown_zone()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;

            var all = await client.GetAsync("/api/records/export");
            all.EnsureSuccessStatusCode();
            all.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
            (await all.Content.ReadAsStringAsync()).Should().StartWith("FQDN,Zone,Name,Type,TTL,Data\n").And.Contain("nahuexolab.com");

            var zoned = await client.GetStringAsync($"/api/records/export?zoneId={zoneId}&type=TXT");
            zoned.Should().Contain("_dmarc.nahuexolab.com").And.NotContain("ns1-33.azure-dns.com.");

            (await client.GetAsync("/api/records/export?zoneId=99999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>Site flows: Scalar reference, OpenAPI document, headers, seed, production boot.</summary>
    public class SiteApiTests : IClassFixture<DnsWebFactory>
    {
        private readonly DnsWebFactory _factory;

        /// <summary>Creates site API tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public SiteApiTests(DnsWebFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task should_serve_scalar_openapi_headers_and_seed()
        {
            var client = _factory.CreateClient();
            var scalar = await client.GetAsync("/scalar");
            scalar.EnsureSuccessStatusCode();

            var openapi = await client.GetStringAsync("/openapi/v1.json");
            openapi.Should().Contain("/api/zones").And.Contain("/api/records");

            var zones = await client.GetAsync("/api/zones");
            zones.EnsureSuccessStatusCode();
            zones.Headers.Should().Contain(h => h.Key == "X-Content-Type-Options");
            (await zones.Content.ReadAsStringAsync()).Should().Contain("demo.example");
        }

        [Fact]
        public async Task should_boot_non_development_pipeline()
        {
            using var factory = new DnsWebFactory().WithWebHostBuilder(builder => builder.UseSetting(WebHostDefaults.EnvironmentKey, "Production"));
            (await factory.CreateClient().GetAsync("/api/zones")).EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task should_reject_malformed_bodies_and_unknown_types()
        {
            var client = _factory.CreateClient();
            (await client.PostAsJsonAsync("/api/zones", new { })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await client.PostAsJsonAsync("/api/records", new { })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var badEnum = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = 1,
                    Name = "x",
                    Type = "NOTATYPE",
                    Ttl = 300,
                    Data = "x",
                }
            );
            badEnum.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var badQuery = await client.GetAsync("/api/records?type=NOTATYPE");
            badQuery.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task should_fail_fast_on_unknown_and_null_requests()
        {
            using var scope = _factory.Services.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(new UnknownQuery(), CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() => sender.SendAsync<int>(null!, CancellationToken.None));
        }
    }
}
