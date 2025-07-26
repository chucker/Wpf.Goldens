using Xunit;

namespace Chucker.Lib.WpfGoldens.Tests;

public class WpfGoldenTests
{
    [Fact]
    public void GoldensDirMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\Goldens", golden.GoldensDir.FullName);
    }

    [Fact]
    public void TestOutputDirMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\TestOutput", golden.TestOutputDir.FullName);
    }

    [Fact]
    public void FilenameBaseMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        var filenameBase = "WpfGoldenTests_FilenameBaseMatches";

        Assert.EndsWith(filenameBase, golden.FilenameBase);
    }
}
