using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Visma.Core.Entities;

namespace Visma.Data;

/// <summary>
/// EF Core database session for VISMA. Maps the domain entities to SQLite tables
/// (Visitors, Hosts, Visits) and declares keys, indexes and relationships.
/// </summary>
public class VismaDbContext(DbContextOptions<VismaDbContext> options) : DbContext(options)
{
    /// <summary>Registered visitors.</summary>
    public DbSet<Visitor> Visitors => Set<Visitor>();

    /// <summary>Staff directory (people who can be visited).</summary>
    public DbSet<Host> Hosts => Set<Host>();

    /// <summary>Check-in / check-out log.</summary>
    public DbSet<Visit> Visits => Set<Visit>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Visitor ----------
        modelBuilder.Entity<Visitor>(e =>
        {
            e.Property(v => v.FullName).IsRequired().HasMaxLength(100);
            e.Property(v => v.Phone).IsRequired().HasMaxLength(20);
            e.Property(v => v.Email).HasMaxLength(100);
            e.Property(v => v.Company).HasMaxLength(100);

            // Phone identifies a returning visitor, so the DATABASE guarantees uniqueness
            // (a second line of defence behind the service-layer check).
            e.HasIndex(v => v.Phone).IsUnique();
            e.HasIndex(v => v.Email);
        });

        // ---------- Host ----------
        modelBuilder.Entity<Host>(e =>
        {
            e.Property(h => h.FullName).IsRequired().HasMaxLength(100);
            e.Property(h => h.Department).IsRequired().HasMaxLength(100);
            e.Property(h => h.Email).HasMaxLength(100);
            e.Property(h => h.Phone).HasMaxLength(20);
            // NOTE: no database default for IsActive. With a bool, EF cannot tell "false"
            // from "not set", so a DB default of true would silently re-activate hosts.
            // The C# initialiser (= true) is the default instead.
        });

        // ---------- Visit ----------
        modelBuilder.Entity<Visit>(e =>
        {
            // Store the enum as readable text ("Meeting") instead of a number.
            e.Property(v => v.Purpose).HasConversion<string>().HasMaxLength(30);
            e.Property(v => v.Notes).HasMaxLength(250);

            // Status is calculated from CheckOutTime, so there is no column for it.
            e.Ignore(v => v.Status);

            // Restrict = you cannot delete a Visitor/Host that still has visits.
            // This protects the audit trail.
            e.HasOne(v => v.Visitor).WithMany(p => p.Visits)
             .HasForeignKey(v => v.VisitorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Host).WithMany(h => h.Visits)
             .HasForeignKey(v => v.HostId).OnDelete(DeleteBehavior.Restrict);

            // Speeds up "does this visitor have an open visit?" (the double check-in rule)
            // and the dashboard's "who is on-site" query.
            e.HasIndex(v => new { v.VisitorId, v.CheckOutTime });
            e.HasIndex(v => v.CheckInTime);
        });

        // SQLite has no native date type and drops DateTimeKind on read. Mark every
        // DateTime coming back from the database as UTC so time maths stays correct.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            toDb => toDb,
            fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
        {
            if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                property.SetValueConverter(utcConverter);
        }
    }
}
