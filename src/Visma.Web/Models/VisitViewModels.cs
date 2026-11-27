using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Visma.Core.Entities;
using Visma.Core.Enums;

namespace Visma.Web.Models;

// Screen models for Increment 2 (Person 3): check-in wizard, visitor pass, visit log.

/// <summary>One visitor in a pick list, plus their open visit if they are already on site.</summary>
/// <param name="Visitor">The visitor.</param>
/// <param name="OpenVisit">Their current visit, or null if they can be checked in.</param>
public record VisitorPickRow(Visitor Visitor, Visit? OpenVisit);

/// <summary>Check-in step 1: find the visitor (GET /Visits/CheckIn?q=…).</summary>
public class CheckInFindViewModel
{
    /// <summary>What the receptionist typed: phone, e-mail or part of a name.</summary>
    public string? Query { get; init; }

    /// <summary>True once a search has been run (so "no match" can be told apart from "not searched").</summary>
    public bool Searched => !string.IsNullOrWhiteSpace(Query);

    /// <summary>Search results (empty before a search).</summary>
    public IReadOnlyList<VisitorPickRow> Results { get; init; } = [];

    /// <summary>The most recently registered visitors, shown before any search.</summary>
    public IReadOnlyList<VisitorPickRow> Recent { get; init; } = [];
}

/// <summary>
/// The fields posted by check-in step 2 (POST /Visits/CheckIn).
/// Nullable HostId / Purpose let [Required] catch "nothing chosen" instead of silently binding 0.
/// </summary>
public class CheckInFormModel
{
    /// <summary>Visitor chosen in step 1.</summary>
    public int VisitorId { get; set; }

    /// <summary>Host the visitor has come to see.</summary>
    [Required(ErrorMessage = "Select the person being visited.")]
    [Display(Name = "Person being visited")]
    public int? HostId { get; set; }

    /// <summary>Why they came.</summary>
    [Required(ErrorMessage = "Select the purpose of the visit.")]
    [Display(Name = "Purpose of visit")]
    public VisitPurpose? Purpose { get; set; }

    /// <summary>Optional detail, e.g. "Final-year project review".</summary>
    [StringLength(250, ErrorMessage = "Keep notes to 250 characters or fewer.")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

/// <summary>Check-in step 2: host and purpose for a chosen visitor (GET /Visits/CheckIn?visitorId=…).</summary>
public class CheckInDetailsViewModel
{
    /// <summary>The visitor being checked in.</summary>
    public required Visitor Visitor { get; init; }

    /// <summary>Set if the visitor is already on site; the page then offers check-out instead of the form.</summary>
    public Visit? OpenVisit { get; init; }

    /// <summary>Form values (empty on first load, the posted values when re-shown with errors).</summary>
    public CheckInFormModel Form { get; init; } = new();

    /// <summary>Active hosts grouped by department (rendered as &lt;optgroup&gt;s).</summary>
    public IReadOnlyList<SelectListItem> Hosts { get; init; } = [];
}

/// <summary>Visit log with filters (GET /Visits/History?from=…&amp;to=…&amp;hostId=…).</summary>
public class VisitHistoryViewModel
{
    /// <summary>First local day included.</summary>
    public DateOnly From { get; init; }

    /// <summary>Last local day included.</summary>
    public DateOnly To { get; init; }

    /// <summary>Only visits to this host (null = everyone).</summary>
    public int? HostId { get; init; }

    /// <summary>Matching visits, most recent first.</summary>
    public IReadOnlyList<Visit> Visits { get; init; } = [];

    /// <summary>Every host, including those no longer receiving visitors, grouped by department.</summary>
    public IReadOnlyList<SelectListItem> Hosts { get; init; } = [];

    /// <summary>Problem with the filters (e.g. start after end), shown instead of results.</summary>
    public string? FilterError { get; init; }
}
