using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;

namespace DnsZoneRecordManager.Cqrs.Records
{
    /// <summary>Lists records with optional zone/search/type filters.</summary>
    /// <param name="ZoneId">Optional zone filter.</param>
    /// <param name="Search">Optional case-insensitive name/data filter.</param>
    /// <param name="Type">Optional type filter.</param>
    public sealed record ListRecordsQuery(int? ZoneId, string? Search, RecordType? Type) : IRequest<ServiceResult<List<RecordDto>>>;

    /// <summary>Gets one record with its zone.</summary>
    /// <param name="Id">Record id.</param>
    public sealed record GetRecordQuery(int Id) : IRequest<ServiceResult<RecordDto>>;

    /// <summary>Creates a record (duplicate, CNAME, and 10-record ceiling enforced).</summary>
    /// <param name="ZoneId">Owning zone id.</param>
    /// <param name="Name">Raw owner name.</param>
    /// <param name="Type">Record type.</param>
    /// <param name="Ttl">TTL in seconds.</param>
    /// <param name="Data">RDATA value.</param>
    public sealed record CreateRecordCommand(int ZoneId, string Name, RecordType Type, int Ttl, string Data) : IRequest<ServiceResult<RecordDto>>;

    /// <summary>Edits a record (duplicate, CNAME, and 4-NS floor enforced).</summary>
    /// <param name="Id">Record id.</param>
    /// <param name="Name">New owner name.</param>
    /// <param name="Type">New type.</param>
    /// <param name="Ttl">New TTL.</param>
    /// <param name="Data">New RDATA value.</param>
    public sealed record UpdateRecordCommand(int Id, string Name, RecordType Type, int Ttl, string Data) : IRequest<ServiceResult<RecordDto>>;

    /// <summary>Deletes a record (4-NS floor enforced).</summary>
    /// <param name="Id">Record id.</param>
    public sealed record DeleteRecordCommand(int Id) : IRequest<ServiceResult<int>>;

    /// <summary>Exports the filtered grid as records-only CSV.</summary>
    /// <param name="ZoneId">Optional zone filter.</param>
    /// <param name="Search">Optional name/data filter.</param>
    /// <param name="Type">Optional type filter.</param>
    public sealed record ExportRecordsQuery(int? ZoneId, string? Search, RecordType? Type) : IRequest<ServiceResult<string>>;
}
