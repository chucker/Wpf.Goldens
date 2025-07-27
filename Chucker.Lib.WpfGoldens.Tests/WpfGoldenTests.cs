using System.Collections.Generic;

using Xunit;

namespace Chucker.Lib.WpfGoldens.Tests;

public class WpfGoldenTests
{
    public static IEnumerable<TheoryDataRow<string, int>> TestParameters
    {
        get
        {
            yield return new TheoryDataRow<string, int>("hello", 123);

            yield return new TheoryDataRow<string, int>("", 0);
        }
    }

    [Fact]
    public void GoldensDirMatches_Fact()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\Goldens\WpfGoldenTests", golden.GoldensDir.FullName);
    }

    [Theory]
    [MemberData(nameof(TestParameters))]
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
    [MemberData(nameof(TestParameters))]
    public void TestOutputDirMatches_Theory(string s, int i)
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.EndsWith(@"WpfGoldens\Chucker.Lib.WpfGoldens.Tests\TestOutput\WpfGoldenTests", golden.TestOutputDir.FullName);
    }

    [Fact]
    public void FilenameBaseMatches_Fact()
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.Equal(nameof(FilenameBaseMatches_Fact), golden.FilenameBase);
    }

    [Theory]
    [MemberData(nameof(TestParameters))]
    public void FilenameBaseMatches_Theory(string s, int i)
    {
        var golden = WpfGolden.FromCurrentTest();

        Assert.Equal($"{nameof(FilenameBaseMatches_Theory)}_{s}_{i}", golden.FilenameBase);
    }
}
