using System.Collections;
using System.Runtime.CompilerServices;

namespace RecompOne.Runtime;

/// <summary>
/// Removes development-only renderer controls before any runtime type can
/// snapshot process environment variables in a public release build.
/// Development builds retain the complete evidence and tracing surface.
/// </summary>
public static class ReleasePolicy
{
    static int _applied;

    public static bool IsReleasePackage
    {
        get
        {
#if OPENGT_RELEASE_PACKAGE
            return true;
#else
            return false;
#endif
        }
    }

#if OPENGT_RELEASE_PACKAGE
#pragma warning disable CA2255 // Release policy must run before runtime type initialization.
    [ModuleInitializer]
    internal static void InitializeModule() => Apply();
#pragma warning restore CA2255
#endif

    public static void Apply()
    {
        if (!IsReleasePackage || Interlocked.Exchange(ref _applied, 1) != 0)
            return;

        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry.Key is not string name || !IsDevelopmentControl(name))
                continue;
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    static bool IsDevelopmentControl(string name)
    {
        ReadOnlySpan<string> prefixes =
        [
            "OPENGT_",
            "RECOMPONE_AUDIT_",
            "RECOMPONE_CAPTURE_",
            "RECOMPONE_DEBUG_",
            "RECOMPONE_DEV_",
            "RECOMPONE_DUMP_",
            "RECOMPONE_GPU_",
            "RECOMPONE_GT2_",
            "RECOMPONE_INPUT_FILE",
            "RECOMPONE_INPUT_SCRIPT",
            "RECOMPONE_NATIVE_WORLD_",
            "RECOMPONE_PROJECTION_",
            "RECOMPONE_RENDER_",
            "RECOMPONE_SOAK_",
            "RECOMPONE_TEST_",
            "RECOMPONE_TRACE_",
            "RECOMPONE_WORLD_CAPTURE",
        ];
        foreach (string prefix in prefixes)
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        return name.Equals(
            "RECOMPONE_GRAPHICS_PRESET_OVERRIDE",
            StringComparison.OrdinalIgnoreCase);
    }
}
