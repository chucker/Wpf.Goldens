using Xunit;

namespace Chucker.Lib.WpfGoldens.Tests;

public class PathTraversalExtensionTests
{
    [Theory]
    [InlineData("some/Path/To/victory.png", "some/Path/To/victory.png")]
    [InlineData("../some/Path/To/victory.png", "some/Path/To/victory.png")]
    [InlineData("../../some/Path/To/victory.png", "some/Path/To/victory.png")]
    [InlineData(@"..\..\some\Path\To\victory.png", @"some\Path\To\victory.png")]
    [InlineData("../../../some/Path/To/victory.png", "some/Path/To/victory.png")]
    public void TestRemoveLeadingUpwardsNavigationFromRelativePath(string input, string expectedOutput)
        => Assert.Equal(expectedOutput, input.RemoveLeadingUpwardsNavigationFromRelativePath());
}
