using Microsoft.EntityFrameworkCore;
using Visma.Data.Repositories;
using Visma.Tests.Infrastructure;

namespace Visma.Tests.Data;

/// <summary>Integration tests: VisitorRepository against a real (in-memory) SQLite database. TC-I01..TC-I05.</summary>
[Trait("Level", "Integration")]
public class VisitorRepositoryTests
{
    // TC-I01: added visitor gets an Id and can be read back in a new request
    [Fact]
    public async Task Add_ThenGetById_RoundTrips()
    {
        using var db = new TestDbFactory();
        var visitor = TestData.Visitor("Ada Okafor", "08031234567");

        await new VisitorRepository(db.Context).AddAsync(visitor);
        var loaded = await new VisitorRepository(db.NewContext()).GetByIdAsync(visitor.Id);

        Assert.True(visitor.Id > 0);
        Assert.Equal("Ada Okafor", loaded?.FullName);
    }

    // TC-I02: the DATABASE rejects a duplicate phone even if the service check is bypassed
    [Fact]
    public async Task Add_DuplicatePhone_IsRejectedByDatabase()
    {
        using var db = new TestDbFactory();
        var repo = new VisitorRepository(db.Context);
        await repo.AddAsync(TestData.Visitor("First", "08031234567"));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            new VisitorRepository(db.NewContext()).AddAsync(TestData.Visitor("Second", "08031234567")));
    }

    // TC-I03: returning-visitor lookup works with formatted phone or any-case e-mail
    [Theory]
    [InlineData("0803 123 4567")]
    [InlineData("ADA.Okafor@Example.com")]
    public async Task FindByPhoneOrEmail_MatchesFormattedPhoneAndEmailCase(string lookup)
    {
        using var db = new TestDbFactory();
        await new VisitorRepository(db.Context).AddAsync(TestData.Visitor("Ada Okafor", "08031234567", "ada.okafor@example.com"));

        var found = await new VisitorRepository(db.NewContext()).FindByPhoneOrEmailAsync(lookup);

        Assert.Equal("Ada Okafor", found?.FullName);
    }

    // TC-I04: unknown lookup returns null (so the UI offers registration)
    [Fact]
    public async Task FindByPhoneOrEmail_Unknown_ReturnsNull()
    {
        using var db = new TestDbFactory();
        Assert.Null(await new VisitorRepository(db.Context).FindByPhoneOrEmailAsync("09099999999"));
    }

    // TC-I05: search is partial and case-insensitive; PhoneExists ignores the visitor being edited
    [Fact]
    public async Task Search_PartialName_And_PhoneExists_ExcludesSelf()
    {
        using var db = new TestDbFactory();
        var repo = new VisitorRepository(db.Context);
        var ada = TestData.Visitor("Ada Okafor", "08031234567");
        await repo.AddAsync(ada);
        await repo.AddAsync(TestData.Visitor("Ibrahim Musa", "08129876543"));

        var results = await repo.SearchAsync("okaf");

        Assert.Single(results);
        Assert.True(await repo.PhoneExistsAsync("0803-123-4567"));
        Assert.False(await repo.PhoneExistsAsync("08031234567", excludeVisitorId: ada.Id));
    }
}
