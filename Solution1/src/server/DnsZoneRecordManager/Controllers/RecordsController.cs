using DnsZoneRecordManager.Models;
using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DnsZoneRecordManager.Controllers
{
    /// <summary>Record pages: grid with zone picker/search/filter, create, edit, delete, CSV export.</summary>
    [Route("[controller]")]
    public class RecordsController : Controller
    {
        private readonly IRecordService _records;
        private readonly IZoneService _zones;

        /// <summary>Creates the controller.</summary>
        /// <param name="records">Record service.</param>
        /// <param name="zones">Zone service (zone picker).</param>
        public RecordsController(IRecordService records, IZoneService zones)
        {
            _records = records;
            _zones = zones;
        }

        /// <summary>Record grid with filters.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>Grid view, or 404 for an unknown zone.</returns>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int? zoneId, string? search, RecordType? type)
        {
            var zones = await _zones.ListAsync(null);
            var result = await _records.ListAsync(zoneId, search, type);
            if (!result.Success)
            {
                return NotFound();
            }

            var data = result.Data!;
            return View(
                new RecordIndexViewModel
                {
                    ZoneId = zoneId,
                    Search = search,
                    Type = type,
                    ZoneOptions = zones
                        .Data!.Select(z => new SelectListItem(z.Name, z.Id.ToString()))
                        .ToList(),
                    TypeOptions = TypeOptions(),
                    Records = data.Records,
                    ZoneName = data.ZoneName,
                    RecordCount = data.RecordCount,
                    NsCount = data.NsCount,
                }
            );
        }

        /// <summary>Empty create form (zone preselected when given).</summary>
        /// <param name="zoneId">Optional owning zone.</param>
        /// <returns>Create view, or 404 for an unknown zone.</returns>
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? zoneId)
        {
            var zones = await _zones.ListAsync(null);
            string zoneName = string.Empty;
            if (zoneId.HasValue)
            {
                var zone = await _zones.GetAsync(zoneId.Value);
                if (!zone.Success)
                {
                    return NotFound();
                }

                zoneName = zone.Data!.Name;
            }

            return View(
                new RecordFormViewModel
                {
                    ZoneId = zoneId ?? 0,
                    ZoneName = zoneName,
                    ZoneOptions = zones
                        .Data!.Select(z => new SelectListItem(z.Name, z.Id.ToString()))
                        .ToList(),
                }
            );
        }

        /// <summary>Creates a record; re-renders with guided errors on failure.</summary>
        /// <param name="vm">Posted form.</param>
        /// <returns>Redirect to the grid, the form with errors, or 404 for an unknown zone.</returns>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await FillAsync(vm);
                return View(vm);
            }

            var result = await _records.CreateAsync(
                vm.ZoneId,
                vm.Name,
                vm.Type!.Value,
                vm.Ttl!.Value,
                vm.Data
            );
            if (!result.Success)
            {
                if (result.HasError(ErrorKind.NotFound))
                {
                    return NotFound();
                }

                AddErrors(result.Errors);
                await FillAsync(vm);
                return View(vm);
            }

            TempData["Toast"] = "Record was created.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index), new { zoneId = vm.ZoneId });
        }

        /// <summary>Edit form for one record.</summary>
        /// <param name="id">Record id.</param>
        /// <returns>Edit view, or 404.</returns>
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _records.GetAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            var record = result.Data!;
            return View(
                "Create",
                new RecordFormViewModel
                {
                    ZoneId = record.ZoneId,
                    ZoneName = record.Zone.Name,
                    Id = record.Id,
                    Name = record.Name,
                    Type = record.Type,
                    Ttl = record.Ttl,
                    Data = record.Data,
                }
            );
        }

        /// <summary>Edits a record; re-renders with guided errors on failure.</summary>
        /// <param name="id">Record id.</param>
        /// <param name="vm">Posted form.</param>
        /// <returns>Redirect to the grid, the form with errors, or 404.</returns>
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", vm);
            }

            var result = await _records.UpdateAsync(
                id,
                vm.Name,
                vm.Type!.Value,
                vm.Ttl!.Value,
                vm.Data
            );
            if (!result.Success)
            {
                if (result.HasError(ErrorKind.NotFound))
                {
                    return NotFound();
                }

                AddErrors(result.Errors);
                return View("Create", vm);
            }

            TempData["Toast"] = "Record was updated.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index), new { zoneId = result.Data!.ZoneId });
        }

        /// <summary>Delete confirmation.</summary>
        /// <param name="id">Record id.</param>
        /// <returns>Delete view, or 404.</returns>
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _records.GetAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            var record = result.Data!;
            return View(
                new RecordDeleteViewModel
                {
                    Id = record.Id,
                    ZoneId = record.ZoneId,
                    ZoneName = record.Zone.Name,
                    Name = record.Name,
                    Fqdn = RecordService.ToFqdn(record.Name, record.Zone.Name),
                    Type = record.Type,
                    Ttl = record.Ttl,
                    Data = record.Data,
                }
            );
        }

        /// <summary>Deletes a record (blocked below the 4-NS floor).</summary>
        /// <param name="id">Record id.</param>
        /// <returns>Redirect to the unfiltered grid, delete view with the rule message, or 404.</returns>
        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _records.DeleteAsync(id);
            if (!result.Success)
            {
                if (result.HasError(ErrorKind.NotFound))
                {
                    return NotFound();
                }

                TempData["Toast"] = string.Join(" ", result.Errors.Select(e => e.Message));
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["Toast"] = "Record was deleted.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Downloads the filtered grid as records-only CSV.</summary>
        /// <param name="zoneId">Optional zone filter.</param>
        /// <param name="search">Optional name/data filter.</param>
        /// <param name="type">Optional type filter.</param>
        /// <returns>CSV file, or 404 for an unknown zone.</returns>
        [HttpGet("Export")]
        public async Task<IActionResult> Export(int? zoneId, string? search, RecordType? type)
        {
            var result = await _records.ExportCsvAsync(zoneId, search, type);
            if (!result.Success)
            {
                return NotFound();
            }

            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            return File(
                System.Text.Encoding.UTF8.GetBytes(result.Data!),
                "text/csv",
                $"dns-records-{stamp}.csv"
            );
        }

        private static List<SelectListItem> TypeOptions()
        {
            return Enum.GetValues<RecordType>()
                .Select(t => new SelectListItem(t.ToString(), t.ToString()))
                .ToList();
        }

        private async Task FillAsync(RecordFormViewModel vm)
        {
            var zones = await _zones.ListAsync(null);
            vm.ZoneOptions = zones
                .Data!.Select(z => new SelectListItem(z.Name, z.Id.ToString()))
                .ToList();
            vm.ZoneName = zones.Data!.Find(z => z.Id == vm.ZoneId)?.Name ?? vm.ZoneName;
        }

        private void AddErrors(IEnumerable<ServiceError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
        }
    }
}
