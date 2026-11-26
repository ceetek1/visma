namespace Visma.Core.Common;

/// <summary>
/// Outcome of a business operation that can fail for an EXPECTED reason
/// (e.g. "visitor is already checked in"). Services return this instead of throwing,
/// so controllers can show a friendly message. Exceptions remain for truly
/// unexpected failures (database down, bugs).
/// </summary>
/// <example>
/// <code>
/// var result = await _visitService.CheckInAsync(visitorId, hostId, purpose, notes);
/// if (!result.Succeeded) { ModelState.AddModelError("", result.Error!); return View(model); }
/// TempData["Success"] = $"Checked in visit #{result.Value!.Id}";
/// </code>
/// </example>
public class OperationResult<T>
{
    /// <summary>True if the operation did what was asked.</summary>
    public bool Succeeded { get; }

    /// <summary>The produced value when <see cref="Succeeded"/> is true.</summary>
    public T? Value { get; }

    /// <summary>Human-readable reason when <see cref="Succeeded"/> is false.</summary>
    public string? Error { get; }

    private OperationResult(bool succeeded, T? value, string? error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    /// <summary>Create a successful result carrying <paramref name="value"/>.</summary>
    public static OperationResult<T> Success(T value) => new(true, value, null);

    /// <summary>Create a failed result with a user-facing <paramref name="error"/> message.</summary>
    public static OperationResult<T> Failure(string error) => new(false, default, error);
}
