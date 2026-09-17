using DnsZoneRecordManager.Models;

namespace DnsZoneRecordManager.Controllers.Api
{
    /// <summary>Zone payload for the JSON API (counts included, no navigation loops).</summary>
    /// <param name="Id">Zone id.</param>
    /// <param name="Name">Zone name.</param>
    /// <param name="RecordCount">Total records.</param>
    /// <param name="NsCount">NS records.</param>
    /// <param name="CreatedUtc">UTC creation timestamp.</param>
    /// <param name="UpdatedUtc">UTC last-update timestamp.</param>
    public sealed record ZoneDto(
        int Id,
        string Name,
        int RecordCount,
        int NsCount,
        DateTime CreatedUtc,
        DateTime UpdatedUtc
    );

    /// <summary>Record payload for the JSON API (FQDN derived, enum serialized as string).</summary>
    /// <param name="Id">Record id.</param>
    /// <param name="ZoneId">Owning zone id.</param>
    /// <param name="ZoneName">Owning zone name.</param>
    /// <param name="Name">Owner name (@ = apex).</param>
    /// <param name="Fqdn">Derived fully-qualified name.</param>
    /// <param name="Type">Record type.</param>
    /// <param name="Ttl">TTL in seconds.</param>
    /// <param name="Data">RDATA value.</param>
    /// <param name="UpdatedUtc">UTC last-update timestamp.</param>
    public sealed record RecordDto(
        int Id,
        int ZoneId,
        string ZoneName,
        string Name,
        string Fqdn,
        RecordType Type,
        int Ttl,
        string Data,
        DateTime UpdatedUtc
    );

    /// <summary>Zone creation payload (missing members are rejected with 400 by the framework).</summary>
    public sealed record CreateZoneRequest
    {
        /// <summary>Raw zone name (normalized server-side).</summary>
        public required string Name { get; init; }
    }

    /// <summary>Zone rename payload.</summary>
    public sealed record RenameZoneRequest
    {
        /// <summary>New raw zone name (normalized server-side).</summary>
        public required string Name { get; init; }
    }

    /// <summary>Record creation payload (missing members are rejected with 400 by the framework).</summary>
    public sealed record CreateRecordRequest
    {
        /// <summary>Owning zone id.</summary>
        public required int ZoneId { get; init; }

        /// <summary>Raw owner name (normalized server-side).</summary>
        public required string Name { get; init; }

        /// <summary>Record type.</summary>
        public required RecordType Type { get; init; }

        /// <summary>TTL in seconds.</summary>
        public required int Ttl { get; init; }

        /// <summary>RDATA value.</summary>
        public required string Data { get; init; }
    }

    /// <summary>Record update payload (the owning zone never moves).</summary>
    public sealed record UpdateRecordRequest
    {
        /// <summary>New owner name (normalized server-side).</summary>
        public required string Name { get; init; }

        /// <summary>New record type.</summary>
        public required RecordType Type { get; init; }

        /// <summary>New TTL in seconds.</summary>
        public required int Ttl { get; init; }

        /// <summary>New RDATA value.</summary>
        public required string Data { get; init; }
    }
}
