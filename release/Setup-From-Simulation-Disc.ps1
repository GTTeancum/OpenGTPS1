param(
    [Parameter(Mandatory = $true)]
    [string]$ImagePath
)

$ErrorActionPreference = 'Stop'
$expectedImageBytes = 691850208L
$expectedImageHash =
    'D0AB6E70539601057590A36299543C0ADAD219254D712F7D4273219094ED5031'
$install = $PSScriptRoot
$image = (Resolve-Path -LiteralPath $ImagePath).Path

if (-not (Test-Path -LiteralPath $image -PathType Leaf)) {
    throw "Disc image not found: $ImagePath"
}

$imageFile = Get-Item -LiteralPath $image
if ($imageFile.Length -ne $expectedImageBytes) {
    throw (
        "Disc image size mismatch. OpenGTPS1 0.8beta supports only the US " +
        "Gran Turismo 2 Simulation Disc, SCUS-94488 NTSC-U revision 2. " +
        "Expected $expectedImageBytes bytes; found $($imageFile.Length)."
    )
}

Write-Output 'Validating the complete Simulation Disc image...'
$actualImageHash = (Get-FileHash -LiteralPath $image -Algorithm SHA256).Hash
if ($actualImageHash -ne $expectedImageHash) {
    throw (
        "Disc image SHA-256 mismatch. OpenGTPS1 0.8beta supports only the US " +
        "Gran Turismo 2 Simulation Disc, SCUS-94488 NTSC-U revision 2. " +
        "Expected $expectedImageHash; found $actualImageHash."
    )
}

Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Security.Cryptography;

public static class OpenGtDiscExtractor
{
    const int RawSectorSize = 2352;
    const int CookedOffset = 24;
    const int CookedSize = 2048;
    const int RawPayloadOffset = 16;
    const int RawPayloadSize = 2336;

    public static void ExtractCooked(
        string imagePath, string destination, long lba, long logicalSize)
    {
        Extract(imagePath, destination, lba, logicalSize, false);
    }

    public static void ExtractRaw(
        string imagePath, string destination, long lba, long logicalSize)
    {
        Extract(imagePath, destination, lba, logicalSize, true);
    }

    static void Extract(
        string imagePath,
        string destination,
        long lba,
        long logicalSize,
        bool rawPayload)
    {
        string temporary = destination + ".tmp";
        if (File.Exists(temporary))
            File.Delete(temporary);

        int sourceOffset = rawPayload ? RawPayloadOffset : CookedOffset;
        int outputPerSector = rawPayload ? RawPayloadSize : CookedSize;
        long sectors = (logicalSize + CookedSize - 1) / CookedSize;
        byte[] sector = new byte[RawSectorSize];

        try
        {
            using (FileStream source = new FileStream(
                imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream output = new FileStream(
                temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                source.Position = lba * RawSectorSize;
                long logicalRemaining = logicalSize;
                for (long index = 0; index < sectors; index++)
                {
                    int read = 0;
                    while (read < sector.Length)
                    {
                        int count = source.Read(
                            sector, read, sector.Length - read);
                        if (count == 0)
                            throw new EndOfStreamException(
                                "Short raw sector read at LBA " + (lba + index));
                        read += count;
                    }

                    int writeCount = rawPayload
                        ? RawPayloadSize
                        : (int)Math.Min(CookedSize, logicalRemaining);
                    output.Write(sector, sourceOffset, writeCount);
                    logicalRemaining -= Math.Min(CookedSize, logicalRemaining);
                }
                output.Flush(true);
            }

            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(temporary, destination);
        }
        catch
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
            throw;
        }
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
}
'@

$files = @(
    @{
        Name = 'DISC_META.DAT'; Lba = 16L; Size = 16384L; Raw = $true
        Hash = '79C869A7B66685B1EC4618B3B0A83DFD3AE59B994741BE74179D092E4A4370D6'
    },
    @{
        Name = 'SYSTEM.CNF'; Lba = 23L; Size = 68L; Raw = $false
        Hash = '667AA36661AAA5F514C851DFB3D7ECD847D7CF4B6911EC0523A63C515779EF7F'
    },
    @{
        Name = 'SCUS_944.88'; Lba = 24L; Size = 628736L; Raw = $false
        Hash = '4DD40D01A3E83967E2D4301106890EB314D72027802BEE077BBBC246F152E331'
    },
    @{
        Name = 'GT2.OVL'; Lba = 331L; Size = 289752L; Raw = $false
        Hash = 'F8C6B8D94B5A5744E97B626CEF1DB247D49A1FC195A7615031F8468B6A86B074'
    },
    @{
        Name = 'GT2.VOL'; Lba = 473L; Size = 488241152L; Raw = $false
        Hash = '9630AAD04CABF50AD702A3DBBCE77069153748385BCFFC02015797A0C708EEDD'
    },
    @{
        Name = 'MUSIC.DAT'; Lba = 238872L; Size = 85262336L; Raw = $true
        Hash = '2D1B7A30F656900213FA1F4AFF9371C22DB9D7F41AE8A9E5C5D51DE8FFC9E102'
    },
    @{
        Name = 'FAULTY.PSX'; Lba = 280504L; Size = 27648000L; Raw = $true
        Hash = '4D2C78C7430DB9A6329A454E8C736D488634E921B203A0EA889FCAF2BAC0FF6E'
    }
)

foreach ($file in $files) {
    $destination = Join-Path $install $file.Name
    if (Test-Path -LiteralPath $destination -PathType Leaf) {
        $existingHash = [OpenGtDiscExtractor]::Sha256($destination)
        if ([string]::Equals(
                [string]$existingHash,
                [string]$file.Hash,
                [StringComparison]::OrdinalIgnoreCase)) {
            Write-Output "Already valid: $($file.Name)"
            continue
        }
    }

    Write-Output "Extracting $($file.Name)..."
    if ($file.Raw) {
        [OpenGtDiscExtractor]::ExtractRaw(
            $image, $destination, $file.Lba, $file.Size)
    } else {
        [OpenGtDiscExtractor]::ExtractCooked(
            $image, $destination, $file.Lba, $file.Size)
    }

    $actualHash = [OpenGtDiscExtractor]::Sha256($destination)
    if (-not [string]::Equals(
            [string]$actualHash,
            [string]$file.Hash,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw (
            "Extracted file failed validation: $($file.Name). " +
            "Expected $($file.Hash); found $actualHash."
        )
    }
    Write-Output "Validated: $($file.Name)"
}

New-Item -ItemType Directory -Path (Join-Path $install 'music') -Force |
    Out-Null
New-Item -ItemType Directory -Path (Join-Path $install 'mods') -Force |
    Out-Null

Write-Output ''
Write-Output 'Installation complete.'
Write-Output 'Run GranTurismo2PC.exe from this folder.'
