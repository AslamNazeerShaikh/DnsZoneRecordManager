using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers.Api
{
    /// <summary>JSON API for zones (Scalar reference + client apps). HTML pages live in the MVC sibling controller.</summary>
    [ApiController]
    [Route("api/zones")]
    [Produces("application/json")]
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _zones;

        /// <summary>Creates the controller.</summary>
        /// <param name="zones">Zone service.</param>
        public ZonesController(IZoneService zones)
        {
            _zones = zones;
        }

        /// <summary>Lists zones with record counts, optionally filtered by name.</summary>
        /// <param name="search">Optional case-insensitive name filter.</param>
        /// <returns>Ordered zone rows.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ZoneDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ZoneDto>>> List([FromQuery] string? search)
        {
            var result = await _zones.ListAsync(search);
            return Ok(result.Data!.Select(ToDto).ToList());
        }

        /// <summary>Gets one zone with its records.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>The zone, or 404 with typed errors. Never throws for domain failures.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZoneDto>> Get(int id)
        {
            var result = await _zones.GetAsync(id);
            return result.Success ? Ok(ToDto(result.Data!)) : Failure(result);
        }

        /// <summary>Creates a zone (name is normalized then validated).</summary>
        /// <param name="request">Zone payload.</param>
        /// <returns>201 with the zone, or 400/409 with typed errors. Never throws for domain failures.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ZoneDto>> Create(CreateZoneRequest request)
        {
            var result = await _zones.CreateAsync(request.Name);
            return result.Success
                ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, ToDto(result.Data))
                : Failure(result);
        }

        /// <summary>Renames a zone.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="request">Rename payload.</param>
        /// <returns>The renamed zone, or 400/404/409 with typed errors. Never throws for domain failures.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ZoneDto>> Rename(int id, RenameZoneRequest request)
        {
            var result = await _zones.RenameAsync(id, request.Name);
            if (!result.Success)
            {
                return Failure(result);
            }

            var fresh = await _zones.GetAsync(id);
            return Ok(ToDto(fresh.Data!));
        }

        /// <summary>Deletes a zone and cascades its records.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>204, or 404 with typed errors. Never throws for domain failures.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _zones.DeleteAsync(id);
            return result.Success ? NoContent() : Failure(result);
        }

        private static ZoneDto ToDto(ZoneListItem item)
        {
            return new ZoneDto(
                item.Id,
                item.Name,
                item.RecordCount,
                item.NsCount,
                item.CreatedUtc,
                item.UpdatedUtc
            );
        }

        private static ZoneDto ToDto(DnsZone zone)
        {
            return new ZoneDto(
                zone.Id,
                zone.Name,
                zone.Records.Count,
                zone.Records.Count(r => r.Type == RecordType.NS),
                zone.CreatedUtc,
                zone.UpdatedUtc
            );
        }

        private ActionResult Failure<T>(ServiceResult<T> result)
        {
            var errors = result.Errors.Select(e => e.Message).ToList();
            return result.HasError(ErrorKind.NotFound)
                ? NotFound(new { errors })
                : StatusCode(
                    result.HasError(ErrorKind.Conflict)
                        ? StatusCodes.Status409Conflict
                        : StatusCodes.Status400BadRequest,
                    new { errors }
                );
        }
    }
}
