# Wpf.Goldens

**Wpf.Goldens** is a library for snapshot testing of WPF views (windows or controls).

A **golden**[^1] is a rendered reference bitmap of what a view is supposed to look like. Running the same test again later on lets you find out if the view's layout, colors, etc. have unexpected changes. When a test is initially run, the library stores a render. Then when the test is run again subsequently, it renders the same view again, and also calculates discrepancies, if any.

[^1]: [based on Flutter's term](https://halesworth.org/snapshot-testing-with-flutter-golden/)

## How it's made

It builds on:

- [**xUnit.net**](https://xunit.net/) as the underlying testing framework,
- [**Xunit.StaFact**](https://github.com/AArnott/Xunit.StaFact) to enables WPF testing, and
- [**Microsoft TestApi**](https://github.com/Microsoft/TestApi) to compare image snapshots.

## How you use it

Suppose you have an about box window. With **Wpf.Goldens**, you can write a test like so:

```csharp
[Xunit.WpfFact]
public void TestAboutBox()
{
    var window = new AboutBox();
    window.Show();

    WpfGolden.FromCurrentTest()
        .Build()
        .WithSize(640, 480)
        .Render(window)
        .AssertIsWithinTolerance(0);
}
```

When first run, this creates, in your tests project, these folders and files:

```
MyProject.Tests
- Goldens
-- AboutBox.png
- TestOutput
-- AboutBox.png
```

The two PNGs will be identical.

When run a second time, you end up with a third file:

```
MyProject.Tests
- Goldens
-- AboutBox.png
- TestOutput
-- AboutBox.png
-- AboutBox_diff.png
```

In our example, the `diff` will be an all-black image the same size as the others. This means no pixel has changed.

Let's go through what this means line by line!

```csharp
[Xunit.WpfFact] // ensures a proper WPF context is established and cleaned up
public void TestAboutBox()
{
    var window = new AboutBox();
    window.Show(); // shows the window; forcing WPF to render it

    WpfGolden.FromCurrentTest() // determines file and folder paths based on the test
        .Build()
        .WithSize(640, 480) // optionally sets explicit size for our view
        .Render(window)
        .AssertIsWithinTolerance(0); // asserts all pixels' colors differ by zero
}
```

`AssertIsWithinTolerance` here is an optional test helper for simple scenarios: if the difference exceeds the tolerance, the test fails.

You can also:

- increase the tolerance (on a scale from `0` to `255`),
- directly call `renderedGolden.VerifyUniformColorDifference(0)` for different assertions
- use `AssertIsWithinToleranceMap` for a `ToleranceMap` (see below)
- for more complex scenarios, call `SnapshotColorVerifier`, `SnapshotHistogramVerifier`, or `SnapshotToleranceMapVerifier` yourself.

A custom `SnapshotColorVerifier` is useful for when you want to tolerate more differences in one color channel than in another; our default implementation treats RGBA uniformly.

We don't provide helper methods for _all_ scenarios; see the implementation of `WpfRenderedGolden` and `WpfRenderedGoldenXunitExtensions` to get started for your own custom scenarios.

### Tolerance Maps

A `SnapshotToleranceMapVerifier` is useful for scenarios where one region of the view is varies highly, but everything else should always render the same.

In our About Box example above, consider what happens if the box mentions the current version number in the bottom right. With a tolerance map, you'd increase the tolerance for that portion of the window.

A tolerance map is a bitmap where black pixels mean no differences, and white pixels a complete difference. In the above, we can therefore use a bitmap that starts all black, but with a white (or grey) rectangle where the version number would be. We can then pass this as a resource:

```csharp
var toleranceMap = (System.Drawing.Bitmap)Image.FromFile("Resources/AboutBoxTests_ToleranceMap.png");

WpfGolden.FromCurrentTest()
    .Build()
    .Render(window)
    .AssertIsWithinToleranceMap(toleranceMap);
```

Now, if the differences in the image are only the "expected" (non-black) ones, the test will pass. The diff file will _still_ tell you what actual differences were found.

## What about version control?

The intended approach here is:

1. you check in the entire contents of `Goldens/`.
2. you ignore `TestOutput/`.
3. if a change to how a view is rendered is _intentional_, you copy the render output from `TestOutput/` to `Goldens/`, making the new version the new golden.

## What about other frameworks?

This is currently tightly coupled against Xunit and WPF.

Adapting it for a different testing framework such as NUnit mainly involves porting `Xunit.StaFact`.

Adapting it for a different UI framework probably wouldn't be too difficult.
