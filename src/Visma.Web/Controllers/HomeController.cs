using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Visma.Core.Services;
using Visma.Web.Infrastructure;
using Visma.Web.Models;

namespace Visma.Web.Controllers;

/// <summary>
/// The live dashboard (Person 3), "coming soon" placeholders and error pages.
/// </summary>
public class HomeController(
    IVisitService visitService,
    NavigationHelper nav,
    TimeProvider clock) : Controller
{
    /// <summary>GET / : today's figures and everyone on site, each with a Check out button.</summary>
    public async Task<IActionResult> Index()
    {
        var localNow = clock.GetLocalNow();

        var model = new HomeIndexViewModel
        {
            OnSite = await visitService.GetActiveVisitsAsync(),
            Stats = await visitService.GetDashboardStatsAsync(),
            LocalNow = localNow,
            Greeting = localNow.Hour switch
            {
                < 12 => "Good morning",
                < 17 => "Good afternoon",
                _ => "Good evening"
            },
            Increments =
            [
                new("Foundation", "Person 1", "Layered solution, database, seed data, layout", true),
                new("Registration and hosts", "Person 2", "Register visitors, find returning visitors, manage the host directory",
                    nav.Exists("Visitors") && nav.Exists("Hosts")),
                new("Visits and dashboard", "Person 3", "Check in, check out, live dashboard and visit history",
                    nav.Exists("Visits", "CheckIn")),
            ]
        };

        return View(model);
    }

    /// <summary>GET /Home/ComingSoon?feature=Visitors : placeholder until a teammate's controller lands.</summary>
    public IActionResult ComingSoon(string? feature)
    {
        var model = feature?.ToLowerInvariant() switch
        {
            "visitors" => new ComingSoonViewModel("Visitor registration", "Person 2", "Increment 1",
                "Register new visitors, look up returning visitors by phone or e-mail, and edit visitor details."),
            "hosts" => new ComingSoonViewModel("Host directory", "Person 2", "Increment 1",
                "Add the staff members visitors come to see, and deactivate those who have left."),
            "visits" => new ComingSoonViewModel("Check-in and check-out", "Person 3", "Increment 2",
                "Check visitors in to a host with a purpose, check them out, and browse the visit history."),
            _ => new ComingSoonViewModel("This page", "the team", "a later increment",
                "This part of VISMA hasn't been built yet.")
        };

        ViewData["Feature"] = feature;
        return View(model);
    }

    /// <summary>Friendly page for 404 and other status codes (wired in Program.cs).</summary>
    [Route("Home/StatusCode")]
    public IActionResult StatusCodePage(int code)
    {
        ViewData["Code"] = code;
        return View("StatusCode");
    }

    /// <summary>Unhandled exception page (production only).</summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
