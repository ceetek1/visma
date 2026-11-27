using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Visma.Core.Entities;
using Visma.Core.Interfaces;
using Visma.Core.Services;
using Visma.Web.Infrastructure;
using Visma.Web.Models;
using Host = Visma.Core.Entities.Host; // not Microsoft.Extensions.Hosting.Host (implicit using)

namespace Visma.Web.Controllers;

/// <summary>
/// Increment 2 (Person 3): check visitors in to a host with a purpose, show their pass,
/// and check them out. Business rules live in <see cref="IVisitService"/>; this controller
/// only gathers input, calls the service and picks the next screen (Post/Redirect/Get).
/// </summary>
public class VisitsController(
    IVisitService visitService,
    IVisitRepository visits,
    IVisitorRepository visitors,
    IHostRepository hosts,
    TimeProvider clock) : Controller
{
    /// <summary>
    /// GET /Visits/CheckIn : step 1 (find the visitor) or, with ?visitorId=, step 2 (host and purpose).
    /// An exact phone / e-mail match skips the result list and goes straight to step 2.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckIn(string? q, int? visitorId)
    {
        if (visitorId is not null)
        {
            var visitor = await visitors.GetByIdAsync(visitorId.Value);
            if (visitor is null)
            {
                TempData["Error"] = "Visitor not found.";
                return RedirectToAction(nameof(CheckIn));
            }

            return View("CheckInDetails", await BuildDetailsAsync(visitor, new CheckInFormModel { VisitorId = visitor.Id }));
        }

        var term = q?.Trim();
        var openByVisitor = (await visitService.GetActiveVisitsAsync())
            .GroupBy(v => v.VisitorId)
            .ToDictionary(g => g.Key, g => g.First());
        VisitorPickRow Row(Visitor v) => new(v, openByVisitor.GetValueOrDefault(v.Id));

        if (string.IsNullOrEmpty(term))
        {
            var everyone = await visitors.SearchAsync(null);
            var recent = everyone.OrderByDescending(v => v.CreatedAt).Take(5).Select(Row).ToList();
            return View("CheckInFind", new CheckInFindViewModel { Recent = recent });
        }

        var exact = await visitors.FindByPhoneOrEmailAsync(term);
        if (exact is not null)
        {
            return RedirectToAction(nameof(CheckIn), new { visitorId = exact.Id });
        }

        var results = (await visitors.SearchAsync(term)).Select(Row).ToList();
        return View("CheckInFind", new CheckInFindViewModel { Query = term, Results = results });
    }

    /// <summary>POST /Visits/CheckIn : create the visit, then show the visitor pass.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn([Bind(Prefix = "Form")] CheckInFormModel form)
    {
        var visitor = await visitors.GetByIdAsync(form.VisitorId);
        if (visitor is null)
        {
            TempData["Error"] = "Visitor not found.";
            return RedirectToAction(nameof(CheckIn));
        }

        if (!ModelState.IsValid)
        {
            return View("CheckInDetails", await BuildDetailsAsync(visitor, form));
        }

        var result = await visitService.CheckInAsync(visitor.Id, form.HostId!.Value, form.Purpose!.Value, form.Notes);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View("CheckInDetails", await BuildDetailsAsync(visitor, form));
        }

        var visit = result.Value!;
        TempData["Success"] = $"Checked in {visit.Visitor?.FullName} to see {visit.Host?.FullName}.";
        return RedirectToAction(nameof(Pass), new { id = visit.Id });
    }

    /// <summary>GET /Visits/Pass/5 : the visitor pass shown after a successful check-in.</summary>
    [HttpGet]
    public async Task<IActionResult> Pass(int id)
    {
        var visit = await visits.GetByIdAsync(id);
        return visit is null ? NotFound() : View(visit);
    }

    /// <summary>
    /// POST /Visits/CheckOut/5 : record the time the visitor left.
    /// Always a POST with an antiforgery token: a GET link could be triggered by a crawler or
    /// another site. <paramref name="returnUrl"/> is only followed if it points inside VISMA.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id, string? returnUrl)
    {
        var result = await visitService.CheckOutAsync(id);
        if (result.Succeeded)
        {
            var visit = result.Value!;
            var stayed = visit.Duration(visit.CheckOutTime!.Value).ToFriendlyDuration();
            TempData["Success"] = $"Checked out {visit.Visitor?.FullName} after {stayed}.";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(HomeController.Index), "Home");
    }

    /// <summary>
    /// GET /Visits/History?from=2026-11-20&amp;to=2026-11-26&amp;hostId=3 : the visit log (FR-17).
    /// Dates are the front desk's local calendar days (default: the last 7 days, today included);
    /// they are turned into a UTC range [from 00:00, to + 1 day 00:00) because that is how times are stored.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> History(DateOnly? from, DateOnly? to, int? hostId)
    {
        var lastDay = to ?? LocalDay.Today(clock);
        var firstDay = from ?? lastDay.AddDays(-6);
        var allHosts = HostOptions(await hosts.GetAllAsync(), hostId, markInactive: true);

        if (firstDay > lastDay)
        {
            return View(new VisitHistoryViewModel
            {
                From = firstDay, To = lastDay, HostId = hostId, Hosts = allHosts,
                FilterError = "The start date is after the end date. Pick a start date on or before the end date.",
            });
        }

        var list = await visitService.GetHistoryAsync(
            LocalDay.StartUtc(firstDay, clock),
            LocalDay.StartUtc(lastDay.AddDays(1), clock),
            hostId,
            visitorId: null);

        return View(new VisitHistoryViewModel
        {
            From = firstDay, To = lastDay, HostId = hostId, Hosts = allHosts, Visits = list,
        });
    }

    /// <summary>Step 2 model: the visitor, any open visit, and active hosts grouped by department.</summary>
    private async Task<CheckInDetailsViewModel> BuildDetailsAsync(Visitor visitor, CheckInFormModel form)
    {
        var openVisit = (await visitService.GetActiveVisitsAsync()).FirstOrDefault(v => v.VisitorId == visitor.Id);
        return new CheckInDetailsViewModel
        {
            Visitor = visitor,
            OpenVisit = openVisit,
            Form = form,
            Hosts = HostOptions(await hosts.GetActiveAsync(), form.HostId, markInactive: false),
        };
    }

    /// <summary>Hosts as &lt;option&gt;s inside one &lt;optgroup&gt; per department.</summary>
    internal static List<SelectListItem> HostOptions(IEnumerable<Host> list, int? selectedId, bool markInactive)
    {
        var groups = new Dictionary<string, SelectListGroup>(StringComparer.OrdinalIgnoreCase);
        return list
            .OrderBy(h => h.Department).ThenBy(h => h.FullName)
            .Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = markInactive && !h.IsActive ? $"{h.FullName} (no longer here)" : h.FullName,
                Selected = h.Id == selectedId,
                Group = groups.TryGetValue(h.Department, out var g)
                    ? g
                    : groups[h.Department] = new SelectListGroup { Name = h.Department },
            })
            .ToList();
    }
}
