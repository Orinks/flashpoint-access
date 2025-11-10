# PowerShell script to dump IL2CPP assemblies using Il2CppDumper
# NOTE: With MelonLoader, this is optional - MelonLoader auto-generates assemblies
# Run this AFTER installing the game and MelonLoader if you want additional dump info

param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint"
)

$ErrorActionPreference = "Stop"

Write-Host "=== IL2CPP Dumper Script ===" -ForegroundColor Cyan
Write-Host ""

# Paths
$toolsDir = Join-Path $PSScriptRoot "..\tools"
$dumperDir = Join-Path $toolsDir "Il2CppDumper"
$outputDir = Join-Path $PSScriptRoot "..\dumped"

# Check if game path exists
if (-not (Test-Path $GamePath)) {
    Write-Host "ERROR: Game path not found: $GamePath" -ForegroundColor Red
    Write-Host "Specify correct path: .\dump-il2cpp.ps1 -GamePath 'C:\Your\Game\Path'" -ForegroundColor Yellow
    exit 1
}

# Find required files
$gameExe = Get-ChildItem -Path $GamePath -Filter "Cyber Knights*.exe" | Select-Object -First 1
if (-not $gameExe) {
    $gameExe = Get-ChildItem -Path $GamePath -Filter "*.exe" | Where-Object { $_.Name -notlike "UnityCrashHandler*" } | Select-Object -First 1
}

$gameAssembly = Join-Path $GamePath "GameAssembly.dll"
$metadata = Join-Path $GamePath "Cyber_Knights__Flashpoint_Data\il2cpp_data\Metadata\global-metadata.dat"

Write-Host "Checking for required files..." -ForegroundColor Cyan

if (-not (Test-Path $gameAssembly)) {
    Write-Host "ERROR: GameAssembly.dll not found at: $gameAssembly" -ForegroundColor Red
    exit 1
}
Write-Host "✓ GameAssembly.dll found" -ForegroundColor Green

if (-not (Test-Path $metadata)) {
    Write-Host "ERROR: global-metadata.dat not found at: $metadata" -ForegroundColor Red
    exit 1
}
Write-Host "✓ global-metadata.dat found" -ForegroundColor Green
Write-Host ""

# Download Il2CppDumper if needed
$dumperExe = Join-Path $dumperDir "Il2CppDumper.exe"
if (-not (Test-Path $dumperExe)) {
    Write-Host "Downloading Il2CppDumper..." -ForegroundColor Cyan
    
    $dumperUrl = "https://github.com/Perfare/Il2CppDumper/releases/latest/download/Il2CppDumper-x64.zip"
    $dumperZip = Join-Path $toolsDir "Il2CppDumper.zip"
    
    if (-not (Test-Path $toolsDir)) {
        New-Item -ItemType Directory -Path $toolsDir | Out-Null
    }
    
    try {
        Invoke-WebRequest -Uri $dumperUrl -OutFile $dumperZip -UseBasicParsing
        Expand-Archive -Path $dumperZip -DestinationPath $dumperDir -Force
        Write-Host "✓ Il2CppDumper downloaded and extracted" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to download Il2CppDumper: $_" -ForegroundColor Red
        Write-Host "Download manually from: https://github.com/Perfare/Il2CppDumper/releases" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✓ Il2CppDumper already available" -ForegroundColor Green
}

# Create output directory
if (Test-Path $outputDir) {
    Write-Host "Removing old dump..." -ForegroundColor Yellow
    Remove-Item -Path $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

# Run Il2CppDumper
Write-Host ""
Write-Host "Running Il2CppDumper (this may take a few minutes)..." -ForegroundColor Cyan
Write-Host "Executable: $gameAssembly" -ForegroundColor Gray
Write-Host "Metadata: $metadata" -ForegroundColor Gray
Write-Host "Output: $outputDir" -ForegroundColor Gray
Write-Host ""

try {
    $process = Start-Process -FilePath $dumperExe -ArgumentList "`"$gameAssembly`"", "`"$metadata`"", "`"$outputDir`"" -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ IL2CPP dump completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Il2CppDumper exited with code $($process.ExitCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "ERROR: Failed to run Il2CppDumper: $_" -ForegroundColor Red
    exit 1
}

# Check output
$dummyDllDir = Join-Path $outputDir "DummyDll"
if (Test-Path $dummyDllDir) {
    $dllCount = (Get-ChildItem -Path $dummyDllDir -Filter "*.dll").Count
    Write-Host "✓ Found $dllCount dummy DLL files" -ForegroundColor Green
    
    # Find Assembly-CSharp.dll
    $assemblyCSharp = Join-Path $dummyDllDir "Assembly-CSharp.dll"
    if (Test-Path $assemblyCSharp) {
        Write-Host "✓ Assembly-CSharp.dll generated successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠ Assembly-CSharp.dll not found - check dump output" -ForegroundColor Yellow
    }
} else {
    Write-Host "ERROR: DummyDll folder not created" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Dump Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: $outputDir" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review dumped DLLs in: $dummyDllDir"
Write-Host "2. Run .\update-references.ps1 to configure the mod project"
Write-Host "3. Use a decompiler (dnSpy/ILSpy) to explore game classes"
Write-Host ""
