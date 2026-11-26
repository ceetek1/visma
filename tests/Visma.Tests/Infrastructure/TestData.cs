using Visma.Core.Entities;
using Visma.Core.Enums;

namespace Visma.Tests.Infrastructure;

/// <summary>Small builders so tests only spell out the fields they care about.</summary>
public static class TestData
{
    public static Visitor Visitor(string name = "Test Visitor", string phone = "08000000001", string? email = null) =>
        new() { FullName = name, Phone = phone, Email = email, CreatedAt = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) };

    public static Host Host(string name = "Test Host", string dept = "Admin", bool active = true) =>
        new() { FullName = name, Department = dept, IsActive = active };

    public static Visit Visit(int visitorId, int hostId, DateTime checkIn, DateTime? checkOut = null,
                              VisitPurpose purpose = VisitPurpose.Meeting) =>
        new() { VisitorId = visitorId, HostId = hostId, CheckInTime = checkIn, CheckOutTime = checkOut, Purpose = purpose };
}
