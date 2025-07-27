# Wpf.Goldens

**Wpf.Goldens** is a library for snapshot testing of WPF views (windows or controls).

A **golden**[^1] is a rendered reference bitmap of what a view is supposed to look like. Running the same test again later on lets you find out if the view's layout, colors, etc. have unexpected changes. When a test is run, the library stores the inital image in a folder `Goldens/`, as a reference. Then when the test is run again subsequently, it creates another folder `TestOutput/`, containing the _new_ render, as well as a `diff` image showing you discrepancies, if any.

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
        .WithSize(640, 480) // sizes our view
        .Render(window)
        .AssertIsWithinTolerance(0); // asserts zero differences
}
```

`AssertIsWithinTolerance` here is an optional test helper for simple scenarios: if the difference exceeds the tolerance, the test fails.

You can also:

- increase the tolerance (on a scale from `0` to `255`),
- for more complex scenarios, not call this method at all and instead use a custom `SnapshotColorVerifier`, `SnapshotHistogramVerifier`, or `SnapshotToleranceMapVerifier`.

A custom `SnapshotColorVerifier` is useful for when you want to tolerate more differences in one color channel than in another; our default implementation treats RGBA uniformly.

A `SnapshotToleranceMapVerifier` is useful for scenarios where one portion of the view is varies highly, but everything else should always render the same. In our About Box example above, consider what happens if the box mentions the current version number in the bottom right. With a tolerance map, you'd increase the tolerance for that portion of the window.

We don't currently provide helper methods for those scenarios; see the implementation of `CompareToCanonical` to get started.

## What about version control?

The intended approach here is:

1. you check in the entire contents of `Goldens/`.
2. you ignore `TestOutput/`.
3. if a change to how a view is rendered is _intentional_, you copy the render output from `TestOutput/` to `Goldens/`, making the new version the new golden.
