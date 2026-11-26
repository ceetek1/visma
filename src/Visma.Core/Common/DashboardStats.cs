namespace Visma.Core.Common;

/// <summary>
/// Figures shown on the dashboard stat cards. Produced by IVisitService (Person 3).
/// </summary>
/// <param name="OnSiteNow">Visitors currently checked in.</param>
/// <param name="CheckInsToday">Check-ins since midnight today.</param>
/// <param name="CheckedOutToday">Of today's check-ins, how many have left.</param>
/// <param name="TotalVisitors">Registered visitors overall.</param>
public record DashboardStats(int OnSiteNow, int CheckInsToday, int CheckedOutToday, int TotalVisitors);
