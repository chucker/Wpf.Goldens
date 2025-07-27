using Xunit;

namespace Chucker.Lib.WpfGoldens.Tests;

public class WpfGoldenTests
{
    [Fact]
    public void GoldensDirMatches_Fact()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\Goldens\WpfGoldenTests", golden.GoldensDir.FullName);
    }

    [Theory]
    [InlineData("hello", 123)]
    public void GoldensDirMatches_Theory(string s, int i)
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\Goldens\WpfGoldenTests", golden.GoldensDir.FullName);
    }

    [Fact]
    public void TestOutputDirMatches_Fact()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\TestOutput\WpfGoldenTests", golden.TestOutputDir.FullName);
    }

    [Theory]
    [InlineData("hello", 123)]
    public void TestOutputDirMatches_Theory(string s, int i)
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\TestOutput\WpfGoldenTests", golden.TestOutputDir.FullName);
    }

    [Fact]
    public void FilenameBaseMatches_Fact()
    {
        var golden = WpfGolden.FromCurrentTest();

        const string filenameBase = "FilenameBaseMatches";

        Assert.EndsWith(filenameBase, golden.FilenameBase);
    }

    [Theory]
    [InlineData("hello", 123)]
    public void FilenameBaseMatches_Theory(string s, int i)
    {
        var golden = WpfGolden.FromCurrentTest();

        const string filenameBase = "FilenameBaseMatches";

        Assert.EndsWith(filenameBase, golden.FilenameBase);
    }
}
