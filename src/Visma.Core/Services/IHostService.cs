using Visma.Core.Common;
using Visma.Core.Entities;

namespace Visma.Core.Services;

/// <summary>
/// BUSINESS LOGIC for the host (staff) directory.  >>> OWNER: PERSON 2 (Increment 1) <<<
/// Person 1 defined this contract; Person 2 implements it as
/// <c>Visma.Core/Services/HostService.cs</c> using <see cref="Interfaces.IHostRepository"/>.
/// </summary>
public interface IHostService
{
    /// <summary>All hosts (for the management page).</summary>
    Task<IReadOnlyList<Host>> GetAllAsync();

    /// <summary>Active hosts only (for the check-in dropdown).</summary>
    Task<IReadOnlyList<Host>> GetActiveAsync();

    /// <summary>One host or null.</summary>
    Task<Host?> GetByIdAsync(int id);

    /// <summary>Add a host to the directory (new hosts start active).</summary>
    Task<OperationResult<Host>> CreateAsync(Host host);

    /// <summary>Edit a host's details. Fails if not found.</summary>
    Task<OperationResult<Host>> UpdateAsync(Host host);

    /// <summary>
    /// Mark a host inactive so they no longer appear in check-in. Never hard-delete:
    /// past visits must keep pointing at the host.
    /// </summary>
    Task<OperationResult<Host>> SetActiveAsync(int hostId, bool isActive);
}
