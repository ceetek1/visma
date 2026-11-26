using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Visma.Web.Infrastructure;

/// <summary>Small formatting helpers used by Razor views.</summary>
public static class DisplayExtensions
{
    /// <summary>
    /// Friendly text for an enum value using its [Display(Name = ...)] attribute,
    /// e.g. VisitPurpose.Maintenance becomes "Maintenance / Repair".
    /// </summary>
    public static string GetDisplayName(this Enum value) =>
        value.GetType().GetMember(value.ToString()).FirstOrDefault()?
             .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();

    /// <summary>
    /// Times are STORED in UTC; show them in the server's local time zone,
    /// e.g. "26 Nov 2026, 14:05".
    /// </summary>
    public static string ToDisplayTime(this DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd MMM yyyy, HH:mm");

    /// <summary>Nullable overload: shows an em dash when there is no value (e.g. not checked out).</summary>
    public static string ToDisplayTime(this DateTime? utc) => utc?.ToDisplayTime() ?? "—";

    /// <summary>Human-friendly duration, e.g. "1h 25m" or "12m".</summary>
    public static string ToFriendlyDuration(this TimeSpan span) =>
        span.TotalHours >= 1 ? $"{(int)span.TotalHours}h {span.Minutes}m" : $"{Math.Max(0, span.Minutes)}m";
}
