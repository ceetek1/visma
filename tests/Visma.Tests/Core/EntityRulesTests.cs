using System.ComponentModel.DataAnnotations;
using Visma.Core.Common;
using Visma.Core.Entities;
using Visma.Core.Enums;

namespace Visma.Tests.Core;

/// <summary>
/// Unit tests for entity behaviour and data-annotation validation (NFR: data validation).
/// Test plan IDs: TC-U03..TC-U08.
/// </summary>
[Trait("Level", "Unit")]
public class EntityRulesTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    // TC-U03: a visit with no check-out time is "on site"; with one it is checked out
    [Fact]
    public void Visit_Status_DerivedFromCheckOutTime()
    {
        var visit = new Visit { CheckInTime = new DateTime(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc) };
        Assert.Equal(VisitStatus.CheckedIn, visit.Status);

        visit.CheckOutTime = visit.CheckInTime.AddHours(1);
        Assert.Equal(VisitStatus.CheckedOut, visit.Status);
    }

    // TC-U04: duration uses "now" while open and the check-out time once closed
    [Fact]
    public void Visit_Duration_OpenAndClosed()
    {
        var checkIn = new DateTime(2026, 11, 26, 9, 0, 0, DateTimeKind.Utc);
        var visit = new Visit { CheckInTime = checkIn };

        Assert.Equal(TimeSpan.FromMinutes(30), visit.Duration(checkIn.AddMinutes(30)));

        visit.CheckOutTime = checkIn.AddMinutes(45);
        Assert.Equal(TimeSpan.FromMinutes(45), visit.Duration(checkIn.AddHours(5)));
    }

    // TC-U05: required visitor fields are enforced
    [Fact]
    public void Visitor_MissingNameAndPhone_IsInvalid()
    {
        var errors = Validate(new Visitor { FullName = "", Phone = "" });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Visitor.FullName)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Visitor.Phone)));
    }

    // TC-U06: badly formatted e-mail / phone are rejected
    [Fact]
    public void Visitor_BadEmailAndPhone_IsInvalid()
    {
        var errors = Validate(new Visitor { FullName = "Ada Okafor", Phone = "call me", Email = "not-an-email" });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Visitor.Email)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Visitor.Phone)));
    }

    // TC-U07: a correct visitor passes validation
    [Fact]
    public void Visitor_ValidData_HasNoErrors() =>
        Assert.Empty(Validate(new Visitor { FullName = "Ada Okafor", Phone = "0803 123 4567", Email = "ada@example.com" }));

    // TC-U08: OperationResult carries either a value or an error message
    [Fact]
    public void OperationResult_SuccessAndFailure()
    {
        var ok = OperationResult<int>.Success(42);
        var fail = OperationResult<int>.Failure("Visitor is already checked in.");

        Assert.True(ok.Succeeded);
        Assert.Equal(42, ok.Value);
        Assert.Null(ok.Error);
        Assert.False(fail.Succeeded);
        Assert.Equal("Visitor is already checked in.", fail.Error);
    }
}
