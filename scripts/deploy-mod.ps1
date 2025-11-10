# PowerShell script to deploy built mod to BepInEx plugins folder

param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint",
    [switch]$Build = $true
)

$ErrorActionPreference = "Stop"

Write-Host "=== Mod Deployment Script ===" -ForegroundColor Cyan
Write-Host ""

$projectDir = Join-Path $PSScriptRoot "..\CKFlashpointAccessibility"
$buildOutput = Join-Path $projectDir "bin\Debug\net6.0"
$modsDir = Join-Path $GamePath "Mods"

# Build if requested
if ($Build) {
    Write-Host "Building mod..." -ForegroundColor Cyan
    Push-Location $projectDir
    
    try {
        $buildResult = dotnet build --configuration Debug 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Build successful" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Build failed!" -ForegroundColor Red
            Write-Host $buildResult
            Pop-Location
            exit 1
        }
    } catch {
        Write-Host "ERROR: Build failed: $_" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Pop-Location
} else {
    Write-Host "Skipping build (use -Build to build before deploying)" -ForegroundColor Yellow
}

Write-Host ""

# Check build output
if (-not (Test-Path $buildOutput)) {
    Write-Host "ERROR: Build output not found: $buildOutput" -ForegroundColor Red
    Write-Host "Run 'dotnet build' in the project directory first" -ForegroundColor Yellow
    exit 1
}

$modDll = Join-Path $buildOutput "CKFlashpointAccessibility.dll"
if (-not (Test-Path $modDll)) {
    Write-Host "ERROR: Mod DLL not found: $modDll" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Found mod DLL: $modDll" -ForegroundColor Green

# Check MelonLoader mods folder
$melonDir = Join-Path $GamePath "MelonLoader"
if (-not (Test-Path $melonDir)) {
    Write-Host "ERROR: MelonLoader not found at: $melonDir" -ForegroundColor Red
    Write-Host "Run .\setup-melonloader.ps1 first!" -ForegroundColor Yellow
    exit 1
}

# Create Mods directory
if (-not (Test-Path $modsDir)) {
    New-Item -ItemType Directory -Path $modsDir | Out-Null
    Write-Host "✓ Created Mods directory" -ForegroundColor Green
} else {
    Write-Host "✓ Mods directory exists" -ForegroundColor Green
}

# Copy files
Write-Host ""
Write-Host "Deploying mod files..." -ForegroundColor Cyan

$filesToCopy = @(
    "CKFlashpointAccessibility.dll",
    "SRAL.dll",
    "nvdaControllerClient64.dll"
)

$copiedCount = 0
foreach ($file in $filesToCopy) {
    $source = Join-Path $buildOutput $file
    if (Test-Path $source) {
        Copy-Item -Path $source -Destination $modsDir -Force
        Write-Host "  ✓ Copied: $file" -ForegroundColor Green
        $copiedCount++
    } else {
        if ($file -eq "CKFlashpointAccessibility.dll") {
            Write-Host "  ERROR: Required file missing: $file" -ForegroundColor Red
        } else {
            Write-Host "  ⚠ Optional file not found: $file" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
if ($copiedCount -gt 0) {
    Write-Host "=== Deployment Complete ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deployed to: $modsDir" -ForegroundColor Yellow
    Write-Host "Files copied: $copiedCount" -ForegroundColor White
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Launch Cyber Knights: Flashpoint via Steam"
    Write-Host "2. Check MelonLoader\Latest.log for mod loading messages"
    Write-Host "3. Screen reader should announce 'Mod loaded successfully!'"
    Write-Host ""
} else {
    Write-Host "ERROR: No files were copied!" -ForegroundColor Red
    exit 1
}
