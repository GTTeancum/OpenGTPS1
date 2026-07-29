namespace RecompOne.Runtime;

public enum WorldObjectKind : uint
{
    Unknown = 0,
    Track = 1,
    Vehicle = 2,
}

public readonly record struct WorldObjectContext(
    WorldObjectKind Kind,
    uint StableId,
    uint ModelPointer);

/// <summary>
/// GT2-specific generated-code hooks identify the object whose model is about
/// to use the GTE. Projection provenance snapshots this value, so later GPU
/// packets retain authored object identity without guessing from screen space.
/// </summary>
public static class WorldCaptureContext
{
    static readonly bool Enabled =
        !string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_WORLD_CAPTURE_PATH"));
    static readonly Dictionary<uint, WorldObjectContext> TrackObjects = [];
    static readonly Dictionary<uint, uint> VehicleIds = [];
    static WorldObjectContext _current;

    public static bool CaptureEnabled => Enabled;
    public static WorldObjectContext Current =>
        Enabled ? _current : default;

    public static void RegisterTrackObject(
        uint submissionPointer,
        uint stableId,
        uint modelPointer)
    {
        if (!Enabled || submissionPointer == 0 || modelPointer == 0)
            return;
        TrackObjects[submissionPointer] = new WorldObjectContext(
            WorldObjectKind.Track,
            stableId,
            modelPointer);
    }

    public static void BeginTrackObject(
        uint submissionPointer,
        uint modelPointer)
    {
        if (!Enabled)
            return;
        if (TrackObjects.TryGetValue(submissionPointer, out var context) &&
            context.ModelPointer == modelPointer)
        {
            _current = context;
            return;
        }
        _current = new WorldObjectContext(WorldObjectKind.Track, 0, modelPointer);
    }

    public static void BeginVehicle(uint carState, uint modelPointer)
    {
        if (!Enabled)
            return;
        if (!VehicleIds.TryGetValue(carState, out uint stableId))
        {
            stableId = checked((uint)VehicleIds.Count);
            VehicleIds[carState] = stableId;
        }
        _current = new WorldObjectContext(
            WorldObjectKind.Vehicle,
            stableId,
            modelPointer);
    }

    public static void EndObject()
    {
        if (Enabled)
            _current = default;
    }
}
