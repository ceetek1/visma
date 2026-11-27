using Visma.Core.Common;
using Visma.Core.Entities;

namespace Visma.Web.Models;

// View models = data shaped for ONE screen. Controllers build them; Razor views read them.
// Persons 2 and 3: put your screen models in this folder too (one file per feature is fine,
// e.g. Models/VisitorViewModels.cs).

/// <summary>Data for the dashboard (Person 1 landing page, made live by Person 3).</summary>
public class HomeIndexViewModel
{
    /// <summary>Visits that are still open (people on site right now), most recent first.</summary>
    public IReadOnlyList<Visit> OnSite { get; init; } = [];

    /// <summary>Today's figures from IVisitService.GetDashboardStatsAsync.</summary>
    public DashboardStats Stats { get; init; } = new(0, 0, 0, 0);

    /// <summary>
    /// Feature progress shown while increments are being built.
    /// Remove (with the panel in Views/Home/Index.cshtml) once all three increments are merged.
    /// </summary>
    public IReadOnlyList<IncrementStatus> Increments { get; init; } = [];

    /// <summary>"Good morning" / "Good afternoon" / "Good evening".</summary>
    public string Greeting { get; init; } = "Hello";

    /// <summary>Local date for the header.</summary>
    public DateTimeOffset LocalNow { get; init; }
}

/// <summary>One increment in the Incremental process model and whether its code has landed.</summary>
public record IncrementStatus(string Name, string Owner, string Summary, bool IsReady);

/// <summary>Sidebar link. Points to "Coming soon" until the controller exists.</summary>
public record NavLinkModel(string Controller, string Action, string Label, string Icon);

/// <summary>Reusable empty-state block: what's missing and what to do about it.</summary>
public record EmptyStateModel(string Icon, string Title, string Message,
                              string? ActionText = null, string? ActionUrl = null);

/// <summary>Placeholder page for features that a teammate hasn't delivered yet.</summary>
public record ComingSoonViewModel(string Feature, string Owner, string Increment, string Description);
