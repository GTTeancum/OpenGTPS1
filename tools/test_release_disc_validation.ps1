param([Parameter(Mandatory = $true)][string]$FixtureRoot)
$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../release/Setup-From-GT2-Discs.ps1') -Raw
$definition = [regex]::Match($source, "(?s)Add-Type -TypeDefinition @'\r?\n(.*?)\r?\n'@").Groups[1].Value
Add-Type -TypeDefinition $definition
$simulation = Join-Path $FixtureRoot 'simulation.bin'
$arcade = Join-Path $FixtureRoot 'arcade.bin'
$sim = [OpenGtUnifiedDiscInstaller]::ReadDisc($simulation, 'SCUS_944.88')
$arc = [OpenGtUnifiedDiscInstaller]::ReadDisc($arcade, 'SCUS_944.55')
foreach ($name in @('pal.bin', 'truncated.bin', 'arcade.bin')) {
    $rejected = $false
    try { [OpenGtUnifiedDiscInstaller]::ReadDisc((Join-Path $FixtureRoot $name), 'SCUS_944.88') | Out-Null }
    catch { $rejected = $true }
    if (-not $rejected) { throw "Incorrectly accepted $name" }
}
$vol = $sim['GT2.VOL']
[OpenGtUnifiedDiscInstaller]::Extract($simulation, (Join-Path $FixtureRoot 'extracted.vol'), $vol.Lba, $vol.Size, $false)
[OpenGtUnifiedDiscInstaller]::ExtractUnifiedVolumes($simulation, $arcade, (Join-Path $FixtureRoot 'unified.vol'),
    $vol.Lba, $vol.Size, $arc['GT2.VOL'].Lba, $arc['GT2.VOL'].Size)
Write-Output 'PowerShell disc identification and ISO-based extraction passed.'
