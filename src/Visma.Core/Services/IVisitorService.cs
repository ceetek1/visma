using Visma.Core.Common;
using Visma.Core.Entities;

namespace Visma.Core.Services;

/// <summary>
/// BUSINESS LOGIC for visitor registration.  >>> OWNER: PERSON 2 (Increment 1) <<<
/// Person 1 defined this contract; Person 2 implements it as
/// <c>Visma.Core/Services/VisitorService.cs</c> using <see cref="Interfaces.IVisitorRepository"/>
/// and registers it in <c>Visma.Web/Program.cs</c>.
/// </summary>
public interface IVisitorService
{
    /// <summary>
    /// Register a NEW visitor. Must trim/normalise input, set <c>CreatedAt</c> from TimeProvider,
    /// and FAIL (not throw) if the phone number is already registered:
    /// the UI should then offer the existing visitor instead (returning-visitor flow).
    /// </summary>
    Task<OperationResult<Visitor>> RegisterAsync(Visitor visitor);

    /// <summary>Update an existing visitor's details. Fails if not found or phone clashes with another visitor.</summary>
    Task<OperationResult<Visitor>> UpdateAsync(Visitor visitor);

    /// <summary>Returning-visitor lookup by exact phone or e-mail. Null if not registered.</summary>
    Task<Visitor?> FindReturningVisitorAsync(string phoneOrEmail);

    /// <summary>Search visitors by partial name / phone / e-mail / company (null = all).</summary>
    Task<IReadOnlyList<Visitor>> SearchAsync(string? term);

    /// <summary>Visitor profile with visit history, or null.</summary>
    Task<Visitor?> GetDetailsAsync(int id);
}
