namespace Visma.Core.Enums;

/// <summary>Lifecycle state of a <see cref="Entities.Visit"/>.</summary>
public enum VisitStatus
{
    /// <summary>Visitor is currently on the premises (no check-out time yet).</summary>
    CheckedIn,

    /// <summary>Visitor has left (check-out time recorded).</summary>
    CheckedOut
}
