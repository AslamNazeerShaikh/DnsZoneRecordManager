using System.Text.RegularExpressions;

namespace DnsZoneRecordManager.Validation
{
    /// <summary>Shared DNS shape rules and normalization used by command validators and handlers.</summary>
    public static class DnsRules
    {
        /// <summary>Zone/record-owner hostname pattern: dot-separated labels, each 1–63 chars, no leading/trailing hyphen or dot, 253 chars max.</summary>
        public const string HostnamePattern = "^(?=.{1,253}$)(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\\.(?!-)[A-Za-z0-9-]{1,63}(?<!-))*$";

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

        /// <summary>Normalizes a zone name: trims, drops the trailing dot, lowercases.</summary>
        /// <param name="name">Raw name.</param>
        /// <returns>Normalized name.</returns>
        public static string NormalizeZoneName(string name)
        {
            return name.Trim().TrimEnd('.').ToLowerInvariant();
        }

        /// <summary>Normalizes an owner name: trims and lowercases (@ survives untouched).</summary>
        /// <param name="name">Raw name.</param>
        /// <returns>Normalized name.</returns>
        public static string NormalizeRecordName(string name)
        {
            return name.Trim().ToLowerInvariant();
        }

        /// <summary>Derives the FQDN (@ maps to the zone apex).</summary>
        /// <param name="name">Owner name.</param>
        /// <param name="zoneName">Zone name.</param>
        /// <returns>Fully-qualified name.</returns>
        public static string ToFqdn(string name, string zoneName)
        {
            return name == "@" ? zoneName : $"{name}.{zoneName}";
        }
    }
}
