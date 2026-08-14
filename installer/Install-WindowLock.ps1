param(
    [string]$PayloadPath = (Join-Path $PSScriptRoot '..\dist\win-x64'),
    [string]$InstallPath = (Join-Path $env:LOCALAPPDATA 'Programs\Window Lock')
)

$ErrorActionPreference = 'Stop'
$source = [System.IO.Path]::GetFullPath($PayloadPath)
$destination = [System.IO.Path]::GetFullPath($InstallPath)
$executable = Join-Path $source 'WindowLock.exe'
$installedName = 'WindowLock-0.1.0.exe'
if (-not (Test-Path -LiteralPath $executable)) { throw "Published WindowLock.exe was not found at $executable" }

New-Item -ItemType Directory -Force -Path $destination | Out-Null
$installedExecutable = Join-Path $destination $installedName
Get-Process -ErrorAction SilentlyContinue | Where-Object {
    try { [System.IO.Path]::GetFullPath($_.Path) -eq $installedExecutable } catch { $false }
} | Stop-Process -Force
Copy-Item -LiteralPath $executable -Destination $installedExecutable -Force

$shell = New-Object -ComObject WScript.Shell
$startMenu = Join-Path ([Environment]::GetFolderPath('Programs')) 'Window Lock.lnk'
$startup = Join-Path ([Environment]::GetFolderPath('Startup')) 'Window Lock.lnk'
foreach ($shortcutPath in @($startMenu, $startup)) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $installedExecutable
    $shortcut.WorkingDirectory = $destination
    $shortcut.Description = 'Window Lock installation posture checker'
    $shortcut.IconLocation = "$installedExecutable,0"
    $shortcut.Save()
}

Write-Output $installedExecutable
