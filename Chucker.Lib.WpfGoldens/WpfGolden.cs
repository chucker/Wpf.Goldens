using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

/// <summary>
/// <para>
/// Prepares a visual unit test that verifies a WPF view (a window or control)
/// against a canonical snapshot. Will help you find out if the layout, colors,
/// etc. have unexpected changes.
/// </para>
///
/// <para>
/// </para>
/// A simple example:
/// <code>
/// WpfGolden.FromCurrentTest()
///     .Build()
///     .WithSize(640, 480) // only needed if you don't want the default size
///     .Render(window)
///     .AssertIsWithinTolerance(0); // increase if small variations are OK
/// </code>
/// </summary>
public class WpfGolden
{
    public string FilenameBase { get; }

    public DirectoryInfo GoldensDir { get; }
    public DirectoryInfo TestOutputDir { get; }

    public string OldPath => Path.Combine(GoldensDir.FullName, $"{FilenameBase}.png");
    public string NewPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}.png");
    public string DiffPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}_diff.png");

    private WpfGolden(string filenameBase,
        DirectoryInfo goldensDir,
        DirectoryInfo testOutputDir)
    {
        FilenameBase = filenameBase;
        GoldensDir = goldensDir;
        TestOutputDir = testOutputDir;
    }

    /// <summary>
    /// Creates a <see cref="WpfRenderPipeline"/> for the golden.
    /// </summary>
    public WpfRenderPipeline Build()
        => new(this);

    /// <summary>
    /// Prepares a <see cref="WpfGolden"/> based on the current unit test. This
    /// will place test output near it. Call <see cref="Build"/> afterwards.
    /// </summary>
    public static WpfGolden FromCurrentTest([CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "")
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

        var parentDir = Directory.GetParent(callerFilePath)!;
        var projectRoot = parentDir.GetAncestorWithFileMatching("*.csproj") ?? parentDir;

        var filenameBase = GetFilenameBaseFromCurrentTest(callerFilePath,
            callerMemberName,
            callingAssembly,
            args);

        return new WpfGolden(filenameBase,
            projectRoot.CreateSubdirectory("Goldens"),
            projectRoot.CreateSubdirectory("TestOutput"));
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
    private static string GetFilenameBaseFromCurrentTest(string callerFilePath,
        string callerMemberName,
        Assembly callingAssembly,
        object?[] callingMemberArgs)
    {
        var callerFilePathWithoutExtension = Path.ChangeExtension(callerFilePath, null);

        var assemblyPath = callingAssembly.Location;
        var relativePath = new Uri(assemblyPath).MakeRelativeUri(new Uri(callerFilePathWithoutExtension));

        char[] charsToFilter = [.. Path.GetInvalidFileNameChars(), '.'];

        var sanitizedFilePath = charsToFilter
            .Aggregate(relativePath.OriginalString, (s, originalChar) => s.Replace(originalChar, '_'));

        foreach (var arg in callingMemberArgs)
            sanitizedFilePath += $"_{arg}";

        // coalesce consecutive underscores
        sanitizedFilePath = Regex.Replace(sanitizedFilePath, @"(_)\1+", "_");

        sanitizedFilePath = sanitizedFilePath.TrimStart('_');

        return $"{sanitizedFilePath}_{callerMemberName}";
    }
}
