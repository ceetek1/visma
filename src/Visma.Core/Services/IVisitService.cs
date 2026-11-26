using Visma.Core.Common;
using Visma.Core.Entities;
using Visma.Core.Enums;

namespace Visma.Core.Services;

/// <summary>
/// BUSINESS LOGIC for check-in / check-out tracking.  >>> OWNER: PERSON 3 (Increment 2) <<<
/// Person 1 defined this contract; Person 3 implements it as
/// <c>Visma.Core/Services/VisitService.cs</c> using <see cref="Interfaces.IVisitRepository"/>,
/// <see cref="Interfaces.IVisitorRepository"/>, <see cref="Interfaces.IHostRepository"/> and
/// <see cref="TimeProvider"/> (never DateTime.Now, so tests can control the clock).
/// </summary>
public interface IVisitService
{
    /// <summary>
    /// Check a visitor in. Must FAIL (OperationResult.Failure) when:
    ///  • the visitor does not exist;
    ///  • the host does not exist or is inactive;
    ///  • the visitor already has an open visit (NO DOUBLE CHECK-IN).
    /// On success, CheckInTime = TimeProvider UTC now.
    /// </summary>
    Task<OperationResult<Visit>> CheckInAsync(int visitorId, int hostId, VisitPurpose purpose, string? notes);

    /// <summary>
    /// Check a visit out (CheckOutTime = now). Must FAIL if the visit does not exist or is
    /// already checked out.
    /// </summary>
    Task<OperationResult<Visit>> CheckOutAsync(int visitId);

    /// <summary>Everyone currently on-site.</summary>
    Task<IReadOnlyList<Visit>> GetActiveVisitsAsync();

    /// <summary>Filtered visit history (see <see cref="Interfaces.IVisitRepository.GetHistoryAsync"/>).</summary>
    Task<IReadOnlyList<Visit>> GetHistoryAsync(DateTime? fromUtc, DateTime? toUtc, int? hostId, int? visitorId);

    /// <summary>Numbers for the dashboard stat cards.</summary>
    Task<DashboardStats> GetDashboardStatsAsync();
}
