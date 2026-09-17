using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using FluentAssertions;

namespace DnsZoneRecordManager.Tests
{
    /// <summary>Record-type member tests (equality, ToString, hash, deconstruction).</summary>
    public class RecordTypesTests
    {
        [Fact]
        public void should_compare_service_errors_by_value()
        {
            var a = new ServiceError(ErrorKind.Conflict, "dup");
            var b = new ServiceError(ErrorKind.Conflict, "dup");

            (a == b).Should().BeTrue();
            (a != new ServiceError(ErrorKind.NotFound, "missing")).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.ToString().Should().Contain("Conflict");
            var (kind, message) = a;
            kind.Should().Be(ErrorKind.Conflict);
            message.Should().Be("dup");
            a.Should().Be(b with { Message = "dup" });
        }

        [Fact]
        public void should_compare_zone_list_items_by_value()
        {
            var now = DateTime.UtcNow;
            var a = new ZoneListItem(1, "svc.example", 5, 4, now, now);
            var b = new ZoneListItem(1, "svc.example", 5, 4, now, now);
            var c = new ZoneListItem(2, "other.example", 0, 0, now, now);

            (a == b).Should().BeTrue();
            (a != c).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.ToString().Should().Contain("svc.example");
            var (id, name, count, ns, created, updated) = a;
            id.Should().Be(1);
            name.Should().Be("svc.example");
            count.Should().Be(5);
            ns.Should().Be(4);
            created.Should().Be(now);
            updated.Should().Be(now);
            a.Should().Be(b with { Name = "svc.example" });
        }

        [Fact]
        public void should_compare_record_rows_by_value()
        {
            var now = DateTime.UtcNow;
            var a = new RecordRow(
                1,
                7,
                "svc.example",
                "@",
                "svc.example",
                RecordType.NS,
                172800,
                "ns1.",
                now
            );
            var b = a with { Ttl = 172800 };

            (a == b).Should().BeTrue();
            (a != b with { Ttl = 300 }).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.ToString().Should().Contain("svc.example");
            var (id, zoneId, zone, name, fqdn, type, ttl, data, updated) = a;
            id.Should().Be(1);
            zoneId.Should().Be(7);
            zone.Should().Be("svc.example");
            name.Should().Be("@");
            fqdn.Should().Be("svc.example");
            type.Should().Be(RecordType.NS);
            ttl.Should().Be(172800);
            data.Should().Be("ns1.");
            updated.Should().Be(now);
        }

        [Fact]
        public void should_compare_record_list_data_by_value()
        {
            var rows = new List<RecordRow>();
            var a = new RecordListData(3, "svc.example", rows, 5, 4);
            var b = new RecordListData(3, "svc.example", rows, 5, 4);

            (a == b).Should().BeTrue();
            (a != b with { RecordCount = 1 }).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
            a.ToString().Should().Contain("svc.example");
            var (zoneId, zoneName, records, count, ns) = a;
            zoneId.Should().Be(3);
            zoneName.Should().Be("svc.example");
            records.Should().BeEmpty();
            count.Should().Be(5);
            ns.Should().Be(4);
        }

        [Fact]
        public void should_compare_api_dtos_by_value()
        {
            var now = DateTime.UtcNow;
            var zone = new Controllers.Api.ZoneDto(1, "svc.example", 5, 4, now, now);
            var sameZone = new Controllers.Api.ZoneDto(1, "svc.example", 5, 4, now, now);
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

            var record = new Controllers.Api.RecordDto(
                2,
                1,
                "svc.example",
                "www",
                "www.svc.example",
                RecordType.A,
                300,
                "10.0.0.1",
                now
            );
            var sameRecord = record with { Ttl = 300 };
            (record == sameRecord).Should().BeTrue();
            (record != sameRecord with { Data = "10.0.0.2" }).Should().BeTrue();
            record.GetHashCode().Should().Be(sameRecord.GetHashCode());
            record.ToString().Should().Contain("www.svc.example");
        }

        [Fact]
        public void should_compare_api_request_dtos_by_value()
        {
            var create = new Controllers.Api.CreateRecordRequest
            {
                ZoneId = 1,
                Name = "www",
                Type = RecordType.A,
                Ttl = 300,
                Data = "10.0.0.1",
            };
            var same = create with { Ttl = 300 };
            (create == same).Should().BeTrue();
            (create != same with { Data = "10.0.0.2" }).Should().BeTrue();
            create.GetHashCode().Should().Be(same.GetHashCode());
            create.ToString().Should().Contain("www");

            var zoneRequest = new Controllers.Api.CreateZoneRequest { Name = "svc.example" };
            (zoneRequest == new Controllers.Api.CreateZoneRequest { Name = "svc.example" })
                .Should()
                .BeTrue();
            (zoneRequest != new Controllers.Api.CreateZoneRequest { Name = "other.example" })
                .Should()
                .BeTrue();
            zoneRequest.ToString().Should().Contain("svc.example");

            var rename = new Controllers.Api.RenameZoneRequest { Name = "svc.example" };
            (rename == new Controllers.Api.RenameZoneRequest { Name = "svc.example" })
                .Should()
                .BeTrue();
            rename.ToString().Should().Contain("svc.example");

            var update = new Controllers.Api.UpdateRecordRequest
            {
                Name = "www",
                Type = RecordType.A,
                Ttl = 300,
                Data = "10.0.0.1",
            };
            (
                update
                == new Controllers.Api.UpdateRecordRequest
                {
                    Name = "www",
                    Type = RecordType.A,
                    Ttl = 300,
                    Data = "10.0.0.1",
                }
            ).Should().BeTrue();
            (update != update with { Ttl = 600 }).Should().BeTrue();
            update.GetHashCode().Should().Be((update with { Ttl = 300 }).GetHashCode());
            update.ToString().Should().Contain("www");
        }
    }
}
