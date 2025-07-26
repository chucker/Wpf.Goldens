using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chucker.Lib.WpfGoldens;

/// <summary>
/// Prepares a <see cref="WpfGolden"/> result by rendering a view.
/// </summary>
public class WpfRenderPipeline(WpfGolden wpfGolden)
{
    public Size? DesiredSize { get; private set; }

    /// <summary>
    /// Renders a window into a bitmap, and returns the comparison.
    /// </summary>
    public WpfRenderedGolden Render(UIElement uiElement)
    {
        var size = DesiredSize ?? uiElement.RenderSize;

        // NOTE: SnapshotHelper.SnapshotFromWindow(window, WindowSnapshotMode.IncludeWindowBorder);
        // exists, but we don't use it, since it doesn't appear to behave correctly

        uiElement.Measure(size);
        uiElement.Arrange(new Rect(size));
        uiElement.UpdateLayout();

        var targetBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
        targetBitmap.Render(uiElement);

        return new WpfRenderedGolden(wpfGolden, BitmapFrame.Create(targetBitmap));
    }

    /// <summary>
    /// Allows you to customize the desired render size.
    /// </summary>
    public WpfRenderPipeline WithSize(double width, double height)
    {
        DesiredSize = new Size(width, height);

        return this;
    }
}
