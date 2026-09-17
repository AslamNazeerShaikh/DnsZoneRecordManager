using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Zone service tests: normalization, validation, duplicates, rename, delete, search.</summary>
    public class ZoneServiceTests
    {
        private static ZoneService Service(Data.IUnitOfWork uow)
        {
            return new ZoneService(uow, new ZoneValidator(), NullLogger<ZoneService>.Instance);
        }

        [Fact]
        public void should_normalize_names_to_lowercase_without_trailing_dot()
        {
            ZoneService.NormalizeZoneName("  NAHUEXOLAB.COM. ").Should().Be("nahuexolab.com");
        }

        [Fact]
        public async Task should_create_and_list_zones_with_counts()
        {
            var service = Service(TestUow.New());
            var created = await service.CreateAsync("Example.COM.");
            created.Success.Should().BeTrue();
            created.Data!.Name.Should().Be("example.com");
            created.Data.CreatedUtc.Kind.Should().Be(DateTimeKind.Utc);

            var list = await service.ListAsync(null);
            list.Data!.Should().HaveCount(1);
            list.Data[0]
                .Should()
                .Be(
                    new ZoneListItem(
                        created.Data.Id,
                        "example.com",
                        0,
                        0,
                        created.Data.CreatedUtc,
                        created.Data.UpdatedUtc
                    )
                );
        }

        [Fact]
        public async Task should_reject_invalid_and_duplicate_zone_names()
        {
            var service = Service(TestUow.New());
            (await service.CreateAsync("")).Success.Should().BeFalse();
            (await service.CreateAsync("bad name!")).Success.Should().BeFalse();

            (await service.CreateAsync("dup.example")).Success.Should().BeTrue();
            var duplicate = await service.CreateAsync("DUP.example.");
            duplicate.Success.Should().BeFalse();
            duplicate.Errors.Should().ContainSingle().Which.Should().Contain("already exists");
        }

        [Fact]
        public async Task should_search_case_insensitively_and_report_no_match()
        {
            var service = Service(TestUow.New());
            await service.CreateAsync("alpha.example");
            await service.CreateAsync("beta.example");

            (await service.ListAsync("ALPHA")).Data!.Should().HaveCount(1);
            (await service.ListAsync("zzz")).Data!.Should().BeEmpty();
        }

        [Fact]
        public async Task should_get_rename_and_delete_zones()
        {
            var service = Service(TestUow.New());
            var created = (await service.CreateAsync("old.example")).Data!;

            (await service.GetAsync(999)).Success.Should().BeFalse();
            (await service.GetAsync(created.Id)).Data!.Name.Should().Be("old.example");

            (await service.RenameAsync(999, "new.example")).Success.Should().BeFalse();
            (await service.RenameAsync(created.Id, "bad name!")).Success.Should().BeFalse();
            await service.CreateAsync("taken.example");
            var clash = await service.RenameAsync(created.Id, "TAKEN.example");
            clash.Success.Should().BeFalse();
            clash.Errors.Should().ContainSingle().Which.Should().Contain("already exists");

            var renamed = await service.RenameAsync(created.Id, "New.EXAMPLE.");
            renamed.Success.Should().BeTrue();
            renamed.Data!.Name.Should().Be("new.example");
            renamed.Data.UpdatedUtc.Kind.Should().Be(DateTimeKind.Utc);

            (await service.DeleteAsync(999)).Success.Should().BeFalse();
            (await service.DeleteAsync(created.Id)).Data.Should().Be(created.Id);
            (await service.GetAsync(created.Id)).Success.Should().BeFalse();
        }

        [Fact]
        public async Task should_count_records_and_ns_records()
        {
            var uow = TestUow.New();
            var service = Service(uow);
            var id = await TestUow.SeedZoneAsync(uow, "count.example");
            var row = (await service.ListAsync(null)).Data!.Single();
            row.RecordCount.Should().Be(4);
            row.NsCount.Should().Be(4);
            id.Should().Be(row.Id);
        }
    }
}
