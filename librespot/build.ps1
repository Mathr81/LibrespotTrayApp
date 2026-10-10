<#
.SYNOPSIS
    Compiles librespot.exe for LibrespotTrayApp (see librespot/README.md).

.DESCRIPTION
    Downloads the pinned librespot release and the cpal crate, applies the patches from
    librespot/patches, and builds an optimised, statically linked librespot.exe into
    librespot/bin. Requires git, Rust (rustup) and the Visual Studio C++ build tools.

.PARAMETER OutDir
    Where to put librespot.exe (default: librespot/bin).
#>
[CmdletBinding()]
param(
    [string]$OutDir = (Join-Path $PSScriptRoot 'bin')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Pinned versions: update them together with the patches.
$LibrespotTag = 'v0.8.0'
$CpalVersion = '0.16.0'
$CpalSha256 = 'cbd307f43cc2a697e2d1f8bc7a1d824b5269e052209e28883e5bc04d095aaa3f'
$Target = 'x86_64-pc-windows-msvc'

$WorkDir = Join-Path $PSScriptRoot '.build'
$PatchDir = Join-Path $PSScriptRoot 'patches'
$LibrespotDir = Join-Path $WorkDir 'librespot'
$CpalDir = Join-Path $WorkDir 'cpal'

function Invoke-Native {
    param([Parameter(Mandatory)][string]$Command, [string[]]$Arguments = @())
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "'$Command $($Arguments -join ' ')' a échoué (code $LASTEXITCODE)."
    }
}

if (Test-Path $WorkDir) {
    Remove-Item -Recurse -Force $WorkDir
}
New-Item -ItemType Directory -Force -Path $WorkDir | Out-Null

Write-Host "==> librespot $LibrespotTag"
# autocrlf must stay off, or the patches would not apply on Windows.
Invoke-Native git @('-c', 'core.autocrlf=false', 'clone', '--quiet', '--depth', '1', '--branch', $LibrespotTag,
    'https://github.com/librespot-org/librespot.git', $LibrespotDir)
Invoke-Native git @('-C', $LibrespotDir, '-c', 'core.autocrlf=false', 'apply', '--whitespace=nowarn',
    (Join-Path $PatchDir 'librespot.patch'))

Write-Host "==> cpal $CpalVersion"
$CpalCrate = Join-Path $WorkDir "cpal-$CpalVersion.crate"
Invoke-WebRequest -UseBasicParsing -Uri "https://static.crates.io/crates/cpal/cpal-$CpalVersion.crate" -OutFile $CpalCrate
$ActualSha256 = (Get-FileHash -Algorithm SHA256 $CpalCrate).Hash.ToLowerInvariant()
if ($ActualSha256 -ne $CpalSha256) {
    throw "Somme de contrôle inattendue pour cpal-$CpalVersion.crate : $ActualSha256"
}
# Relative paths only: GNU tar (from Git for Windows) takes "C:" for a remote host.
Push-Location $WorkDir
try {
    Invoke-Native tar @('-xzf', "cpal-$CpalVersion.crate")
}
finally {
    Pop-Location
}
Rename-Item (Join-Path $WorkDir "cpal-$CpalVersion") $CpalDir
# Patched as a standalone repository so that git does not apply it relative to an enclosing one.
Invoke-Native git @('-C', $CpalDir, 'init', '--quiet')
Invoke-Native git @('-C', $CpalDir, '-c', 'core.autocrlf=false', 'apply', '--whitespace=nowarn',
    (Join-Path $PatchDir 'cpal.patch'))

Write-Host "==> cargo build ($Target)"
# Statically linked C runtime (no Visual C++ redistributable needed) and whole-program optimisation.
$env:RUSTFLAGS = '-C target-feature=+crt-static'
$env:CARGO_PROFILE_RELEASE_LTO = 'fat'
$env:CARGO_PROFILE_RELEASE_CODEGEN_UNITS = '1'
$env:CARGO_PROFILE_RELEASE_STRIP = 'symbols'
Invoke-Native rustup @('target', 'add', $Target)
Invoke-Native cargo @('build', '--release', '--locked', '--target', $Target,
    '--manifest-path', (Join-Path $LibrespotDir 'Cargo.toml'))

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$Exe = Join-Path $LibrespotDir "target/$Target/release/librespot.exe"
Copy-Item $Exe (Join-Path $OutDir 'librespot.exe') -Force
Write-Host "==> $(Join-Path $OutDir 'librespot.exe')"
