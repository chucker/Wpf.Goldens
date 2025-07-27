using Microsoft.Test.VisualVerification;

using System.Drawing;

using Xunit;

namespace Chucker.Lib.WpfGoldens.Samples;

public class AboutBoxTests
{
    [WpfFact]
    public void TestDoesNotPassWithUniformColorDifference()
    {
        var window = new AboutBox();

        window.Show();

        var renderedGolden = WpfGolden.FromCurrentTest()
            .Build()
            .Render(window);

        Assert.NotEqual(VerificationResult.Pass, renderedGolden.VerifyUniformColorDifference(0));
    }

    [WpfFact]
    public void TestPassesWithToleranceMap()
    {
        var window = new AboutBox();

        window.Show();

        var toleranceMap = (Bitmap)Image.FromFile("Resources/AboutBoxTests_ToleranceMap.png");

        WpfGolden.FromCurrentTest()
            .Build()
            .Render(window)
            .AssertIsWithinToleranceMap(toleranceMap);
    }
}
