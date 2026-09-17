using DnsZoneRecordManager.Services;
using DnsZoneRecordManager.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers
{
    /// <summary>Zone pages: list/search, create, rename, delete with confirm.</summary>
    public class ZonesController : Controller
    {
        private readonly IZoneService _zones;

        /// <summary>Creates the controller.</summary>
        /// <param name="zones">Zone service.</param>
        public ZonesController(IZoneService zones)
        {
            _zones = zones;
        }

        /// <summary>Zone grid with search.</summary>
        /// <param name="search">Optional name filter.</param>
        /// <returns>Zone list view.</returns>
        public async Task<IActionResult> Index(string? search)
        {
            var result = await _zones.ListAsync(search);
            return View(new ZoneIndexViewModel { Search = search, Zones = result.Data! });
        }

        /// <summary>Empty create form.</summary>
        /// <returns>Create view.</returns>
        public IActionResult Create()
        {
            return View(new ZoneFormViewModel());
        }

        /// <summary>Creates a zone; re-renders with guided errors on failure.</summary>
        /// <param name="vm">Posted form.</param>
        /// <returns>Redirect to the grid, or the form with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZoneFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _zones.CreateAsync(vm.Name);
            if (!result.Success)
            {
                AddErrors(result.Errors);
                return View(vm);
            }

            TempData["Toast"] = $"Zone '{result.Data!.Name}' was created.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Rename form for one zone.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>Edit view, or 404.</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _zones.GetAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            return View(
                "Create",
                new ZoneFormViewModel { Id = result.Data!.Id, Name = result.Data.Name }
            );
        }

        /// <summary>Renames a zone; re-renders with guided errors on failure.</summary>
        /// <param name="id">Zone id.</param>
        /// <param name="vm">Posted form.</param>
        /// <returns>Redirect to the grid, the form with errors, or 404.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ZoneFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", vm);
            }

            var result = await _zones.RenameAsync(id, vm.Name);
            if (!result.Success)
            {
                if (result.HasError(ErrorKind.NotFound))
                {
                    return NotFound();
                }

                AddErrors(result.Errors);
                return View("Create", vm);
            }

            TempData["Toast"] = $"Zone renamed to '{result.Data!.Name}'.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>Delete confirmation showing the cascade impact.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>Delete view, or 404.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _zones.GetAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            var zone = result.Data!;
            return View(
                new ZoneDeleteViewModel
                {
                    Id = zone.Id,
                    Name = zone.Name,
                    RecordCount = zone.Records.Count,
                    NsCount = zone.Records.Count(r => r.Type == Models.RecordType.NS),
                }
            );
        }

        /// <summary>Deletes a zone and its records.</summary>
        /// <param name="id">Zone id.</param>
        /// <returns>Redirect to the grid, or 404.</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _zones.DeleteAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }

            TempData["Toast"] = "Zone and its records were deleted.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
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
