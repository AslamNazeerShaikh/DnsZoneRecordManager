namespace DnsZoneRecordManager.Models
{
    /// <summary>A DNS zone (e.g. nahuexolab.com) holding up to 10 records with at least 4 NS records.</summary>
    public class DnsZone
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }

        /// <summary>Zone name, lowercase without trailing dot (e.g. nahuexolab.com). Unique (case-insensitive).</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>UTC creation timestamp.</summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>UTC last-update timestamp.</summary>
        public DateTime UpdatedUtc { get; set; }

        /// <summary>Records in this zone.</summary>
        public ICollection<DnsRecord> Records { get; set; } = new List<DnsRecord>();
    }
}
