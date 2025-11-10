# PowerShell script to update project references after IL2CPP dump
# Run this AFTER dump-il2cpp.ps1

param(
    [string]$GamePath = "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Update Project References Script ===" -ForegroundColor Cyan
Write-Host ""

$projectFile = Join-Path $PSScriptRoot "..\CKFlashpointAccessibility\CKFlashpointAccessibility.csproj"
$melonManagedDir = Join-Path $GamePath "MelonLoader\Il2CppAssemblies"
$melonDependenciesDir = Join-Path $GamePath "MelonLoader\Dependencies"

# Validate paths
if (-not (Test-Path $projectFile)) {
    Write-Host "ERROR: Project file not found: $projectFile" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $melonManagedDir)) {
    Write-Host "ERROR: MelonLoader Managed folder not found: $melonManagedDir" -ForegroundColor Red
    Write-Host "Make sure you've run the game at least once after installing MelonLoader!" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Project file: $projectFile" -ForegroundColor Green
Write-Host "✓ MelonLoader Assemblies: $melonManagedDir" -ForegroundColor Green
Write-Host ""

# Read project file
$content = Get-Content $projectFile -Raw

# Define references to add
$references = @(
    @{
        Name = "Assembly-CSharp"
        Path = Join-Path $melonManagedDir "Assembly-CSharp.dll"
    },
    @{
        Name = "UnityEngine.CoreModule"
        Path = Join-Path $melonManagedDir "UnityEngine.CoreModule.dll"
    },
    @{
        Name = "UnityEngine.UI"
        Path = Join-Path $melonManagedDir "UnityEngine.UI.dll"
    },
    @{
        Name = "Il2CppInterop.Runtime"
        Path = Join-Path $melonDependenciesDir "Il2CppInterop.Runtime.dll"
    }
)

# Check which references exist
$missingRefs = @()
$validRefs = @()

foreach ($ref in $references) {
    if (Test-Path $ref.Path) {
        $validRefs += $ref
        Write-Host "✓ Found: $($ref.Name)" -ForegroundColor Green
    } else {
        $missingRefs += $ref
        Write-Host "⚠ Missing: $($ref.Name) at $($ref.Path)" -ForegroundColor Yellow
    }
}

if ($validRefs.Count -eq 0) {
    Write-Host "ERROR: No valid references found!" -ForegroundColor Red
    exit 1
}

# Build reference XML
$referenceXml = "`n  <ItemGroup>`n    <!-- Game assemblies - Auto-updated by update-references.ps1 -->`n"

foreach ($ref in $validRefs) {
    $escapedPath = $ref.Path -replace '\\', '\\'
    $referenceXml += @"
    <Reference Include="$($ref.Name)">
      <HintPath>$($ref.Path)</HintPath>
      <Private>False</Private>
    </Reference>

"@
}

$referenceXml += "  </ItemGroup>"

# Update project file
if ($content -match '<!--\s*Uncomment and update paths after installing the game\s*-->') {
    # Remove the commented-out section
    $content = $content -replace '(?s)<!--\s*Uncomment and update paths after installing the game.*?-->', $referenceXml
    
    Set-Content -Path $projectFile -Value $content -NoNewline
    Write-Host ""
    Write-Host "✓ Project references updated successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠ Could not find placeholder comment in project file" -ForegroundColor Yellow
    Write-Host "Add references manually or check project file structure" -ForegroundColor Yellow
}

# Display summary
Write-Host ""
Write-Host "=== Update Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Added $($validRefs.Count) references:" -ForegroundColor Cyan
foreach ($ref in $validRefs) {
    Write-Host "  - $($ref.Name)" -ForegroundColor White
}

if ($missingRefs.Count -gt 0) {
    Write-Host ""
    Write-Host "Missing $($missingRefs.Count) references:" -ForegroundColor Yellow
    foreach ($ref in $missingRefs) {
        Write-Host "  - $($ref.Name)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run 'dotnet restore' in the project folder"
Write-Host "2. Run 'dotnet build' to compile the mod"
Write-Host "3. Use dnSpy/ILSpy to explore MelonLoader\Managed\Assembly-CSharp.dll"
Write-Host ""
