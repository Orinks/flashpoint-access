#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Install UnityExplorer for MelonLoader to debug Cyber Knights: Flashpoint UI
.DESCRIPTION
    Downloads and installs UnityExplorer MelonLoader plugin for runtime UI component inspection.
    This helps debug text extraction issues by showing all components on UI elements.
.PARAMETER GamePath
    Path to game installation (auto-detects Steam default)
.EXAMPLE
    .\install-unityexplorer.ps1
    .\install-unityexplorer.ps1 -GamePath "D:\Games\Cyber Knights Flashpoint"
#>

param(
    [string]$GamePath = ""
)

$ErrorActionPreference = "Stop"

Write-Host "=== UnityExplorer Installation for MelonLoader ===" -ForegroundColor Cyan

# Find game installation
if ([string]::IsNullOrEmpty($GamePath)) {
    $steamPaths = @(
        "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint",
        "C:\Program Files\Steam\steamapps\common\Cyber Knights Flashpoint",
        "$env:ProgramFiles\Steam\steamapps\common\Cyber Knights Flashpoint"
    )
    
    foreach ($path in $steamPaths) {
        if (Test-Path $path) {
            $GamePath = $path
            Write-Host "Found game at: $GamePath" -ForegroundColor Green
            break
        }
    }
    
    if ([string]::IsNullOrEmpty($GamePath)) {
        Write-Host "Error: Game not found. Please specify -GamePath" -ForegroundColor Red
        Write-Host "Usage: .\install-unityexplorer.ps1 -GamePath 'C:\Path\To\Game'" -ForegroundColor Yellow
        exit 1
    }
}

# Verify MelonLoader is installed
$modsFolder = Join-Path $GamePath "Mods"
if (-not (Test-Path $modsFolder)) {
    Write-Host "Error: MelonLoader not installed (Mods folder missing)" -ForegroundColor Red
    Write-Host "Run .\setup-melonloader.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Download UnityExplorer for MelonLoader IL2CPP
Write-Host "`nDownloading UnityExplorer..." -ForegroundColor Yellow

$unityExplorerVersion = "4.11.4"  # Latest stable as of 2025
$downloadUrl = "https://github.com/sinai-dev/UnityExplorer/releases/download/$unityExplorerVersion/UnityExplorer.MelonLoader.Il2Cpp.CoreCLR.zip"
$tempZip = Join-Path $env:TEMP "UnityExplorer.zip"

try {
    Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZip -UseBasicParsing
    Write-Host "Downloaded UnityExplorer v$unityExplorerVersion" -ForegroundColor Green
}
catch {
    Write-Host "Error downloading UnityExplorer: $_" -ForegroundColor Red
    Write-Host "Manual download: $downloadUrl" -ForegroundColor Yellow
    exit 1
}

# Extract to Mods folder
Write-Host "Installing to Mods folder..." -ForegroundColor Yellow

try {
    # UnityExplorer zip contains a folder, extract it directly
    Expand-Archive -Path $tempZip -DestinationPath $modsFolder -Force
    
    # Verify installation
    $dllPath = Join-Path $modsFolder "UnityExplorer.ML.IL2CPP.CoreCLR.dll"
    if (Test-Path $dllPath) {
        Write-Host "UnityExplorer installed successfully!" -ForegroundColor Green
    }
    else {
        # Check alternative paths (structure may vary)
        $installedFiles = Get-ChildItem -Path $modsFolder -Recurse -Filter "UnityExplorer*.dll"
        if ($installedFiles.Count -gt 0) {
            Write-Host "UnityExplorer installed: $($installedFiles[0].FullName)" -ForegroundColor Green
        }
        else {
            Write-Host "Warning: Installation unclear, but files extracted to Mods/" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "Error extracting UnityExplorer: $_" -ForegroundColor Red
    exit 1
}
finally {
    Remove-Item $tempZip -Force -ErrorAction SilentlyContinue
}

# Create usage instructions file
$instructionsPath = Join-Path $GamePath "UnityExplorer-Usage.txt"
@"
=== UnityExplorer for Cyber Knights: Flashpoint ===

INSTALLATION: Complete!

USAGE:
1. Launch the game
2. Press F7 to toggle UnityExplorer UI (default key)
3. Use these tabs:
   - Object Explorer: Search for GameObjects by name (e.g., "Button", "MainMenu")
   - Inspector: View components on selected objects
   - C# Console: Run code to inspect objects (results log to MelonLoader\Latest.log)

ACCESSIBILITY NOTES:
- UnityExplorer's GUI is NOT screen reader accessible
- Use Mouse Inspect mode (requires sighted assistance) OR
- Use C# Console to log component info to Latest.log (screen reader accessible)

DEBUGGING TEXT EXTRACTION:
In C# Console tab, type:

// Find a button GameObject
var button = GameObject.Find("MainMenu_2");
if (button != null) {
    // Log all components
    var comps = button.GetComponents<Component>();
    foreach (var c in comps) {
        UnityEngine.Debug.Log($"Component: {c.GetType().FullName}");
    }
    
    // Try finding text component
    var tmpro = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    if (tmpro != null) {
        UnityEngine.Debug.Log($"TMP Text: {tmpro.text}");
    }
}

Results appear in: MelonLoader\Latest.log (read with screen reader)

CONFIGURATION:
Edit: UserData\UnityExplorer\config.cfg
- Change startup_delay_time for slower systems
- Change ui_scale for better visibility
- Change disable_eventSystem_override if input issues

TROUBLESHOOTING:
- If F7 doesn't work, check config.cfg for keybind
- If game crashes, increase startup_delay_time to 5-10 seconds
- If UI conflicts with accessibility mod, disable temporarily

See: DEBUGGING-TEXT-EXTRACTION.md in mod project for full guide
"@ | Out-File -FilePath $instructionsPath -Encoding UTF8

Write-Host "`nUsage instructions written to:" -ForegroundColor Cyan
Write-Host $instructionsPath -ForegroundColor White

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
Write-Host "1. Enable debug logging in your mod:" -ForegroundColor White
Write-Host "   Edit: $GamePath\UserData\MelonPreferences.cfg" -ForegroundColor Gray
Write-Host "   Set: DebugTextExtraction = true" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Launch the game and test main menu navigation" -ForegroundColor White
Write-Host ""
Write-Host "3. Check logs for component details:" -ForegroundColor White
Write-Host "   $GamePath\MelonLoader\Latest.log" -ForegroundColor Gray
Write-Host ""
Write-Host "4. (Optional) Use UnityExplorer for deeper inspection:" -ForegroundColor White
Write-Host "   Press F7 in-game to open UnityExplorer" -ForegroundColor Gray
Write-Host ""
Write-Host "See DEBUGGING-TEXT-EXTRACTION.md for full troubleshooting guide" -ForegroundColor Yellow
Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
