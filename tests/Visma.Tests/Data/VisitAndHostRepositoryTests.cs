using Visma.Data.Repositories;
using Visma.Tests.Infrastructure;

namespace Visma.Tests.Data;

/// <summary>Integration tests: HostRepository and VisitRepository. TC-I06..TC-I10.</summary>
[Trait("Level", "Integration")]
public class VisitAndHostRepositoryTests
{
    private static readonly DateTime Nine = new(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc);

    // TC-I06: only active hosts are offered for check-in
    [Fact]
    public async Task Hosts_GetActive_ExcludesInactive()
    {
        using var db = new TestDbFactory();
        var repo = new HostRepository(db.Context);
        await repo.AddAsync(TestData.Host("Active Host"));
        await repo.AddAsync(TestData.Host("Former Host", active: false));

        var active = await new HostRepository(db.NewContext()).GetActiveAsync();

        Assert.Single(active);
        Assert.Equal("Active Host", active[0].FullName);
    }

    // TC-I07: open-visit lookup (basis of the no-double-check-in rule)
    [Fact]
    public async Task GetActiveVisitForVisitor_ReturnsOnlyOpenVisit()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor();
        var host = TestData.Host();
        await new VisitorRepository(db.Context).AddAsync(visitor);
        await new HostRepository(db.Context).AddAsync(host);
        var visits = new VisitRepository(db.Context);

        Assert.Null(await visits.GetActiveVisitForVisitorAsync(visitor.Id));

        await visits.AddAsync(TestData.Visit(visitor.Id, host.Id, Nine.AddDays(-1), Nine.AddDays(-1).AddHours(1)));
        Assert.Null(await visits.GetActiveVisitForVisitorAsync(visitor.Id)); // closed visit doesn't count

        await visits.AddAsync(TestData.Visit(visitor.Id, host.Id, Nine));
        var open = await new VisitRepository(db.NewContext()).GetActiveVisitForVisitorAsync(visitor.Id);

        Assert.NotNull(open);
        Assert.Equal(Nine, open.CheckInTime);
        Assert.NotNull(open.Host);    // navigation loaded for the UI
        Assert.NotNull(open.Visitor);
        Assert.True(await new HostRepository(db.Context).HasVisitsAsync(host.Id));
    }

    // TC-I08: times come back from SQLite marked as UTC
    [Fact]
    public async Task Visit_DateTimes_ReadBackAsUtc()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor();
        var host = TestData.Host();
        await new VisitorRepository(db.Context).AddAsync(visitor);
        await new HostRepository(db.Context).AddAsync(host);
        await new VisitRepository(db.Context).AddAsync(TestData.Visit(visitor.Id, host.Id, Nine, Nine.AddMinutes(30)));

        var loaded = (await new VisitRepository(db.NewContext()).GetHistoryAsync()).Single();

        Assert.Equal(DateTimeKind.Utc, loaded.CheckInTime.Kind);
        Assert.Equal(DateTimeKind.Utc, loaded.CheckOutTime!.Value.Kind);
    }

    // TC-I09: history filters by date range and host; count of check-ins in a window
    [Fact]
    public async Task History_FiltersByDateAndHost_AndCountsCheckIns()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor();
        var hostA = TestData.Host("A");
        var hostB = TestData.Host("B");
        await new VisitorRepository(db.Context).AddAsync(visitor);
        var hosts = new HostRepository(db.Context);
        await hosts.AddAsync(hostA);
        await hosts.AddAsync(hostB);
        var visits = new VisitRepository(db.Context);
        await visits.AddAsync(TestData.Visit(visitor.Id, hostA.Id, Nine.AddDays(-2), Nine.AddDays(-2).AddHours(1)));
        await visits.AddAsync(TestData.Visit(visitor.Id, hostB.Id, Nine.AddDays(-1), Nine.AddDays(-1).AddHours(1)));
        await visits.AddAsync(TestData.Visit(visitor.Id, hostA.Id, Nine));

        var today = Nine.Date;
        var repo = new VisitRepository(db.NewContext());

        Assert.Equal(3, (await repo.GetHistoryAsync()).Count);
        Assert.Single(await repo.GetHistoryAsync(fromUtc: today, toUtc: today.AddDays(1)));
        Assert.Equal(2, (await repo.GetHistoryAsync(hostId: hostA.Id)).Count);
        Assert.Equal(1, await repo.CountCheckInsAsync(today, today.AddDays(1)));
        Assert.Single(await repo.GetActiveVisitsAsync());
    }

    // TC-I10: recording a check-out on a loaded visit persists
    [Fact]
    public async Task Update_RecordsCheckOut()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor();
        var host = TestData.Host();
        await new VisitorRepository(db.Context).AddAsync(visitor);
        await new HostRepository(db.Context).AddAsync(host);
        var visit = TestData.Visit(visitor.Id, host.Id, Nine);
        await new VisitRepository(db.Context).AddAsync(visit);

        var ctx = db.NewContext();
        var repo = new VisitRepository(ctx);
        var loaded = await repo.GetByIdAsync(visit.Id);
        loaded!.CheckOutTime = Nine.AddHours(2);
        await repo.UpdateAsync(loaded);

        Assert.Empty(await new VisitRepository(db.NewContext()).GetActiveVisitsAsync());
    }
}
