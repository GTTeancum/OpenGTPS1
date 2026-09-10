$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

$runtimeProject = Get-Content -LiteralPath (
    Join-Path $repo 'vendor\RecompOne\RecompOne.Runtime\RecompOne.Runtime.csproj') -Raw
$hostWindow = Get-Content -LiteralPath (
    Join-Path $repo 'vendor\RecompOne\RecompOne.Runtime\Host\Window\HostWindow.cs') -Raw
$presentation = Get-Content -LiteralPath (
    Join-Path $repo 'vendor\RecompOne\RecompOne.Runtime\Host\Window\PresentationRenderer.cs') -Raw
$compositor = Get-Content -LiteralPath (
    Join-Path $repo 'vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\D3D11GpuBackend.cs') -Raw
$selector = Get-Content -LiteralPath (
    Join-Path $repo 'tests\fixtures\unified-arcade-ssr11-selector.input') -Raw

Require ($runtimeProject.Contains('Vortice.Direct3D11')) 'D3D11 package is missing.'
Require ($runtimeProject.Contains('Vortice.DXGI')) 'DXGI package is missing.'
Require ($runtimeProject.Contains('ImGui.NET')) 'Direct ImGui package is missing.'
Require (-not $runtimeProject.Contains('Silk.NET.OpenGL')) 'OpenGL package remains.'
Require ($hostWindow.Contains('GraphicsAPI.None')) 'Silk still creates a graphics API.'
Require ($hostWindow.Contains('new D3D11Renderer')) 'D3D11 host device is missing.'
Require ($hostWindow.Contains('new Hle.D3D11GpuBackend')) 'D3D11 compositor is missing.'
Require ($hostWindow.Contains('new D3D11ImGuiController')) 'D3D11 ImGui path is missing.'
Require ($hostWindow.Contains('d3d.EndFrame(present: false)')) 'Silent D3D11 submission flush is missing.'
Require ($presentation.Contains('api=D3D11')) 'D3D11 presentation identity is missing.'
Require ($compositor.Contains('WritebackFeedbackRegion')) 'Texture feedback barrier is missing.'
Require ($compositor.Contains('BlendOperation.ReverseSubtract')) 'PS1 reverse-subtract blend is missing.'
Require ($compositor.Contains('MapMode.WriteNoOverwrite')) 'D3D11 compositor vertex ring is missing.'
Require ($compositor.Contains('_vertexBufferCursor')) 'D3D11 compositor vertex-ring cursor is missing.'
Require ($selector.Contains('10000+1=CAPTURE')) 'Settled SSR11 selector capture is missing.'

$legacyFiles = @(
    'GlBackend.cs',
    'GlDisplayRt.cs',
    'GlShaders.cs',
    'GlVram.cs'
)
foreach ($name in $legacyFiles) {
    Require (-not (Test-Path -LiteralPath (
        Join-Path $repo "vendor\RecompOne\RecompOne.Runtime\Gpu\Hle\Gl\$name"))) (
        "Legacy OpenGL source remains: $name")
}

$shippingText = Get-ChildItem -LiteralPath (
    Join-Path $repo 'vendor\RecompOne\RecompOne.Runtime') -Recurse -File |
    Where-Object { $_.Extension -in '.cs', '.csproj' } |
    Select-String -Pattern 'Silk\.NET\.OpenGL|GlBackend|GlDisplayRt|GlShaders|GlVram|OpenGL'
Require ($null -eq $shippingText) 'Shipping runtime still references OpenGL.'

Write-Output 'directx_presentation=true api=D3D11 dxgi=true authored_2d=true imgui=true opengl=false ssr11_fixture=true'
