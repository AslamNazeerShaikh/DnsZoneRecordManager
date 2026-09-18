using DnsZoneRecordManager.Cqrs;
using DnsZoneRecordManager.Cqrs.Records;
using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Results;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers
{
    /// <summary>JSON API for records (Scalar reference + Next.js client).</summary>
    [ApiController]
    [Route("api/records")]
    [Produces("application/json")]
    public class RecordsController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>Creates the controller.</summary>
        /// <param name="sender">Command/query dispatcher.</param>
        public RecordsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>Lists records with optional zone/search/type filters.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional case-insensitive name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filtered rows, or 404. Never throws for domain failures.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RecordDto>>> List(
            [FromQuery] int? zoneId,
            [FromQuery] string? search,
            [FromQuery] RecordType? type,
            CancellationToken cancellationToken
        )
        {
            var result = await _sender.SendAsync(new ListRecordsQuery(zoneId, search, type), cancellationToken);
            return result.Success ? Ok(result.Data!) : Failure(result);
        }

        /// <summary>Gets one record with its zone.</summary>
        /// <param name="id">Record id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The record, or 404. Never throws for domain failures.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordDto>> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(new GetRecordQuery(id), cancellationToken);
            return result.Success ? Ok(result.Data!) : Failure(result);
        }

        /// <summary>Creates a record (duplicate, CNAME, and 10-record ceiling enforced).</summary>
        /// <param name="request">Record payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>201 with the record, or 400/404/409. Never throws for domain failures.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordDto>> Create(CreateRecordCommand request, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(request, cancellationToken);
            return result.Success ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, result.Data) : Failure(result);
        }

        /// <summary>Edits a record (duplicate, CNAME, and 4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <param name="request">Update payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated record, or 400/404/409. Never throws for domain failures.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordDto>> Update(int id, UpdateRecordCommand request, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(request with { Id = id }, cancellationToken);
            return result.Success ? Ok(result.Data!) : Failure(result);
        }

        /// <summary>Deletes a record (4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>204, or 400/404. Never throws for domain failures.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _sender.SendAsync(new DeleteRecordCommand(id), cancellationToken);
            return result.Success ? NoContent() : Failure(result);
        }

        /// <summary>Downloads the filtered grid as records-only CSV.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>CSV file, or 404. Never throws for domain failures.</returns>
        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Export(
            [FromQuery] int? zoneId,
            [FromQuery] string? search,
            [FromQuery] RecordType? type,
            CancellationToken cancellationToken
        )
        {
            var result = await _sender.SendAsync(new ExportRecordsQuery(zoneId, search, type), cancellationToken);
            if (!result.Success)
            {
                return Failure(result);
            }

            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            return File(System.Text.Encoding.UTF8.GetBytes(result.Data!), "text/csv", $"dns-records-{stamp}.csv");
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
