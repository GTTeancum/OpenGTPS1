using ImGuiNET;
using RecompOne.Runtime.Config;

namespace RecompOne.Runtime.Host.Window;

internal sealed class DisplaySettingsSection : ISettingsSection
{
    static readonly string[] OutputResolutions = ["1280x720", "1920x1080", "2560x1440", "3840x2160"];
    static readonly string[] AntiAliasingModes = ["Off", "FXAA"];
    static readonly string[] GraphicsPresets = ["PS1 Quality", "Enhanced", "Custom"];

    public string Id => "display";
    public string Title => "Display";
    public int Order => 5;

    public void Draw()
    {
        string resolution = ConfigManager.View.OutputResolution;
        if (!OutputResolutions.Contains(resolution, StringComparer.OrdinalIgnoreCase))
            resolution = OutputResolutions[0];
        if (ImGui.BeginCombo("Output resolution", resolution))
        {
            foreach (string candidate in OutputResolutions)
            {
                bool selected = candidate.Equals(resolution, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(candidate, selected))
                {
                    ConfigManager.View.OutputResolution = candidate;
                    HostWindow.SetOutputResolution(candidate);
                    ConfigManager.SaveView(PanelManager.Panels);
                }
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.TextDisabled("Fullscreen uses the desktop resolution.");

        bool fullscreen = ConfigManager.View.Fullscreen;
        if (ImGui.Checkbox("Fullscreen", ref fullscreen))
        {
            ConfigManager.View.Fullscreen = fullscreen;
            HostWindow.SetFullscreen(fullscreen);
            ConfigManager.SaveView(PanelManager.Panels);
        }

        string antiAliasing = ConfigManager.View.AntiAliasing;
        if (!AntiAliasingModes.Contains(antiAliasing, StringComparer.OrdinalIgnoreCase))
            antiAliasing = AntiAliasingModes[0];
        if (ImGui.BeginCombo("Anti-aliasing", antiAliasing))
        {
            foreach (string candidate in AntiAliasingModes)
            {
                bool selected = candidate.Equals(antiAliasing, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(candidate, selected))
                {
                    ConfigManager.View.AntiAliasing = candidate;
                    ConfigManager.SaveView(PanelManager.Panels);
                }
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.TextDisabled("FXAA affects presentation only; PS1 rendering stays native.");

        ImGui.SeparatorText("Graphics quality preset");
        string preset = ConfigManager.View.GraphicsPreset;
        if (!GraphicsPresets.Contains(preset, StringComparer.OrdinalIgnoreCase))
            preset = "Custom";
        if (ImGui.BeginCombo("Preset", preset))
        {
            foreach (string candidate in GraphicsPresets)
            {
                bool selected = candidate.Equals(
                    preset, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(candidate, selected))
                {
                    if (candidate == "Custom")
                        ConfigManager.View.MarkGraphicsCustom();
                    else
                        ConfigManager.View.ApplyGraphicsPreset(candidate);
                    ConfigManager.SaveView(PanelManager.Panels);
                    preset = candidate;
                }
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.TextDisabled(
            "PS1 Quality preserves original rendering. Enhanced enables the recommended fixes.");

        ImGui.SeparatorText("Output rendering");
        bool highResolution3D = ConfigManager.View.HighResolution3D;
        if (ImGui.Checkbox("High-resolution 3D (4x)", ref highResolution3D))
        {
            ConfigManager.View.HighResolution3D = highResolution3D;
            ConfigManager.SaveView(PanelManager.Panels);
        }
        ImGui.TextDisabled("Rasterizes PS1 polygons at 4x internal resolution.");
        if (ConfigManager.View.HighResolution3D != Hle.GpuHle.Active)
            ImGui.TextDisabled("A restart is required.");

        bool textureSmoothing = ConfigManager.View.TextureSmoothing;
        if (ImGui.Checkbox("Upscale/smooth in-game textures", ref textureSmoothing))
        {
            ConfigManager.View.TextureSmoothing = textureSmoothing;
            ConfigManager.SaveView(PanelManager.Panels);
        }
        ImGui.TextDisabled("Reconstructs in-game PS1 textures without allocating assets over 512x512.");

        if (preset.Equals("Custom", StringComparison.OrdinalIgnoreCase))
        {
            ImGui.SeparatorText("Custom graphics");

            bool perspectiveCorrectTextures =
                ConfigManager.View.PerspectiveCorrectTextures;
            if (ImGui.Checkbox(
                    "Fix PS1 texture projection", ref perspectiveCorrectTextures))
            {
                ConfigManager.View.PerspectiveCorrectTextures =
                    perspectiveCorrectTextures;
                SaveCustom();
            }
            ImGui.TextDisabled(
                "Uses recovered GTE depth for perspective-correct texture interpolation.");

            bool stabilizeSeams = ConfigManager.View.StabilizeGeometrySeams;
            if (ImGui.Checkbox(
                    "Stabilize road and model seams", ref stabilizeSeams))
            {
                ConfigManager.View.StabilizeGeometrySeams = stabilizeSeams;
                SaveCustom();
            }
            ImGui.TextDisabled(
                "Adds a subpixel overlap to projected 3D triangles only.");

            bool extendedDrawDistance = ConfigManager.View.ExtendedDrawDistance;
            if (ImGui.Checkbox(
                    "Extended track draw distance", ref extendedDrawDistance))
            {
                ConfigManager.View.ExtendedDrawDistance = extendedDrawDistance;
                SaveCustom();
            }
            ImGui.TextDisabled(
                "Uses GT2's longest track-data visibility and expanded polygon buffers.");

            bool maximumLod = ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
            if (ImGui.Checkbox("Maximum LOD", ref maximumLod))
            {
                ConfigManager.View.LevelOfDetail =
                    maximumLod ? "Maximum" : "Stock";
                SaveCustom();
            }
            ImGui.TextDisabled(
                "Forces the highest-detail vehicle, track, and scenery models at every distance.");

            bool ps1Dithering = ConfigManager.View.Ps1Dithering;
            if (ImGui.Checkbox("PS1 color dithering", ref ps1Dithering))
            {
                ConfigManager.View.Ps1Dithering = ps1Dithering;
                SaveCustom();
            }
            ImGui.TextDisabled("Reproduces the original 15-bit color dither pattern.");
        }
        else
        {
            string presetSummary = preset.Equals(
                "PS1 Quality", StringComparison.OrdinalIgnoreCase)
                ? "Projection: PS1  |  Seams: PS1  |  Distance: Stock  |  LOD: Stock  |  Dithering: On"
                : "Projection: Corrected  |  Seams: Stabilized  |  Distance: Extended  |  LOD: Maximum  |  Dithering: Off";
            ImGui.TextWrapped(presetSummary);
            ImGui.TextDisabled("Choose Custom to adjust these five options individually.");
        }
    }

    static void SaveCustom()
    {
        ConfigManager.View.MarkGraphicsCustom();
        ConfigManager.SaveView(PanelManager.Panels);
    }
}
