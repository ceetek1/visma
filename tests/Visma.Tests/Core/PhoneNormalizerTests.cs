using Visma.Core.Common;

namespace Visma.Tests.Core;

/// <summary>Unit tests for <see cref="PhoneNormalizer"/>. Test plan IDs: TC-U01..TC-U02.</summary>
[Trait("Level", "Unit")]
public class PhoneNormalizerTests
{
    // TC-U01: different ways of writing the same number normalise to one form
    [Theory]
    [InlineData("0803 123 4567", "08031234567")]
    [InlineData("0803-123-4567", "08031234567")]
    [InlineData("(0803) 1234567", "08031234567")]
    [InlineData(" +234 803 123 4567 ", "+2348031234567")]
    public void Normalize_StripsFormatting_KeepsLeadingPlus(string input, string expected) =>
        Assert.Equal(expected, PhoneNormalizer.Normalize(input));

    // TC-U02: blank input is safe
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_BlankInput_ReturnsEmpty(string? input) =>
        Assert.Equal(string.Empty, PhoneNormalizer.Normalize(input));
}
