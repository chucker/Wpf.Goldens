using Microsoft.Test.VisualVerification;

using Xunit;

namespace Chucker.Lib.WpfGoldens;

/// <summary>
/// Helper extension methods to use <see cref="WpfGolden"/> from Xunit.
/// </summary>
public static class WpfRenderedGoldenXunitExtensions
{
    public static void AssertIsWithinTolerance(this WpfRenderedGolden renderedGolden, byte tolerance)
    {
        var actual = renderedGolden.VerifyUniformColorDifference(tolerance);
        Assert.Equal(VerificationResult.Pass, actual);
    }
}
