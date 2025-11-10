# PowerShell script to set up MelonLoader for Cyber Knights: Flashpoint
# Run this AFTER installing the game from Steam

param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint"
)

$ErrorActionPreference = "Stop"

Write-Host "=== MelonLoader Setup Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if game path exists
if (-not (Test-Path $GamePath)) {
    Write-Host "ERROR: Game path not found: $GamePath" -ForegroundColor Red
    Write-Host "Please specify the correct path using: .\setup-bepinex.ps1 -GamePath 'C:\Your\Game\Path'" -ForegroundColor Yellow
    exit 1
}

# Find game executable
$gameExe = Get-ChildItem -Path $GamePath -Filter "*.exe" | Where-Object { $_.Name -notlike "UnityCrashHandler*" -and $_.Name -notlike "UnityPlayer*" } | Select-Object -First 1

if (-not $gameExe) {
    Write-Host "ERROR: Could not find game executable in $GamePath" -ForegroundColor Red
    exit 1
}

Write-Host "Found game: $($gameExe.Name)" -ForegroundColor Green
Write-Host "Game path: $GamePath" -ForegroundColor Green
Write-Host ""

# Check for IL2CPP indicators
$gameAssembly = Join-Path $GamePath "GameAssembly.dll"
$il2cppData = Join-Path $GamePath "Cyber_Knights__Flashpoint_Data\il2cpp_data\Metadata\global-metadata.dat"

if (Test-Path $gameAssembly) {
    Write-Host "✓ GameAssembly.dll found - IL2CPP confirmed" -ForegroundColor Green
} else {
    Write-Host "⚠ GameAssembly.dll not found - may not be IL2CPP" -ForegroundColor Yellow
}

if (Test-Path $il2cppData) {
    Write-Host "✓ global-metadata.dat found - IL2CPP confirmed" -ForegroundColor Green
} else {
    Write-Host "⚠ global-metadata.dat not found" -ForegroundColor Yellow
}

Write-Host ""

# Download MelonLoader
$melonVersion = "0.6.1"
$melonUrl = "https://github.com/LavaGang/MelonLoader/releases/latest/download/MelonLoader.x64.zip"
$melonZip = Join-Path $PSScriptRoot "..\tools\MelonLoader.zip"
$toolsDir = Join-Path $PSScriptRoot "..\tools"

if (-not (Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir | Out-Null
}

if (-not (Test-Path $melonZip)) {
    Write-Host "Downloading MelonLoader..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $melonUrl -OutFile $melonZip -UseBasicParsing
        Write-Host "✓ Downloaded MelonLoader" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to download MelonLoader: $_" -ForegroundColor Red
        Write-Host "Please download manually from: https://github.com/LavaGang/MelonLoader/releases" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✓ MelonLoader already downloaded" -ForegroundColor Green
}

# Extract MelonLoader to game folder
Write-Host "Extracting MelonLoader to game folder..." -ForegroundColor Cyan
try {
    Expand-Archive -Path $melonZip -DestinationPath $GamePath -Force
    Write-Host "✓ MelonLoader extracted successfully" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Failed to extract MelonLoader: $_" -ForegroundColor Red
    exit 1
}

# Check installation
$melonFolder = Join-Path $GamePath "MelonLoader"
if (Test-Path $melonFolder) {
    Write-Host "✓ MelonLoader folder created" -ForegroundColor Green
} else {
    Write-Host "ERROR: MelonLoader folder not found after extraction" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== MelonLoader Installation Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Launch the game ONCE via Steam to generate MelonLoader files"
Write-Host "2. Check for MelonLoader\Latest.log in the game folder"
Write-Host "3. MelonLoader will automatically generate managed assemblies"
Write-Host "4. Run .\update-references.ps1 to configure the mod project"
Write-Host ""
Write-Host "MelonLoader folder: $melonFolder" -ForegroundColor Yellow
