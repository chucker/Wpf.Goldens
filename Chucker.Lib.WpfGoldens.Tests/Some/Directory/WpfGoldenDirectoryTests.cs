using Xunit;

namespace Chucker.Lib.WpfGoldens.Tests.Some.Directory;

public class WpfGoldenDirectoryTests
{
    [Fact]
    public void GoldensDirMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\Goldens\Some\Directory\WpfGoldenDirectoryTests", golden.GoldensDir.FullName);
    }

    [Fact]
    public void TestOutputDirMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\TestOutput\Some\Directory\WpfGoldenDirectoryTests", golden.TestOutputDir.FullName);
    }

    [Fact]
    public void FilenameBaseMatches()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.Equal(nameof(FilenameBaseMatches), golden.FilenameBase);
    }
}
