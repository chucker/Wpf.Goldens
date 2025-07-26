using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
        while (ancestor.Parent != ancestor.Root);

        return null;
    }
}

/// <summary>
/// <para>
/// Prepares a visual unit test that verifies a WPF view against a canonical
/// snapshot. Will help you find out if the layout, colors, etc. unexpectedly.
/// </para>
///
/// <para>
/// </para>
/// A simple example:
/// <code>
/// WpfGolden.FromCurrentTest()
///     .Build()
///     .WithSize(640, 480) // only needed if you don't want the default size
///     .RenderWindow(window)
///     .AssertIsWithinTolerance(0); // increase if small variations are OK
/// </code>
/// </summary>
public class WpfGolden
{
    private const string _testOutputDirName = "TestOutput";

    public string FilenameBase { get; private set; }

    public DirectoryInfo TestOutputDir { get; } = GetTestOutputDir();
    private static DirectoryInfo GetTestOutputDir([CallerFilePath] string callerFilePath = "")
        => Directory.GetParent(callerFilePath)
            .GetAncestorWithFileMatching("*.csproj")! // get ancestor containing the project
            .CreateSubdirectory(_testOutputDirName);

    public string OldPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}.png");
    public string NewPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}_new.png");
    public string DiffPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}_diff.png");

    private WpfGolden(string filenameBase)
    {
        FilenameBase = filenameBase;
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

        return new WpfGolden(GetFilenameBaseFromCurrentTest(callerFilePath, callerMemberName, callingAssembly));
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
        Assembly callingAssembly)
    {
        var callerFilePathWithoutExtension = Path.ChangeExtension(callerFilePath, null);

        var assemblyPath = callingAssembly.Location;
        var relativePath = new Uri(assemblyPath).MakeRelativeUri(new Uri(callerFilePathWithoutExtension));

        char[] charsToFilter = [.. Path.GetInvalidFileNameChars(), '.'];

        var sanitizedFilePath = charsToFilter
            .Aggregate(relativePath.OriginalString, (s, originalChar) => s.Replace(originalChar, '_'));

        // coalesce consecutive underscores
        sanitizedFilePath = Regex.Replace(sanitizedFilePath, @"(_)\1+", "_");

        sanitizedFilePath = sanitizedFilePath.TrimStart('_');

        return $"{sanitizedFilePath}_{callerMemberName}";
    }
}
