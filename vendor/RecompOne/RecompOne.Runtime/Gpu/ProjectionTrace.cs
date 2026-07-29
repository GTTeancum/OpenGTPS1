using System.Text.Json;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Host;

namespace RecompOne.Runtime;

internal readonly record struct ProjectionTraceVertex(
    int X,
    int Y,
    int U,
    int V,
    int Z,
    bool Trusted,
    uint PacketAddress,
    int Age,
    GteDepthProvenance Provenance);

/// <summary>
/// Opt-in structured diagnostics for the PS1 projection enhancement.
/// The trace records where every depth came from; a screenshot can show a
/// regression, but it cannot establish that a GPU vertex received the depth
/// produced for that exact packet word.
/// </summary>
internal static class ProjectionTrace
{
    static readonly string? ConfiguredPath =
        Environment.GetEnvironmentVariable("RECOMPONE_PROJECTION_TRACE_PATH");
    static readonly int DetailStartPoll =
        ParsePoll("RECOMPONE_PROJECTION_TRACE_START_POLL", int.MaxValue);
    static readonly int DetailEndPoll =
        ParsePoll("RECOMPONE_PROJECTION_TRACE_END_POLL", int.MinValue);
    static readonly int SummaryInterval =
        Math.Max(
            1,
            ParsePoll(
                "RECOMPONE_PROJECTION_TRACE_SUMMARY_INTERVAL",
                120));
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    static StreamWriter? _writer;
    static bool _initializationAttempted;
    static int _summaryPoll = -1;
    static int _texturedPolygons;
    static int _texturedTriangles;
    static int _perspectiveTriangles;
    static int _affineTriangles;
    static int _depthRatioRejectedTriangles;
    static int _directVertices;
    static int _cpuFlowVertices;
    static int _valueMatchVertices;
    static int _missingVertices;
    static int _maximumAge;
    static float _maximumAvailableDepthRatio;
    static float _maximumTrustedDepthRatio;
    static float _maximumRawTrustedDepthRatio;

    public static bool Enabled => !string.IsNullOrWhiteSpace(ConfiguredPath);

    static int ParsePoll(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out int value)
            ? Math.Max(0, value)
            : fallback;

    static void EnsureWriter()
    {
        if (_writer != null || _initializationAttempted || !Enabled)
            return;

        _initializationAttempted = true;
        try
        {
            string path = Path.GetFullPath(ConfiguredPath!);
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            _writer = new StreamWriter(path, append: false)
            {
                AutoFlush = true,
            };
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                FlushSummary();
                _writer?.Dispose();
            };

            var view = ConfigManager.View;
            Write(new
            {
                type = "projection_trace_header",
                version = 1,
                utc = DateTimeOffset.UtcNow,
                detailStartPoll = DetailStartPoll,
                detailEndPoll = DetailEndPoll,
                summaryInterval = SummaryInterval,
                graphics = new
                {
                    preset = view.GraphicsPreset,
                    perspectiveCorrectTextures =
                        view.PerspectiveCorrectTextures,
                    ps1Dithering = view.Ps1Dithering,
                    textureSmoothing = view.TextureSmoothing,
                    stabilizeGeometrySeams =
                        view.StabilizeGeometrySeams,
                    extendedDrawDistance =
                        view.ExtendedDrawDistance,
                    levelOfDetail = view.LevelOfDetail,
                },
                policy = new
                {
                    trustedDepth = new[]
                    {
                        "DirectStore",
                        "CpuRegisterFlow",
                    },
                    ambiguousDepth = "AffineFallback",
                    maximumTrustedDepthRatio =
                        Gte.MaximumPerspectiveDepthRatio,
                },
            });
            Console.Error.WriteLine(
                $"[GPU] structured projection trace={path} " +
                $"detailPolls={DetailStartPoll}..{DetailEndPoll}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[GPU] unable to create projection trace: {ex.Message}");
        }
    }

    public static void RecordPolygon(
        uint command,
        bool textured,
        bool quad,
        ReadOnlySpan<ProjectionTraceVertex> vertices)
    {
        if (!Enabled || !textured)
            return;

        EnsureWriter();
        if (_writer == null)
            return;

        int poll = InputManager.CurrentPoll;
        int summaryPoll = poll / SummaryInterval * SummaryInterval;
        if (_summaryPoll != summaryPoll)
        {
            FlushSummary();
            _summaryPoll = summaryPoll;
        }

        _texturedPolygons++;
        int triangleCount = quad ? 2 : 1;
        _texturedTriangles += triangleCount;
        CountTriangle(vertices[0], vertices[1], vertices[2]);
        if (quad)
            CountTriangle(vertices[1], vertices[2], vertices[3]);

        foreach (ref readonly ProjectionTraceVertex vertex in vertices)
        {
            switch (vertex.Provenance)
            {
                case GteDepthProvenance.DirectStore:
                    _directVertices++;
                    break;
                case GteDepthProvenance.CpuRegisterFlow:
                    _cpuFlowVertices++;
                    break;
                case GteDepthProvenance.RegisterValueMatch:
                    _valueMatchVertices++;
                    break;
                default:
                    _missingVertices++;
                    break;
            }
            _maximumAge = Math.Max(_maximumAge, vertex.Age);
        }

        if (poll < DetailStartPoll || poll > DetailEndPoll)
            return;

        var detailVertices = new object[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            ProjectionTraceVertex vertex = vertices[i];
            detailVertices[i] = new
            {
                x = vertex.X,
                y = vertex.Y,
                u = vertex.U,
                v = vertex.V,
                z = vertex.Z,
                trusted = vertex.Trusted,
                packetAddress = vertex.PacketAddress == uint.MaxValue
                    ? null
                    : $"0x{vertex.PacketAddress:X8}",
                age = vertex.Age,
                provenance = vertex.Provenance.ToString(),
            };
        }

        Write(new
        {
            type = "textured_polygon",
            poll,
            generation = Gte.ProjectionGeneration,
            command = $"0x{command:X8}",
            quad,
            perspectiveTriangle0 = IsPerspectiveEligible(
                vertices[0], vertices[1], vertices[2]),
            perspectiveTriangle1 =
                quad &&
                IsPerspectiveEligible(
                    vertices[1], vertices[2], vertices[3]),
            vertices = detailVertices,
        });
    }

    static void CountTriangle(
        in ProjectionTraceVertex a,
        in ProjectionTraceVertex b,
        in ProjectionTraceVertex c)
    {
        bool allTrusted = a.Trusted && b.Trusted && c.Trusted;
        bool eligible = IsPerspectiveEligible(a, b, c);
        if (eligible)
            _perspectiveTriangles++;
        else
            _affineTriangles++;

        if (a.Z <= 0 || b.Z <= 0 || c.Z <= 0)
            return;
        int minimum = Math.Min(a.Z, Math.Min(b.Z, c.Z));
        int maximum = Math.Max(a.Z, Math.Max(b.Z, c.Z));
        float ratio = (float)maximum / Math.Max(1, minimum);
        _maximumAvailableDepthRatio = Math.Max(
            _maximumAvailableDepthRatio,
            ratio);
        if (allTrusted)
            _maximumRawTrustedDepthRatio = Math.Max(
                _maximumRawTrustedDepthRatio,
                ratio);
        if (eligible)
            _maximumTrustedDepthRatio = Math.Max(
                _maximumTrustedDepthRatio,
                ratio);
        else if (allTrusted)
            _depthRatioRejectedTriangles++;
    }

    static bool IsPerspectiveEligible(
        in ProjectionTraceVertex a,
        in ProjectionTraceVertex b,
        in ProjectionTraceVertex c)
    {
        if (!a.Trusted || !b.Trusted || !c.Trusted ||
            a.Z <= 0 || b.Z <= 0 || c.Z <= 0)
            return false;
        int minimum = Math.Min(a.Z, Math.Min(b.Z, c.Z));
        int maximum = Math.Max(a.Z, Math.Max(b.Z, c.Z));
        return (float)maximum / minimum <=
            Gte.MaximumPerspectiveDepthRatio;
    }

    static void FlushSummary()
    {
        if (_writer == null || _summaryPoll < 0)
            return;

        Write(new
        {
            type = "poll_summary",
            pollStart = _summaryPoll,
            pollEnd = _summaryPoll + SummaryInterval - 1,
            texturedPolygons = _texturedPolygons,
            texturedTriangles = _texturedTriangles,
            perspectiveTriangles = _perspectiveTriangles,
            affineTriangles = _affineTriangles,
            depthRatioRejectedTriangles =
                _depthRatioRejectedTriangles,
            vertices = new
            {
                directStore = _directVertices,
                cpuRegisterFlow = _cpuFlowVertices,
                registerValueMatch = _valueMatchVertices,
                missing = _missingVertices,
                maximumAge = _maximumAge,
            },
            maximumAvailableDepthRatio = _maximumAvailableDepthRatio,
            maximumTrustedDepthRatio = _maximumTrustedDepthRatio,
            maximumRawTrustedDepthRatio =
                _maximumRawTrustedDepthRatio,
        });

        _texturedPolygons = 0;
        _texturedTriangles = 0;
        _perspectiveTriangles = 0;
        _affineTriangles = 0;
        _depthRatioRejectedTriangles = 0;
        _directVertices = 0;
        _cpuFlowVertices = 0;
        _valueMatchVertices = 0;
        _missingVertices = 0;
        _maximumAge = 0;
        _maximumAvailableDepthRatio = 0f;
        _maximumTrustedDepthRatio = 0f;
        _maximumRawTrustedDepthRatio = 0f;
    }

    static void Write<T>(T value)
    {
        _writer?.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
    }
}
