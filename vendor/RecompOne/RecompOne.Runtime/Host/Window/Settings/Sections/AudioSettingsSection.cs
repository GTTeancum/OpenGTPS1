using ImGuiNET;
using RecompOne.Runtime.Config;

namespace RecompOne.Runtime.Host.Window;

internal sealed class AudioSettingsSection : ISettingsSection
{
    public string Id => "audio";
    public string Title => "Audio";
    public int Order => 10;

    public void Draw()
    {
        var game = ConfigManager.Game;

        float volume = game.MasterVolume;
        if (ImGui.SliderFloat("Master Volume", ref volume, 0f, 1f, "%.2f"))
        {
            game.MasterVolume = Math.Clamp(volume, 0f, 1f);
            Audio.SetMasterVolume(game.Muted ? 0f : game.MasterVolume);
            ConfigManager.SaveGame();
        }

        bool muted = game.Muted;
        if (ImGui.Checkbox("Mute", ref muted))
        {
            game.Muted = muted;
            Audio.SetMasterVolume(muted ? 0f : game.MasterVolume);
            ConfigManager.SaveGame();
        }

        ImGui.Separator();
        string[] tracks = OggMusic.TrackLabels;
        ImGui.Text($"External OGG music: {tracks.Length} track(s)");
        ImGui.TextDisabled("Add files to music as [artist] - [song].ogg");
        string currentTrack = OggMusic.CurrentTrackLabel;
        if (currentTrack.Length > 0)
            ImGui.Text($"Now playing: {currentTrack}");
        foreach (string track in tracks)
            ImGui.BulletText(track);
    }
}
