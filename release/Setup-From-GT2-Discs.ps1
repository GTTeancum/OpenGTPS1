param(
    [Parameter(Mandatory = $true)]
    [string]$SimulationImagePath,
    [Parameter(Mandatory = $true)]
    [string]$ArcadeImagePath
)

$ErrorActionPreference = 'Stop'
$expectedSimulationBytes = 691850208L
$expectedSimulationHash =
    'D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031'
$expectedArcadeBytes = 729423408L
$expectedArcadeHash =
    'C2E97D6B0C847CA4336D9D84D8D98C349D1240ED075E81AB3FD5C977E9A45075'
$install = $PSScriptRoot

function Resolve-AuthoritativeImage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Label,
        [Parameter(Mandatory = $true)]
        [long]$ExpectedBytes,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedHash
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Label disc image not found: $Path"
    }
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $file = Get-Item -LiteralPath $resolved
    if ($file.Length -ne $ExpectedBytes) {
        throw (
            "$Label disc image size mismatch. This build accepts only the " +
            "authoritative NTSC-U GT2 images. Expected $ExpectedBytes bytes; " +
            "found $($file.Length)."
        )
    }

    Write-Host "Validating the complete $Label disc image..."
    $actualHash = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
    if ($actualHash -ne $ExpectedHash) {
        throw (
            "$Label disc image SHA-256 mismatch. Expected $ExpectedHash; " +
            "found $actualHash."
        )
    }
    return $resolved
}

$simulationImage = Resolve-AuthoritativeImage `
    -Path $SimulationImagePath `
    -Label 'Simulation (SCUS-94488 revision 2)' `
    -ExpectedBytes $expectedSimulationBytes `
    -ExpectedHash $expectedSimulationHash
$arcadeImage = Resolve-AuthoritativeImage `
    -Path $ArcadeImagePath `
    -Label 'Arcade (SCUS-94455)' `
    -ExpectedBytes $expectedArcadeBytes `
    -ExpectedHash $expectedArcadeHash

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

public static class OpenGtUnifiedDiscInstaller
{
    const int RawSectorSize = 2352;
    const int CookedOffset = 24;
    const int CookedSize = 2048;
    const int RawPayloadOffset = 16;
    const int RawPayloadSize = 2336;
    const int GtfsEntrySize = 0x20;

    sealed class PendingGtfsEntry
    {
        public string Name;
        public long Offset;
        public int TailBytes;
    }

    sealed class TimImage
    {
        public int Width;
        public int Height;
        public ushort[] Pixels;
    }

    public static string Sha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    public static void Extract(
        string imagePath,
        string destination,
        long lba,
        long logicalSize,
        bool rawPayload)
    {
        string temporary = destination + ".tmp";
        DeleteIfPresent(temporary);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        try
        {
            using (FileStream source = new FileStream(
                imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream output = new FileStream(
                temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                AppendImageRange(source, output, lba, logicalSize, rawPayload);
                output.Flush(true);
            }
            Replace(temporary, destination);
        }
        catch
        {
            DeleteIfPresent(temporary);
            throw;
        }
    }

    public static void ExtractUnifiedVolumes(
        string simulationImage,
        string arcadeImage,
        string destination)
    {
        const long simulationVolumeSize = 488241152L;
        const long arcadeVolumeSize = 213596160L;
        string temporary = destination + ".tmp";
        DeleteIfPresent(temporary);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        try
        {
            using (FileStream output = new FileStream(
                temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (FileStream source = File.OpenRead(simulationImage))
                    AppendImageRange(
                        source, output, 473L, simulationVolumeSize, false);
                using (FileStream source = File.OpenRead(arcadeImage))
                    AppendImageRange(
                        source, output, 472L, arcadeVolumeSize, false);
                if (output.Length != simulationVolumeSize + arcadeVolumeSize)
                    throw new InvalidDataException(
                        "Unified GT2.VOL has an unexpected size: " + output.Length);
                output.Flush(true);
            }
            Replace(temporary, destination);
        }
        catch
        {
            DeleteIfPresent(temporary);
            throw;
        }
    }

    static void AppendImageRange(
        FileStream source,
        FileStream output,
        long lba,
        long logicalSize,
        bool rawPayload)
    {
        int sourceOffset = rawPayload ? RawPayloadOffset : CookedOffset;
        int outputPerSector = rawPayload ? RawPayloadSize : CookedSize;
        long sectors = (logicalSize + CookedSize - 1) / CookedSize;
        byte[] sector = new byte[RawSectorSize];
        source.Position = checked(lba * RawSectorSize);
        long logicalRemaining = logicalSize;
        for (long index = 0; index < sectors; index++)
        {
            ReadExactly(source, sector, 0, sector.Length);
            int writeCount = rawPayload
                ? outputPerSector
                : checked((int)Math.Min(CookedSize, logicalRemaining));
            output.Write(sector, sourceOffset, writeCount);
            logicalRemaining -= Math.Min(CookedSize, logicalRemaining);
        }
    }

    public static void PatchIsoRecord(
        string metadataPath,
        string isoName,
        uint lba,
        uint logicalSize)
    {
        byte[] data = File.ReadAllBytes(metadataPath);
        byte[] needle = Encoding.ASCII.GetBytes(isoName);
        int nameOffset = FindUnique(data, needle);
        if (nameOffset < 33)
            throw new InvalidDataException(
                "ISO record is missing for " + isoName + " in " + metadataPath);
        int record = nameOffset - 33;
        int recordLength = data[record];
        int nameLength = data[record + 32];
        if (recordLength < 33 + nameLength || nameLength != needle.Length)
            throw new InvalidDataException(
                "Malformed ISO record for " + isoName + " in " + metadataPath);

        WriteU32Le(data, record + 2, lba);
        WriteU32Be(data, record + 6, lba);
        WriteU32Le(data, record + 10, logicalSize);
        WriteU32Be(data, record + 14, logicalSize);

        string temporary = metadataPath + ".tmp";
        DeleteIfPresent(temporary);
        File.WriteAllBytes(temporary, data);
        Replace(temporary, metadataPath);
    }

    public static void BuildTitleAsset(
        string unifiedVolume,
        string destination)
    {
        const long simulationVolumeSize = 488241152L;
        long memberOffset;
        int memberSize;
        FindGtfsMember(
            unifiedVolume,
            simulationVolumeSize,
            "arcade/arc_topmenu_usa",
            out memberOffset,
            out memberSize);
        byte[] pack = new byte[memberSize];
        using (FileStream stream = File.OpenRead(unifiedVolume))
        {
            stream.Position = memberOffset;
            ReadExactly(stream, pack, 0, pack.Length);
        }
        if (!Sha256(pack).Equals(
                "128556787BBA8AB8317B8BE47D11DC8670C1503B79668CB3AF8BDF4B69EB564F",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                "The authoritative arcade/arc_topmenu_usa payload did not match");

        ushort[] basePanel = DecodeNativeDemoTitle(pack);
        int[,] destinations = new int[,] {
            { 124, 284 }, { 124, 312 }, { 264, 284 }, { 264, 312 }
        };
        int[] selectedOffsets = new int[] {
            0x14800, 0x16800, 0x18800, 0x1A800
        };
        int[] unselectedOffsets = new int[] {
            0x15800, 0x17800, 0x19800, 0x1B800
        };
        ushort[][] panels = new ushort[4][];
        for (int selected = 0; selected < 4; selected++)
        {
            ushort[] panel = (ushort[])basePanel.Clone();
            for (int item = 0; item < 4; item++)
            {
                int sourceOffset = item == selected
                    ? selectedOffsets[item]
                    : unselectedOffsets[item];
                TimImage label = DecodeTim(DecompressSlice(
                    pack, sourceOffset, 0x1000));
                if (label.Width != 140 || label.Height != 28)
                    throw new InvalidDataException(
                        "Unexpected native GT2 title label dimensions");
                Blit(
                    panel,
                    512,
                    destinations[item, 0],
                    destinations[item, 1],
                    label);
            }
            panels[selected] = panel;
        }

        string temporary = destination + ".tmp";
        DeleteIfPresent(temporary);
        try
        {
            using (FileStream stream = new FileStream(
                temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(Encoding.ASCII.GetBytes("GT2TITLE"));
                writer.Write(512);
                writer.Write(480);
                writer.Write(4);
                foreach (ushort[] panel in panels)
                    foreach (ushort pixel in panel)
                        writer.Write(pixel);
                writer.Flush();
                stream.Flush(true);
            }
            if (new FileInfo(temporary).Length != 1966100L)
                throw new InvalidDataException(
                    "Generated TITLE_EXACT.DAT has an unexpected size");
            Replace(temporary, destination);
        }
        catch
        {
            DeleteIfPresent(temporary);
            throw;
        }
    }

    static ushort[] DecodeNativeDemoTitle(byte[] pack)
    {
        int[,] ranges = new int[,] {
            { 0x00000, 0x01000 },
            { 0x01000, 0x06800 },
            { 0x06800, 0x0E800 },
            { 0x0E800, 0x14800 }
        };
        ushort[] canvas = new ushort[512 * 480];
        int destination = 0;
        for (int index = 0; index < 4; index++)
        {
            int start = ranges[index, 0];
            TimImage strip = DecodeTim(DecompressSlice(
                pack, start, ranges[index, 1] - start));
            if (strip.Width != 512 || strip.Height != 120)
                throw new InvalidDataException(
                    "Unexpected native GT2 title background dimensions");
            Array.Copy(strip.Pixels, 0, canvas, destination, strip.Pixels.Length);
            destination += strip.Pixels.Length;
        }

        int[] dimOffsets = new int[] {
            0x15800, 0x17800, 0x19800, 0x1B800
        };
        int[,] destinations = new int[,] {
            { 124, 284 }, { 124, 312 }, { 264, 284 }, { 264, 312 }
        };
        for (int index = 0; index < 4; index++)
        {
            TimImage label = DecodeTim(DecompressSlice(
                pack, dimOffsets[index], 0x1000));
            if (label.Width != 140 || label.Height != 28)
                throw new InvalidDataException(
                    "Unexpected native GT2 title label dimensions");
            Blit(
                canvas,
                512,
                destinations[index, 0],
                destinations[index, 1],
                label);
        }
        return canvas;
    }

    static void Blit(
        ushort[] destination,
        int destinationWidth,
        int x,
        int y,
        TimImage source)
    {
        for (int row = 0; row < source.Height; row++)
            Array.Copy(
                source.Pixels,
                row * source.Width,
                destination,
                (y + row) * destinationWidth + x,
                source.Width);
    }

    static TimImage DecodeTim(byte[] tim)
    {
        if (tim.Length < 20 || ReadU32Le(tim, 0) != 0x10u ||
            ReadU32Le(tim, 4) != 2u)
            throw new InvalidDataException("Unexpected native GT2 menu-label TIM");
        int blockSize = checked((int)ReadU32Le(tim, 8));
        int width = ReadU16Le(tim, 16);
        int height = ReadU16Le(tim, 18);
        int pixelBytes = checked(width * height * 2);
        if (blockSize != 12 + pixelBytes || tim.Length < 20 + pixelBytes)
            throw new InvalidDataException("Malformed native GT2 menu-label TIM");
        ushort[] pixels = new ushort[width * height];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = ReadU16Le(tim, 20 + index * 2);
        return new TimImage { Width = width, Height = height, Pixels = pixels };
    }

    static byte[] DecompressSlice(byte[] source, int offset, int count)
    {
        using (MemoryStream input = new MemoryStream(
            source, offset, count, false))
        using (GZipStream gzip = new GZipStream(
            input, CompressionMode.Decompress, false))
        using (MemoryStream output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }

    static void FindGtfsMember(
        string volume,
        long archiveSize,
        string target,
        out long targetOffset,
        out int targetSize)
    {
        using (FileStream stream = File.OpenRead(volume))
        using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
        {
            byte[] magic = reader.ReadBytes(8);
            if (!Encoding.ASCII.GetString(magic).Equals("GTFS\0\0\0\0"))
                throw new InvalidDataException("GT2.VOL is not a GTFS archive");
            int offsetCount = reader.ReadUInt16();
            int entryCount = reader.ReadUInt16();
            reader.ReadUInt32();
            uint[] offsets = new uint[offsetCount];
            for (int index = 0; index < offsets.Length; index++)
                offsets[index] = reader.ReadUInt32();
            if (offsets.Length < 2)
                throw new InvalidDataException("GTFS offset table is truncated");
            long tocOffset = offsets[1] & ~((long)CookedSize - 1L);
            stream.Position = tocOffset;
            byte[] toc = reader.ReadBytes(checked(entryCount * GtfsEntrySize));
            if (toc.Length != entryCount * GtfsEntrySize)
                throw new InvalidDataException("GTFS table of contents is truncated");

            List<string> directories = new List<string>();
            string currentDirectory = "";
            int directoryIndex = 0;
            int directoryInsertIndex = 0;
            List<PendingGtfsEntry> pending = new List<PendingGtfsEntry>();
            for (int index = 0; index < entryCount; index++)
            {
                int record = index * GtfsEntrySize;
                short offsetIndex = unchecked((short)ReadU16Le(toc, record + 4));
                byte flags = toc[record + 6];
                string leaf = ReadAsciiZ(toc, record + 7, 25);
                string name = currentDirectory.Length == 0
                    ? leaf
                    : currentDirectory + "/" + leaf;
                if ((flags & 1) != 0)
                {
                    if (!leaf.Equals("..", StringComparison.Ordinal))
                    {
                        if (currentDirectory.Length != 0)
                            directories.Insert(directoryInsertIndex, name);
                        else
                            directories.Add(name);
                        directoryInsertIndex++;
                    }
                }
                else if (offsetIndex != 0)
                {
                    if (offsetIndex < 0 || offsetIndex >= offsets.Length)
                        throw new InvalidDataException(
                            "GTFS file offset index is outside its table");
                    uint packed = offsets[offsetIndex];
                    pending.Add(new PendingGtfsEntry {
                        Name = name,
                        Offset = packed & ~((long)CookedSize - 1L),
                        TailBytes = checked((int)(packed & (CookedSize - 1)))
                    });
                }

                if ((flags & 0x80) != 0)
                {
                    currentDirectory = directoryIndex < directories.Count
                        ? directories[directoryIndex]
                        : "";
                    directoryIndex++;
                    directoryInsertIndex = directoryIndex;
                }
            }

            for (int index = 0; index < pending.Count; index++)
            {
                PendingGtfsEntry entry = pending[index];
                long nextOffset = index + 1 < pending.Count
                    ? pending[index + 1].Offset
                    : archiveSize;
                long size = Math.Max(
                    0L, nextOffset - entry.Offset - entry.TailBytes);
                if (size == 0)
                    size = CookedSize;
                if (entry.Name.Equals(target, StringComparison.Ordinal))
                {
                    if (size > Int32.MaxValue)
                        throw new InvalidDataException(
                            "GTFS member is too large: " + target);
                    targetOffset = entry.Offset;
                    targetSize = checked((int)size);
                    return;
                }
            }
        }
        throw new InvalidDataException("GTFS member is missing: " + target);
    }

    static string Sha256(byte[] data)
    {
        using (SHA256 sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "");
    }

    static string ReadAsciiZ(byte[] data, int offset, int count)
    {
        int length = 0;
        while (length < count && data[offset + length] != 0)
            length++;
        return Encoding.ASCII.GetString(data, offset, length);
    }

    static int FindUnique(byte[] haystack, byte[] needle)
    {
        int found = -1;
        for (int index = 0; index <= haystack.Length - needle.Length; index++)
        {
            int candidate = 0;
            while (candidate < needle.Length &&
                   haystack[index + candidate] == needle[candidate])
                candidate++;
            if (candidate != needle.Length)
                continue;
            if (found >= 0)
                throw new InvalidDataException(
                    "ISO metadata contains a duplicate record name: " +
                    Encoding.ASCII.GetString(needle));
            found = index;
        }
        return found;
    }

    static void ReadExactly(
        Stream stream,
        byte[] destination,
        int offset,
        int count)
    {
        int read = 0;
        while (read < count)
        {
            int current = stream.Read(destination, offset + read, count - read);
            if (current == 0)
                throw new EndOfStreamException("Short source read");
            read += current;
        }
    }

    static ushort ReadU16Le(byte[] data, int offset)
    {
        return (ushort)(data[offset] | data[offset + 1] << 8);
    }

    static uint ReadU32Le(byte[] data, int offset)
    {
        return (uint)(
            data[offset] |
            data[offset + 1] << 8 |
            data[offset + 2] << 16 |
            data[offset + 3] << 24);
    }

    static void WriteU32Le(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    static void WriteU32Be(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    static void Replace(string temporary, string destination)
    {
        DeleteIfPresent(destination);
        File.Move(temporary, destination);
    }

    static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
'@

function Install-ExtractedFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Image,
        [Parameter(Mandatory = $true)]
        [string]$Destination,
        [Parameter(Mandatory = $true)]
        [long]$Lba,
        [Parameter(Mandatory = $true)]
        [long]$LogicalSize,
        [Parameter(Mandatory = $true)]
        [long]$StoredSize,
        [Parameter(Mandatory = $true)]
        [string]$Hash,
        [switch]$Raw,
        [switch]$AlwaysExtract
    )

    $valid = $false
    if (-not $AlwaysExtract -and
        (Test-Path -LiteralPath $Destination -PathType Leaf)) {
        $existing = Get-Item -LiteralPath $Destination
        $valid = $existing.Length -eq $StoredSize -and
            [OpenGtUnifiedDiscInstaller]::Sha256($Destination) -eq $Hash
    }
    if ($valid) {
        Write-Output "Already valid: $Destination"
        return
    }

    Write-Output "Extracting $Destination..."
    [OpenGtUnifiedDiscInstaller]::Extract(
        $Image,
        $Destination,
        $Lba,
        $LogicalSize,
        [bool]$Raw)
    $actualFile = Get-Item -LiteralPath $Destination
    $actualHash = [OpenGtUnifiedDiscInstaller]::Sha256($Destination)
    if ($actualFile.Length -ne $StoredSize -or $actualHash -ne $Hash) {
        throw (
            "Extracted file failed validation: $Destination. " +
            "Expected $StoredSize bytes / $Hash; found " +
            "$($actualFile.Length) bytes / $actualHash."
        )
    }
}

$simulation = Join-Path $install 'simulation'
$arcade = Join-Path $install 'arcade'
$manifests = Join-Path $install 'manifests'
New-Item -ItemType Directory -Path $simulation -Force | Out-Null
New-Item -ItemType Directory -Path $arcade -Force | Out-Null
if (-not (Test-Path -LiteralPath (Join-Path $manifests 'simulation.json') -PathType Leaf) -or
    -not (Test-Path -LiteralPath (Join-Path $manifests 'arcade.json') -PathType Leaf)) {
    throw 'Release package is missing its bounded unified manifests'
}

Install-ExtractedFile -Image $simulationImage `
    -Destination (Join-Path $simulation 'DISC_META.DAT') `
    -Lba 16 -LogicalSize 16384 -StoredSize 18688 `
    -Hash '79C869A7B66685B1EC4618B3B0A83DFD3AE59B994741BE74179D092E4A4370D6' `
    -Raw
Install-ExtractedFile -Image $simulationImage `
    -Destination (Join-Path $simulation 'SYSTEM.CNF') `
    -Lba 23 -LogicalSize 68 -StoredSize 68 `
    -Hash '667AA36661AAA5F514C851DFB3D7ECD847D7CF4B6911EC0523A63C515779EF7F'
Install-ExtractedFile -Image $simulationImage `
    -Destination (Join-Path $simulation 'SCUS_944.88') `
    -Lba 24 -LogicalSize 628736 -StoredSize 628736 `
    -Hash '4DD40D01A3E83967E2D4301106890EB314D72027802BEE077BBBC246F152E331'
Install-ExtractedFile -Image $simulationImage `
    -Destination (Join-Path $simulation 'GT2.OVL') `
    -Lba 331 -LogicalSize 289752 -StoredSize 289752 `
    -Hash 'F8C6B8D94B5A5744E97B626CEF1DB247D49A1FC195A7615031F8468B6A86B074'
Install-ExtractedFile -Image $simulationImage `
    -Destination (Join-Path $simulation 'FAULTY.PSX') `
    -Lba 280504 -LogicalSize 27648000 -StoredSize 31536000 `
    -Hash '4D2C78C7430DB9A6329A454E8C736D488634E921B203A0EA889FCAF2BAC0FF6E' `
    -Raw

Install-ExtractedFile -Image $arcadeImage `
    -Destination (Join-Path $arcade 'DISC_META.DAT') `
    -Lba 16 -LogicalSize 16384 -StoredSize 18688 `
    -Hash 'E9F562BB067DC849F14400A0406C1F598AE4221A488C294E3E97CA1B38580617' `
    -Raw -AlwaysExtract
Install-ExtractedFile -Image $arcadeImage `
    -Destination (Join-Path $arcade 'SYSTEM.CNF') `
    -Lba 23 -LogicalSize 68 -StoredSize 68 `
    -Hash '679D41C5B44C08A1BCF7D7705FE7A64831502269163C11B83FE37E0511475BCC'
Install-ExtractedFile -Image $arcadeImage `
    -Destination (Join-Path $arcade 'SCUS_944.55') `
    -Lba 24 -LogicalSize 628736 -StoredSize 628736 `
    -Hash '67782AE7520105B39BD1716D910332240D0F11171DABFD04518F8AA04E1CA519'
Install-ExtractedFile -Image $arcadeImage `
    -Destination (Join-Path $arcade 'GT2.OVL') `
    -Lba 331 -LogicalSize 287088 -StoredSize 287088 `
    -Hash '97CEC9C847923EE1843BCF422A1DC50547CD2878884735DAE1C9E32B2A866556'
Install-ExtractedFile -Image $arcadeImage `
    -Destination (Join-Path $arcade 'STREAM.DAT') `
    -Lba 146399 -LogicalSize 335011840 -StoredSize 382122880 `
    -Hash 'B99CA3F8FCED81507C341F2D189283FFB62CCD754597E3641F45172C21E6F717' `
    -Raw

$music = Join-Path $install 'MUSIC.DAT'
Install-ExtractedFile -Image $simulationImage `
    -Destination $music `
    -Lba 238872 -LogicalSize 85262336 -StoredSize 97252352 `
    -Hash '2D1B7A30F656900213FA1F4AFF9371C22DB9D7F41AE8A9E5C5D51DE8FFC9E102' `
    -Raw

$unifiedVolume = Join-Path $install 'GT2.VOL'
$unifiedVolumeHash =
    '7C3BF68061E5867DE5AF831121C50091128DDBDE4F13026A050D3C71EF0EEE53'
$unifiedValid = (Test-Path -LiteralPath $unifiedVolume -PathType Leaf) -and
    (Get-Item -LiteralPath $unifiedVolume).Length -eq 701837312L -and
    [OpenGtUnifiedDiscInstaller]::Sha256($unifiedVolume) -eq $unifiedVolumeHash
if ($unifiedValid) {
    Write-Output 'Already valid: unified GT2.VOL'
} else {
    Write-Output 'Extracting byte-exact Simulation and Arcade GT2.VOL members...'
    [OpenGtUnifiedDiscInstaller]::ExtractUnifiedVolumes(
        $simulationImage,
        $arcadeImage,
        $unifiedVolume)
    $actualUnifiedHash = [OpenGtUnifiedDiscInstaller]::Sha256($unifiedVolume)
    if ((Get-Item -LiteralPath $unifiedVolume).Length -ne 701837312L -or
        $actualUnifiedHash -ne $unifiedVolumeHash) {
        throw (
            'Unified GT2.VOL failed validation. Expected ' +
            "$unifiedVolumeHash; found $actualUnifiedHash."
        )
    }
}

[OpenGtUnifiedDiscInstaller]::PatchIsoRecord(
    (Join-Path $simulation 'DISC_META.DAT'),
    'GT2.VOL;1',
    473,
    488241152)
[OpenGtUnifiedDiscInstaller]::PatchIsoRecord(
    (Join-Path $arcade 'DISC_META.DAT'),
    'GT2.VOL;1',
    473,
    213596160)
[OpenGtUnifiedDiscInstaller]::PatchIsoRecord(
    (Join-Path $arcade 'DISC_META.DAT'),
    'MUSIC.DAT;1',
    238872,
    85262336)
[OpenGtUnifiedDiscInstaller]::PatchIsoRecord(
    (Join-Path $arcade 'DISC_META.DAT'),
    'STREAM.DAT;1',
    280504,
    335011840)
$arcadeMetadataHash = [OpenGtUnifiedDiscInstaller]::Sha256(
    (Join-Path $arcade 'DISC_META.DAT'))
if ($arcadeMetadataHash -ne
    'BEF591A382F4DCEC1990F5DB01B43CD42ED9CBDFE504BCB47E3FDB4013495A0E') {
    throw "Relocated Arcade metadata failed validation: $arcadeMetadataHash"
}

$titleAsset = Join-Path $install 'TITLE_EXACT.DAT'
Write-Output 'Deriving the pixel-exact unified GT2 title from GT2.VOL...'
[OpenGtUnifiedDiscInstaller]::BuildTitleAsset($unifiedVolume, $titleAsset)
$titleHash = [OpenGtUnifiedDiscInstaller]::Sha256($titleAsset)
if ($titleHash -ne
    '735D838C3A0F12E2917593648790F9FD1CB6ADA13D402E19022D7C814737321C') {
    throw "Derived unified title failed validation: $titleHash"
}

New-Item -ItemType Directory -Path (Join-Path $install 'music') -Force |
    Out-Null
New-Item -ItemType Directory -Path (Join-Path $install 'mods') -Force |
    Out-Null

Write-Output ''
Write-Output 'Installation complete.'
Write-Output 'Both authoritative NTSC-U GT2 programs and data sets are installed.'
Write-Output 'Run GranTurismo2PC.exe from this folder.'
