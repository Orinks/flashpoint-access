# Automated Mod Testing Script
# Monitors MelonLoader log for accessibility mod patches and functionality

param(
    [switch]$LaunchGame,
    [int]$TimeoutSeconds = 60,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Paths
$gamePath = "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint"
$gameExe = Join-Path $gamePath "CyberKnights.exe"
$logPath = Join-Path $gamePath "MelonLoader\Latest.log"
$modPath = Join-Path $gamePath "Mods\CKFlashpointAccessibility.dll"

# Test expectations
$expectedPatches = @(
    "STEButton.OnSelect",
    "STEButton.OnDeselect",
    "STEButton.OnPointerClick",
    "STETextBlock.SetText",
    "UIScreenBase.Show",
    "STESelectableWidgetBase.Select",
    "STESelectableWidgetBase.OnSelect",
    "STEDialogAnswerButton.OnSelect",
    "STETextInput.OnSelect",
    "STETextItem.OnSelect"
)

$criticalMessages = @(
    "SRAL initialized with engine:",
    "CK Flashpoint Accessibility",
    "All UI patches applied successfully"
)

Write-Host "=== Cyber Knights Accessibility Mod Test ===" -ForegroundColor Cyan
Write-Host ""

# Check mod exists
if (-not (Test-Path $modPath)) {
    Write-Host "❌ ERROR: Mod DLL not found at: $modPath" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Mod DLL found" -ForegroundColor Green

# Check game exists
if (-not (Test-Path $gameExe)) {
    Write-Host "❌ ERROR: Game executable not found at: $gameExe" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Game executable found" -ForegroundColor Green

# Clear old log if exists
if (Test-Path $logPath) {
    Remove-Item $logPath -Force
    Write-Host "✓ Cleared old log file" -ForegroundColor Green
}

Write-Host ""

# Launch game if requested
$gameProcess = $null
if ($LaunchGame) {
    Write-Host "Launching game..." -ForegroundColor Yellow
    $gameProcess = Start-Process -FilePath $gameExe -PassThru
    Write-Host "✓ Game launched (PID: $($gameProcess.Id))" -ForegroundColor Green
    Write-Host "Waiting for log file to be created..." -ForegroundColor Yellow
    
    # Wait for log file to appear
    $waited = 0
    while (-not (Test-Path $logPath) -and $waited -lt 30) {
        Start-Sleep -Seconds 1
        $waited++
    }
    
    if (-not (Test-Path $logPath)) {
        Write-Host "❌ Log file not created after 30 seconds" -ForegroundColor Red
        exit 1
    }
    
    Start-Sleep -Seconds 2 # Give it time to write initial entries
    Write-Host "✓ Log file created" -ForegroundColor Green
}

if (-not (Test-Path $logPath)) {
    Write-Host "❌ ERROR: Log file not found. Launch the game first or use -LaunchGame" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Monitoring Log File ===" -ForegroundColor Cyan
Write-Host ""

# Results tracking
$results = @{
    ModLoaded = $false
    SRALInitialized = $false
    SRALEngine = ""
    PatchesApplied = 0
    FailedPatches = @()
    SuccessfulPatches = @()
    Warnings = @()
    Errors = @()
    Selections = @()
    Activations = @()
}

# Monitor log file
$startTime = Get-Date
$lastPosition = 0

Write-Host "Analyzing log entries..." -ForegroundColor Yellow
Write-Host ""

while ($true) {
    # Check timeout
    if ((Get-Date) -gt $startTime.AddSeconds($TimeoutSeconds)) {
        Write-Host "⏱ Timeout reached ($TimeoutSeconds seconds)" -ForegroundColor Yellow
        break
    }
    
    # Check if game is still running
    if ($LaunchGame -and $gameProcess -and $gameProcess.HasExited) {
        Write-Host "Game process exited" -ForegroundColor Yellow
        break
    }
    
    # Read new log entries
    try {
        $content = Get-Content $logPath -Raw -ErrorAction SilentlyContinue
        if ($content -and $content.Length -gt $lastPosition) {
            $newContent = $content.Substring($lastPosition)
            $lastPosition = $content.Length
            
            # Parse new lines
            $lines = $newContent -split "`n"
            foreach ($line in $lines) {
                if ([string]::IsNullOrWhiteSpace($line)) { continue }
                
                # Verbose output
                if ($Verbose) {
                    Write-Host $line -ForegroundColor Gray
                }
                
                # Check for mod loaded
                if ($line -match "CK Flashpoint Accessibility.*loaded") {
                    $results.ModLoaded = $true
                    Write-Host "✓ Mod loaded successfully" -ForegroundColor Green
                }
                
                # Check for SRAL initialization
                if ($line -match "SRAL initialized with engine: (\w+)") {
                    $results.SRALInitialized = $true
                    $results.SRALEngine = $matches[1]
                    Write-Host "✓ SRAL initialized with: $($matches[1])" -ForegroundColor Green
                }
                
                # Check for successful patches
                if ($line -match "Patched: (\S+)\.(\S+)") {
                    $patchName = "$($matches[1]).$($matches[2])"
                    $results.SuccessfulPatches += $patchName
                    $results.PatchesApplied++
                    Write-Host "  ✓ $patchName" -ForegroundColor Cyan
                }
                
                # Check for failed patches
                if ($line -match "Failed to patch|Type not found|Method not found") {
                    $results.FailedPatches += $line
                    Write-Host "  ⚠ $line" -ForegroundColor Yellow
                }
                
                # Check for auto-selection
                if ($line -match "Auto-selected first button: (.+)") {
                    $buttonName = $matches[1]
                    Write-Host "✓ Auto-selected button: $buttonName" -ForegroundColor Green
                }
                
                # Check for button selections (our patches working!)
                if ($line -match "STEButton_OnSelect|STESelectableWidgetBase.*Select") {
                    $results.Selections += $line
                    if ($Verbose) {
                        Write-Host "  → Selection detected" -ForegroundColor Magenta
                    }
                }
                
                # Check for button activations
                if ($line -match "Button activated:") {
                    $results.Activations += $line
                    if ($Verbose) {
                        Write-Host "  → Activation detected" -ForegroundColor Magenta
                    }
                }
                
                # Check for warnings
                if ($line -match "\[Warning\]") {
                    $results.Warnings += $line
                }
                
                # Check for errors
                if ($line -match "\[Error\]|Exception") {
                    $results.Errors += $line
                    if (-not $Verbose) {
                        Write-Host "  ❌ Error: $line" -ForegroundColor Red
                    }
                }
            }
        }
    }
    catch {
        Write-Host "Error reading log: $_" -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "=== Test Results ===" -ForegroundColor Cyan
Write-Host ""

# Summary
$totalExpected = $expectedPatches.Count
$successRate = if ($totalExpected -gt 0) { 
    [math]::Round(($results.PatchesApplied / $totalExpected) * 100, 1) 
} else { 0 }

Write-Host "Mod Status:" -ForegroundColor White
Write-Host "  Loaded: $(if ($results.ModLoaded) { '✓ Yes' } else { '❌ No' })" -ForegroundColor $(if ($results.ModLoaded) { 'Green' } else { 'Red' })
Write-Host "  SRAL Initialized: $(if ($results.SRALInitialized) { "✓ Yes ($($results.SRALEngine))" } else { '❌ No' })" -ForegroundColor $(if ($results.SRALInitialized) { 'Green' } else { 'Red' })
Write-Host ""

Write-Host "Patches:" -ForegroundColor White
Write-Host "  Applied: $($results.PatchesApplied) / $totalExpected ($successRate%)" -ForegroundColor $(if ($successRate -ge 80) { 'Green' } elseif ($successRate -ge 50) { 'Yellow' } else { 'Red' })
Write-Host "  Failed: $($results.FailedPatches.Count)" -ForegroundColor $(if ($results.FailedPatches.Count -eq 0) { 'Green' } else { 'Yellow' })
Write-Host ""

Write-Host "Runtime Activity:" -ForegroundColor White
Write-Host "  Selections Detected: $($results.Selections.Count)" -ForegroundColor $(if ($results.Selections.Count -gt 0) { 'Green' } else { 'Gray' })
Write-Host "  Activations Detected: $($results.Activations.Count)" -ForegroundColor $(if ($results.Activations.Count -gt 0) { 'Green' } else { 'Gray' })
Write-Host ""

Write-Host "Issues:" -ForegroundColor White
Write-Host "  Warnings: $($results.Warnings.Count)" -ForegroundColor $(if ($results.Warnings.Count -eq 0) { 'Green' } else { 'Yellow' })
Write-Host "  Errors: $($results.Errors.Count)" -ForegroundColor $(if ($results.Errors.Count -eq 0) { 'Green' } else { 'Red' })
Write-Host ""

# Missing patches
$missingPatches = $expectedPatches | Where-Object { $results.SuccessfulPatches -notcontains $_ }
if ($missingPatches.Count -gt 0) {
    Write-Host "Missing Patches:" -ForegroundColor Yellow
    foreach ($patch in $missingPatches) {
        Write-Host "  - $patch" -ForegroundColor Gray
    }
    Write-Host ""
}

# Show errors if any
if ($results.Errors.Count -gt 0) {
    Write-Host "Errors Found:" -ForegroundColor Red
    foreach ($error in $results.Errors | Select-Object -First 5) {
        Write-Host "  $error" -ForegroundColor Red
    }
    if ($results.Errors.Count -gt 5) {
        Write-Host "  ... and $($results.Errors.Count - 5) more" -ForegroundColor Red
    }
    Write-Host ""
}

# Overall status
Write-Host "=== Overall Status ===" -ForegroundColor Cyan
if ($results.ModLoaded -and $results.SRALInitialized -and $successRate -ge 70 -and $results.Errors.Count -eq 0) {
    Write-Host "✓ PASS - Mod is functioning correctly" -ForegroundColor Green
    $exitCode = 0
} elseif ($results.ModLoaded -and $results.SRALInitialized -and $successRate -ge 50) {
    Write-Host "⚠ PARTIAL - Mod loaded but some issues detected" -ForegroundColor Yellow
    $exitCode = 1
} else {
    Write-Host "❌ FAIL - Mod has critical issues" -ForegroundColor Red
    $exitCode = 2
}

Write-Host ""
Write-Host "Log file location: $logPath" -ForegroundColor Gray
Write-Host ""

# Cleanup
if ($LaunchGame -and $gameProcess -and -not $gameProcess.HasExited) {
    Write-Host "Game is still running. Press Ctrl+C to exit or close the game manually." -ForegroundColor Yellow
}

exit $exitCode
