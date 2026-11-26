using System.ComponentModel.DataAnnotations;

namespace Visma.Core.Enums;

/// <summary>
/// Category of reason for a visit. Stored as text in the database (see VismaDbContext)
/// so the data stays readable, and the categories make reporting easy.
/// Use <see cref="Entities.Visit.Notes"/> for free-text detail.
/// </summary>
public enum VisitPurpose
{
    /// <summary>Scheduled or ad-hoc meeting with a staff member.</summary>
    [Display(Name = "Meeting")] Meeting,

    /// <summary>Job / admission interview.</summary>
    [Display(Name = "Interview")] Interview,

    /// <summary>Dropping off or collecting a parcel or goods.</summary>
    [Display(Name = "Delivery")] Delivery,

    /// <summary>Contractor or technician doing repairs/maintenance.</summary>
    [Display(Name = "Maintenance / Repair")] Maintenance,

    /// <summary>Official business, e.g. submitting documents, inspections.</summary>
    [Display(Name = "Official business")] Official,

    /// <summary>Personal / family visit.</summary>
    [Display(Name = "Personal")] Personal,

    /// <summary>Anything else (explain in Notes).</summary>
    [Display(Name = "Other")] Other
}
