using System.ComponentModel.DataAnnotations;

namespace Visma.Core.Enums;

/// <summary>
/// Category of reason for a visit. Stored as text in the database (see VismaDbContext)
/// so the data stays readable, and the categories make reporting easy.
/// Use <see cref="Entities.Visit.Notes"/> for free-text detail.
/// </summary>
public enum VisitPurpose
{
    [Display(Name = "Meeting")] Meeting,
    [Display(Name = "Interview")] Interview,
    [Display(Name = "Delivery")] Delivery,
    [Display(Name = "Maintenance / Repair")] Maintenance,
    [Display(Name = "Official business")] Official,
    [Display(Name = "Personal")] Personal,
    [Display(Name = "Other")] Other
}
