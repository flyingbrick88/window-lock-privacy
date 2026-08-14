[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

[xml]$project = Get-Content -LiteralPath (Join-Path $repoRoot 'src\WindowLock\WindowLock.csproj') -Raw
$version = [string]$project.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Project version '$version' is not a three-part release version."
}

$checks = [ordered]@{
    'installer version' = @{ Path = 'installer\WindowLock.iss'; Pattern = "#define AppVersion `"$([regex]::Escape($version))`"" }
    'README installer' = @{ Path = 'README.md'; Pattern = "WindowLock-Setup-$([regex]::Escape($version))\.exe" }
    'installation guide' = @{ Path = 'docs\installation.md'; Pattern = "WindowLock-Setup-$([regex]::Escape($version))\.exe" }
    'release notes' = @{ Path = "docs\release-notes-$version.md"; Pattern = "Window Lock $([regex]::Escape($version))" }
    'workflow artifact' = @{ Path = '.github\workflows\build.yml'; Pattern = "WindowLock-$([regex]::Escape($version))" }
    'workflow installer' = @{ Path = '.github\workflows\build.yml'; Pattern = "WindowLock-Setup-$([regex]::Escape($version))\.exe" }
    'issue template' = @{ Path = '.github\ISSUE_TEMPLATE\bug_report.yml'; Pattern = "placeholder: $([regex]::Escape($version))" }
}

foreach ($entry in $checks.GetEnumerator()) {
    $path = Join-Path $repoRoot $entry.Value.Path
    if (-not (Test-Path -LiteralPath $path)) {
        throw "$($entry.Key) file is missing: $($entry.Value.Path)"
    }

    $text = Get-Content -LiteralPath $path -Raw
    if ($text -notmatch $entry.Value.Pattern) {
        throw "$($entry.Key) does not match project version $version in $($entry.Value.Path)."
    }
}

if ($env:GITHUB_REF_TYPE -eq 'tag' -and $env:GITHUB_REF_NAME -ne "v$version") {
    throw "Git tag '$($env:GITHUB_REF_NAME)' does not match project version v$version."
}

Write-Output "Release metadata is consistent for Window Lock $version."
