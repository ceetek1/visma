using Visma.Core.Enums;
using Visma.Core.Services;
using Visma.Data;
using Visma.Data.Repositories;
using Visma.Tests.Infrastructure;

namespace Visma.Tests.Services;

/// <summary>
/// Integration tests: VisitService + repositories + EF Core + SQLite working together.
/// Each step uses a NEW DbContext, the way each web request gets its own, so nothing passes
/// only because EF still had the entity in memory. TC-I40..TC-I41.
/// </summary>
[Trait("Level", "Integration")]
public class VisitFlowIntegrationTests
{
    private static readonly DateTime Nine = new(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc);

    private static VisitService ServiceFor(VismaDbContext db, TimeProvider clock) =>
        new(new VisitRepository(db), new VisitorRepository(db), new HostRepository(db), clock);

    // TC-I40 (FR-11 to FR-14, FR-17): check in -> on site -> check out -> history
    [Fact]
    public async Task FullVisit_CheckInToCheckOut_IsStoredAndFoundInHistory()
    {
        using var db = new TestDbFactory();
        var clock = new FakeClock(Nine);
        var visitor = TestData.Visitor("Ada Okafor", "08031234567");
        var host = TestData.Host("Dr. Chinedu Eze", "ICT");
        await new VisitorRepository(db.Context).AddAsync(visitor);
        await new HostRepository(db.Context).AddAsync(host);

        // Request 1: the receptionist checks Ada in.
        int visitId;
        using (var ctx = db.NewContext())
        {
            var result = await ServiceFor(ctx, clock).CheckInAsync(visitor.Id, host.Id, VisitPurpose.Meeting, "Project review");
            Assert.True(result.Succeeded, result.Error);
            visitId = result.Value!.Id;
        }

        // Request 2: the dashboard shows her on site.
        using (var ctx = db.NewContext())
        {
            var service = ServiceFor(ctx, clock);
            var onSite = Assert.Single(await service.GetActiveVisitsAsync());
            Assert.Equal("Ada Okafor", onSite.Visitor?.FullName);
            Assert.Equal("Dr. Chinedu Eze", onSite.Host?.FullName);
            Assert.Equal(1, (await service.GetDashboardStatsAsync()).OnSiteNow);
        }

        // Request 3: 50 minutes later she leaves.
        clock.Advance(TimeSpan.FromMinutes(50));
        using (var ctx = db.NewContext())
        {
            var result = await ServiceFor(ctx, clock).CheckOutAsync(visitId);
            Assert.True(result.Succeeded, result.Error);
        }

        // Request 4: nobody on site; the visit log has both times.
        using (var ctx = db.NewContext())
        {
            var service = ServiceFor(ctx, clock);
            Assert.Empty(await service.GetActiveVisitsAsync());

            var day = await service.GetHistoryAsync(Nine.Date, Nine.Date.AddDays(1), host.Id, null);
            var logged = Assert.Single(day);
            Assert.Equal(Nine, logged.CheckInTime);
            Assert.Equal(Nine.AddMinutes(50), logged.CheckOutTime);
            Assert.Equal(VisitStatus.CheckedOut, logged.Status);
            Assert.Equal("Project review", logged.Notes);

            var stats = await service.GetDashboardStatsAsync();
            Assert.Equal((0, 1, 1), (stats.OnSiteNow, stats.CheckInsToday, stats.CheckedOutToday));
        }
    }

    // TC-I41 (FR-17): the history range includes its start and excludes its end,
    // so consecutive days never count the same visit twice.
    [Fact]
    public async Task History_RangeIsStartInclusiveEndExclusive()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor();
        var host = TestData.Host();
        await new VisitorRepository(db.Context).AddAsync(visitor);
        await new HostRepository(db.Context).AddAsync(host);
        var midnight = Nine.Date; // 26 Nov 00:00 UTC
        await new VisitRepository(db.Context).AddAsync(TestData.Visit(visitor.Id, host.Id, midnight, midnight.AddHours(1)));

        var service = ServiceFor(db.NewContext(), new FakeClock(Nine));
        var dayBefore = await service.GetHistoryAsync(midnight.AddDays(-1), midnight, null, null);
        var sameDay = await service.GetHistoryAsync(midnight, midnight.AddDays(1), null, null);

        Assert.Empty(dayBefore);
        Assert.Single(sameDay);
    }
}
