---
name: api-design
description: REST API design for DnsZoneRecordManager (ASP.NET Core controllers + OpenAPI + Next.js client)
version: 2.0.0
author: opencode
tags:
  - rest
  - openapi
  - api-design
  - aspnet-core
  - nextjs
capabilities:
  - rest-best-practices
  - openapi-specification
  - fluentvalidation-errors
  - pagination
  - filtering
references:
  - "https://learn.microsoft.com/aspnet/core/web-api/"
  - "https://learn.microsoft.com/aspnet/core/fundamentals/openapi/using-openapi-documents"
  - "https://docs.fluentvalidation.net/"
examples:
  - name: "OpenAPI (built-in, no extra package)"
    description: "Solution2 already wires Microsoft.AspNetCore.OpenApi; serves /openapi/v1.json in Development"
    code: |
      // Program.cs (explicit Program class, block-scoped namespace)
      builder.Services.AddControllers();
      builder.Services.AddOpenApi();
      // ...
      if (app.Environment.IsDevelopment())
      {
          app.MapOpenApi();
      }
      app.MapControllers();
  - name: "Zones/Records Controller"
    description: "Thin controllers; validation lives in FluentValidation validators, handlers own the rules"
    code: |
      using FluentValidation;
      using Microsoft.AspNetCore.Mvc;

      namespace DnsZoneRecordManager.Controllers
      {
          [ApiController]
          [Route("api/[controller]")]
          public class ZonesController : ControllerBase
          {
              [HttpGet]
              public async Task<ActionResult<IEnumerable<ZoneDto>>> List(
                  [FromQuery] string? search, CancellationToken ct)
              {
                  var zones = await _sender.SendAsync(new ListZones(search), ct);
                  return Ok(zones);
              }

              [HttpPost]
              public async Task<ActionResult<ZoneDto>> Create(
                  [FromBody] CreateZone request, CancellationToken ct)
              {
                  var result = await _validator.ValidateAsync(request, ct);
                  if (!result.IsValid)
                  {
                      return ValidationProblem(result.ToDictionary());
                  }

                  var zone = await _sender.SendAsync(request, ct);
                  return CreatedAtAction(nameof(Get), new { id = zone.Id }, zone);
              }
          }
      }
  - name: "FluentValidation error shape"
    description: "400 with per-field messages the non-technical UI renders inline (A4/A5)"
    code: |
      // ZoneNameValidator.cs
      using FluentValidation;

      namespace DnsZoneRecordManager.Validation
      {
          public class ZoneNameValidator : AbstractValidator<CreateZone>
          {
              public ZoneNameValidator()
              {
                  RuleFor(x => x.Name)
                      .NotEmpty().WithMessage("Give the zone a name, e.g. nahuexolab.com.")
                      .MaximumLength(253);
              }
          }
      }
  - name: "Next.js data fetch (Solution2 client)"
    description: "Server re-validates everything; client mirrors messages only"
    code: |
      // src/client/src/app/zones/page.tsx
      const res = await fetch(`${process.env.API_URL}/api/zones?search=${q}`, { cache: "no-store" });
      if (!res.ok) throw new Error("Could not load zones.");
      const zones: ZoneDto[] = await res.json();
