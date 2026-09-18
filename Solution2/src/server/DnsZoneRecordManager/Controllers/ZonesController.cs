using DnsZoneRecordManager.Cqrs;
using DnsZoneRecordManager.Cqrs.Zones;
using DnsZoneRecordManager.Results;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers
{
    /// <summary>JSON API for zones (Scalar reference + Next.js client).</summary>
    [ApiController]
    [Route("api/zones")]
    [Produces("application/json")]
    public class ZonesController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>Creates the controller.</summary>
        /// <param name="sender">Command/query dispatcher.</param>
        public ZonesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Lists zones with record counts, optionally filtered by name.</summary>
        /// <param name="search">Optional case-insensitive name filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Ordered zone rows.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ZoneDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ZoneDto>>> List([FromQuery] string? search, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(new ListZonesQuery(search), cancellationToken);
            return Ok(result.Data!);
        }

        /// <summary>Gets one zone with its record counts.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The zone, or 404. Never throws for domain failures.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ZoneDto>> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(new GetZoneQuery(id), cancellationToken);
            return result.Success ? Ok(result.Data!) : Failure(result);
        }

        /// <summary>Creates a zone (name is normalized then validated).</summary>
        /// <param name="request">Zone payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>201 with the zone, or 400/409. Never throws for domain failures.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ZoneDto>> Create(CreateZoneCommand request, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(request, cancellationToken);
            return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result.Data) : Failure(result);
        }

        /// <summary>Renames a zone.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="request">Rename payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The renamed zone, or 400/404/409. Never throws for domain failures.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ZoneDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ZoneDto>> Rename(int id, RenameZoneCommand request, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(request with { Id = id }, cancellationToken);
            return result.Success ? Ok(result.Data!) : Failure(result);
        }

        /// <summary>Deletes a zone and cascades its records.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>204, or 404. Never throws for domain failures.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(new DeleteZoneCommand(id), cancellationToken);
            return result.Success ? NoContent() : Failure(result);
        }

        private ActionResult Failure<T>(ServiceResult<T> result)
        {
            var errors = result.Errors.Select(e => e.Message).ToList();
            return result.HasError(ErrorKind.NotFound)
                ? NotFound(new { errors })
                : StatusCode(
                    result.HasError(ErrorKind.Conflict) ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
                    new { errors }
                );
        }
    }
}
