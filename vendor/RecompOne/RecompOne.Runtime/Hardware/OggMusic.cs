using NVorbis;

namespace RecompOne.Runtime;

public static class OggMusic
{
    private sealed record Track(
        string Path,
        string Artist,
        string Title,
        string QueueLabel)
    {
        public string Label => $"{Artist} - {Title}";
    }

    private static readonly object Gate = new();
    private static readonly List<Track> Tracks = [];
    private static VorbisReader? _reader;
    private static float[] _decode = [];
    private static int _decodeOffset;
    private static int _decodeCount;
    private static int _nextTrack;
    private static int _sourceRate = 44100;
    private static int _channels = 2;
    private static double _phase;
    private static float _s0L, _s0R, _s1L, _s1R;
    private static bool _streamActive;
    private static string _currentTrackLabel = "";
    private static int _currentTrackNumber;
    private static int _completedTracks;
    private static int _queueLoops;
    private static long _outputFrames;
    private static readonly bool TraceMusic =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_MUSIC") == "1";

    public static bool HasTracks
    {
        get { lock (Gate) return Tracks.Count > 0; }
    }

    public static string[] TrackLabels
    {
        get { lock (Gate) return Tracks.Select(track => track.QueueLabel).ToArray(); }
    }

    public static string CurrentTrackLabel
    {
        get { lock (Gate) return _currentTrackLabel; }
    }

    public static string Status
    {
        get
        {
            lock (Gate)
            {
                if (Tracks.Count == 0) return "empty";
                if (!_streamActive) return $"idle queue={Tracks.Count}";
                return
                    $"playing={_currentTrackNumber}/{Tracks.Count} " +
                    $"label=\"{_currentTrackLabel}\" completed={_completedTracks} " +
                    $"loops={_queueLoops} outputFrames={_outputFrames}";
            }
        }
    }

    public static void Initialize(string looseRoot)
    {
        lock (Gate)
        {
            DisposeReader();
            Tracks.Clear();
            _nextTrack = 0;
            _streamActive = false;
            _currentTrackLabel = "";
            _currentTrackNumber = 0;
            _completedTracks = 0;
            _queueLoops = 0;
            _outputFrames = 0;

            string musicDirectory = Path.Combine(looseRoot, "music");
            Directory.CreateDirectory(musicDirectory);
            var parsedTracks = Directory.EnumerateFiles(
                         musicDirectory, "*", SearchOption.TopDirectoryOnly)
                     .Where(path => Path.GetExtension(path).Equals(
                         ".ogg", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(path => Path.GetFileName(path),
                         StringComparer.OrdinalIgnoreCase)
                     .Select(path =>
                     {
                         ParseFileName(path, out string artist, out string title);
                         return (Path: Path.GetFullPath(path), Artist: artist, Title: title);
                     })
                     .ToArray();
            var labelCounts = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);
            foreach (var parsed in parsedTracks)
            {
                string label = $"{parsed.Artist} - {parsed.Title}";
                int occurrence = labelCounts.TryGetValue(label, out int count)
                    ? count + 1
                    : 1;
                labelCounts[label] = occurrence;
                string queueLabel = occurrence == 1
                    ? label
                    : $"{label} ({occurrence})";
                Tracks.Add(new Track(
                    parsed.Path, parsed.Artist, parsed.Title, queueLabel));
            }

            Console.WriteLine(
                $"[Music] indexed {Tracks.Count} OGG track(s) from {musicDirectory}");
            foreach (Track track in Tracks)
                Console.WriteLine(
                    $"[Music] queued: {track.QueueLabel} " +
                    $"source={Path.GetFileName(track.Path)}");
        }
    }

    public static void SetMusicStreamActive(bool active)
    {
        lock (Gate)
        {
            active &= Tracks.Count > 0;
            if (_streamActive == active) return;
            _streamActive = active;
            if (active)
            {
                Console.WriteLine($"[Music] stream active; queue={Tracks.Count}");
                if (!OpenNextTrack())
                    _streamActive = false;
            }
            else
            {
                if (_currentTrackLabel.Length > 0)
                    Console.WriteLine(
                        $"[Music] stream inactive; completed={_completedTracks} " +
                        $"loops={_queueLoops} outputFrames={_outputFrames}");
                DisposeReader();
                _currentTrackLabel = "";
                _currentTrackNumber = 0;
            }
        }
    }

    public static bool Next(out short left, out short right)
    {
        lock (Gate)
        {
            if (!_streamActive || Tracks.Count == 0)
            {
                left = right = 0;
                return false;
            }
            if (_reader == null && !OpenNextTrack())
            {
                _streamActive = false;
                left = right = 0;
                return false;
            }

            float f = (float)_phase;
            left = FloatToPcm(_s0L + (_s1L - _s0L) * f);
            right = FloatToPcm(_s0R + (_s1R - _s0R) * f);
            _outputFrames++;
            _phase += (double)_sourceRate / 44100.0;
            while (_phase >= 1.0)
            {
                // Consume the phase before opening a new track. OpenNextTrack
                // resets it; subtracting afterwards would leave a negative
                // phase and extrapolate one bad sample at every queue boundary.
                _phase -= 1.0;
                _s0L = _s1L;
                _s0R = _s1R;
                bool sourceFrameRead;
                try
                {
                    sourceFrameRead = ReadSourceFrame(out _s1L, out _s1R);
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(
                        $"[Music] decode failed for '{_currentTrackLabel}': " +
                        exception.Message);
                    sourceFrameRead = false;
                }
                if (!sourceFrameRead)
                {
                    _completedTracks++;
                    if (TraceMusic)
                        Console.Error.WriteLine(
                            $"[MUSIC-QUEUE] completed=\"{_currentTrackLabel}\" " +
                            $"completedTracks={_completedTracks} outputFrames={_outputFrames}");
                    if (!OpenNextTrack())
                    {
                        _streamActive = false;
                        left = right = 0;
                        return false;
                    }
                    break;
                }
            }
            return true;
        }
    }

    private static bool OpenNextTrack()
    {
        DisposeReader();
        if (Tracks.Count == 0) return false;

        for (int attempt = 0; attempt < Tracks.Count; attempt++)
        {
            int trackIndex = _nextTrack;
            Track track = Tracks[trackIndex];
            if (trackIndex == 0 && _completedTracks > 0)
                _queueLoops++;
            _nextTrack = (trackIndex + 1) % Tracks.Count;
            try
            {
                _reader = new VorbisReader(track.Path);
                _sourceRate = _reader.SampleRate;
                _channels = _reader.Channels;
                if (_sourceRate <= 0 || _channels is < 1 or > 2)
                    throw new InvalidDataException(
                        $"unsupported format {_sourceRate} Hz/{_channels}ch");

                _decode = new float[8192 * _channels];
                _decodeOffset = _decodeCount = 0;
                _phase = 0;
                if (!ReadSourceFrame(out _s0L, out _s0R))
                    throw new InvalidDataException("track contains no audio samples");
                if (!ReadSourceFrame(out _s1L, out _s1R))
                {
                    _s1L = _s0L;
                    _s1R = _s0R;
                }
                _currentTrackLabel = track.QueueLabel;
                _currentTrackNumber = trackIndex + 1;
                Console.WriteLine(
                    $"[Music] now playing: {track.QueueLabel} " +
                    $"queue={_currentTrackNumber}/{Tracks.Count} " +
                    $"loop={_queueLoops} ({_sourceRate} Hz, {_channels}ch)");
                return true;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"[Music] skipped '{track.QueueLabel}': {exception.Message}");
                DisposeReader();
            }
        }
        _currentTrackLabel = "";
        _currentTrackNumber = 0;
        return false;
    }

    private static void ParseFileName(
        string path,
        out string artist,
        out string title)
    {
        string name = Path.GetFileNameWithoutExtension(path).Trim();
        int separator = name.IndexOf(" - ", StringComparison.Ordinal);
        if (separator > 0 && separator + 3 < name.Length)
        {
            artist = name[..separator].Trim();
            title = name[(separator + 3)..].Trim();
            if (artist.Length > 0 && title.Length > 0)
                return;
        }

        // Malformed names remain playable and visible instead of silently
        // disappearing from the queue.
        artist = "Unknown Artist";
        title = name.Length > 0 ? name : "Untitled";
    }

    private static bool ReadSourceFrame(out float left, out float right)
    {
        if (_reader == null)
        {
            left = right = 0;
            return false;
        }
        if (_decodeOffset + _channels > _decodeCount)
        {
            _decodeCount = _reader.ReadSamples(_decode, 0, _decode.Length);
            _decodeOffset = 0;
            if (_decodeCount < _channels)
            {
                left = right = 0;
                return false;
            }
        }

        left = _decode[_decodeOffset];
        right = _channels == 1 ? left : _decode[_decodeOffset + 1];
        _decodeOffset += _channels;
        return true;
    }

    private static short FloatToPcm(float sample)
    {
        sample = Math.Clamp(sample, -1f, 1f);
        return sample <= -1f
            ? short.MinValue
            : (short)MathF.Round(sample * short.MaxValue);
    }

    private static void DisposeReader()
    {
        _reader?.Dispose();
        _reader = null;
        _decode = [];
        _decodeOffset = _decodeCount = 0;
        _phase = 0;
        _s0L = _s0R = _s1L = _s1R = 0;
    }
}
