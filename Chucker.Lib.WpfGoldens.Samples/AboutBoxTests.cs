using Xunit;

namespace Chucker.Lib.WpfGoldens.Samples;

public class AboutBoxTests
{
    [WpfFact]
    public void Test()
    {
        var window = new AboutBox();

        window.Show();

        WpfGolden.FromCurrentTest()
            .Build()
            .Render(window)
            .AssertIsWithinTolerance(0);
    }
}
