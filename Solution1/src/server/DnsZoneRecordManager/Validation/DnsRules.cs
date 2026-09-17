using System.Text.RegularExpressions;
using DnsZoneRecordManager.Models;

namespace DnsZoneRecordManager.Validation
{
    /// <summary>Shared DNS shape rules used by the FluentValidation validators (mirrored client-side by DataAnnotations).</summary>
    public static class DnsRules
    {
        /// <summary>Zone/record-owner hostname pattern: dot-separated labels, each 1–63 chars, no leading/trailing hyphen or dot, 253 chars max.</summary>
        public const string HostnamePattern =
            "^(?=.{1,253}$)(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\\.(?!-)[A-Za-z0-9-]{1,63}(?<!-))*$";

        /// <summary>Record owner-name pattern: @ for the apex, or a hostname optionally starting with _ (e.g. _dmarc).</summary>
        public const string RecordNamePattern = "^(@|_?[A-Za-z0-9]([A-Za-z0-9.-]*[A-Za-z0-9])?)$";

        /// <summary>Maximum total DNS records per zone (assumption A2).</summary>
        public const int MaxRecordsPerZone = 10;

        /// <summary>Minimum NS records per zone (assumption A1).</summary>
        public const int MinNsPerZone = 4;

        private static readonly Regex HostnameRegex = new(HostnamePattern, RegexOptions.Compiled);

        /// <summary>Checks a hostname (trailing dot allowed, e.g. ns1-33.azure-dns.com.).</summary>
        /// <param name="value">Hostname to check.</param>
        /// <returns>True when the value is a valid hostname.</returns>
        public static bool IsValidHostname(string value)
        {
            return HostnameRegex.IsMatch(value.TrimEnd('.'));
        }
    }
}
