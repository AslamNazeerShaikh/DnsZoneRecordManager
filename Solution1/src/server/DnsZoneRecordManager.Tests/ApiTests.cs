using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;

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

    /// <summary>Zone JSON API tests: statuses, shapes, and typed errors.</summary>
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
        public async Task should_list_and_search_zones_as_json()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>("/api/zones");
            zones.Should().ContainSingle(z => z.Name == "nahuexolab.com");
            zones.Should().ContainSingle(z => z.Name == "demo.example");

            var hit = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>(
                "/api/zones?search=NAHU"
            );
            hit.Should().ContainSingle().Which.Name.Should().Be("nahuexolab.com");
        }

        [Fact]
        public async Task should_get_zone_or_404_with_errors_shape()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>("/api/zones");
            var id = zones!.First(z => z.Name == "nahuexolab.com").Id;

            var zone = await client.GetFromJsonAsync<Controllers.Api.ZoneDto>($"/api/zones/{id}");
            zone!.Name.Should().Be("nahuexolab.com");
            zone.RecordCount.Should().Be(5);

            var missing = await client.GetAsync("/api/zones/99999");
            missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await missing.Content.ReadAsStringAsync()).Should().Contain("errors");
        }

        [Fact]
        public async Task should_create_rename_and_delete_zone_with_statuses()
        {
            var client = _factory.CreateClient();
            var name = $"api{Guid.NewGuid():N}.example";

            var created = await client.PostAsJsonAsync("/api/zones", new { Name = name });
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            created.Headers.Location.Should().NotBeNull();

            var invalid = await client.PostAsJsonAsync("/api/zones", new { Name = "" });
            invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var duplicate = await client.PostAsJsonAsync(
                "/api/zones",
                new { Name = "nahuexolab.com" }
            );
            duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var missingMember = await client.PostAsJsonAsync("/api/zones", new { });
            missingMember.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var id = int.Parse(created.Headers.Location!.Segments.Last());
            var renamed = await client.PutAsJsonAsync(
                $"/api/zones/{id}",
                new { Name = $"r{name}" }
            );
            renamed.StatusCode.Should().Be(HttpStatusCode.OK);
            (await renamed.Content.ReadFromJsonAsync<Controllers.Api.ZoneDto>())!
                .Name.Should()
                .Be($"r{name}");

            (await client.PutAsJsonAsync("/api/zones/99999", new { Name = name }))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
            (await client.PutAsJsonAsync($"/api/zones/{id}", new { Name = "nahuexolab.com" }))
                .StatusCode.Should()
                .Be(HttpStatusCode.Conflict);

            (await client.DeleteAsync($"/api/zones/{id}"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NoContent);
            (await client.GetAsync($"/api/zones/{id}"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
            (await client.DeleteAsync("/api/zones/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>Record JSON API tests: statuses, shapes, filters, and typed errors.</summary>
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
        public async Task should_list_filter_and_shape_records_as_json()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;

            var all = await client.GetStringAsync("/api/records");
            all.Should().Contain("_dmarc.nahuexolab.com").And.Contain("\"type\":\"NS\"");

            var zoned = await client.GetStringAsync($"/api/records?zoneId={zoneId}");
            zoned.Should().Contain("nahuexolab.com").And.NotContain("demo.example");
            var typed = await client.GetStringAsync("/api/records?type=A");
            typed.Should().NotContain("ns1-33.azure-dns.com.");
            var found = await client.GetStringAsync("/api/records?search=_dmarc");
            found.Should().Contain("_dmarc.nahuexolab.com");

            (await client.GetAsync("/api/records?zoneId=99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_get_record_or_404()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>("/api/zones");
            var zoneId = zones!.First(z => z.Name == "nahuexolab.com").Id;
            var rows = await client.GetFromJsonAsync<List<Controllers.Api.RecordDto>>(
                $"/api/records?zoneId={zoneId}&type=TXT",
                ApiJson.Options
            );
            var id = rows!.Single().Id;

            var record = await client.GetFromJsonAsync<Controllers.Api.RecordDto>(
                $"/api/records/{id}",
                ApiJson.Options
            );
            record!.Fqdn.Should().Be("_dmarc.nahuexolab.com");
            record.Type.ToString().Should().Be("TXT");

            (await client.GetAsync("/api/records/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_create_update_and_delete_record_with_statuses()
        {
            var client = _factory.CreateClient();
            var zones = await client.GetFromJsonAsync<List<Controllers.Api.ZoneDto>>("/api/zones");
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

            var badZone = await client.PostAsJsonAsync(
                "/api/records",
                new
                {
                    ZoneId = 99999,
                    Name = name,
                    Type = "A",
                    Ttl = 300,
                    Data = "10.3.3.3",
                }
            );
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

            var updated = await client.PutAsJsonAsync(
                $"/api/records/{id}",
                new
                {
                    Name = name,
                    Type = "A",
                    Ttl = 600,
                    Data = "10.3.3.4",
                }
            );
            updated.StatusCode.Should().Be(HttpStatusCode.OK);
            (
                await client.PutAsJsonAsync(
                    "/api/records/99999",
                    new
                    {
                        Name = name,
                        Type = "A",
                        Ttl = 600,
                        Data = "10.3.3.4",
                    }
                )
            ).StatusCode.Should().Be(HttpStatusCode.NotFound);

            (await client.DeleteAsync($"/api/records/{id}"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NoContent);
            (await client.GetAsync($"/api/records/{id}"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var nsRows = await client.GetFromJsonAsync<List<Controllers.Api.RecordDto>>(
                $"/api/records?zoneId={zoneId}&type=NS",
                ApiJson.Options
            );
            (await client.DeleteAsync($"/api/records/{nsRows!.First().Id}"))
                .StatusCode.Should()
                .Be(HttpStatusCode.BadRequest);
            (await client.DeleteAsync("/api/records/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }
    }
}
