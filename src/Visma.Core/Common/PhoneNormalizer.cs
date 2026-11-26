namespace Visma.Core.Common;

/// <summary>
/// Puts phone numbers into ONE canonical form before they are stored or searched, so
/// "0803 123 4567", "0803-123-4567" and "(0803) 1234567" are recognised as the same person.
/// Used by the repository lookup (Person 1) and must be used by VisitorService (Person 2)
/// before saving.
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>
    /// Keep digits only, plus a single leading '+'. Returns an empty string for null/blank input.
    /// </summary>
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var trimmed = phone.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        return trimmed.StartsWith('+') ? "+" + digits : digits;
    }
}
