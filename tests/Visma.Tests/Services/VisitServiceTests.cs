using Visma.Core.Entities;
using Visma.Core.Enums;
using Visma.Core.Services;
using Visma.Data.Repositories;
using Visma.Tests.Infrastructure;

namespace Visma.Tests.Services;

/// <summary>
/// Unit tests for <see cref="VisitService"/>: the check-in / check-out rules (FR-11 to FR-16).
/// TC-U40..TC-U51.
///
/// The service is the unit under test. Its repositories run on a throw-away in-memory SQLite
/// database (<see cref="TestDbFactory"/>) instead of hand-written fakes, so the "open visit"
/// query the double-check-in rule relies on is the real one. Time comes from
/// <see cref="FakeClock"/>, so every timestamp below is exact.
/// </summary>
[Trait("Level", "Unit")]
public class VisitServiceTests
{
    private static readonly DateTime Nine = new(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc);

    /// <summary>A service wired to one test database, plus a visitor and an active host in it.</summary>
    private sealed class Rig : IDisposable
    {
        public TestDbFactory Db { get; } = new();
        public FakeClock Clock { get; }
        public VisitService Service { get; }
        public Visitor Visitor { get; }
        public Host Host { get; }

        public Rig(TimeProvider? clock = null)
        {
            Clock = clock as FakeClock ?? new FakeClock(Nine);
            Service = new VisitService(new VisitRepository(Db.Context), new VisitorRepository(Db.Context),
                                       new HostRepository(Db.Context), clock ?? Clock);
            Visitor = AddVisitor("Ada Okafor", "08031234567");
            Host = AddHost("Dr. Chinedu Eze", "ICT");
        }

        public Visitor AddVisitor(string name, string phone)
        {
            var visitor = TestData.Visitor(name, phone);
            new VisitorRepository(Db.Context).AddAsync(visitor).GetAwaiter().GetResult();
            return visitor;
        }

        public Host AddHost(string name, string dept, bool active = true)
        {
            var host = TestData.Host(name, dept, active);
            new HostRepository(Db.Context).AddAsync(host).GetAwaiter().GetResult();
            return host;
        }

        public void Dispose() => Db.Dispose();
    }

    // TC-U40 (FR-11): a valid check-in is stamped with the clock's time and carries names for the pass
    [Fact]
    public async Task CheckIn_Valid_RecordsClockTimeAndLoadsNames()
    {
        using var rig = new Rig();

        var result = await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Meeting, "Project review");

        Assert.True(result.Succeeded, result.Error);
        var visit = result.Value!;
        Assert.True(visit.Id > 0);
        Assert.Equal(Nine, visit.CheckInTime);
        Assert.Null(visit.CheckOutTime);
        Assert.Equal(VisitStatus.CheckedIn, visit.Status);
        Assert.Equal("Ada Okafor", visit.Visitor?.FullName);
        Assert.Equal("Dr. Chinedu Eze", visit.Host?.FullName);
        Assert.Equal("Project review", visit.Notes);
    }

    // TC-U41 (FR-13): no double check-in
    [Fact]
    public async Task CheckIn_SameVisitorTwice_SecondIsRefused()
    {
        using var rig = new Rig();
        await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Meeting, null);

        var second = await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Delivery, null);

        Assert.False(second.Succeeded);
        Assert.Equal("Ada Okafor is already checked in. Check them out before checking in again.", second.Error);
        Assert.Single(await rig.Service.GetActiveVisitsAsync());
    }

    // TC-U42 (FR-12): an inactive host cannot receive visitors
    [Fact]
    public async Task CheckIn_InactiveHost_IsRefused()
    {
        using var rig = new Rig();
        var former = rig.AddHost("Mr. Segun Afolabi", "Security", active: false);

        var result = await rig.Service.CheckInAsync(rig.Visitor.Id, former.Id, VisitPurpose.Official, null);

        Assert.False(result.Succeeded);
        Assert.Equal("Mr. Segun Afolabi is no longer receiving visitors. Choose another host.", result.Error);
    }

    // TC-U43 (FR-11): unknown visitor / unknown host
    [Fact]
    public async Task CheckIn_UnknownVisitorOrHost_IsRefused()
    {
        using var rig = new Rig();

        var noVisitor = await rig.Service.CheckInAsync(9999, rig.Host.Id, VisitPurpose.Meeting, null);
        var noHost = await rig.Service.CheckInAsync(rig.Visitor.Id, 9999, VisitPurpose.Meeting, null);

        Assert.Equal("Visitor not found.", noVisitor.Error);
        Assert.Equal("Host not found.", noHost.Error);
    }

    // TC-U44 (FR-14, FR-15): check-out records the clock's time, and the duration follows
    [Fact]
    public async Task CheckOut_After75Minutes_RecordsTimeAndDuration()
    {
        using var rig = new Rig();
        var visit = (await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Interview, null)).Value!;

        rig.Clock.Advance(TimeSpan.FromMinutes(75));
        var result = await rig.Service.CheckOutAsync(visit.Id);

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(Nine.AddMinutes(75), result.Value!.CheckOutTime);
        Assert.Equal(TimeSpan.FromMinutes(75), result.Value.Duration(rig.Clock.UtcNow.AddHours(5)));
        Assert.Equal(VisitStatus.CheckedOut, result.Value.Status);
        Assert.Empty(await rig.Service.GetActiveVisitsAsync());
    }

    // TC-U45 (FR-14): a visit can only be checked out once
    [Fact]
    public async Task CheckOut_Twice_SecondIsRefused()
    {
        using var rig = new Rig();
        var visit = (await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Meeting, null)).Value!;
        await rig.Service.CheckOutAsync(visit.Id);

        rig.Clock.Advance(TimeSpan.FromMinutes(10));
        var second = await rig.Service.CheckOutAsync(visit.Id);

        Assert.False(second.Succeeded);
        Assert.Equal("This visit was already checked out.", second.Error);
    }

    // TC-U46 (FR-13): after checking out, the same visitor may check in again (a new visit)
    [Fact]
    public async Task CheckOut_ThenCheckInAgain_CreatesNewVisit()
    {
        using var rig = new Rig();
        var first = (await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Meeting, null)).Value!;
        rig.Clock.Advance(TimeSpan.FromMinutes(30));
        await rig.Service.CheckOutAsync(first.Id);

        rig.Clock.Advance(TimeSpan.FromHours(2));
        var again = await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Personal, null);

        Assert.True(again.Succeeded, again.Error);
        Assert.NotEqual(first.Id, again.Value!.Id);
        Assert.Equal(2, (await rig.Service.GetHistoryAsync(null, null, null, rig.Visitor.Id)).Count);
    }

    // TC-U47 (FR-16): dashboard figures count today only
    [Fact]
    public async Task DashboardStats_CountTodayOnly()
    {
        using var rig = new Rig();
        var ibrahim = rig.AddVisitor("Ibrahim Musa", "08129876543");
        var grace = rig.AddVisitor("Grace Olaniyan", "07065554433");
        var tunde = rig.AddVisitor("Tunde Bello", "08051112222");
        var visits = new VisitRepository(rig.Db.Context);

        // Yesterday: one visit, already closed.
        await visits.AddAsync(TestData.Visit(tunde.Id, rig.Host.Id, Nine.AddDays(-1), Nine.AddDays(-1).AddHours(1)));
        // Today: two still on site, one already gone.
        await visits.AddAsync(TestData.Visit(rig.Visitor.Id, rig.Host.Id, Nine.AddHours(-2)));
        await visits.AddAsync(TestData.Visit(ibrahim.Id, rig.Host.Id, Nine.AddHours(-1)));
        await visits.AddAsync(TestData.Visit(grace.Id, rig.Host.Id, Nine.AddHours(-3), Nine.AddHours(-2)));

        var stats = await rig.Service.GetDashboardStatsAsync();

        Assert.Equal(2, stats.OnSiteNow);
        Assert.Equal(3, stats.CheckInsToday);
        Assert.Equal(1, stats.CheckedOutToday);
        Assert.Equal(4, stats.TotalVisitors);
    }

    // TC-U48 (FR-11): blank notes are stored as null; real notes are trimmed
    [Fact]
    public async Task CheckIn_BlankNotes_StoredAsNull()
    {
        using var rig = new Rig();
        var ibrahim = rig.AddVisitor("Ibrahim Musa", "08129876543");

        var blank = await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, VisitPurpose.Meeting, "   ");
        var padded = await rig.Service.CheckInAsync(ibrahim.Id, rig.Host.Id, VisitPurpose.Meeting, "  Parcel  ");

        Assert.Null(blank.Value!.Notes);
        Assert.Equal("Parcel", padded.Value!.Notes);
    }

    // TC-U49 (FR-14): checking out a visit that doesn't exist
    [Fact]
    public async Task CheckOut_UnknownVisit_IsRefused()
    {
        using var rig = new Rig();

        var result = await rig.Service.CheckOutAsync(9999);

        Assert.False(result.Succeeded);
        Assert.Equal("Visit not found.", result.Error);
    }

    // TC-U50 (FR-11): a purpose value outside the VisitPurpose list is refused
    [Fact]
    public async Task CheckIn_UndefinedPurpose_IsRefused()
    {
        using var rig = new Rig();

        var result = await rig.Service.CheckInAsync(rig.Visitor.Id, rig.Host.Id, (VisitPurpose)99, null);

        Assert.False(result.Succeeded);
        Assert.Equal("Choose a purpose for the visit.", result.Error);
    }

    // TC-U51 (FR-16, NFR-08): "today" means the front desk's local day, not the UTC day.
    // In Lagos (UTC+1), 23:30 UTC on 25 Nov is already 00:30 on 26 Nov, so it counts as today;
    // 22:30 UTC on 25 Nov is 23:30 local on the 25th, so it doesn't.
    [Fact]
    public async Task DashboardStats_TodayUsesLocalTimeZone()
    {
        var lagos = new ZonedClock(Nine, TimeZoneInfo.CreateCustomTimeZone("WAT", TimeSpan.FromHours(1), "WAT", "WAT"));
        using var rig = new Rig(lagos);
        var ibrahim = rig.AddVisitor("Ibrahim Musa", "08129876543");
        var visits = new VisitRepository(rig.Db.Context);

        await visits.AddAsync(TestData.Visit(rig.Visitor.Id, rig.Host.Id,
            new DateTime(2026, 11, 25, 23, 30, 0, DateTimeKind.Utc), Nine));            // 00:30 local today
        await visits.AddAsync(TestData.Visit(ibrahim.Id, rig.Host.Id,
            new DateTime(2026, 11, 25, 22, 30, 0, DateTimeKind.Utc), Nine));            // 23:30 local yesterday

        var stats = await rig.Service.GetDashboardStatsAsync();

        Assert.Equal(1, stats.CheckInsToday);
        Assert.Equal(1, stats.CheckedOutToday);
    }

    /// <summary>A frozen clock in a chosen time zone (FakeClock is always UTC).</summary>
    private sealed class ZonedClock(DateTime utcNow, TimeZoneInfo zone) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
        public override TimeZoneInfo LocalTimeZone => zone;
    }
}
