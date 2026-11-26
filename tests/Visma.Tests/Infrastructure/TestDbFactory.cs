using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Visma.Data;

namespace Visma.Tests.Infrastructure;

/// <summary>
/// Gives each test its own throw-away SQLite database that lives only in memory.
///
/// Why real SQLite and not EF's "InMemory" provider? InMemory ignores unique indexes and
/// foreign keys, so a "duplicate phone" test would wrongly pass. SQLite in memory enforces
/// the same constraints as the real visma.db, and is still fast.
///
/// Usage (dispose at the end so the database is destroyed):
/// <code>
/// using var db = new TestDbFactory();
/// var repo = new VisitorRepository(db.Context);
/// </code>
/// Use <see cref="NewContext"/> to simulate a SECOND web request reading the same database.
/// </summary>
public sealed class TestDbFactory : IDisposable
{
    // An in-memory SQLite database exists only while its connection is open,
    // so we keep one connection open for the life of the test.
    private readonly SqliteConnection _connection;

    /// <summary>The main context for arranging and acting.</summary>
    public VismaDbContext Context { get; }

    public TestDbFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Context = NewContext();
        Context.Database.Migrate(); // runs the real migrations, so they are tested too
    }

    /// <summary>A fresh context on the same database (no cached/tracked entities).</summary>
    public VismaDbContext NewContext() =>
        new(new DbContextOptionsBuilder<VismaDbContext>().UseSqlite(_connection).Options);

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
