using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Xunit.v3;

namespace Chucker.Lib.WpfGoldens;

file static class DirectoryInfoAncestorTraversalExtensions
{
    /// <summary>
    /// <para>
    /// Finds an ancestor directory that contains a file matching the glob.
    /// </para>
    /// <para>
    /// For example, given a path
    /// <c>/Users/me/Projects/MyProject/Fancy/Subdir</c>, it may find
    /// <c>/Users/me/Projects/MyProject/MyProject.csproj</c>, and therefore
    /// return <c>/Users/me/Projects/MyProject</c>.
    /// </para>
    /// </summary>
    public static DirectoryInfo? GetAncestorWithFileMatching(this DirectoryInfo descendant, string globPattern)
    {
        var ancestor = descendant;

        do
        {
            if (ancestor.EnumerateFiles(globPattern).Any())
                return ancestor;

            ancestor = ancestor.Parent;
        }
        while (ancestor is not null && ancestor.Parent != ancestor.Root);

        return null;
    }
}

internal static class PathTraversalExtensions
{
    /// <summary>
    /// <para>
    /// Removes leading upwards navigation (typically <c>..\</c> on Windows or
    /// <c>../</c> on Unix) from a relative path.
    /// </para>
    /// <para>
    /// For example, turns
    /// <c>../../some/fancy/path</c> into just <c>some/fancy/path</c>.
    /// </para>
    /// </summary>
    public static string RemoveLeadingUpwardsNavigationFromRelativePath(this string path)
    {
        foreach (var separatorChar in new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar })
        {
            path = path.Replace($"..{separatorChar}", "");
        }

        return path;
    }
}

internal class WpfGoldenTestBinder(string testFilePath, string testMemberName)
{
    private string TestFilePath { get; } = testFilePath;
    private string TestMemberName { get; } = testMemberName;

    public WpfGolden CreateGolden()
    {
        var callingAssembly = Assembly.GetCallingAssembly();

        object?[] args;

        if (Xunit.TestContext.Current.Test is XunitTest test)
            args = test.TestMethodArguments;
        else
            args = [];

        // LATER: for primitive types, we should add the param name
        // Dictionary<ParameterInfo, object?>? args = null;
        //
        // if (Xunit.TestContext.Current.Test is XunitTest test &&
        //     test.TestMethodArguments.Length != 0 &&
        //     new StackFrame(1).GetMethod() is { } callerMethodInfo)
        // {
        //     args = callerMethodInfo.GetParameters().Zip(test.TestMethodArguments)
        //         .ToDictionary(x => x.First, x => x.Second);
        // }

        var parentDir = Directory.GetParent(TestFilePath)!;
        var projectRoot = parentDir.GetAncestorWithFileMatching("*.csproj") ?? parentDir;

        (Uri relativePath, string filenameBase) = GetRelativePaths(callingAssembly, args);

        string relativePathWithoutUpwardsNavigation =
            relativePath.OriginalString.RemoveLeadingUpwardsNavigationFromRelativePath();

        var goldensSubdir = projectRoot
            .CreateSubdirectory("Goldens")
            .CreateSubdirectory(relativePathWithoutUpwardsNavigation);

        var testOutputSubdir = projectRoot
            .CreateSubdirectory("TestOutput")
            .CreateSubdirectory(relativePathWithoutUpwardsNavigation);

        return new WpfGolden(filenameBase,
            goldensSubdir,
            testOutputSubdir);
    }

    /// <summary>
    /// <para>
    /// Returns the path to the executing test, relative to the project base,
    /// sanitized to have no invalid file name characters and no file
    /// extension.
    /// </para>
    /// <para>
    /// For example, takes
    /// <c>C:\Users\someUser\source\repos\TRENZ\easyLogic\Client\EL.Client.WpfUi.Tests\AboutBox\AboutBoxTests.cs</c>
    /// and turns that into <c>AboutBox_AboutBoxTests</c>.
    /// </para>
    /// </summary>
    private (Uri RelativePath, string FilenameBase)
        GetRelativePaths(Assembly callingAssembly, object?[] callingMemberArgs)
    {
        var callerFilePathWithoutExtension = Path.ChangeExtension(TestFilePath, null);

        var assemblyPath = callingAssembly.Location;
        var relativePath = new Uri(assemblyPath).MakeRelativeUri(new Uri(callerFilePathWithoutExtension));

        var filenameBase = TestMemberName;

        foreach (var arg in callingMemberArgs)
            filenameBase += $"_{arg}";

        return (relativePath, filenameBase);
    }
}
