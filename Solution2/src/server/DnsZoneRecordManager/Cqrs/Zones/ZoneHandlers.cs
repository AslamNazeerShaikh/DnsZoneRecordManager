using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DnsZoneRecordManager.Cqrs.Handlers
{
    /// <summary>Handles <see cref="ListZonesQuery"/>: zones with record counts, optionally filtered by name.</summary>
    public sealed class ListZonesHandler : IRequestHandler<ListZonesQuery, ServiceResult<List<ZoneDto>>>
    {
        private readonly AppDbContext _db;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        public ListZonesHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<List<ZoneDto>>> HandleAsync(ListZonesQuery request, CancellationToken cancellationToken)
        {
            var term = string.IsNullOrWhiteSpace(request.Search) ? null : DnsRules.NormalizeZoneName(request.Search);
            var rows = await _db.Zones.AsNoTracking()
                .Where(z => term == null || z.Name.Contains(term))
                .OrderBy(z => z.Name)
                .Select(z => new ZoneDto(
                    z.Id,
                    z.Name,
                    z.Records.Count,
                    z.Records.Count(r => r.Type == RecordType.NS),
                    z.CreatedUtc,
                    z.UpdatedUtc
                ))
                .ToListAsync(cancellationToken);
            return ServiceResult<List<ZoneDto>>.Ok(rows);
        }
    }

    /// <summary>Handles <see cref="GetZoneQuery"/>.</summary>
    public sealed class GetZoneHandler : IRequestHandler<GetZoneQuery, ServiceResult<ZoneDto>>
    {
        private readonly AppDbContext _db;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        public GetZoneHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ZoneDto>> HandleAsync(GetZoneQuery request, CancellationToken cancellationToken)
        {
            var dto = await ZoneDtos.ById(_db, request.Id, cancellationToken);
            return dto == null
                ? ServiceResult<ZoneDto>.Fail(ErrorKind.NotFound, "Zone not found.")
                : ServiceResult<ZoneDto>.Ok(dto);
        }
    }

    /// <summary>Handles <see cref="CreateZoneCommand"/> (name is normalized then validated).</summary>
    public sealed class CreateZoneHandler : IRequestHandler<CreateZoneCommand, ServiceResult<ZoneDto>>
    {
        private readonly AppDbContext _db;
        private readonly IValidator<CreateZoneCommand> _validator;
        private readonly ILogger<CreateZoneHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="validator">Zone shape validator.</param>
        /// <param name="logger">Audit logger.</param>
        public CreateZoneHandler(AppDbContext db, IValidator<CreateZoneCommand> validator, ILogger<CreateZoneHandler> logger)
        {
            _db = db;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ZoneDto>> HandleAsync(CreateZoneCommand request, CancellationToken cancellationToken)
        {
            var normalized = DnsRules.NormalizeZoneName(request.Name);
            var validation = await _validator.ValidateAsync(request with { Name = normalized }, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<ZoneDto>.Fail(ErrorKind.Validation, validation.Errors.Select(e => e.ErrorMessage));
            }

            if (await _db.Zones.AnyAsync(z => z.Name == normalized, cancellationToken))
            {
                return ServiceResult<ZoneDto>.Fail(ErrorKind.Conflict, $"Zone '{normalized}' already exists.");
            }

            var now = DateTime.UtcNow;
            var zone = new DnsZone { Name = normalized, CreatedUtc = now, UpdatedUtc = now };
            await _db.Zones.AddAsync(zone, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Zone {ZoneName} created with id {ZoneId}.", zone.Name, zone.Id);
            return ServiceResult<ZoneDto>.Ok(new ZoneDto(zone.Id, zone.Name, 0, 0, zone.CreatedUtc, zone.UpdatedUtc));
        }
    }

    /// <summary>Handles <see cref="RenameZoneCommand"/>.</summary>
    public sealed class RenameZoneHandler : IRequestHandler<RenameZoneCommand, ServiceResult<ZoneDto>>
    {
        private readonly AppDbContext _db;
        private readonly IValidator<RenameZoneCommand> _validator;
        private readonly ILogger<RenameZoneHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="validator">Rename shape validator.</param>
        /// <param name="logger">Audit logger.</param>
        public RenameZoneHandler(AppDbContext db, IValidator<RenameZoneCommand> validator, ILogger<RenameZoneHandler> logger)
        {
            _db = db;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<ZoneDto>> HandleAsync(RenameZoneCommand request, CancellationToken cancellationToken)
        {
            var zone = await _db.Zones.FindAsync([request.Id], cancellationToken);
            if (zone == null)
            {
                return ServiceResult<ZoneDto>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var normalized = DnsRules.NormalizeZoneName(request.Name);
            var validation = await _validator.ValidateAsync(request with { Name = normalized }, cancellationToken);
            if (!validation.IsValid)
            {
                return ServiceResult<ZoneDto>.Fail(ErrorKind.Validation, validation.Errors.Select(e => e.ErrorMessage));
            }

            if (await _db.Zones.AnyAsync(z => z.Name == normalized && z.Id != request.Id, cancellationToken))
            {
                return ServiceResult<ZoneDto>.Fail(ErrorKind.Conflict, $"Zone '{normalized}' already exists.");
            }

            zone.Name = normalized;
            zone.UpdatedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Zone {ZoneId} renamed to {ZoneName}.", request.Id, normalized);
            return ServiceResult<ZoneDto>.Ok((await ZoneDtos.ById(_db, request.Id, cancellationToken))!);
        }
    }

    /// <summary>Handles <see cref="DeleteZoneCommand"/> (records cascade).</summary>
    public sealed class DeleteZoneHandler : IRequestHandler<DeleteZoneCommand, ServiceResult<int>>
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DeleteZoneHandler> _logger;

        /// <summary>Creates the handler.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="logger">Audit logger.</param>
        public DeleteZoneHandler(AppDbContext db, ILogger<DeleteZoneHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ServiceResult<int>> HandleAsync(DeleteZoneCommand request, CancellationToken cancellationToken)
        {
            var zone = await _db.Zones.FindAsync([request.Id], cancellationToken);
            if (zone == null)
            {
                return ServiceResult<int>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            _db.Zones.Remove(zone);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Zone {ZoneId} deleted with its records.", request.Id);
            return ServiceResult<int>.Ok(request.Id);
        }
    }

    /// <summary>Shared zone projections (single query, no N+1).</summary>
    public static class ZoneDtos
    {
        /// <summary>Projects one zone with counts, or null when missing.</summary>
        /// <param name="db">EF Core context.</param>
        /// <param name="id">Zone id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Zone DTO or null.</returns>
        public static async Task<ZoneDto?> ById(AppDbContext db, int id, CancellationToken cancellationToken)
        {
            return await db.Zones.AsNoTracking()
                .Where(z => z.Id == id)
                .Select(z => new ZoneDto(
                    z.Id,
                    z.Name,
                    z.Records.Count,
                    z.Records.Count(r => r.Type == RecordType.NS),
                    z.CreatedUtc,
                    z.UpdatedUtc
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
