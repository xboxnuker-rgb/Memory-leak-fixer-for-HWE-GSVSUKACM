[CmdletBinding()]
param(
    [string] $MelonLoaderRoot = "C:\Program Files (x86)\Steam\steamapps\common\Schedule I\MelonLoader",
    [string] $DotNet = "dotnet"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "build.ps1") -MelonLoaderRoot $MelonLoaderRoot -DotNet $DotNet
if ($LASTEXITCODE) { exit $LASTEXITCODE }

$version = "1.1.1"
$staging = Join-Path $repositoryRoot "dist\StorageMaterialLeakFix-v$version"
$zipPath = "$staging.zip"
$modsDirectory = Join-Path $staging "Mods"

if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Path $modsDirectory -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot "bin\Release\StorageMaterialLeakFix.dll") -Destination $modsDirectory
Copy-Item -LiteralPath (Join-Path $repositoryRoot "README.md") -Destination $staging
Copy-Item -LiteralPath (Join-Path $repositoryRoot "CHANGELOG.md") -Destination $staging
Copy-Item -LiteralPath (Join-Path $repositoryRoot "manifest.json") -Destination $staging
Copy-Item -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Destination $staging

Compress-Archive -Path (Join-Path $staging "*") -DestinationPath $zipPath -CompressionLevel Optimal
Write-Output "Packaged $zipPath"
