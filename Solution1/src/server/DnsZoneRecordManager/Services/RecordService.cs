using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DnsZoneRecordManager.Services
{
    /// <summary>One record row for the grid (FQDN is derived, never stored).</summary>
    /// <param name="Id">Record id.</param>
    /// <param name="ZoneId">Owning zone id.</param>
    /// <param name="ZoneName">Owning zone name.</param>
    /// <param name="Name">Owner name (@ = apex).</param>
    /// <param name="Fqdn">Derived fully-qualified name.</param>
    /// <param name="Type">Record type.</param>
    /// <param name="Ttl">TTL in seconds.</param>
    /// <param name="Data">RDATA value.</param>
    /// <param name="UpdatedUtc">UTC last-update timestamp (the grid's Modified column).</param>
    public sealed record RecordRow(
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

    /// <summary>Record list payload: rows plus the selected zone's A1/A2 meter values.</summary>
    /// <param name="ZoneId">Selected zone id (null = all zones).</param>
    /// <param name="ZoneName">Selected zone name (null = all zones).</param>
    /// <param name="Records">Filtered rows.</param>
    /// <param name="RecordCount">Total records in the selected zone (0 when unselected).</param>
    /// <param name="NsCount">NS records in the selected zone (0 when unselected).</param>
    public sealed record RecordListData(
        int? ZoneId,
        string? ZoneName,
        List<RecordRow> Records,
        int RecordCount,
        int NsCount
    );

    /// <summary>Record operations: list/search/filter, create, edit, delete, CSV export (A1–A6 enforced).</summary>
    public interface IRecordService
    {
        /// <summary>Lists records with optional zone/search/type filters.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional case-insensitive name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>Rows plus meter values, or a <see cref="ErrorKind.NotFound"/> failure. Never throws for domain failures.</returns>
        Task<ServiceResult<RecordListData>> ListAsync(
            int? zoneId,
            string? search,
            RecordType? type
        );

        /// <summary>Gets one record with its zone.</summary>
        /// <param name="id">Record id.</param>
        /// <returns>The record, or a <see cref="ErrorKind.NotFound"/> failure. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsRecord>> GetAsync(int id);

        /// <summary>Creates a record (duplicate, CNAME, and 10-record ceiling enforced).</summary>
        /// <param name="zoneId">Owning zone id.</param>
        /// <param name="name">Raw owner name.</param>
        /// <param name="type">Record type.</param>
        /// <param name="ttl">TTL in seconds.</param>
        /// <param name="data">RDATA value.</param>
        /// <returns>The created record, or <see cref="ErrorKind.NotFound"/> / <see cref="ErrorKind.Validation"/> / <see cref="ErrorKind.Conflict"/> / <see cref="ErrorKind.RuleViolation"/> failures. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsRecord>> CreateAsync(
            int zoneId,
            string name,
            RecordType type,
            int ttl,
            string data
        );

        /// <summary>Edits a record (duplicate, CNAME, and 4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <param name="name">New owner name.</param>
        /// <param name="type">New type.</param>
        /// <param name="ttl">New TTL.</param>
        /// <param name="data">New RDATA value.</param>
        /// <returns>The updated record, or <see cref="ErrorKind.NotFound"/> / <see cref="ErrorKind.Validation"/> / <see cref="ErrorKind.Conflict"/> / <see cref="ErrorKind.RuleViolation"/> failures. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsRecord>> UpdateAsync(
            int id,
            string name,
            RecordType type,
            int ttl,
            string data
        );

        /// <summary>Deletes a record (4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <returns>The deleted id, or <see cref="ErrorKind.NotFound"/> / <see cref="ErrorKind.RuleViolation"/> failures. Never throws for domain failures.</returns>
        Task<ServiceResult<int>> DeleteAsync(int id);

        /// <summary>Exports the filtered grid as records-only CSV.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>CSV text, or "zone not found".</returns>
        Task<ServiceResult<string>> ExportCsvAsync(int? zoneId, string? search, RecordType? type);
    }

    /// <summary>EF Core implementation of <see cref="IRecordService"/>.</summary>
    public class RecordService : IRecordService
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<DnsRecord> _validator;
        private readonly ILogger<RecordService> _logger;

        /// <summary>Creates the service.</summary>
        /// <param name="uow">Unit of work.</param>
        /// <param name="validator">Record shape validator.</param>
        /// <param name="logger">Audit logger (mutations only, no secrets exist in this model).</param>
        public RecordService(
            IUnitOfWork uow,
            IValidator<DnsRecord> validator,
            ILogger<RecordService> logger
        )
        {
            _uow = uow;
            _validator = validator;
            _logger = logger;
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

        /// <inheritdoc />
        public async Task<ServiceResult<RecordListData>> ListAsync(
            int? zoneId,
            string? search,
            RecordType? type
        )
        {
            string? zoneName = null;
            if (zoneId.HasValue)
            {
                var zone = await _uow.Zones.GetByIdAsync(zoneId.Value);
                if (zone == null)
                {
                    return ServiceResult<RecordListData>.Fail(
                        ErrorKind.NotFound,
                        "Zone not found."
                    );
                }

                zoneName = zone.Name;
            }

            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();
            var records = await _uow.Records.FindAsync(
                r =>
                    (!zoneId.HasValue || r.ZoneId == zoneId.Value)
                    && (!type.HasValue || r.Type == type.Value)
                    && (
                        term == null
                        || r.Name.Contains(term, StringComparison.Ordinal)
                        || r.Data.ToLowerInvariant().Contains(term, StringComparison.Ordinal)
                    ),
                r => r.Zone
            );

            var zoneRecords = zoneId.HasValue
                ? await _uow.Records.FindAsync(r => r.ZoneId == zoneId.Value)
                : [];
            var rows = records
                .OrderBy(r => r.Zone.Name)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.Type)
                .Select(r => new RecordRow(
                    r.Id,
                    r.ZoneId,
                    r.Zone.Name,
                    r.Name,
                    ToFqdn(r.Name, r.Zone.Name),
                    r.Type,
                    r.Ttl,
                    r.Data,
                    r.UpdatedUtc
                ))
                .ToList();
            return ServiceResult<RecordListData>.Ok(
                new RecordListData(
                    zoneId,
                    zoneName,
                    rows,
                    zoneRecords.Count,
                    zoneRecords.Count(r => r.Type == RecordType.NS)
                )
            );
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsRecord>> GetAsync(int id)
        {
            var records = await _uow.Records.FindAsync(r => r.Id == id, r => r.Zone);
            var record = records.FirstOrDefault();
            return record == null
                ? ServiceResult<DnsRecord>.Fail(ErrorKind.NotFound, "Record not found.")
                : ServiceResult<DnsRecord>.Ok(record);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsRecord>> CreateAsync(
            int zoneId,
            string name,
            RecordType type,
            int ttl,
            string data
        )
        {
            var zone = await _uow.Zones.GetByIdAsync(zoneId);
            if (zone == null)
            {
                return ServiceResult<DnsRecord>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var record = new DnsRecord
            {
                ZoneId = zoneId,
                Name = NormalizeRecordName(name),
                Type = type,
                Ttl = ttl,
                Data = data.Trim(),
            };
            var validation = await _validator.ValidateAsync(record);
            if (!validation.IsValid)
            {
                return ServiceResult<DnsRecord>.Fail(
                    ErrorKind.Validation,
                    validation.Errors.Select(e => e.ErrorMessage)
                );
            }

            var siblings = await _uow.Records.FindAsync(r => r.ZoneId == zoneId);
            var guard = GuardSiblings(siblings, null, record.Name, record.Type, record.Data);
            if (guard is not null)
            {
                return ServiceResult<DnsRecord>.Fail([guard]);
            }

            var now = DateTime.UtcNow;
            record.CreatedUtc = now;
            record.UpdatedUtc = now;
            await _uow.Records.AddAsync(record);
            await _uow.SaveAsync();
            _logger.LogInformation(
                "Record {RecordId} created in zone {ZoneId}.",
                record.Id,
                zoneId
            );
            return ServiceResult<DnsRecord>.Ok(record);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsRecord>> UpdateAsync(
            int id,
            string name,
            RecordType type,
            int ttl,
            string data
        )
        {
            var record = await _uow.Records.GetByIdAsync(id);
            if (record == null)
            {
                return ServiceResult<DnsRecord>.Fail(ErrorKind.NotFound, "Record not found.");
            }

            var candidate = new DnsRecord
            {
                Name = NormalizeRecordName(name),
                Type = type,
                Ttl = ttl,
                Data = data.Trim(),
            };
            var validation = await _validator.ValidateAsync(candidate);
            if (!validation.IsValid)
            {
                return ServiceResult<DnsRecord>.Fail(
                    ErrorKind.Validation,
                    validation.Errors.Select(e => e.ErrorMessage)
                );
            }

            var siblings = await _uow.Records.FindAsync(r => r.ZoneId == record.ZoneId);
            if (
                record.Type == RecordType.NS
                && type != RecordType.NS
                && siblings.Count(r => r.Type == RecordType.NS) <= DnsRules.MinNsPerZone
            )
            {
                return ServiceResult<DnsRecord>.Fail(
                    ErrorKind.RuleViolation,
                    "A zone must keep at least 4 NS records."
                );
            }

            var guard = GuardSiblings(siblings, id, candidate.Name, candidate.Type, candidate.Data);
            if (guard is not null)
            {
                return ServiceResult<DnsRecord>.Fail([guard]);
            }

            record.Name = candidate.Name;
            record.Type = candidate.Type;
            record.Ttl = candidate.Ttl;
            record.Data = candidate.Data;
            record.UpdatedUtc = DateTime.UtcNow;
            _uow.Records.Update(record);
            await _uow.SaveAsync();
            _logger.LogInformation("Record {RecordId} updated.", id);
            return ServiceResult<DnsRecord>.Ok(record);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<int>> DeleteAsync(int id)
        {
            var record = await _uow.Records.GetByIdAsync(id);
            if (record == null)
            {
                return ServiceResult<int>.Fail(ErrorKind.NotFound, "Record not found.");
            }

            if (record.Type == RecordType.NS)
            {
                var siblings = await _uow.Records.FindAsync(r => r.ZoneId == record.ZoneId);
                if (siblings.Count(r => r.Type == RecordType.NS) <= DnsRules.MinNsPerZone)
                {
                    return ServiceResult<int>.Fail(
                        ErrorKind.RuleViolation,
                        "A zone must keep at least 4 NS records."
                    );
                }
            }

            _uow.Records.Remove(record);
            await _uow.SaveAsync();
            _logger.LogInformation("Record {RecordId} deleted.", id);
            return ServiceResult<int>.Ok(id);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<string>> ExportCsvAsync(
            int? zoneId,
            string? search,
            RecordType? type
        )
        {
            var list = await ListAsync(zoneId, search, type);
            if (!list.Success)
            {
                return ServiceResult<string>.Fail(list.Errors);
            }

            var lines = new List<string> { "FQDN,Zone,Name,Type,TTL,Data" };
            lines.AddRange(
                list.Data!.Records.Select(r =>
                    string.Join(
                        ",",
                        new[]
                        {
                            r.Fqdn,
                            r.ZoneName,
                            r.Name,
                            r.Type.ToString(),
                            r.Ttl.ToString(),
                            Quote(r.Data),
                        }
                    )
                )
            );
            return ServiceResult<string>.Ok(string.Join("\n", lines) + "\n");
        }

        private static string Quote(string value)
        {
            return value.Contains(',') || value.Contains('"')
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }

        /// <summary>Checks duplicates, CNAME exclusivity, and the 10-record ceiling against sibling records.</summary>
        /// <param name="siblings">All records in the zone.</param>
        /// <param name="selfId">Id to exclude (updates) or null (creates).</param>
        /// <param name="name">Candidate owner name.</param>
        /// <param name="type">Candidate type.</param>
        /// <param name="data">Candidate data.</param>
        /// <returns>A typed guard error, or null when the candidate is allowed.</returns>
        private static ServiceError? GuardSiblings(
            List<DnsRecord> siblings,
            int? selfId,
            string name,
            RecordType type,
            string data
        )
        {
            if (
                siblings.Any(r =>
                    r.Id != selfId && r.Name == name && r.Type == type && r.Data == data
                )
            )
            {
                return new ServiceError(
                    ErrorKind.Conflict,
                    "This exact record already exists in the zone."
                );
            }

            if (type == RecordType.CNAME && siblings.Any(r => r.Id != selfId && r.Name == name))
            {
                return new ServiceError(
                    ErrorKind.RuleViolation,
                    "A name with a CNAME record cannot hold any other record."
                );
            }

            if (
                type != RecordType.CNAME
                && siblings.Any(r => r.Id != selfId && r.Name == name && r.Type == RecordType.CNAME)
            )
            {
                return new ServiceError(
                    ErrorKind.RuleViolation,
                    "This name already has a CNAME record, which cannot share its name."
                );
            }

            if (selfId == null && siblings.Count >= DnsRules.MaxRecordsPerZone)
            {
                return new ServiceError(
                    ErrorKind.RuleViolation,
                    "A zone cannot hold more than 10 records."
                );
            }

            return null;
        }
    }
}
