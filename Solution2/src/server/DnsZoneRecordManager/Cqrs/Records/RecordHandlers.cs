using System.Linq.Expressions;
using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DnsZoneRecordManager.Cqrs.Handlers
{
    /// <summary>Shared record filter, row projection, guard, and CSV quoting.</summary>
    public static class RecordQueries
    {
        /// <summary>Applies the optional zone/search/type filters (single query, join for the zone name).</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional case-insensitive name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>Filtered, untracked query.</returns>
        public static IQueryable<DnsRecord> Filtered(AppDbContext db, int? zoneId, string? search, RecordType? type)
        {
            var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();
            return db.Records.AsNoTracking()
                .Where(r =>
                    (!zoneId.HasValue || r.ZoneId == zoneId.Value)
                    && (!type.HasValue || r.Type == type.Value)
                    && (term == null || r.Name.Contains(term) || r.Data.ToLower().Contains(term))
                );
        }

        /// <summary>Row projection with the derived FQDN (translatable CASE, no client eval).</summary>
        public static readonly Expression<Func<DnsRecord, RecordDto>> Row =
            r =>
                new RecordDto(
                    r.Id,
                    r.ZoneId,
                    r.Zone.Name,
                    r.Name,
                    r.Name == "@" ? r.Zone.Name : r.Name + "." + r.Zone.Name,
                    r.Type,
                    r.Ttl,
                    r.Data,
                    r.UpdatedUtc
                );

        /// <summary>Checks duplicates, CNAME exclusivity, and the 10-record ceiling against sibling records.</summary>
        /// <param name="siblings">All records in the zone.</param>
        /// <param name="selfId">Id to exclude (updates) or null (creates).</param>
        /// <param name="name">Candidate owner name.</param>
        /// <param name="type">Candidate type.</param>
        /// <param name="data">Candidate data.</param>
        /// <returns>A typed guard error, or null when the candidate is allowed.</returns>
        public static ServiceError? GuardSiblings(List<DnsRecord> siblings, int? selfId, string name, RecordType type, string data)
        {
            if (siblings.Any(r => r.Id != selfId && r.Name == name && r.Type == type && r.Data == data))
            {
                return new ServiceError(ErrorKind.Conflict, "This exact record already exists in the zone.");
            }

            if (type == RecordType.CNAME && siblings.Any(r => r.Id != selfId && r.Name == name))
            {
                return new ServiceError(ErrorKind.RuleViolation, "A name with a CNAME record cannot hold any other record.");
            }

            if (type != RecordType.CNAME && siblings.Any(r => r.Id != selfId && r.Name == name && r.Type == RecordType.CNAME))
            {
                return new ServiceError(ErrorKind.RuleViolation, "This name already has a CNAME record, which cannot share its name.");
            }

            if (selfId == null && siblings.Count >= DnsRules.MaxRecordsPerZone)
            {
                return new ServiceError(ErrorKind.RuleViolation, "A zone cannot hold more than 10 records.");
            }

            return null;
        }

        /// <summary>Quotes one CSV cell when it contains a comma or quote.</summary>
        /// <param name="value">Cell value.</param>
        /// <returns>Quoted or raw cell.</returns>
        public static string Quote(string value)
        {
            return value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }
    }

    /// <summary>Handles <see cref="ListRecordsQuery"/>.</summary>
    public sealed class ListRecordsHandler : IRequestHandler<ListRecordsQuery, ServiceResult<List<RecordDto>>>
    {
        private readonly AppDbContext _db;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        public ListRecordsHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<List<RecordDto>>> HandleAsync(ListRecordsQuery request, CancellationToken cancellationToken)
        {
            if (request.ZoneId.HasValue && !await _db.Zones.AnyAsync(z => z.Id == request.ZoneId.Value, cancellationToken))
            {
                return ServiceResult<List<RecordDto>>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var rows = await RecordQueries.Filtered(_db, request.ZoneId, request.Search, request.Type)
                .OrderBy(r => r.Zone.Name)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.Type)
                .Select(RecordQueries.Row)
                .ToListAsync(cancellationToken);
            return ServiceResult<List<RecordDto>>.Ok(rows);
        }
    }

    /// <summary>Handles <see cref="GetRecordQuery"/>.</summary>
    public sealed class GetRecordHandler : IRequestHandler<GetRecordQuery, ServiceResult<RecordDto>>
    {
        private readonly AppDbContext _db;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        public GetRecordHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<RecordDto>> HandleAsync(GetRecordQuery request, CancellationToken cancellationToken)
        {
            var row = await RecordQueries.Filtered(_db, null, null, null).Where(r => r.Id == request.Id).Select(RecordQueries.Row).FirstOrDefaultAsync(cancellationToken);
            return row == null ? ServiceResult<RecordDto>.Fail(ErrorKind.NotFound, "Record not found.") : ServiceResult<RecordDto>.Ok(row);
        }
    }

    /// <summary>Handles <see cref="CreateRecordCommand"/>.</summary>
    public sealed class CreateRecordHandler : IRequestHandler<CreateRecordCommand, ServiceResult<RecordDto>>
    {
        private readonly AppDbContext _db;
        private readonly IValidator<CreateRecordCommand> _validator;
        private readonly ILogger<CreateRecordHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="validator">Record shape validator.</param>
        /// <param name="logger">Audit logger.</param>
        public CreateRecordHandler(AppDbContext db, IValidator<CreateRecordCommand> validator, ILogger<CreateRecordHandler> logger)
        {
            _db = db;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<RecordDto>> HandleAsync(CreateRecordCommand request, CancellationToken cancellationToken)
        {
            var zone = await _db.Zones.FindAsync([request.ZoneId], cancellationToken);
            if (zone == null)
            {
                return ServiceResult<RecordDto>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var normalized = request with { Name = DnsRules.NormalizeRecordName(request.Name), Data = request.Data.Trim() };
            var validation = await _validator.ValidateAsync(normalized, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<RecordDto>.Fail(ErrorKind.Validation, validation.Errors.Select(e => e.ErrorMessage));
            }

            var siblings = await _db.Records.Where(r => r.ZoneId == request.ZoneId).ToListAsync(cancellationToken);
            var guard = RecordQueries.GuardSiblings(siblings, null, normalized.Name, normalized.Type, normalized.Data);
            if (guard is not null)
            {
                return ServiceResult<RecordDto>.Fail([guard]);
            }

            var now = DateTime.UtcNow;
            var record = new DnsRecord
            {
                ZoneId = request.ZoneId,
                Name = normalized.Name,
                Type = normalized.Type,
                Ttl = normalized.Ttl,
                Data = normalized.Data,
                CreatedUtc = now,
                UpdatedUtc = now,
            };
            await _db.Records.AddAsync(record, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Record {RecordId} created in zone {ZoneId}.", record.Id, request.ZoneId);
            return ServiceResult<RecordDto>.Ok(
                new RecordDto(
                    record.Id,
                    record.ZoneId,
                    zone.Name,
                    record.Name,
                    DnsRules.ToFqdn(record.Name, zone.Name),
                    record.Type,
                    record.Ttl,
                    record.Data,
                    record.UpdatedUtc
                )
            );
        }
    }

    /// <summary>Handles <see cref="UpdateRecordCommand"/>.</summary>
    public sealed class UpdateRecordHandler : IRequestHandler<UpdateRecordCommand, ServiceResult<RecordDto>>
    {
        private readonly AppDbContext _db;
        private readonly IValidator<UpdateRecordCommand> _validator;
        private readonly ILogger<UpdateRecordHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="validator">Update shape validator.</param>
        /// <param name="logger">Audit logger.</param>
        public UpdateRecordHandler(AppDbContext db, IValidator<UpdateRecordCommand> validator, ILogger<UpdateRecordHandler> logger)
        {
            _db = db;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<RecordDto>> HandleAsync(UpdateRecordCommand request, CancellationToken cancellationToken)
        {
            var record = await _db.Records.FindAsync([request.Id], cancellationToken);
            if (record == null)
            {
                return ServiceResult<RecordDto>.Fail(ErrorKind.NotFound, "Record not found.");
            }

            var normalized = request with { Name = DnsRules.NormalizeRecordName(request.Name), Data = request.Data.Trim() };
            var validation = await _validator.ValidateAsync(normalized, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<RecordDto>.Fail(ErrorKind.Validation, validation.Errors.Select(e => e.ErrorMessage));
            }

            var siblings = await _db.Records.Where(r => r.ZoneId == record.ZoneId).ToListAsync(cancellationToken);
            if (record.Type == RecordType.NS && normalized.Type != RecordType.NS && siblings.Count(r => r.Type == RecordType.NS) <= DnsRules.MinNsPerZone)
            {
                return ServiceResult<RecordDto>.Fail(ErrorKind.RuleViolation, "A zone must keep at least 4 NS records.");
            }

            var guard = RecordQueries.GuardSiblings(siblings, request.Id, normalized.Name, normalized.Type, normalized.Data);
            if (guard is not null)
            {
                return ServiceResult<RecordDto>.Fail([guard]);
            }

            record.Name = normalized.Name;
            record.Type = normalized.Type;
            record.Ttl = normalized.Ttl;
            record.Data = normalized.Data;
            record.UpdatedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Record {RecordId} updated.", request.Id);
            var zoneName = (await _db.Zones.FindAsync([record.ZoneId], cancellationToken))!.Name;
            return ServiceResult<RecordDto>.Ok(
                new RecordDto(
                    record.Id,
                    record.ZoneId,
                    zoneName,
                    record.Name,
                    DnsRules.ToFqdn(record.Name, zoneName),
                    record.Type,
                    record.Ttl,
                    record.Data,
                    record.UpdatedUtc
                )
            );
        }
    }

    /// <summary>Handles <see cref="DeleteRecordCommand"/>.</summary>
    public sealed class DeleteRecordHandler : IRequestHandler<DeleteRecordCommand, ServiceResult<int>>
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DeleteRecordHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="logger">Audit logger.</param>
        public DeleteRecordHandler(AppDbContext db, ILogger<DeleteRecordHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<int>> HandleAsync(DeleteRecordCommand request, CancellationToken cancellationToken)
        {
            var record = await _db.Records.FindAsync([request.Id], cancellationToken);
            if (record == null)
            {
                return ServiceResult<int>.Fail(ErrorKind.NotFound, "Record not found.");
            }

            if (record.Type == RecordType.NS)
            {
                var nsCount = await _db.Records.CountAsync(r => r.ZoneId == record.ZoneId && r.Type == RecordType.NS, cancellationToken);
                if (nsCount <= DnsRules.MinNsPerZone)
                {
                    return ServiceResult<int>.Fail(ErrorKind.RuleViolation, "A zone must keep at least 4 NS records.");
                }
            }

            _db.Records.Remove(record);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Record {RecordId} deleted.", request.Id);
            return ServiceResult<int>.Ok(request.Id);
        }
    }

    /// <summary>Handles <see cref="ExportRecordsQuery"/> (records-only CSV over the same filters).</summary>
    public sealed class ExportRecordsHandler : IRequestHandler<ExportRecordsQuery, ServiceResult<string>>
    {
        private readonly AppDbContext _db;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        public ExportRecordsHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<string>> HandleAsync(ExportRecordsQuery request, CancellationToken cancellationToken)
        {
            if (request.ZoneId.HasValue && !await _db.Zones.AnyAsync(z => z.Id == request.ZoneId.Value, cancellationToken))
            {
                return ServiceResult<string>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var rows = await RecordQueries.Filtered(_db, request.ZoneId, request.Search, request.Type)
                .OrderBy(r => r.Zone.Name)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.Type)
                .Select(RecordQueries.Row)
                .ToListAsync(cancellationToken);
            var lines = new List<string> { "FQDN,Zone,Name,Type,TTL,Data" };
            lines.AddRange(rows.Select(r => string.Join(",", new[] { r.Fqdn, r.ZoneName, r.Name, r.Type.ToString(), r.Ttl.ToString(), RecordQueries.Quote(r.Data) })));
            return ServiceResult<string>.Ok(string.Join("\n", lines) + "\n");
        }
    }
}
