using DnsZoneRecordManager.Cqrs.Handlers;
using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Record handler tests: A1/A2 limits, CNAME exclusivity, duplicates, filters, CSV.</summary>
    public class RecordHandlerTests
    {
        private static async Task<int> SeededZoneAsync(AppDbContext context)
        {
            return await TestDb.SeedZoneAsync(context, "svc.example");
        }

        private static CreateRecordHandler Creator(AppDbContext context)
        {
            return new CreateRecordHandler(context, new CreateRecordValidator(), NullLogger<CreateRecordHandler>.Instance);
        }

        [Fact]
        public async Task should_create_get_update_and_delete_records()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var updater = new UpdateRecordHandler(context, new UpdateRecordValidator(), NullLogger<UpdateRecordHandler>.Instance);
                    var getter = new GetRecordHandler(context);
                    var deleter = new DeleteRecordHandler(context, NullLogger<DeleteRecordHandler>.Instance);
                    var zoneId = await SeededZoneAsync(context);

                    var created = await creator.HandleAsync(new CreateRecordCommand(zoneId, "WWW", RecordType.A, 300, "10.0.0.1"), CancellationToken.None);
                    created.Success.Should().BeTrue();
                    created.Data!.Name.Should().Be("www");

                    (await getter.HandleAsync(new GetRecordQuery(999), CancellationToken.None)).Success.Should().BeFalse();
                    (await getter.HandleAsync(new GetRecordQuery(created.Data.Id), CancellationToken.None)).Data!.Data.Should().Be("10.0.0.1");

                    var updated = await updater.HandleAsync(new UpdateRecordCommand(created.Data.Id, "www", RecordType.A, 600, "10.0.0.2"), CancellationToken.None);
                    updated.Success.Should().BeTrue();
                    updated.Data!.Ttl.Should().Be(600);

                    (await deleter.HandleAsync(new DeleteRecordCommand(999), CancellationToken.None)).Success.Should().BeFalse();
                    (await deleter.HandleAsync(new DeleteRecordCommand(created.Data.Id), CancellationToken.None)).Data.Should().Be(created.Data.Id);
                    (await getter.HandleAsync(new GetRecordQuery(created.Data.Id), CancellationToken.None)).Success.Should().BeFalse();
                }
            }
        }

        [Fact]
        public async Task should_reject_missing_zone_invalid_data_and_duplicates()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var updater = new UpdateRecordHandler(context, new UpdateRecordValidator(), NullLogger<UpdateRecordHandler>.Instance);
                    var zoneId = await SeededZoneAsync(context);

                    var missingZone = await creator.HandleAsync(new CreateRecordCommand(999, "www", RecordType.A, 300, "10.0.0.1"), CancellationToken.None);
                    missingZone.HasError(ErrorKind.NotFound).Should().BeTrue();
                    (await creator.HandleAsync(new CreateRecordCommand(zoneId, "www", RecordType.A, 300, "not-an-ip"), CancellationToken.None)).Success.Should()
                        .BeFalse();

                    (await creator.HandleAsync(new CreateRecordCommand(zoneId, "dup", RecordType.A, 300, "10.0.0.9"), CancellationToken.None)).Success.Should()
                        .BeTrue();
                    var duplicate = await creator.HandleAsync(new CreateRecordCommand(zoneId, "DUP", RecordType.A, 300, "10.0.0.9"), CancellationToken.None);
                    duplicate.HasError(ErrorKind.Conflict).Should().BeTrue();
                    duplicate.Errors.Should().ContainSingle().Which.Message.Should().Contain("already exists");

                    var missingRecord = await updater.HandleAsync(new UpdateRecordCommand(999, "www", RecordType.A, 300, "10.0.0.1"), CancellationToken.None);
                    missingRecord.HasError(ErrorKind.NotFound).Should().BeTrue();
                    (await updater.HandleAsync(new UpdateRecordCommand(zoneId, "www", RecordType.A, 0, "10.0.0.1"), CancellationToken.None)).Success.Should()
                        .BeFalse();
                }
            }
        }

        [Fact]
        public async Task should_enforce_cname_exclusivity_both_ways_on_create_and_update()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var updater = new UpdateRecordHandler(context, new UpdateRecordValidator(), NullLogger<UpdateRecordHandler>.Instance);
                    var zoneId = await SeededZoneAsync(context);
                    var host = (await creator.HandleAsync(new CreateRecordCommand(zoneId, "alias", RecordType.A, 300, "10.0.0.5"), CancellationToken.None)).Data!;

                    var cnameClash = await creator.HandleAsync(new CreateRecordCommand(zoneId, "alias", RecordType.CNAME, 300, "target.example.com."), CancellationToken.None);
                    cnameClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();

                    var target = (
                        await creator.HandleAsync(new CreateRecordCommand(zoneId, "other", RecordType.CNAME, 300, "target.example.com."), CancellationToken.None)
                    ).Data!;
                    var hostClash = await creator.HandleAsync(new CreateRecordCommand(zoneId, "other", RecordType.A, 300, "10.0.0.6"), CancellationToken.None);
                    hostClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();

                    var updateClash = await updater.HandleAsync(new UpdateRecordCommand(host.Id, "other", RecordType.A, 300, "10.0.0.7"), CancellationToken.None);
                    updateClash.HasError(ErrorKind.RuleViolation).Should().BeTrue();

                    var selfKeep = await updater.HandleAsync(new UpdateRecordCommand(target.Id, "other", RecordType.CNAME, 600, "target.example.com."), CancellationToken.None);
                    selfKeep.Success.Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task should_enforce_ten_record_ceiling()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var updater = new UpdateRecordHandler(context, new UpdateRecordValidator(), NullLogger<UpdateRecordHandler>.Instance);
                    var lister = new ListRecordsHandler(context);
                    var zoneId = await SeededZoneAsync(context);
                    for (var i = 1; i <= 6; i++)
                    {
                        (await creator.HandleAsync(new CreateRecordCommand(zoneId, $"h{i}", RecordType.A, 300, $"10.0.0.{i}"), CancellationToken.None)).Success.Should()
                            .BeTrue();
                    }

                    var over = await creator.HandleAsync(new CreateRecordCommand(zoneId, "one-too-many", RecordType.A, 300, "10.9.9.9"), CancellationToken.None);
                    over.Success.Should().BeFalse();
                    over.HasError(ErrorKind.RuleViolation).Should().BeTrue();

                    var existing = (await lister.HandleAsync(new ListRecordsQuery(zoneId, "h1", null), CancellationToken.None)).Data!.Single();
                    var selfUpdate = await updater.HandleAsync(new UpdateRecordCommand(existing.Id, "h1", RecordType.A, 999, "10.0.0.1"), CancellationToken.None);
                    selfUpdate.Success.Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task should_enforce_four_ns_floor_on_delete_and_update()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var updater = new UpdateRecordHandler(context, new UpdateRecordValidator(), NullLogger<UpdateRecordHandler>.Instance);
                    var lister = new ListRecordsHandler(context);
                    var deleter = new DeleteRecordHandler(context, NullLogger<DeleteRecordHandler>.Instance);
                    var zoneId = await SeededZoneAsync(context);
                    var victim = (await lister.HandleAsync(new ListRecordsQuery(zoneId, null, RecordType.NS), CancellationToken.None)).Data!.First();

                    var blocked = await deleter.HandleAsync(new DeleteRecordCommand(victim.Id), CancellationToken.None);
                    blocked.Success.Should().BeFalse();
                    blocked.HasError(ErrorKind.RuleViolation).Should().BeTrue();

                    var convertBlocked = await updater.HandleAsync(new UpdateRecordCommand(victim.Id, "@", RecordType.A, 300, "10.0.0.1"), CancellationToken.None);
                    convertBlocked.Success.Should().BeFalse();

                    var extra = await creator.HandleAsync(new CreateRecordCommand(zoneId, "@", RecordType.NS, 300, "ns5.example.com."), CancellationToken.None);
                    extra.Success.Should().BeTrue();
                    (await deleter.HandleAsync(new DeleteRecordCommand(victim.Id), CancellationToken.None)).Success.Should().BeTrue();

                    var survivor = (await lister.HandleAsync(new ListRecordsQuery(zoneId, null, RecordType.NS), CancellationToken.None)).Data!.First();
                    var convert = await updater.HandleAsync(new UpdateRecordCommand(survivor.Id, "@", RecordType.NS, 400, survivor.Data), CancellationToken.None);
                    convert.Success.Should().BeTrue();
                }
            }
        }

        [Fact]
        public async Task should_filter_list_and_report_not_found()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var lister = new ListRecordsHandler(context);
                    var zoneId = await SeededZoneAsync(context);
                    await creator.HandleAsync(new CreateRecordCommand(zoneId, "www", RecordType.A, 300, "10.0.0.1"), CancellationToken.None);

                    var all = await lister.HandleAsync(new ListRecordsQuery(null, null, null), CancellationToken.None);
                    all.Data!.Should().HaveCount(5);

                    var zoned = await lister.HandleAsync(new ListRecordsQuery(zoneId, null, null), CancellationToken.None);
                    zoned.Data!.Should().HaveCount(5);

                    (await lister.HandleAsync(new ListRecordsQuery(999, null, null), CancellationToken.None)).HasError(ErrorKind.NotFound).Should().BeTrue();
                    (await lister.HandleAsync(new ListRecordsQuery(null, "@", null), CancellationToken.None)).Data!.Should().OnlyContain(r => r.Name == "@");
                    (await lister.HandleAsync(new ListRecordsQuery(null, "ns1.svc", null), CancellationToken.None)).Data!.Should()
                        .OnlyContain(r => r.Data.Contains("ns1.svc"));
                    (await lister.HandleAsync(new ListRecordsQuery(null, "zzz-no-match", null), CancellationToken.None)).Data!.Should().BeEmpty();
                    (await lister.HandleAsync(new ListRecordsQuery(null, null, RecordType.A), CancellationToken.None)).Data!.Should()
                        .OnlyContain(r => r.Type == RecordType.A);
                }
            }
        }

        [Fact]
        public async Task should_export_csv_with_quoting_and_fail_for_missing_zone()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = Creator(context);
                    var exporter = new ExportRecordsHandler(context);
                    var zoneId = await SeededZoneAsync(context);
                    await creator.HandleAsync(new CreateRecordCommand(zoneId, "comma", RecordType.TXT, 300, "a,b"), CancellationToken.None);
                    await creator.HandleAsync(new CreateRecordCommand(zoneId, "quoted", RecordType.TXT, 300, "say \"hi\""), CancellationToken.None);

                    var csv = await exporter.HandleAsync(new ExportRecordsQuery(zoneId, null, null), CancellationToken.None);
                    csv.Success.Should().BeTrue();
                    csv.Data!.Should().StartWith("FQDN,Zone,Name,Type,TTL,Data\n");
                    csv.Data.Should().Contain("svc.example,svc.example,@,NS,172800,ns1.svc.example.");
                    csv.Data.Should().Contain("\"a,b\"");
                    csv.Data.Should().Contain("\"say \"\"hi\"\"\"");

                    (await exporter.HandleAsync(new ExportRecordsQuery(999, null, null), CancellationToken.None)).HasError(ErrorKind.NotFound).Should().BeTrue();
                }
            }
        }
    }
}
