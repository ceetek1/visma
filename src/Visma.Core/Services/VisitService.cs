using Visma.Core.Common;
using Visma.Core.Entities;
using Visma.Core.Enums;
using Visma.Core.Interfaces;

namespace Visma.Core.Services;

/// <summary>
/// Implements visit check-in, check-out, and visit-log business operations.
/// </summary>
public class VisitService(
    IVisitRepository visits,
    IVisitorRepository visitors,
    IHostRepository hosts,
    TimeProvider clock) : IVisitService
{
    /// <inheritdoc />
    public async Task<OperationResult<Visit>> CheckInAsync(
        int visitorId,
        int hostId,
        VisitPurpose purpose,
        string? notes)
    {
        var visitor = await visitors.GetByIdAsync(visitorId);
        if (visitor is null)
        {
            return OperationResult<Visit>.Failure("Visitor not found.");
        }

        var host = await hosts.GetByIdAsync(hostId);
        if (host is null)
        {
            return OperationResult<Visit>.Failure("Host not found.");
        }

        if (!host.IsActive)
        {
            return OperationResult<Visit>.Failure(
                $"{host.FullName} is no longer receiving visitors. Choose another host.");
        }

        if (!Enum.IsDefined(typeof(VisitPurpose), purpose))
        {
            return OperationResult<Visit>.Failure("Choose a purpose for the visit.");
        }

        var activeVisit = await visits.GetActiveVisitForVisitorAsync(visitorId);
        if (activeVisit is not null)
        {
            return OperationResult<Visit>.Failure(
                $"{visitor.FullName} is already checked in. Check them out before checking in again.");
        }

        var trimmedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        var visit = new Visit
        {
            VisitorId = visitor.Id,
            HostId = host.Id,
            Purpose = purpose,
            Notes = trimmedNotes,
            CheckInTime = clock.GetUtcNow().UtcDateTime,
            Visitor = visitor,
            Host = host
        };

        await visits.AddAsync(visit);
        return OperationResult<Visit>.Success(visit);
    }

    /// <inheritdoc />
    public async Task<OperationResult<Visit>> CheckOutAsync(int visitId)
    {
        var visit = await visits.GetByIdAsync(visitId);
        if (visit is null)
        {
            return OperationResult<Visit>.Failure("Visit not found.");
        }

        if (visit.CheckOutTime is not null)
        {
            return OperationResult<Visit>.Failure("This visit was already checked out.");
        }

        visit.CheckOutTime = clock.GetUtcNow().UtcDateTime;
        await visits.UpdateAsync(visit);
        return OperationResult<Visit>.Success(visit);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Visit>> GetActiveVisitsAsync()
    {
        return visits.GetActiveVisitsAsync();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Visit>> GetHistoryAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        int? hostId,
        int? visitorId)
    {
        return visits.GetHistoryAsync(fromUtc, toUtc, hostId, visitorId);
    }

    /// <inheritdoc />
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var localNow = clock.GetLocalNow();
        var startLocal = new DateTimeOffset(localNow.Date, localNow.Offset);
        var todayStartUtc = startLocal.UtcDateTime;
        var todayEndUtc = startLocal.AddDays(1).UtcDateTime;

        var activeVisits = await visits.GetActiveVisitsAsync();
        var checkInsToday = await visits.CountCheckInsAsync(todayStartUtc, todayEndUtc);
        var todayVisits = await visits.GetHistoryAsync(todayStartUtc, todayEndUtc);
        var checkedOutToday = todayVisits.Count(visit => visit.CheckOutTime is not null);
        var totalVisitors = (await visitors.SearchAsync(null)).Count;

        return new DashboardStats(activeVisits.Count, checkInsToday, checkedOutToday, totalVisitors);
    }
}
