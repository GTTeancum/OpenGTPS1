using ImGuiNET;
using RecompOne.Runtime.Config;

namespace RecompOne.Runtime.Host.Window;

internal sealed class DisplaySettingsSection : ISettingsSection
{
    static readonly string[] OutputResolutions =
    [
        "1280x720", "1920x1080", "2560x1440", "3840x2160",
        "2560x1080", "3440x1440", "3840x1600", "5120x1440",
    ];
    static readonly string[] AntiAliasingModes = ["Off", "FXAA"];

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
        ImGui.TextDisabled("FXAA affects presentation only; native scene geometry is unchanged.");

        ImGui.SeparatorText("Modern renderer");
        ImGui.TextWrapped(
            "The native renderer is always enabled for races and replays. " +
            "There is no legacy 3D compatibility mode.");
        ImGui.BulletText("3D scene rasterization: 4x (320x240 to 1280x960)");
        ImGui.BulletText("Texture assets: external 4x replacements when installed");
        ImGui.BulletText("Perspective-correct textures and stabilized seams");
        ImGui.BulletText("Extended authored draw distance and maximum LOD");
        ImGui.BulletText("Texture reconstruction enabled; color dithering disabled");
    }
}
