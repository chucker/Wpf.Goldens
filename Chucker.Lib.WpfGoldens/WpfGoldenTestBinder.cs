using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Xunit.v3;

namespace Chucker.Lib.WpfGoldens;

file static class DirectoryInfoAncestorTraversalExtensions
{
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

        (string relativeDir, string filenameBase) = GetRelativePaths(callingAssembly, args);

        var goldensSubdir = projectRoot
            .CreateSubdirectory("Goldens")
            .CreateSubdirectory(relativeDir);

        var testOutputSubdir = projectRoot
            .CreateSubdirectory("TestOutput")
            .CreateSubdirectory(relativeDir);

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
    private (string RelativeDir, string FilenameBase)
        GetRelativePaths(Assembly callingAssembly, object?[] callingMemberArgs)
    {
        var callerFilePathWithoutExtension = Path.ChangeExtension(TestFilePath, null);

        var assemblyPath = callingAssembly.Location;
        var relativePath = new Uri(assemblyPath).MakeRelativeUri(new Uri(callerFilePathWithoutExtension));

        char[] charsToFilter = [.. Path.GetInvalidFileNameChars(), '.'];

        var sanitizedFilePath = charsToFilter
            .Aggregate(relativePath.OriginalString, (s, originalChar) => s.Replace(originalChar, '_'));

        var filenameBase = TestMemberName;

        foreach (var arg in callingMemberArgs)
            filenameBase += $"_{arg}";

        // coalesce consecutive underscores
        sanitizedFilePath = Regex.Replace(sanitizedFilePath, @"(_)\1+", "_");

        sanitizedFilePath = sanitizedFilePath.TrimStart('_');

        return (sanitizedFilePath, filenameBase);
    }
}
