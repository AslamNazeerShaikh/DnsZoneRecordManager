using DnsZoneRecordManager.Results;

namespace DnsZoneRecordManager.Cqrs.Zones
{
    /// <summary>Lists zones with record counts, optionally filtered by name.</summary>
    /// <param name="Search">Optional case-insensitive name filter.</param>
    public sealed record ListZonesQuery(string? Search) : IRequest<ServiceResult<List<ZoneDto>>>;

    /// <summary>Gets one zone with its record counts.</summary>
    /// <param name="Id">Zone id.</param>
    public sealed record GetZoneQuery(int Id) : IRequest<ServiceResult<ZoneDto>>;

    /// <summary>Creates a zone (name is normalized then validated).</summary>
    /// <param name="Name">Raw zone name.</param>
    public sealed record CreateZoneCommand(string Name) : IRequest<ServiceResult<ZoneDto>>;

    /// <summary>Renames a zone.</summary>
    /// <param name="Id">Zone id.</param>
    /// <param name="Name">New raw name.</param>
    public sealed record RenameZoneCommand(int Id, string Name) : IRequest<ServiceResult<ZoneDto>>;

    /// <summary>Deletes a zone and cascades its records.</summary>
    /// <param name="Id">Zone id.</param>
    public sealed record DeleteZoneCommand(int Id) : IRequest<ServiceResult<int>>;
}
