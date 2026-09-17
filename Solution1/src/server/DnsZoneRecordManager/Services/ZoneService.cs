using DnsZoneRecordManager.Data;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Validation;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DnsZoneRecordManager.Services
{
    /// <summary>One zone row for the list grid (counts make the A1/A2 limits visible before submit).</summary>
    /// <param name="Id">Zone id.</param>
    /// <param name="Name">Zone name.</param>
    /// <param name="RecordCount">Total records (max 10).</param>
    /// <param name="NsCount">NS records (min 4).</param>
    /// <param name="CreatedUtc">UTC creation timestamp.</param>
    /// <param name="UpdatedUtc">UTC last-update timestamp.</param>
    public sealed record ZoneListItem(
        int Id,
        string Name,
        int RecordCount,
        int NsCount,
        DateTime CreatedUtc,
        DateTime UpdatedUtc
    );

    /// <summary>Zone operations: list/search, create, rename, delete (A6 enforced).</summary>
    public interface IZoneService
    {
        /// <summary>Lists zones with record counts, optionally filtered by name.</summary>
        /// <param name="search">Optional case-insensitive name filter.</param>
        /// <returns>Ordered zone rows.</returns>
        Task<ServiceResult<List<ZoneListItem>>> ListAsync(string? search);

        /// <summary>Gets one zone with its records.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>The zone, or a <see cref="ErrorKind.NotFound"/> failure. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsZone>> GetAsync(int id);

        /// <summary>Creates a zone (name is normalized then validated).</summary>
        /// <param name="name">Raw zone name.</param>
        /// <returns>The created zone, or <see cref="ErrorKind.Validation"/> / <see cref="ErrorKind.Conflict"/> failures. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsZone>> CreateAsync(string name);

        /// <summary>Renames a zone.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="name">New raw name.</param>
        /// <returns>The renamed zone, or <see cref="ErrorKind.NotFound"/> / <see cref="ErrorKind.Validation"/> / <see cref="ErrorKind.Conflict"/> failures. Never throws for domain failures.</returns>
        Task<ServiceResult<DnsZone>> RenameAsync(int id, string name);

        /// <summary>Deletes a zone and cascades its records after confirmation.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>The deleted id, or a <see cref="ErrorKind.NotFound"/> failure. Never throws for domain failures.</returns>
        Task<ServiceResult<int>> DeleteAsync(int id);
    }

    /// <summary>EF Core implementation of <see cref="IZoneService"/>.</summary>
    public class ZoneService : IZoneService
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidator<DnsZone> _validator;
        private readonly ILogger<ZoneService> _logger;

        /// <summary>Creates the service.</summary>
        /// <param name="uow">Unit of work.</param>
        /// <param name="validator">Zone shape validator.</param>
        /// <param name="logger">Audit logger (mutations only, no secrets exist in this model).</param>
        public ZoneService(
            IUnitOfWork uow,
            IValidator<DnsZone> validator,
            ILogger<ZoneService> logger
        )
        {
            _uow = uow;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>Normalizes a zone name: trims, drops the trailing dot, lowercases.</summary>
        /// <param name="name">Raw name.</param>
        /// <returns>Normalized name.</returns>
        public static string NormalizeZoneName(string name)
        {
            return name.Trim().TrimEnd('.').ToLowerInvariant();
        }

        /// <inheritdoc />
        public async Task<ServiceResult<List<ZoneListItem>>> ListAsync(string? search)
        {
            var zones = await _uow.Zones.FindAsync(null, z => z.Records);
            var term = string.IsNullOrWhiteSpace(search) ? null : NormalizeZoneName(search);
            var rows = zones
                .Where(z => term == null || z.Name.Contains(term, StringComparison.Ordinal))
                .OrderBy(z => z.Name)
                .Select(z => new ZoneListItem(
                    z.Id,
                    z.Name,
                    z.Records.Count,
                    z.Records.Count(r => r.Type == RecordType.NS),
                    z.CreatedUtc,
                    z.UpdatedUtc
                ))
                .ToList();
            return ServiceResult<List<ZoneListItem>>.Ok(rows);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsZone>> GetAsync(int id)
        {
            var zones = await _uow.Zones.FindAsync(z => z.Id == id, z => z.Records);
            var zone = zones.FirstOrDefault();
            return zone == null
                ? ServiceResult<DnsZone>.Fail(ErrorKind.NotFound, "Zone not found.")
                : ServiceResult<DnsZone>.Ok(zone);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsZone>> CreateAsync(string name)
        {
            var normalized = NormalizeZoneName(name);
            var zone = new DnsZone { Name = normalized };
            var validation = await _validator.ValidateAsync(zone);
            if (!validation.IsValid)
            {
                return ServiceResult<DnsZone>.Fail(
                    ErrorKind.Validation,
                    validation.Errors.Select(e => e.ErrorMessage)
                );
            }

            if ((await _uow.Zones.FindAsync(z => z.Name == normalized)).Count > 0)
            {
                return ServiceResult<DnsZone>.Fail(
                    ErrorKind.Conflict,
                    $"Zone '{normalized}' already exists."
                );
            }

            var now = DateTime.UtcNow;
            zone.CreatedUtc = now;
            zone.UpdatedUtc = now;
            await _uow.Zones.AddAsync(zone);
            await _uow.SaveAsync();
            _logger.LogInformation("Zone {ZoneName} created with id {ZoneId}.", zone.Name, zone.Id);
            return ServiceResult<DnsZone>.Ok(zone);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<DnsZone>> RenameAsync(int id, string name)
        {
            var zone = await _uow.Zones.GetByIdAsync(id);
            if (zone == null)
            {
                return ServiceResult<DnsZone>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            var normalized = NormalizeZoneName(name);
            var validation = await _validator.ValidateAsync(new DnsZone { Name = normalized });
            if (!validation.IsValid)
            {
                return ServiceResult<DnsZone>.Fail(
                    ErrorKind.Validation,
                    validation.Errors.Select(e => e.ErrorMessage)
                );
            }

            if ((await _uow.Zones.FindAsync(z => z.Name == normalized && z.Id != id)).Count > 0)
            {
                return ServiceResult<DnsZone>.Fail(
                    ErrorKind.Conflict,
                    $"Zone '{normalized}' already exists."
                );
            }

            zone.Name = normalized;
            zone.UpdatedUtc = DateTime.UtcNow;
            _uow.Zones.Update(zone);
            await _uow.SaveAsync();
            _logger.LogInformation("Zone {ZoneId} renamed to {ZoneName}.", id, normalized);
            return ServiceResult<DnsZone>.Ok(zone);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<int>> DeleteAsync(int id)
        {
            var zone = await _uow.Zones.GetByIdAsync(id);
            if (zone == null)
            {
                return ServiceResult<int>.Fail(ErrorKind.NotFound, "Zone not found.");
            }

            _uow.Zones.Remove(zone);
            await _uow.SaveAsync();
            _logger.LogInformation("Zone {ZoneId} deleted with its records.", id);
            return ServiceResult<int>.Ok(id);
        }
    }
}
