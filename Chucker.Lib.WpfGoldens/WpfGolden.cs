using System.IO;
using System.Runtime.CompilerServices;

namespace Chucker.Lib.WpfGoldens;

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
    public string DiffPath => Path.Combine(TestOutputDir.FullName, $"{FilenameBase}.diff.png");

    internal WpfGolden(string filenameBase,
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
    public static WpfGolden FromCurrentTest([CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMemberName = "")
    {
        return new WpfGoldenTestBinder(callerFilePath, callerMemberName).CreateGolden();
    }
}
