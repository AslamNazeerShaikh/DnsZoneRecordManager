using DnsZoneRecordManager.Cqrs;
using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Models;
using FluentAssertions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>DTO/command record member tests (equality, ToString, hash, deconstruction).</summary>
    public class RecordTypesTests
    {
        [Fact]
        public void should_compare_dtos_by_value()
        {
            var now = DateTime.UtcNow;
            var zone = new ZoneDto(1, "svc.example", 5, 4, now, now);
            var sameZone = new ZoneDto(1, "svc.example", 5, 4, now, now);
            (zone == sameZone).Should().BeTrue();
            (zone != sameZone with { Name = "other.example" }).Should().BeTrue();
            zone.GetHashCode().Should().Be(sameZone.GetHashCode());
            zone.ToString().Should().Contain("svc.example");
            var (zid, zname, zcount, zns, zcreated, zupdated) = zone;
            zid.Should().Be(1);
            zname.Should().Be("svc.example");
            zcount.Should().Be(5);
            zns.Should().Be(4);
            zcreated.Should().Be(now);
            zupdated.Should().Be(now);

            var record = new RecordDto(2, 1, "svc.example", "www", "www.svc.example", RecordType.A, 300, "10.0.0.1", now);
            var sameRecord = record with { Ttl = 300 };
            (record == sameRecord).Should().BeTrue();
            (record != sameRecord with { Data = "10.0.0.2" }).Should().BeTrue();
            record.GetHashCode().Should().Be(sameRecord.GetHashCode());
            record.ToString().Should().Contain("www.svc.example");
            var (rid, rzid, rzname, rname, rfqdn, rtype, rttl, rdata, rupdated) = record;
            rid.Should().Be(2);
            rzid.Should().Be(1);
            rzname.Should().Be("svc.example");
            rname.Should().Be("www");
            rfqdn.Should().Be("www.svc.example");
            rtype.Should().Be(RecordType.A);
            rttl.Should().Be(300);
            rdata.Should().Be("10.0.0.1");
            rupdated.Should().Be(now);
        }

        [Fact]
        public void should_compare_zone_commands_by_value()
        {
            var list = new ListZonesQuery("svc");
            (list == new ListZonesQuery("svc")).Should().BeTrue();
            (list != new ListZonesQuery("other")).Should().BeTrue();
            list.GetHashCode().Should().Be(new ListZonesQuery("svc").GetHashCode());
            list.ToString().Should().Contain("svc");

            var get = new GetZoneQuery(3);
            (get == new GetZoneQuery(3)).Should().BeTrue();
            get.ToString().Should().Contain("3");
            get.Deconstruct(out var gid);
            gid.Should().Be(3);

            var create = new CreateZoneCommand("svc.example");
            (create == new CreateZoneCommand("svc.example")).Should().BeTrue();
            (create != create with { Name = "other.example" }).Should().BeTrue();
            create.GetHashCode().Should().Be(new CreateZoneCommand("svc.example").GetHashCode());
            create.ToString().Should().Contain("svc.example");

            var rename = new RenameZoneCommand(3, "svc.example");
            (rename == new RenameZoneCommand(3, "svc.example")).Should().BeTrue();
            rename.ToString().Should().Contain("svc.example");
            var (rid, rname) = rename;
            rid.Should().Be(3);
            rname.Should().Be("svc.example");

            var delete = new DeleteZoneCommand(3);
            (delete == new DeleteZoneCommand(3)).Should().BeTrue();
            delete.ToString().Should().Contain("3");
            delete.Deconstruct(out var did);
            did.Should().Be(3);
        }

        [Fact]
        public void should_compare_record_commands_by_value()
        {
            var list = new ListRecordsQuery(1, "svc", RecordType.A);
            (list == new ListRecordsQuery(1, "svc", RecordType.A)).Should().BeTrue();
            (list != list with { Search = "other" }).Should().BeTrue();
            list.GetHashCode().Should().Be(new ListRecordsQuery(1, "svc", RecordType.A).GetHashCode());
            list.ToString().Should().Contain("svc");

            var get = new GetRecordQuery(4);
            (get == new GetRecordQuery(4)).Should().BeTrue();
            get.ToString().Should().Contain("4");
            get.Deconstruct(out var gid);
            gid.Should().Be(4);

            var create = new CreateRecordCommand(1, "www", RecordType.A, 300, "10.0.0.1");
            (create == new CreateRecordCommand(1, "www", RecordType.A, 300, "10.0.0.1")).Should().BeTrue();
            (create != create with { Data = "10.0.0.2" }).Should().BeTrue();
            create.GetHashCode().Should().Be(new CreateRecordCommand(1, "www", RecordType.A, 300, "10.0.0.1").GetHashCode());
            create.ToString().Should().Contain("www");

            var update = new UpdateRecordCommand(4, "www", RecordType.A, 300, "10.0.0.1");
            (update == new UpdateRecordCommand(4, "www", RecordType.A, 300, "10.0.0.1")).Should().BeTrue();
            update.ToString().Should().Contain("www");
            var (uid, uname, utype, uttl, udata) = update;
            uid.Should().Be(4);
            uname.Should().Be("www");
            utype.Should().Be(RecordType.A);
            uttl.Should().Be(300);
            udata.Should().Be("10.0.0.1");

            var delete = new DeleteRecordCommand(4);
            (delete == new DeleteRecordCommand(4)).Should().BeTrue();
            delete.ToString().Should().Contain("4");
            delete.Deconstruct(out var did);
            did.Should().Be(4);

            var export = new ExportRecordsQuery(null, null, null);
            (export == new ExportRecordsQuery(null, null, null)).Should().BeTrue();
            export.ToString().Should().Contain("ExportRecordsQuery");
        }
    }
}
