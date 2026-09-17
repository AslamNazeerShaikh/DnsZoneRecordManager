using System.Net;
using System.Text.RegularExpressions;
using DnsZoneRecordManager.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Test host with an isolated InMemory database per fixture instance.</summary>
    public class DnsWebFactory : WebApplicationFactory<DnsZoneRecordManager.Program>
    {
        private readonly string _dbName = "IntegrationDb-" + Guid.NewGuid();

        /// <summary>Replaces the shared store with an isolated database.</summary>
        /// <param name="builder">Web host builder.</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                );
                services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName)
                );
            });
        }
    }

    /// <summary>Shared HTTP + antiforgery helpers for the flow tests.</summary>
    public abstract class FlowTests : IClassFixture<DnsWebFactory>
    {
        private readonly DnsWebFactory _factory;

        /// <summary>Creates flow tests over the given factory.</summary>
        /// <param name="factory">Test host factory.</param>
        protected FlowTests(DnsWebFactory factory)
        {
            _factory = factory;
        }

        /// <summary>Creates an HTTP client for the test host.</summary>
        /// <returns>Client.</returns>
        protected HttpClient Client()
        {
            return _factory.CreateClient();
        }

        /// <summary>Finds a zone id by name via a service scope.</summary>
        /// <param name="name">Zone name.</param>
        /// <returns>Zone id.</returns>
        protected int ZoneId(string name)
        {
            using var scope = _factory.Services.CreateScope();
            return scope
                .ServiceProvider.GetRequiredService<AppDbContext>()
                .Zones.Single(z => z.Name == name)
                .Id;
        }

        /// <summary>Finds a record id by zone, name, and type via a service scope.</summary>
        /// <param name="zoneId">Zone id.</param>
        /// <param name="name">Owner name.</param>
        /// <param name="type">Record type.</param>
        /// <returns>Record id.</returns>
        protected int RecordId(int zoneId, string name, string type)
        {
            using var scope = _factory.Services.CreateScope();
            return scope
                .ServiceProvider.GetRequiredService<AppDbContext>()
                .Records.First(r =>
                    r.ZoneId == zoneId && r.Name == name && r.Type.ToString() == type
                )
                .Id;
        }

        /// <summary>Reads response HTML.</summary>
        /// <param name="response">HTTP response.</param>
        /// <returns>Body text.</returns>
        protected static async Task<string> Html(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>Posts a form with a fresh antiforgery token from the given form page.</summary>
        /// <param name="client">HTTP client.</param>
        /// <param name="url">POST url.</param>
        /// <param name="formUrl">GET url of the form page.</param>
        /// <param name="form">Form fields.</param>
        /// <returns>POST response (redirects followed).</returns>
        protected static async Task<HttpResponseMessage> PostFormAsync(
            HttpClient client,
            string url,
            string formUrl,
            Dictionary<string, string> form
        )
        {
            var get = await client.GetAsync(formUrl);
            get.EnsureSuccessStatusCode();
            var html = await get.Content.ReadAsStringAsync();
            form["__RequestVerificationToken"] = Regex
                .Match(
                    html,
                    "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\""
                )
                .Groups[1]
                .Value;
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(form),
            };
            request.Headers.Add(
                "Cookie",
                string.Join("; ", get.Headers.GetValues("Set-Cookie").Select(c => c.Split(';')[0]))
            );
            return await client.SendAsync(request);
        }

        /// <summary>Builds a unique zone name (hostname-safe).</summary>
        /// <returns>Unique zone name.</returns>
        protected static string UniqueZone()
        {
            return $"zone{Guid.NewGuid():N}.example";
        }
    }

    /// <summary>Zone page flows: grid, search, create, rename, delete, validation, toasts.</summary>
    public class ZonesFlowsTests : FlowTests
    {
        /// <summary>Creates zone flow tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public ZonesFlowsTests(DnsWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task should_list_seed_zone_with_meter_and_search()
        {
            var html = await Html(await Client().GetAsync("/Zones"));
            html.Should().Contain("nahuexolab.com").And.Contain("/ 10");

            var empty = await Html(await Client().GetAsync("/Zones?search=zzz-no-match"));
            empty.Should().Contain("No zones found");
            var hit = await Html(await Client().GetAsync("/Zones?search=NAHU"));
            hit.Should().Contain("nahuexolab.com");
        }

        [Fact]
        public async Task should_create_zone_end_to_end()
        {
            var client = Client();
            var zone = UniqueZone();
            (await client.GetAsync("/Zones/Create")).EnsureSuccessStatusCode();
            var created = await Html(
                await PostFormAsync(
                    client,
                    "/Zones/Create",
                    "/Zones/Create",
                    new Dictionary<string, string> { ["Name"] = zone }
                )
            );
            created.Should().Contain("was created.").And.Contain(zone);
        }

        [Fact]
        public async Task should_reject_invalid_and_duplicate_zone_posts()
        {
            var client = Client();
            var invalid = await Html(
                await PostFormAsync(
                    client,
                    "/Zones/Create",
                    "/Zones/Create",
                    new Dictionary<string, string> { ["Name"] = "" }
                )
            );
            invalid.Should().Contain("Give the zone a name");
            var duplicate = await Html(
                await PostFormAsync(
                    client,
                    "/Zones/Create",
                    "/Zones/Create",
                    new Dictionary<string, string> { ["Name"] = "nahuexolab.com" }
                )
            );
            duplicate.Should().Contain("already exists");
        }

        [Fact]
        public async Task should_rename_zone_end_to_end()
        {
            var client = Client();
            var zone = UniqueZone();
            await PostFormAsync(
                client,
                "/Zones/Create",
                "/Zones/Create",
                new Dictionary<string, string> { ["Name"] = zone }
            );
            var id = ZoneId(zone);

            (await client.GetAsync($"/Zones/Edit/{id}")).EnsureSuccessStatusCode();
            (await client.GetAsync("/Zones/Edit/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var renamed = UniqueZone();
            var done = await Html(
                await PostFormAsync(
                    client,
                    $"/Zones/Edit/{id}",
                    $"/Zones/Edit/{id}",
                    new Dictionary<string, string> { ["Name"] = renamed }
                )
            );
            done.Should().Contain("renamed to").And.Contain(renamed);

            var clash = await Html(
                await PostFormAsync(
                    client,
                    $"/Zones/Edit/{id}",
                    $"/Zones/Edit/{id}",
                    new Dictionary<string, string> { ["Name"] = "nahuexolab.com" }
                )
            );
            clash.Should().Contain("already exists");
            var bad = await Html(
                await PostFormAsync(
                    client,
                    $"/Zones/Edit/{id}",
                    $"/Zones/Edit/{id}",
                    new Dictionary<string, string> { ["Name"] = "" }
                )
            );
            bad.Should().Contain("Give the zone a name");
            (
                await PostFormAsync(
                    client,
                    "/Zones/Edit/99999",
                    $"/Zones/Edit/{id}",
                    new Dictionary<string, string> { ["Name"] = UniqueZone() }
                )
            )
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_delete_zone_with_confirm_and_counts()
        {
            var client = Client();
            var zone = UniqueZone();
            await PostFormAsync(
                client,
                "/Zones/Create",
                "/Zones/Create",
                new Dictionary<string, string> { ["Name"] = zone }
            );
            var id = ZoneId(zone);

            var confirm = await Html(await client.GetAsync($"/Zones/Delete/{id}"));
            confirm.Should().Contain("cascades").And.Contain(zone);
            (await client.GetAsync("/Zones/Delete/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var done = await Html(
                await PostFormAsync(
                    client,
                    $"/Zones/Delete/{id}",
                    $"/Zones/Delete/{id}",
                    new Dictionary<string, string>()
                )
            );
            done.Should().Contain("were deleted.");
            done.Should().NotContain(zone);
            (
                await PostFormAsync(
                    client,
                    "/Zones/Delete/99999",
                    "/Zones/Create",
                    new Dictionary<string, string>()
                )
            )
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>Record page flows: grid, filters, create, edit, delete, NS floor, CSV export.</summary>
    public class RecordsFlowsTests : FlowTests
    {
        /// <summary>Creates record flow tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public RecordsFlowsTests(DnsWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task should_list_filter_and_reject_unknown_zone()
        {
            var client = Client();
            var html = await Html(await client.GetAsync("/Records"));
            html.Should().Contain("_dmarc.nahuexolab.com").And.Contain("Export CSV");

            var zoneId = ZoneId("nahuexolab.com");
            var zoned = await Html(await client.GetAsync($"/Records?zoneId={zoneId}"));
            zoned.Should().Contain("/ 10 records").And.Contain("4 NS");
            (await client.GetAsync("/Records?zoneId=99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var empty = await Html(await client.GetAsync("/Records?search=zzz-no-match"));
            empty.Should().Contain("No records match");
            var typed = await Html(await client.GetAsync("/Records?type=A"));
            typed.Should().NotContain("ns1-33.azure-dns.com.");
            var found = await Html(await client.GetAsync("/Records?search=ns1-33"));
            found.Should().Contain("ns1-33.azure-dns.com.");
        }

        [Fact]
        public async Task should_create_record_end_to_end_with_guards()
        {
            var client = Client();
            var zoneId = ZoneId("nahuexolab.com");
            (await client.GetAsync("/Records/Create")).EnsureSuccessStatusCode();
            (await client.GetAsync($"/Records/Create?zoneId={zoneId}")).EnsureSuccessStatusCode();
            (await client.GetAsync("/Records/Create?zoneId=99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var name = $"h{Guid.NewGuid():N}";
            var done = await Html(
                await PostFormAsync(
                    client,
                    "/Records/Create",
                    "/Records/Create",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = name,
                        ["Type"] = "A",
                        ["Ttl"] = "300",
                        ["Data"] = "10.1.2.3",
                    }
                )
            );
            done.Should().Contain("was created.").And.Contain($"{name}.nahuexolab.com");

            var invalid = await Html(
                await PostFormAsync(
                    client,
                    "/Records/Create",
                    "/Records/Create",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = "",
                        ["Type"] = "A",
                        ["Ttl"] = "300",
                        ["Data"] = "10.1.2.3",
                    }
                )
            );
            invalid.Should().Contain("Give the record a name");

            var bogusModel = await Html(
                await PostFormAsync(
                    client,
                    "/Records/Create",
                    "/Records/Create",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = "99999",
                        ["Name"] = "",
                        ["Type"] = "A",
                        ["Ttl"] = "300",
                        ["Data"] = "10.1.2.3",
                    }
                )
            );
            bogusModel.Should().Contain("Give the record a name");

            var bogusZone = await PostFormAsync(
                client,
                "/Records/Create",
                "/Records/Create",
                new Dictionary<string, string>
                {
                    ["ZoneId"] = "99999",
                    ["Name"] = "ghost",
                    ["Type"] = "A",
                    ["Ttl"] = "300",
                    ["Data"] = "10.9.9.9",
                }
            );
            bogusZone.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var duplicate = await Html(
                await PostFormAsync(
                    client,
                    "/Records/Create",
                    "/Records/Create",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = "@",
                        ["Type"] = "NS",
                        ["Ttl"] = "172800",
                        ["Data"] = "ns1-33.azure-dns.com.",
                    }
                )
            );
            duplicate.Should().Contain("already exists in the zone");

            var clash = await Html(
                await PostFormAsync(
                    client,
                    "/Records/Create",
                    "/Records/Create",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = "@",
                        ["Type"] = "CNAME",
                        ["Ttl"] = "300",
                        ["Data"] = "target.example.com.",
                    }
                )
            );
            clash.Should().Contain("CNAME");
        }

        [Fact]
        public async Task should_edit_record_end_to_end()
        {
            var client = Client();
            var zoneId = ZoneId("nahuexolab.com");
            var name = $"e{Guid.NewGuid():N}";
            await PostFormAsync(
                client,
                "/Records/Create",
                "/Records/Create",
                new Dictionary<string, string>
                {
                    ["ZoneId"] = zoneId.ToString(),
                    ["Name"] = name,
                    ["Type"] = "A",
                    ["Ttl"] = "300",
                    ["Data"] = "10.2.2.2",
                }
            );
            var id = RecordId(zoneId, name, "A");

            (await client.GetAsync($"/Records/Edit/{id}")).EnsureSuccessStatusCode();
            (await client.GetAsync("/Records/Edit/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var done = await Html(
                await PostFormAsync(
                    client,
                    $"/Records/Edit/{id}",
                    $"/Records/Edit/{id}",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = name,
                        ["Type"] = "A",
                        ["Ttl"] = "600",
                        ["Data"] = "10.2.2.3",
                    }
                )
            );
            done.Should().Contain("was updated.");

            var bad = await Html(
                await PostFormAsync(
                    client,
                    $"/Records/Edit/{id}",
                    $"/Records/Edit/{id}",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = name,
                        ["Type"] = "A",
                        ["Ttl"] = "0",
                        ["Data"] = "10.2.2.3",
                    }
                )
            );
            bad.Should().Contain("positive");

            await PostFormAsync(
                client,
                "/Records/Create",
                "/Records/Create",
                new Dictionary<string, string>
                {
                    ["ZoneId"] = zoneId.ToString(),
                    ["Name"] = "clash",
                    ["Type"] = "A",
                    ["Ttl"] = "300",
                    ["Data"] = "10.2.2.9",
                }
            );
            var clash = await Html(
                await PostFormAsync(
                    client,
                    $"/Records/Edit/{id}",
                    $"/Records/Edit/{id}",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = "clash",
                        ["Type"] = "A",
                        ["Ttl"] = "300",
                        ["Data"] = "10.2.2.9",
                    }
                )
            );
            clash.Should().Contain("already exists in the zone");
            (
                await PostFormAsync(
                    client,
                    "/Records/Edit/99999",
                    $"/Records/Edit/{id}",
                    new Dictionary<string, string>
                    {
                        ["ZoneId"] = zoneId.ToString(),
                        ["Name"] = name,
                        ["Type"] = "A",
                        ["Ttl"] = "600",
                        ["Data"] = "10.2.2.3",
                    }
                )
            ).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_delete_record_and_block_ns_floor()
        {
            var client = Client();
            var zoneId = ZoneId("nahuexolab.com");
            var name = $"d{Guid.NewGuid():N}";
            await PostFormAsync(
                client,
                "/Records/Create",
                "/Records/Create",
                new Dictionary<string, string>
                {
                    ["ZoneId"] = zoneId.ToString(),
                    ["Name"] = name,
                    ["Type"] = "TXT",
                    ["Ttl"] = "300",
                    ["Data"] = "temp",
                }
            );
            var id = RecordId(zoneId, name, "TXT");

            (await client.GetAsync($"/Records/Delete/{id}")).EnsureSuccessStatusCode();
            (await client.GetAsync("/Records/Delete/99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);

            var done = await Html(
                await PostFormAsync(
                    client,
                    $"/Records/Delete/{id}?zoneId={zoneId}",
                    $"/Records/Delete/{id}",
                    new Dictionary<string, string>()
                )
            );
            done.Should().Contain("was deleted.");

            var nsId = RecordId(zoneId, "@", "NS");
            var blocked = await Html(
                await PostFormAsync(
                    client,
                    $"/Records/Delete/{nsId}?zoneId={zoneId}",
                    $"/Records/Delete/{nsId}",
                    new Dictionary<string, string>()
                )
            );
            blocked.Should().Contain("4 NS");

            (
                await PostFormAsync(
                    client,
                    "/Records/Delete/99999?zoneId=1",
                    $"/Records/Delete/{nsId}",
                    new Dictionary<string, string>()
                )
            )
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_export_csv_and_reject_unknown_zone()
        {
            var client = Client();
            var zoneId = ZoneId("nahuexolab.com");

            var all = await client.GetAsync("/Records/Export");
            all.EnsureSuccessStatusCode();
            all.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
            var csv = await all.Content.ReadAsStringAsync();
            csv.Should().StartWith("FQDN,Zone,Name,Type,TTL,Data\n").And.Contain("nahuexolab.com");

            var zoned = await Html(
                await client.GetAsync($"/Records/Export?zoneId={zoneId}&type=TXT")
            );
            zoned.Should().Contain("_dmarc.nahuexolab.com").And.NotContain("ns1-33.azure-dns.com.");

            (await client.GetAsync("/Records/Export?zoneId=99999"))
                .StatusCode.Should()
                .Be(HttpStatusCode.NotFound);
        }
    }

    /// <summary>Site flows: landing, seed data, error page, production pipeline, app builder.</summary>
    public class SiteFlowsTests : FlowTests
    {
        /// <summary>Creates site flow tests.</summary>
        /// <param name="factory">Test host factory.</param>
        public SiteFlowsTests(DnsWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task should_serve_home_seed_and_error_pages()
        {
            var client = Client();
            (await Html(await client.GetAsync("/"))).Should().Contain("DNS Zones");
            var home = await client.GetAsync("/Home/Privacy");
            home.EnsureSuccessStatusCode();
            home.Headers.Should()
                .Contain(h => h.Key == "X-Content-Type-Options" && h.Value.Contains("nosniff"));
            home.Headers.Should().Contain(h => h.Key == "X-Frame-Options");
            (await client.GetAsync("/Home/Error")).EnsureSuccessStatusCode();

            var zones = await Html(await client.GetAsync("/Zones"));
            zones.Should().Contain("nahuexolab.com");
            var records = await Html(await client.GetAsync("/Records"));
            records.Should().Contain("_dmarc.nahuexolab.com");
        }

        [Fact]
        public async Task should_boot_non_development_pipeline()
        {
            using var factory = new DnsWebFactory().WithWebHostBuilder(builder =>
                builder.UseSetting(WebHostDefaults.EnvironmentKey, "Production")
            );
            (await factory.CreateClient().GetAsync("/")).EnsureSuccessStatusCode();
        }
    }
}
