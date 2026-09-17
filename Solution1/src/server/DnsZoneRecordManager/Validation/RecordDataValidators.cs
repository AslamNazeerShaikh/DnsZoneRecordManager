using System.Net;
using System.Net.Sockets;
using DnsZoneRecordManager.Models;

namespace DnsZoneRecordManager.Validation
{
    /// <summary>Strategy: validates RDATA against one record type (assumption A3). New types plug in without touching callers.</summary>
    public interface IRecordDataValidator
    {
        /// <summary>Checks one RDATA value.</summary>
        /// <param name="data">RDATA value.</param>
        /// <returns>True when the data fits the strategy's type.</returns>
        bool IsValid(string data);
    }

    /// <summary>Strategy for A records: dotted-decimal IPv4.</summary>
    public sealed class IPv4Validator : IRecordDataValidator
    {
        /// <inheritdoc />
        public bool IsValid(string data)
        {
            return IPAddress.TryParse(data, out var address)
                && address.AddressFamily == AddressFamily.InterNetwork;
        }
    }

    /// <summary>Strategy for AAAA records: IPv6.</summary>
    public sealed class IPv6Validator : IRecordDataValidator
    {
        /// <inheritdoc />
        public bool IsValid(string data)
        {
            return IPAddress.TryParse(data, out var address)
                && address.AddressFamily == AddressFamily.InterNetworkV6;
        }
    }

    /// <summary>Strategy for CNAME/NS records: hostnames (trailing dot allowed).</summary>
    public sealed class HostnameValidator : IRecordDataValidator
    {
        /// <inheritdoc />
        public bool IsValid(string data)
        {
            return DnsRules.IsValidHostname(data);
        }
    }

    /// <summary>Strategy for TXT records: non-empty text (length cap enforced by the validator).</summary>
    public sealed class TextValidator : IRecordDataValidator
    {
        /// <inheritdoc />
        public bool IsValid(string data)
        {
            return data.Length > 0;
        }
    }

    /// <summary>Registry mapping each <see cref="RecordType"/> to its validation strategy.</summary>
    public static class RecordDataValidators
    {
        private static readonly Dictionary<RecordType, IRecordDataValidator> Strategies = new()
        {
            [RecordType.A] = new IPv4Validator(),
            [RecordType.AAAA] = new IPv6Validator(),
            [RecordType.CNAME] = new HostnameValidator(),
            [RecordType.NS] = new HostnameValidator(),
            [RecordType.TXT] = new TextValidator(),
        };

        /// <summary>Checks record data against its type's strategy.</summary>
        /// <param name="type">Record type.</param>
        /// <param name="data">RDATA value.</param>
        /// <returns>True when the data fits the type; false for unknown types.</returns>
        public static bool IsValid(RecordType type, string data)
        {
            return Strategies.TryGetValue(type, out var validator) && validator.IsValid(data);
        }
    }
}
