using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Chucker.Lib.WpfGoldens;

public class WpfGoldensAssemblyConfiguration(Assembly testAssembly)
{
    public static WpfGoldensAssemblyConfiguration ForTestAssembly()
    {
        var wpfGoldensAssembly = typeof(WpfGoldensAssemblyConfiguration).Assembly!;

        // get the first stack frame that _isn't_ ours
        var testAssembly = new StackTrace().GetFrames()!
            .Select(f => f.GetMethod()?.DeclaringType?.Assembly)
            .First(a => a != null && a != wpfGoldensAssembly)!;

        return new WpfGoldensAssemblyConfiguration(testAssembly);
    }

    public bool AllowUpdates =>
        testAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "AllowUpdates")
            ?.Value is { } s &&
        bool.TryParse(s, out bool enabled) &&
        enabled;
}
