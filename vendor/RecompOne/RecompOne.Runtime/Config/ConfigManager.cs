using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImGuiNET;
using RecompOne.Runtime.Host.Window;
using RecompOne.Runtime.Serialization;

namespace RecompOne.Runtime.Config;

static file class PanelDefaults
{
    public static bool IsOpenByDefault(IPanel p) => p.Name == "Output";
}

public static class ConfigManager
{
    static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    const string GameConfigPath = "settings.json";
    const string InterfaceFile = "interface.ini";

    public static GameConfig Game { get; private set; } = new();
    public static ViewConfig  View { get; private set; } = new();

    static string? _pendingImGuiIni;
    static bool _suppressViewPersistence;
    static bool _suppressionReported;

    public static void Load()
    {
        bool saveGame = false;
        if (File.Exists(GameConfigPath))
        {
            try { Game = JsonSerializer.Deserialize(File.ReadAllText(GameConfigPath), RuntimeJsonContext.Default.GameConfig) ?? new(); }
            catch { Game = new(); saveGame = true; }
        }
        else
        {
            Game = new();
            saveGame = true;
        }

        if (Game.InputBindingsVersion < 1)
        {
            // Pad2 was historically serialized as an all-empty legacy default.
            // Migrate only that legacy shape; any customized non-empty mapping
            // is retained byte-for-byte by the serializer.
            if (!Game.Pad2.HasAnyBinding()) Game.Pad2 = new GamepadBindings();
            Game.InputBindingsVersion = 1;
            saveGame = true;
        }
        if (saveGame) SaveGame();

        if (File.Exists(InterfaceFile))
        {
            var (view, imguiIni) = ParseInterfaceFile(File.ReadAllText(InterfaceFile));
            View = view;
            _pendingImGuiIni = imguiIni;
        }
        // A named preset is authoritative. This repairs stale or contradictory
        // files so the wrapper can never display "Enhanced" while running
        // stock draw distance/LOD (or the inverse for PS1 Quality).
        View.EnforceGraphicsPreset();

        string? graphicsPresetOverride =
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GRAPHICS_PRESET_OVERRIDE");
        if (!string.IsNullOrWhiteSpace(graphicsPresetOverride))
        {
            _suppressViewPersistence = true;
            View.ApplyGraphicsPreset(graphicsPresetOverride);
            Console.Error.WriteLine(
                $"[Host] graphics preset test override={View.GraphicsPreset}");
        }

        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_PERSPECTIVE_CORRECT_TEXTURES",
            value => View.PerspectiveCorrectTextures = value);
        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_STABILIZE_GEOMETRY_SEAMS",
            value => View.StabilizeGeometrySeams = value);
        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_EXTENDED_DRAW_DISTANCE",
            value => View.ExtendedDrawDistance = value);
        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_MAXIMUM_LOD",
            value => View.LevelOfDetail = value ? "Maximum" : "Stock");
        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_PS1_DITHERING",
            value => View.Ps1Dithering = value);
        _suppressViewPersistence |= ApplyBoolOverride(
            "RECOMPONE_TEXTURE_SMOOTHING",
            value => View.TextureSmoothing = value);
        if (_suppressViewPersistence)
            Console.Error.WriteLine(
                "[Host] graphics test overrides are transient; " +
                "interface.ini persistence disabled");
        Gte.SetProjectionTrackingEnabled(
            View.PerspectiveCorrectTextures ||
            View.StabilizeGeometrySeams ||
            ProjectionTrace.Enabled);
    }

    static bool ApplyBoolOverride(string name, Action<bool> apply)
    {
        string? text = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(text))
            return false;
        bool value = text == "1" ||
            (text != "0" && bool.TryParse(text, out bool parsed) && parsed);
        apply(value);
        Console.Error.WriteLine($"[Host] graphics test override {name}={value}");
        return true;
    }

    
    public static bool ApplyImGuiLayout()
    {
        if (_pendingImGuiIni == null) return false;
        ImGui.LoadIniSettingsFromMemory(_pendingImGuiIni);
        _pendingImGuiIni = null;
        return true;
    }

    public static void ApplyViewToPanels(IReadOnlyList<IPanel> panels)
    {
        foreach (var p in panels)
        {
            if (View.Panels.TryGetValue(p.Name, out var state))
                p.IsOpen = state.Open;
        }
    }

    public static void SaveView(IReadOnlyList<IPanel> panels)
    {
        if (_suppressViewPersistence)
        {
            if (!_suppressionReported)
            {
                _suppressionReported = true;
                Console.Error.WriteLine(
                    "[Host] skipped interface.ini save because transient " +
                    "graphics overrides are active");
            }
            return;
        }
        foreach (var p in panels)
            View.Panels[p.Name] = new PanelState { Open = p.IsOpen };

        var imguiIni = ImGui.SaveIniSettingsToMemory();
        var sb = new StringBuilder();
        sb.AppendLine("[RecompOne]");
        foreach (var (key, value) in View.Values)
            sb.AppendLine($"{key}={value}");
        foreach (var (name, state) in View.Panels)
            sb.AppendLine($"Panels.{name}={state.Open}");
        sb.AppendLine();
        sb.Append(imguiIni);
        File.WriteAllText(InterfaceFile, sb.ToString());
    }

    public static void ResetView(IReadOnlyList<IPanel> panels)
    {
        View = new();
        foreach (var p in panels)
            p.IsOpen = PanelDefaults.IsOpenByDefault(p);
        ImGui.LoadIniSettingsFromMemory("");
        SaveView(panels);
    }

    public static void SaveGame()
    {
        File.WriteAllText(GameConfigPath, JsonSerializer.Serialize(Game, RuntimeJsonContext.Default.GameConfig));
    }

    static (ViewConfig view, string imguiIni) ParseInterfaceFile(string content)
    {
        var view = new ViewConfig();
        var imguiLines = new List<string>();
        bool inRecompOne = false;

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (line == "[RecompOne]")
            {
                inRecompOne = true;
                continue;
            }

            if (line.StartsWith('['))
                inRecompOne = false;

            if (inRecompOne)
            {
                if (line.Length == 0) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                var key = line[..eq];
                var value = line[(eq + 1)..];
                if (key.StartsWith("Panels."))
                {
                    var panelName = key[7..];
                    var open = bool.TryParse(value, out var b) && b;
                    view.Panels[panelName] = new PanelState { Open = open };
                }
                else
                {
                    view.Values[key] = value;
                }
            }
            else
            {
                imguiLines.Add(line);
            }
        }

        return (view, string.Join('\n', imguiLines));
    }
}
