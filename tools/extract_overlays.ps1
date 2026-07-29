param(
    [string]$InputPath = (Join-Path $PSScriptRoot '..\work\disc\GT2.OVL'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\work\overlays')
)

$ErrorActionPreference = 'Stop'
$inputFile = (Resolve-Path -LiteralPath $InputPath).Path
$raw = [IO.File]::ReadAllBytes($inputFile)

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

for ($index = 0; $index -lt 6; $index++) {
    $offset = [BitConverter]::ToUInt32($raw, $index * 8)
    $packedSize = [BitConverter]::ToUInt32($raw, $index * 8 + 4)
    if ($offset + $packedSize -gt $raw.Length) {
        throw "GT2.OVL entry $index exceeds the container"
    }

    $input = New-Object IO.MemoryStream(,$raw[$offset..($offset + $packedSize - 1)])
    $gzip = New-Object IO.Compression.GZipStream(
        $input,
        [IO.Compression.CompressionMode]::Decompress
    )
    $output = New-Object IO.MemoryStream
    try {
        $gzip.CopyTo($output)
        $path = Join-Path $OutputDirectory ('overlay_{0}.bin' -f $index)
        [IO.File]::WriteAllBytes($path, $output.ToArray())
        Write-Output (
            'entry={0} offset=0x{1:X} packed={2} unpacked={3} path={4}' -f
            $index, $offset, $packedSize, $output.Length, $path
        )
    }
    finally {
        $gzip.Dispose()
        $input.Dispose()
        $output.Dispose()
    }
}
