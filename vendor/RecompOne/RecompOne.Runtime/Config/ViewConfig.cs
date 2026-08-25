using System.Globalization;

namespace RecompOne.Runtime.Config;

public class PanelState
{
    public bool Open { get; set; }
}

public class ViewConfig
{
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, PanelState> Panels { get; set; } = [];

    public bool GetBool(string key, bool fallback = false)
        => Values.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : fallback;

    public void SetBool(string key, bool value) => Values[key] = value.ToString();

    public int GetInt(string key, int fallback = 0)
        => Values.TryGetValue(key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : fallback;

    public void SetInt(string key, int value) => Values[key] = value.ToString(CultureInfo.InvariantCulture);

    public float GetFloat(string key, float fallback = 0f)
        => Values.TryGetValue(key, out var v) && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : fallback;

    public void SetFloat(string key, float value) => Values[key] = value.ToString(CultureInfo.InvariantCulture);

    public string GetString(string key, string fallback = "")
        => Values.TryGetValue(key, out var v) ? v : fallback;

    public void SetString(string key, string value) => Values[key] = value;

    public bool HideTopBar
    {
        get => GetBool("HideTopBar");
        set => SetBool("HideTopBar", value);
    }

    public bool Fullscreen
    {
        get => GetBool("Fullscreen");
        set => SetBool("Fullscreen", value);
    }

    public bool HighResolution3D
    {
        get => true;
        set => SetBool("HighResolution3D", true);
    }

    public bool Ps1Dithering
    {
        get => false;
        set => SetBool("Ps1Dithering", false);
    }

    public bool TextureSmoothing
    {
        get => true;
        set => SetBool("TextureSmoothing", true);
    }

    public bool HighResolutionTextures
    {
        get => true;
        set => SetBool("HighResolutionTextures", true);
    }

    public bool PerspectiveCorrectTextures
    {
        // The shipping renderer consumes authored model-space geometry and
        // uses hardware perspective interpolation. Legacy projected packets
        // may still be classified conservatively inside the renderer while
        // their GT-specific UV reconstruction is being completed, but the
        // global modern-renderer contract itself is never affine-only.
        get => true;
        set
        {
            SetBool("PerspectiveCorrectTextures", true);
            Gte.SetProjectionTrackingEnabled(true);
        }
    }

    public bool StabilizeGeometrySeams
    {
        get => true;
        set
        {
            SetBool("StabilizeGeometrySeams", true);
            Gte.SetProjectionTrackingEnabled(true);
        }
    }

    public bool ExtendedDrawDistance
    {
        get => true;
        set => SetBool("ExtendedDrawDistance", true);
    }

    public string OutputResolution
    {
        get => GetString("OutputResolution", "1920x1080");
        set => SetString("OutputResolution", value);
    }

    public string AntiAliasing
    {
        get => GetString("AntiAliasing", "FXAA");
        set => SetString("AntiAliasing", value);
    }

    public string LevelOfDetail
    {
        get => "Maximum";
        set => SetString("LevelOfDetail", "Maximum");
    }

    public string GraphicsPreset
    {
        get => "Enhanced";
        set => SetString("GraphicsPreset", "Enhanced");
    }

    public void ApplyGraphicsPreset(string _)
    {
        // The PC port ships one modern renderer configuration. Preserve the
        // serialized keys for forward compatibility, but migrate every legacy,
        // custom, or contradictory file to the proven full-quality contract.
        GraphicsPreset = "Enhanced";
        HighResolution3D = true;
        TextureSmoothing = true;
        HighResolutionTextures = true;
        PerspectiveCorrectTextures = true;
        StabilizeGeometrySeams = true;
        ExtendedDrawDistance = true;
        LevelOfDetail = "Maximum";
        Ps1Dithering = false;
    }

    public void EnforceGraphicsPreset() => ApplyGraphicsPreset("Enhanced");
}
