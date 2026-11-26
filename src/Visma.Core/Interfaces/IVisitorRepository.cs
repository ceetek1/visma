using Visma.Core.Entities;

namespace Visma.Core.Interfaces;

/// <summary>
/// Data-access contract for <see cref="Visitor"/>s. Implemented in Visma.Data by
/// <c>VisitorRepository</c>. Repositories only store and fetch: business rules
/// (validation, duplicates) belong in the service layer.
/// </summary>
public interface IVisitorRepository
{
    /// <summary>Get one visitor by id, or null if not found.</summary>
    Task<Visitor?> GetByIdAsync(int id);

    /// <summary>Get one visitor with their visit history (visits include the host).</summary>
    Task<Visitor?> GetWithVisitsAsync(int id);

    /// <summary>
    /// Returning-visitor lookup: exact match on phone OR e-mail (case-insensitive e-mail).
    /// Returns null if nobody matches.
    /// </summary>
    Task<Visitor?> FindByPhoneOrEmailAsync(string phoneOrEmail);

    /// <summary>
    /// Search by partial name, phone, e-mail or company. Empty/null term returns everyone.
    /// Results are ordered by name.
    /// </summary>
    Task<IReadOnlyList<Visitor>> SearchAsync(string? term);

    /// <summary>True if another visitor already uses this phone number.</summary>
    /// <param name="phone">Phone to check.</param>
    /// <param name="excludeVisitorId">Ignore this visitor (use when editing).</param>
    Task<bool> PhoneExistsAsync(string phone, int? excludeVisitorId = null);

    /// <summary>Insert a new visitor and save. The generated Id is set on the entity.</summary>
    Task AddAsync(Visitor visitor);

    /// <summary>Save changes to an existing visitor.</summary>
    Task UpdateAsync(Visitor visitor);
}
