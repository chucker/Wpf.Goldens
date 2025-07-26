using Microsoft.Test.VisualVerification;

using System.IO;
using System.Windows.Media.Imaging;

namespace Chucker.Lib.WpfGoldens;

/// <summary>
/// Handles the final result of the <see cref="WpfGolden"/>. Generally, you
/// won't use this directly, but rather call
/// <see cref="WpfRenderedGoldenXunitExtensions.AssertIsWithinTolerance"/>.
/// </summary>
public class WpfRenderedGolden(WpfGolden wpfGolden, BitmapFrame renderedFrame)
{
    public WpfGolden WpfGolden { get; } = wpfGolden;

    public VerificationResult CompareToCanonical(byte tolerance)
    {
        var encoder = new PngBitmapEncoder()
        {
            Frames = [renderedFrame]
        };

        using (var stream = File.Create(WpfGolden.NewPath))
            encoder.Save(stream);

        var actual = Snapshot.FromFile(WpfGolden.NewPath);

        // on first run, the old path won't exist yet, so we'll simply do a no-op diff
        if (!File.Exists(WpfGolden.OldPath))
        {
            File.Copy(WpfGolden.NewPath, WpfGolden.OldPath);

            // if there was nothing to compare to, assume success
            return VerificationResult.Pass;
        }

        var expected = Snapshot.FromFile(WpfGolden.OldPath);

        var difference = actual.CompareTo(expected);
        difference.ToFile(WpfGolden.DiffPath, System.Drawing.Imaging.ImageFormat.Png);

        var colorTolerance = new ColorDifference(tolerance, tolerance, tolerance, tolerance);
        SnapshotVerifier v = new SnapshotColorVerifier(System.Drawing.Color.Black, colorTolerance);

        return v.Verify(difference);
    }
}
