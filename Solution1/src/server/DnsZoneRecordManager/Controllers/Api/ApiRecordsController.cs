using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers.Api
{
    /// <summary>JSON API for records (Scalar reference + client apps). HTML pages live in the MVC sibling controller.</summary>
    [ApiController]
    [Route("api/records")]
    [Produces("application/json")]
    public class RecordsController : ControllerBase
    {
        private readonly IRecordService _records;

        /// <summary>Creates the controller.</summary>
        /// <param name="records">Record service.</param>
        public RecordsController(IRecordService records)
        {
            _records = records;
        }

        /// <summary>Lists records with optional zone/search/type filters.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional case-insensitive name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>Filtered rows, or 404 with typed errors. Never throws for domain failures.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<RecordDto>>> List(
            [FromQuery] int? zoneId,
            [FromQuery] string? search,
            [FromQuery] RecordType? type
        )
        {
            var result = await _records.ListAsync(zoneId, search, type);
            return result.Success
                ? Ok(result.Data!.Records.Select(ToDto).ToList())
                : Failure(result);
        }

        /// <summary>Gets one record with its zone.</summary>
        /// <param name="id">Record id.</param>
        /// <returns>The record, or 404 with typed errors. Never throws for domain failures.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecordDto>> Get(int id)
        {
            var result = await _records.GetAsync(id);
            return result.Success ? Ok(ToDto(result.Data!)) : Failure(result);
        }

        /// <summary>Creates a record (duplicate, CNAME, and 10-record ceiling enforced).</summary>
        /// <param name="request">Record payload.</param>
        /// <returns>201 with the record, or 400/404/409 with typed errors. Never throws for domain failures.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordDto>> Create(CreateRecordRequest request)
        {
            var result = await _records.CreateAsync(
                request.ZoneId,
                request.Name,
                request.Type,
                request.Ttl,
                request.Data
            );
            return result.Success
                ? CreatedAtAction(nameof(Get), new { id = result.Data!.Id }, ToDto(result.Data))
                : Failure(result);
        }

        /// <summary>Edits a record (duplicate, CNAME, and 4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <param name="request">Update payload.</param>
        /// <returns>The updated record, or 400/404/409 with typed errors. Never throws for domain failures.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RecordDto>> Update(int id, UpdateRecordRequest request)
        {
            var result = await _records.UpdateAsync(
                id,
                request.Name,
                request.Type,
                request.Ttl,
                request.Data
            );
            if (!result.Success)
            {
                return Failure(result);
            }

            var fresh = await _records.GetAsync(id);
            return Ok(ToDto(fresh.Data!));
        }

        /// <summary>Deletes a record (4-NS floor enforced).</summary>
        /// <param name="id">Record id.</param>
        /// <returns>204, or 400/404 with typed errors. Never throws for domain failures.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _records.DeleteAsync(id);
            return result.Success ? NoContent() : Failure(result);
        }

        private static RecordDto ToDto(DnsRecord record)
        {
            return new RecordDto(
                record.Id,
                record.ZoneId,
                record.Zone.Name,
                record.Name,
                RecordService.ToFqdn(record.Name, record.Zone.Name),
                record.Type,
                record.Ttl,
                record.Data,
                record.UpdatedUtc
            );
        }

        private static RecordDto ToDto(RecordRow row)
        {
            return new RecordDto(
                row.Id,
                row.ZoneId,
                row.ZoneName,
                row.Name,
                row.Fqdn,
                row.Type,
                row.Ttl,
                row.Data,
                row.UpdatedUtc
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
