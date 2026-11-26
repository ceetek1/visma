using Microsoft.EntityFrameworkCore;
using Visma.Core.Entities;
using Visma.Core.Enums;

namespace Visma.Data.Seed;

/// <summary>
/// Fills an EMPTY database with demo data so the app is usable straight after cloning:
/// a host directory, a few registered visitors and some visits (one still on-site).
/// Idempotent: each table is only seeded when it is empty, so running twice is safe.
/// </summary>
public static class DbSeeder
{
    /// <summary>Seed hosts, visitors and visits if their tables are empty.</summary>
    /// <param name="db">Database context.</param>
    /// <param name="clock">Time source (lets tests control "now").</param>
    public static async Task SeedAsync(VismaDbContext db, TimeProvider clock)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        if (!await db.Hosts.AnyAsync())
        {
            db.Hosts.AddRange(
                new Host { FullName = "Mrs. Ngozi Adeyemi",  Department = "Administration",  Email = "n.adeyemi@visma.local",  Phone = "101" },
                new Host { FullName = "Mr. Tunde Bakare",    Department = "Admissions",      Email = "t.bakare@visma.local",   Phone = "102" },
                new Host { FullName = "Dr. Chinedu Eze",     Department = "ICT",             Email = "c.eze@visma.local",      Phone = "103" },
                new Host { FullName = "Ms. Amina Bello",     Department = "Human Resources", Email = "a.bello@visma.local",    Phone = "104" },
                new Host { FullName = "Mr. Femi Okonkwo",    Department = "Bursary",         Email = "f.okonkwo@visma.local",  Phone = "105" },
                new Host { FullName = "Prof. Halima Yusuf",  Department = "Library",         Email = "h.yusuf@visma.local",    Phone = "106" },
                // Inactive host: shows that deactivated staff disappear from check-in.
                new Host { FullName = "Mr. Segun Afolabi",   Department = "Security",        Email = "s.afolabi@visma.local",  Phone = "107", IsActive = false });
            await db.SaveChangesAsync();
        }

        if (!await db.Visitors.AnyAsync())
        {
            db.Visitors.AddRange(
                new Visitor { FullName = "Ada Okafor",     Phone = "08031234567", Email = "ada.okafor@example.com", Company = "Zenith Logistics", CreatedAt = now.AddDays(-10) },
                new Visitor { FullName = "Ibrahim Musa",   Phone = "08129876543", Email = "ibrahim.musa@example.com",                              CreatedAt = now.AddDays(-3) },
                new Visitor { FullName = "Grace Olaniyan", Phone = "07065554433",                                   Company = "TechHub Lagos",    CreatedAt = now.AddDays(-1) });
            await db.SaveChangesAsync();
        }

        if (!await db.Visits.AnyAsync())
        {
            var ada     = await db.Visitors.FirstAsync(v => v.Phone == "08031234567");
            var ibrahim = await db.Visitors.FirstAsync(v => v.Phone == "08129876543");
            var grace   = await db.Visitors.FirstAsync(v => v.Phone == "07065554433");
            var ict     = await db.Hosts.FirstAsync(h => h.Department == "ICT");
            var admit   = await db.Hosts.FirstAsync(h => h.Department == "Admissions");
            var hr      = await db.Hosts.FirstAsync(h => h.Department == "Human Resources");

            db.Visits.AddRange(
                // Completed visits (history)
                new Visit { VisitorId = ada.Id,     HostId = admit.Id, Purpose = VisitPurpose.Official,  Notes = "Document submission",
                            CheckInTime = now.AddDays(-10).AddHours(-2), CheckOutTime = now.AddDays(-10).AddHours(-1) },
                new Visit { VisitorId = ibrahim.Id, HostId = hr.Id,    Purpose = VisitPurpose.Interview, Notes = "Graduate trainee interview",
                            CheckInTime = now.AddDays(-3).AddHours(-3),  CheckOutTime = now.AddDays(-3).AddHours(-2) },
                // Still on-site (no check-out yet)
                new Visit { VisitorId = grace.Id,   HostId = ict.Id,   Purpose = VisitPurpose.Meeting,   Notes = "Network upgrade proposal",
                            CheckInTime = now.AddMinutes(-40) });
            await db.SaveChangesAsync();
        }
    }
}
