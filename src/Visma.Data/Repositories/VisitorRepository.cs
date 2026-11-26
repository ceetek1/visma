using Microsoft.EntityFrameworkCore;
using Visma.Core.Common;
using Visma.Core.Entities;
using Visma.Core.Interfaces;

namespace Visma.Data.Repositories;

/// <summary>EF Core implementation of <see cref="IVisitorRepository"/>.</summary>
public class VisitorRepository(VismaDbContext db) : IVisitorRepository
{
    /// <inheritdoc />
    public Task<Visitor?> GetByIdAsync(int id) =>
        db.Visitors.FirstOrDefaultAsync(v => v.Id == id);

    /// <inheritdoc />
    public Task<Visitor?> GetWithVisitsAsync(int id) =>
        db.Visitors
          .Include(v => v.Visits.OrderByDescending(x => x.CheckInTime))
              .ThenInclude(x => x.Host)
          .FirstOrDefaultAsync(v => v.Id == id);

    /// <inheritdoc />
    public async Task<Visitor?> FindByPhoneOrEmailAsync(string phoneOrEmail)
    {
        if (string.IsNullOrWhiteSpace(phoneOrEmail)) return null;

        var term = phoneOrEmail.Trim();
        var phone = PhoneNormalizer.Normalize(term);
        var email = term.ToLowerInvariant();

        return await db.Visitors.FirstOrDefaultAsync(v =>
            (phone != "" && v.Phone == phone) ||
            (v.Email != null && v.Email.ToLower() == email));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Visitor>> SearchAsync(string? term)
    {
        var query = db.Visitors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            // SQLite LIKE is case-insensitive for English letters.
            var pattern = $"%{term.Trim()}%";
            query = query.Where(v =>
                EF.Functions.Like(v.FullName, pattern) ||
                EF.Functions.Like(v.Phone, pattern) ||
                (v.Email != null && EF.Functions.Like(v.Email, pattern)) ||
                (v.Company != null && EF.Functions.Like(v.Company, pattern)));
        }

        return await query.OrderBy(v => v.FullName).ToListAsync();
    }

    /// <inheritdoc />
    public Task<bool> PhoneExistsAsync(string phone, int? excludeVisitorId = null)
    {
        var normalized = PhoneNormalizer.Normalize(phone);
        return db.Visitors.AnyAsync(v => v.Phone == normalized &&
                                         (excludeVisitorId == null || v.Id != excludeVisitorId));
    }

    /// <inheritdoc />
    public async Task AddAsync(Visitor visitor)
    {
        db.Visitors.Add(visitor);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Visitor visitor)
    {
        if (db.Entry(visitor).State == EntityState.Detached) db.Visitors.Update(visitor);
        await db.SaveChangesAsync();
    }
}
