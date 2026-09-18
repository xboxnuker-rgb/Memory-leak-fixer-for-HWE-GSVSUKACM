[CmdletBinding()]
param(
    [string] $MelonLoaderRoot = "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader",
    [string] $DotNet = "dotnet"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot "StorageMaterialLeakFix.csproj"

if (-not (Test-Path -LiteralPath $MelonLoaderRoot -PathType Container)) {
    throw "MelonLoader directory not found: $MelonLoaderRoot"
}

& $DotNet build $project `
    --configuration Release `
    --property:MelonLoaderRoot="$MelonLoaderRoot" `
    --nologo

if ($LASTEXITCODE) { exit $LASTEXITCODE }

$output = Join-Path $repositoryRoot "bin\Release\StorageMaterialLeakFix.dll"
if (-not (Test-Path -LiteralPath $output -PathType Leaf)) {
    throw "Build completed without the expected output: $output"
}

Write-Output "Built $output"

