using System;
using System.Collections.Generic;
using System.Linq;

namespace SaturnEdit.Systems;

public static class LaunchSystem
{
    public static void Initialize()
    {
        LaunchArguments = new LaunchArguments(RawLaunchArguments);
    }

    public static LaunchArguments LaunchArguments { get; private set; } = new([]);
    public static string[] RawLaunchArguments
    {
        get => rawLaunchArguments;
        set
        {
            if (rawLaunchArguments != null) throw new InvalidOperationException("Launch arguments have already been initialized.");

            rawLaunchArguments = value;
        }
    }
    private static string[]? rawLaunchArguments;
}

public sealed class LaunchArguments
{
    public LaunchArguments(IEnumerable<string> args)
    {
        arguments = args.ToArray();
    }

    public string? this[int index]
    {
        get => index >= 0 && index < arguments.Length
            ? arguments[index]
            : null;
    }

    public string? this[string argument]
    {
        get
        {
            if (!argument.StartsWith("--")) return null;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].StartsWith("--")) continue;
                if (!string.Equals(arguments[i], argument, StringComparison.OrdinalIgnoreCase)) continue;

                return i + 1 < arguments.Length && !arguments[i + 1].StartsWith("--")
                    ? arguments[i + 1]
                    : null;
            }

            return null;
        }
    }

    public bool Has(string argument)
    {
        if (!argument.StartsWith("--")) return false;

        return arguments.Any(x =>
            x.StartsWith("--") &&
            string.Equals(x, argument, StringComparison.OrdinalIgnoreCase));
    }

    public int Count => arguments.Length;

    private readonly string[] arguments = [];
}