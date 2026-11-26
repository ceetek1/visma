namespace Visma.Tests.Infrastructure;

/// <summary>
/// A clock you control. Pass it wherever production code takes a <see cref="TimeProvider"/>
/// so time-based rules (check-in/out, "today") give the same answer on every run.
/// <code>
/// var clock = new FakeClock(new DateTime(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc));
/// clock.Advance(TimeSpan.FromMinutes(45)); // 09:45
/// </code>
/// </summary>
public sealed class FakeClock(DateTime startUtc) : TimeProvider
{
    private DateTimeOffset _now = new(DateTime.SpecifyKind(startUtc, DateTimeKind.Utc));

    /// <summary>Default start: 26 Nov 2026 09:00 UTC.</summary>
    public FakeClock() : this(new DateTime(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc)) { }

    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Treat UTC as local time so "today" boundaries are predictable in tests.</summary>
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    /// <summary>Move time forward.</summary>
    public void Advance(TimeSpan by) => _now = _now.Add(by);

    /// <summary>Current UTC time as a DateTime (handy for building entities).</summary>
    public DateTime UtcNow => _now.UtcDateTime;
}
