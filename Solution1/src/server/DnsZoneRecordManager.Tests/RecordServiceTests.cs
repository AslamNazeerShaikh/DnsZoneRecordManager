using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Record service tests: A1/A2 limits, CNAME exclusivity, duplicates, filters, CSV.</summary>
    public class RecordServiceTests
    {
        private static RecordService Service(IUnitOfWork uow)
        {
            return new RecordService(
                uow,
                new RecordValidator(),
                NullLogger<RecordService>.Instance
            );
        }

        private static async Task<int> SeededZoneAsync(IUnitOfWork uow)
        {
            return await TestUow.SeedZoneAsync(uow, "svc.example");
        }

        [Fact]
        public void should_normalize_names_and_derive_fqdn()
        {
            RecordService.NormalizeRecordName("  WWW ").Should().Be("www");
            RecordService.ToFqdn("@", "svc.example").Should().Be("svc.example");
            RecordService.ToFqdn("www", "svc.example").Should().Be("www.svc.example");
        }

        [Fact]
        public async Task should_create_get_update_and_delete_records()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);

            var created = await service.CreateAsync(zoneId, "WWW", RecordType.A, 300, "10.0.0.1");
            created.Success.Should().BeTrue();
            created.Data!.Name.Should().Be("www");
            created.Data.CreatedUtc.Kind.Should().Be(DateTimeKind.Utc);

            (await service.GetAsync(999)).Success.Should().BeFalse();
            (await service.GetAsync(created.Data.Id)).Data!.Data.Should().Be("10.0.0.1");

            var updated = await service.UpdateAsync(
                created.Data.Id,
                "www",
                RecordType.A,
                600,
                "10.0.0.2"
            );
            updated.Success.Should().BeTrue();
            updated.Data!.Ttl.Should().Be(600);

            (await service.DeleteAsync(999)).Success.Should().BeFalse();
            (await service.DeleteAsync(created.Data.Id)).Data.Should().Be(created.Data.Id);
            (await service.GetAsync(created.Data.Id)).Success.Should().BeFalse();
        }

        [Fact]
        public async Task should_reject_missing_zone_invalid_data_and_duplicates()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);

            var missingZone = await service.CreateAsync(999, "www", RecordType.A, 300, "10.0.0.1");
            missingZone.Success.Should().BeFalse();
            missingZone.HasError(ErrorKind.NotFound).Should().BeTrue();
            missingZone
                .Errors.Should()
                .ContainSingle()
                .Which.Message.Should()
                .Be("Zone not found.");
            (await service.CreateAsync(zoneId, "www", RecordType.A, 300, "not-an-ip"))
                .Success.Should()
                .BeFalse();

            (await service.CreateAsync(zoneId, "dup", RecordType.A, 300, "10.0.0.9"))
                .Success.Should()
                .BeTrue();
            var duplicate = await service.CreateAsync(zoneId, "DUP", RecordType.A, 300, "10.0.0.9");
            duplicate.Success.Should().BeFalse();
            duplicate.HasError(ErrorKind.Conflict).Should().BeTrue();
            duplicate
                .Errors.Should()
                .ContainSingle()
                .Which.Message.Should()
                .Contain("already exists");

            var missingRecord = await service.UpdateAsync(
                999,
                "www",
                RecordType.A,
                300,
                "10.0.0.1"
            );
            missingRecord.HasError(ErrorKind.NotFound).Should().BeTrue();
            missingRecord
                .Errors.Should()
                .ContainSingle()
                .Which.Message.Should()
                .Be("Record not found.");
            (await service.UpdateAsync(zoneId, "www", RecordType.A, 0, "10.0.0.1"))
                .Success.Should()
                .BeFalse();
        }

        [Fact]
        public async Task should_enforce_cname_exclusivity_both_ways_on_create_and_update()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);
            var host = (
                await service.CreateAsync(zoneId, "alias", RecordType.A, 300, "10.0.0.5")
            ).Data!;

            var cnameClash = await service.CreateAsync(
                zoneId,
                "alias",
                RecordType.CNAME,
                300,
                "target.example.com."
            );
            cnameClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();
            cnameClash.Errors.Should().ContainSingle().Which.Message.Should().Contain("CNAME");

            var target = (
                await service.CreateAsync(
                    zoneId,
                    "other",
                    RecordType.CNAME,
                    300,
                    "target.example.com."
                )
            ).Data!;
            var hostClash = await service.CreateAsync(
                zoneId,
                "other",
                RecordType.A,
                300,
                "10.0.0.6"
            );
            hostClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();
            hostClash.Errors.Should().ContainSingle().Which.Message.Should().Contain("CNAME");

            var updateClash = await service.UpdateAsync(
                host.Id,
                "other",
                RecordType.A,
                300,
                "10.0.0.7"
            );
            updateClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();
            updateClash.Errors.Should().ContainSingle().Which.Message.Should().Contain("CNAME");

            var selfKeep = await service.UpdateAsync(
                target.Id,
                "other",
                RecordType.CNAME,
                600,
                "target.example.com."
            );
            selfKeep.Success.Should().BeTrue();
        }

        [Fact]
        public async Task should_enforce_ten_record_ceiling()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);
            for (var i = 1; i <= 6; i++)
            {
                (await service.CreateAsync(zoneId, $"h{i}", RecordType.A, 300, $"10.0.0.{i}"))
                    .Success.Should()
                    .BeTrue();
            }

            var over = await service.CreateAsync(
                zoneId,
                "one-too-many",
                RecordType.A,
                300,
                "10.9.9.9"
            );
            over.Success.Should().BeFalse();
            over.HasError(ErrorKind.RuleViolation).Should().BeTrue();
            over.Errors.Should().ContainSingle().Which.Message.Should().Contain("10 records");

            var existing = (await service.ListAsync(zoneId, "h1", null)).Data!.Records.Single();
            var selfUpdate = await service.UpdateAsync(
                existing.Id,
                "h1",
                RecordType.A,
                999,
                "10.0.0.1"
            );
            selfUpdate.Success.Should().BeTrue();
        }

        [Fact]
        public async Task should_enforce_four_ns_floor_on_delete_and_update()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);
            var victim = (
                await service.ListAsync(zoneId, null, RecordType.NS)
            ).Data!.Records.First();

            var blocked = await service.DeleteAsync(victim.Id);
            blocked.Success.Should().BeFalse();
            blocked.HasError(ErrorKind.RuleViolation).Should().BeTrue();
            blocked.Errors.Should().ContainSingle().Which.Message.Should().Contain("4 NS");

            var convertBlocked = await service.UpdateAsync(
                victim.Id,
                "@",
                RecordType.A,
                300,
                "10.0.0.1"
            );
            convertBlocked.Success.Should().BeFalse();

            var extra = await service.CreateAsync(
                zoneId,
                "@",
                RecordType.NS,
                300,
                "ns5.example.com."
            );
            extra.Success.Should().BeTrue();
            (await service.DeleteAsync(victim.Id)).Success.Should().BeTrue();

            var survivor = (
                await service.ListAsync(zoneId, null, RecordType.NS)
            ).Data!.Records.First();
            var convert = await service.UpdateAsync(
                survivor.Id,
                "@",
                RecordType.NS,
                400,
                survivor.Data
            );
            convert.Success.Should().BeTrue();
        }

        [Fact]
        public async Task should_filter_list_and_report_meter()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);
            await service.CreateAsync(zoneId, "www", RecordType.A, 300, "10.0.0.1");

            var all = await service.ListAsync(null, null, null);
            all.Data!.Records.Should().HaveCount(5);
            all.Data.RecordCount.Should().Be(0);
            all.Data.ZoneName.Should().BeNull();

            var zoned = await service.ListAsync(zoneId, null, null);
            zoned.Data!.ZoneName.Should().Be("svc.example");
            zoned.Data.RecordCount.Should().Be(5);
            zoned.Data.NsCount.Should().Be(4);

            (await service.ListAsync(999, null, null))
                .HasError(ErrorKind.NotFound)
                .Should()
                .BeTrue();
            (await service.ListAsync(null, "@", null))
                .Data!.Records.Should()
                .OnlyContain(r => r.Name == "@");
            (await service.ListAsync(null, "ns1.svc", null))
                .Data!.Records.Should()
                .OnlyContain(r => r.Data.Contains("ns1.svc"));
            (await service.ListAsync(null, "zzz-no-match", null)).Data!.Records.Should().BeEmpty();
            (await service.ListAsync(null, null, RecordType.A))
                .Data!.Records.Should()
                .OnlyContain(r => r.Type == RecordType.A);
        }

        [Fact]
        public async Task should_export_csv_with_quoting_and_fail_for_missing_zone()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var zoneId = await SeededZoneAsync(uow);
            await service.CreateAsync(zoneId, "comma", RecordType.TXT, 300, "a,b");
            await service.CreateAsync(zoneId, "quoted", RecordType.TXT, 300, "say \"hi\"");

            var csv = await service.ExportCsvAsync(zoneId, null, null);
            csv.Success.Should().BeTrue();
            csv.Data!.Should().StartWith("FQDN,Zone,Name,Type,TTL,Data\n");
            csv.Data.Should().Contain("svc.example,svc.example,@,NS,172800,ns1.svc.example.");
            csv.Data.Should().Contain("\"a,b\"");
            csv.Data.Should().Contain("\"say \"\"hi\"\"\"");

            (await service.ExportCsvAsync(999, null, null))
                .HasError(ErrorKind.NotFound)
                .Should()
                .BeTrue();
        }
    }
}
