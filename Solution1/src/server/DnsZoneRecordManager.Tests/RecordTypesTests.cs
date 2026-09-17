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
    }
}
