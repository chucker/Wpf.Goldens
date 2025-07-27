using Microsoft.Test.VisualVerification;

using System.IO;
using System.Windows.Media.Imaging;

using Gdi = System.Drawing;

namespace Chucker.Lib.WpfGoldens;

/// <summary>
/// Handles the final result of the <see cref="WpfGolden"/>. Generally, you
/// won't use this directly, but rather call
/// <see cref="WpfRenderedGoldenXunitExtensions.AssertIsWithinTolerance"/>.
/// </summary>
public class WpfRenderedGolden(WpfGolden wpfGolden, BitmapFrame renderedFrame)
{
    private WpfGolden WpfGolden { get; } = wpfGolden;

    public VerificationResult VerifyUniformColorDifference(byte tolerance)
    {
        if (SaveSnapshots(out var actual))
            return VerificationResult.Pass;

        var difference = CalculateDifference(actual);

        var colorTolerance = new ColorDifference(tolerance, tolerance, tolerance, tolerance);
        SnapshotVerifier v = new SnapshotColorVerifier(Gdi.Color.Black, colorTolerance);

        return v.Verify(difference);
    }

    public VerificationResult VerifyToleranceMapDifference(Gdi.Bitmap toleranceMap)
    {
        if (SaveSnapshots(out var actual))
            return VerificationResult.Pass;

        var difference = CalculateDifference(actual);

        SnapshotVerifier v = new SnapshotToleranceMapVerifier(Snapshot.FromBitmap(toleranceMap));

        return v.Verify(difference);
    }

    private Snapshot CalculateDifference(Snapshot actual)
    {
        var expected = Snapshot.FromFile(WpfGolden.OldPath);

        var difference = actual.CompareTo(expected);
        difference.ToFile(WpfGolden.DiffPath, Gdi.Imaging.ImageFormat.Png);
        return difference;
    }

    private bool SaveSnapshots(out Snapshot actual)
    {
        var encoder = new PngBitmapEncoder()
        {
            Frames = [renderedFrame]
        };

        using (var stream = File.Create(WpfGolden.NewPath))
            encoder.Save(stream);

        actual = Snapshot.FromFile(WpfGolden.NewPath);

        // on first run, the old path won't exist yet, so we'll simply do a no-op diff
        if (!File.Exists(WpfGolden.OldPath))
        {
            File.Copy(WpfGolden.NewPath, WpfGolden.OldPath);
            return true;
        }

        return false;
    }
}
