using Visma.Core.Entities;

namespace Visma.Core.Interfaces;

/// <summary>
/// Data-access contract for <see cref="Visit"/>s (the check-in / check-out log).
/// Implemented in Visma.Data by <c>VisitRepository</c>. All read methods include the
/// related Visitor and Host so views can show names without extra queries.
/// </summary>
public interface IVisitRepository
{
    /// <summary>Get one visit (with Visitor and Host), or null.</summary>
    Task<Visit?> GetByIdAsync(int id);

    /// <summary>
    /// The visitor's currently OPEN visit (no check-out yet), or null if they are not on-site.
    /// Used to enforce the "no double check-in" rule.
    /// </summary>
    Task<Visit?> GetActiveVisitForVisitorAsync(int visitorId);

    /// <summary>Everyone currently on-site, most recent check-in first.</summary>
    Task<IReadOnlyList<Visit>> GetActiveVisitsAsync();

    /// <summary>
    /// Visit history filtered by optional check-in date range (UTC, inclusive from / exclusive to),
    /// host and visitor. Most recent first.
    /// </summary>
    Task<IReadOnlyList<Visit>> GetHistoryAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
                                               int? hostId = null, int? visitorId = null);

    /// <summary>Number of visits whose check-in falls in [fromUtc, toUtc).</summary>
    Task<int> CountCheckInsAsync(DateTime fromUtc, DateTime toUtc);

    /// <summary>Insert a new visit (check-in) and save.</summary>
    Task AddAsync(Visit visit);

    /// <summary>Save changes to an existing visit (e.g. recording check-out).</summary>
    Task UpdateAsync(Visit visit);
}
