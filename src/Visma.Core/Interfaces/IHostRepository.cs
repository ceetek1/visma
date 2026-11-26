using Visma.Core.Entities;

namespace Visma.Core.Interfaces;

/// <summary>
/// Data-access contract for the <see cref="Host"/> directory. Implemented in Visma.Data
/// by <c>HostRepository</c>.
/// </summary>
public interface IHostRepository
{
    /// <summary>Get one host by id, or null if not found.</summary>
    Task<Host?> GetByIdAsync(int id);

    /// <summary>All hosts (active and inactive), ordered by department then name.</summary>
    Task<IReadOnlyList<Host>> GetAllAsync();

    /// <summary>Only active hosts, i.e. the ones a visitor can be checked in to.</summary>
    Task<IReadOnlyList<Host>> GetActiveAsync();

    /// <summary>True if the host has at least one visit on record (so must not be hard-deleted).</summary>
    Task<bool> HasVisitsAsync(int hostId);

    /// <summary>Insert a new host and save. The generated Id is set on the entity.</summary>
    Task AddAsync(Host host);

    /// <summary>Save changes to an existing host (including activate/deactivate).</summary>
    Task UpdateAsync(Host host);
}
