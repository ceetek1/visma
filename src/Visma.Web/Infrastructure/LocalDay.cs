namespace Visma.Web.Infrastructure;

/// <summary>
/// Converts the front desk's calendar days into the UTC instants the database stores.
/// A local day runs from local midnight to the next local midnight; in Lagos (UTC+1),
/// 26 Nov starts at 25 Nov 23:00 UTC. Always go through the injected <see cref="TimeProvider"/>
/// so tests can pin both the time and the time zone.
/// </summary>
public static class LocalDay
{
    /// <summary>Today's date in the clock's local time zone.</summary>
    public static DateOnly Today(TimeProvider clock) => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    /// <summary>The UTC instant at which a local day begins (its local midnight).</summary>
    public static DateTime StartUtc(DateOnly day, TimeProvider clock) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), clock.LocalTimeZone);
}
