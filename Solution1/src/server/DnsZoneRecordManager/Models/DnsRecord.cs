namespace DnsZoneRecordManager.Models
{
    /// <summary>Allowed DNS record types (assumption A3).</summary>
    public enum RecordType
    {
        /// <summary>IPv4 host address.</summary>
        A = 0,

        /// <summary>IPv6 host address.</summary>
        AAAA = 1,

        /// <summary>Canonical name (exclusive on its owner name).</summary>
        CNAME = 2,

        /// <summary>Name server (at least 4 per zone, assumption A1).</summary>
        NS = 3,

        /// <summary>Text record.</summary>
        TXT = 4,
    }

    /// <summary>A single DNS record inside a <see cref="DnsZone"/>.</summary>
    public class DnsRecord
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }

        /// <summary>Owning zone id (foreign key, cascade delete).</summary>
        public int ZoneId { get; set; }

        /// <summary>Owning zone navigation.</summary>
        public DnsZone Zone { get; set; } = null!;

        /// <summary>Owner name within the zone (@ = apex, e.g. _dmarc, www).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Record type (A/AAAA/CNAME/NS/TXT).</summary>
        public RecordType Type { get; set; }

        /// <summary>Time-to-live in seconds.</summary>
        public int Ttl { get; set; }

        /// <summary>RDATA value (IP, hostname, or text depending on <see cref="Type"/>).</summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>UTC creation timestamp.</summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>UTC last-update timestamp.</summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
