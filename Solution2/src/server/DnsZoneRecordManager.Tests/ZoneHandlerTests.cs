using DnsZoneRecordManager.Cqrs.Handlers;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Zone handler tests: normalization, validation, duplicates, rename, delete, search.</summary>
    public class ZoneHandlerTests
    {
        private static ListZonesHandler Lister(AppDbContext context)
        {
            return new ListZonesHandler(context);
        }

        [Fact]
        public async Task should_create_list_search_and_count_zones()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = new CreateZoneHandler(context, new CreateZoneValidator(), NullLogger<CreateZoneHandler>.Instance);
                    var created = await creator.HandleAsync(new CreateZoneCommand("Example.COM."), CancellationToken.None);
                    created.Success.Should().BeTrue();
                    created.Data!.Name.Should().Be("example.com");
                    created.Data.CreatedUtc.Kind.Should().Be(DateTimeKind.Utc);

                    var lister = Lister(context);
                    (await lister.HandleAsync(new ListZonesQuery(null), CancellationToken.None)).Data!.Should().HaveCount(1);
                    (await lister.HandleAsync(new ListZonesQuery("EXAMPLE"), CancellationToken.None)).Data!.Should().HaveCount(1);
                    (await lister.HandleAsync(new ListZonesQuery("zzz"), CancellationToken.None)).Data!.Should().BeEmpty();
                }
            }
        }

        [Fact]
        public async Task should_reject_invalid_and_duplicate_zone_names()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = new CreateZoneHandler(context, new CreateZoneValidator(), NullLogger<CreateZoneHandler>.Instance);
                    (await creator.HandleAsync(new CreateZoneCommand(""), CancellationToken.None)).Success.Should().BeFalse();
                    (await creator.HandleAsync(new CreateZoneCommand("bad name!"), CancellationToken.None)).Success.Should().BeFalse();

                    (await creator.HandleAsync(new CreateZoneCommand("dup.example"), CancellationToken.None)).Success.Should().BeTrue();
                    var duplicate = await creator.HandleAsync(new CreateZoneCommand("DUP.example."), CancellationToken.None);
                    duplicate.Success.Should().BeFalse();
                    duplicate.HasError(ErrorKind.Conflict).Should().BeTrue();
                    duplicate.HasError(ErrorKind.NotFound).Should().BeFalse();
                    duplicate.Errors.Should().ContainSingle().Which.Message.Should().Contain("already exists");
                }
            }
        }

        [Fact]
        public async Task should_get_rename_and_delete_zones()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var creator = new CreateZoneHandler(context, new CreateZoneValidator(), NullLogger<CreateZoneHandler>.Instance);
                    var renamer = new RenameZoneHandler(context, new RenameZoneValidator(), NullLogger<RenameZoneHandler>.Instance);
                    var getter = new GetZoneHandler(context);
                    var deleter = new DeleteZoneHandler(context, NullLogger<DeleteZoneHandler>.Instance);

                    var created = (await creator.HandleAsync(new CreateZoneCommand("old.example"), CancellationToken.None)).Data!;

                    var missing = await getter.HandleAsync(new GetZoneQuery(999), CancellationToken.None);
                    missing.Success.Should().BeFalse();
                    missing.HasError(ErrorKind.NotFound).Should().BeTrue();
                    (await getter.HandleAsync(new GetZoneQuery(created.Id), CancellationToken.None)).Data!.Name.Should().Be("old.example");

                    (await renamer.HandleAsync(new RenameZoneCommand(999, "new.example"), CancellationToken.None)).Success.Should().BeFalse();
                    (await renamer.HandleAsync(new RenameZoneCommand(created.Id, "bad name!"), CancellationToken.None)).Success.Should().BeFalse();
                    await creator.HandleAsync(new CreateZoneCommand("taken.example"), CancellationToken.None);
                    var clash = await renamer.HandleAsync(new RenameZoneCommand(created.Id, "TAKEN.example"), CancellationToken.None);
                    clash.Success.Should().BeFalse();
                    clash.HasError(ErrorKind.Conflict).Should().BeTrue();

                    var renamed = await renamer.HandleAsync(new RenameZoneCommand(created.Id, "New.EXAMPLE."), CancellationToken.None);
                    renamed.Success.Should().BeTrue();
                    renamed.Data!.Name.Should().Be("new.example");
                    renamed.Data.RecordCount.Should().Be(0);

                    (await deleter.HandleAsync(new DeleteZoneCommand(999), CancellationToken.None)).Success.Should().BeFalse();
                    (await deleter.HandleAsync(new DeleteZoneCommand(created.Id), CancellationToken.None)).Data.Should().Be(created.Id);
                    (await getter.HandleAsync(new GetZoneQuery(created.Id), CancellationToken.None)).Success.Should().BeFalse();
                }
            }
        }

        [Fact]
        public async Task should_count_records_and_ns_records()
        {
            var (context, connection) = TestDb.New();
            await using (context)
            {
                await using (connection)
                {
                    var id = await TestDb.SeedZoneAsync(context, "count.example");
                    var row = (await Lister(context).HandleAsync(new ListZonesQuery(null), CancellationToken.None)).Data!.Single();
                    row.RecordCount.Should().Be(4);
                    row.NsCount.Should().Be(4);
                    row.Id.Should().Be(id);
                }
            }
        }
    }
}
