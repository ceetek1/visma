using Microsoft.EntityFrameworkCore;
using Visma.Core.Entities;
using Visma.Core.Interfaces;

namespace Visma.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IHostRepository"/>.</summary>
public class HostRepository(VismaDbContext db) : IHostRepository
{
    /// <inheritdoc />
    public Task<Host?> GetByIdAsync(int id) =>
        db.Hosts.FirstOrDefaultAsync(h => h.Id == id);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Host>> GetAllAsync() =>
        await db.Hosts.AsNoTracking()
                .OrderBy(h => h.Department).ThenBy(h => h.FullName)
                .ToListAsync();

    /// <inheritdoc />
    public async Task<IReadOnlyList<Host>> GetActiveAsync() =>
        await db.Hosts.AsNoTracking()
                .Where(h => h.IsActive)
                .OrderBy(h => h.Department).ThenBy(h => h.FullName)
                .ToListAsync();

    /// <inheritdoc />
    public Task<bool> HasVisitsAsync(int hostId) =>
        db.Visits.AnyAsync(v => v.HostId == hostId);

    /// <inheritdoc />
    public async Task AddAsync(Host host)
    {
        db.Hosts.Add(host);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Host host)
    {
        if (db.Entry(host).State == EntityState.Detached) db.Hosts.Update(host);
        await db.SaveChangesAsync();
    }
}
