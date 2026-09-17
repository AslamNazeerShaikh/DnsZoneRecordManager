using System.Diagnostics;
using DnsZoneRecordManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace DnsZoneRecordManager.Controllers;

/// <summary>Landing, privacy, and error pages.</summary>
public class HomeController : Controller
{
    /// <summary>CRM-style dashboard landing page.</summary>
    /// <returns>Index view.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Privacy page.</summary>
    /// <returns>Privacy view.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>Error page (no caching).</summary>
    /// <returns>Error view with the request id.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
