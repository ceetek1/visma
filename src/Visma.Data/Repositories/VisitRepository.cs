using Microsoft.EntityFrameworkCore;
using Visma.Core.Entities;
using Visma.Core.Interfaces;

namespace Visma.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IVisitRepository"/>.</summary>
public class VisitRepository(VismaDbContext db) : IVisitRepository
{
    /// <summary>Visits with their Visitor and Host loaded (what every screen needs).</summary>
    private IQueryable<Visit> WithDetails() =>
        db.Visits.Include(v => v.Visitor).Include(v => v.Host);

    /// <inheritdoc />
    public Task<Visit?> GetByIdAsync(int id) =>
        WithDetails().FirstOrDefaultAsync(v => v.Id == id);

    /// <inheritdoc />
    public Task<Visit?> GetActiveVisitForVisitorAsync(int visitorId) =>
        WithDetails().FirstOrDefaultAsync(v => v.VisitorId == visitorId && v.CheckOutTime == null);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Visit>> GetActiveVisitsAsync() =>
        await WithDetails().AsNoTracking()
              .Where(v => v.CheckOutTime == null)
              .OrderByDescending(v => v.CheckInTime)
              .ToListAsync();

    /// <inheritdoc />
    public async Task<IReadOnlyList<Visit>> GetHistoryAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
                                                            int? hostId = null, int? visitorId = null)
    {
        var query = WithDetails().AsNoTracking();

        if (fromUtc is not null)   query = query.Where(v => v.CheckInTime >= fromUtc);
        if (toUtc is not null)     query = query.Where(v => v.CheckInTime < toUtc);
        if (hostId is not null)    query = query.Where(v => v.HostId == hostId);
        if (visitorId is not null) query = query.Where(v => v.VisitorId == visitorId);

        return await query.OrderByDescending(v => v.CheckInTime).ToListAsync();
    }

    /// <inheritdoc />
    public Task<int> CountCheckInsAsync(DateTime fromUtc, DateTime toUtc) =>
        db.Visits.CountAsync(v => v.CheckInTime >= fromUtc && v.CheckInTime < toUtc);

    /// <inheritdoc />
    public async Task AddAsync(Visit visit)
    {
        db.Visits.Add(visit);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Visit visit)
    {
        if (db.Entry(visit).State == EntityState.Detached) db.Visits.Update(visit);
        await db.SaveChangesAsync();
    }
}
