using DnsZoneRecordManager.Models;

namespace DnsZoneRecordManager.Cqrs
{
    /// <summary>Zone payload (counts included, no navigation loops).</summary>
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

    /// <summary>Record payload (FQDN derived, enum serialized as string).</summary>
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
}
