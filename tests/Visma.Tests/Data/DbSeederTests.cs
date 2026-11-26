using Microsoft.EntityFrameworkCore;
using Visma.Data.Seed;
using Visma.Tests.Infrastructure;

namespace Visma.Tests.Data;

/// <summary>Integration tests for demo-data seeding. TC-I11..TC-I12.</summary>
[Trait("Level", "Integration")]
public class DbSeederTests
{
    // TC-I11: an empty database gets hosts, visitors and visits; running again adds nothing
    [Fact]
    public async Task Seed_IsIdempotent()
    {
        using var db = new TestDbFactory();
        var clock = new FakeClock();

        await DbSeeder.SeedAsync(db.Context, clock);
        await DbSeeder.SeedAsync(db.NewContext(), clock);

        var ctx = db.NewContext();
        Assert.Equal(7, await ctx.Hosts.CountAsync());
        Assert.Equal(3, await ctx.Visitors.CountAsync());
        Assert.Equal(3, await ctx.Visits.CountAsync());
        Assert.Equal(1, await ctx.Visits.CountAsync(v => v.CheckOutTime == null));
    }

    // TC-I12: regression: an inactive host must STAY inactive after saving
    // (a database default of IsActive = true once silently re-activated it).
    [Fact]
    public async Task Seed_InactiveHost_StaysInactive()
    {
        using var db = new TestDbFactory();
        await DbSeeder.SeedAsync(db.Context, new FakeClock());

        var inactive = await db.NewContext().Hosts.SingleAsync(h => h.Department == "Security");

        Assert.False(inactive.IsActive);
    }
}
