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
        get => GetBool("HighResolution3D", true);
        set => SetBool("HighResolution3D", value);
    }

    public bool Ps1Dithering
    {
        get => GetBool("Ps1Dithering", false);
        set => SetBool("Ps1Dithering", value);
    }

    public bool TextureSmoothing
    {
        get => GetBool("TextureSmoothing", true);
        set => SetBool("TextureSmoothing", value);
    }

    public bool PerspectiveCorrectTextures
    {
        get => GetBool("PerspectiveCorrectTextures", true);
        set
        {
            SetBool("PerspectiveCorrectTextures", value);
            Gte.SetProjectionTrackingEnabled(
                value || StabilizeGeometrySeams ||
                Hle.LiveWorldRenderer.Requested ||
                ProjectionTrace.Enabled);
        }
    }

    public bool StabilizeGeometrySeams
    {
        get => GetBool("StabilizeGeometrySeams", true);
        set
        {
            SetBool("StabilizeGeometrySeams", value);
            Gte.SetProjectionTrackingEnabled(
                value || PerspectiveCorrectTextures ||
                Hle.LiveWorldRenderer.Requested ||
                ProjectionTrace.Enabled);
        }
    }

    public bool ExtendedDrawDistance
    {
        get => GetBool("ExtendedDrawDistance", true);
        set => SetBool("ExtendedDrawDistance", value);
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
        get => GetString("LevelOfDetail", "Maximum");
        set => SetString("LevelOfDetail", value);
    }

    public string GraphicsPreset
    {
        get => GetString("GraphicsPreset", "Enhanced");
        set => SetString("GraphicsPreset", value);
    }

    public void MarkGraphicsCustom() => GraphicsPreset = "Custom";

    public void ApplyGraphicsPreset(string preset)
    {
        bool ps1Quality = preset.Equals(
            "PS1 Quality", StringComparison.OrdinalIgnoreCase);

        GraphicsPreset = ps1Quality ? "PS1 Quality" : "Enhanced";
        PerspectiveCorrectTextures = !ps1Quality;
        StabilizeGeometrySeams = !ps1Quality;
        ExtendedDrawDistance = !ps1Quality;
        LevelOfDetail = ps1Quality ? "Stock" : "Maximum";
        Ps1Dithering = ps1Quality;
    }

    public void EnforceGraphicsPreset()
    {
        if (GraphicsPreset.Equals(
                "PS1 Quality", StringComparison.OrdinalIgnoreCase))
        {
            ApplyGraphicsPreset("PS1 Quality");
        }
        else if (GraphicsPreset.Equals(
                     "Enhanced", StringComparison.OrdinalIgnoreCase))
        {
            ApplyGraphicsPreset("Enhanced");
        }
        else
        {
            // Unknown or individually edited configurations are Custom. Keep
            // their individual values intact instead of silently selecting a
            // named preset whose advertised settings do not match.
            GraphicsPreset = "Custom";
        }
    }
}
