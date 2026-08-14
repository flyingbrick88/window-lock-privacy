param([string]$InstallPath = (Join-Path $env:LOCALAPPDATA 'Programs\Window Lock'))

$ErrorActionPreference = 'Stop'
$destination = [System.IO.Path]::GetFullPath($InstallPath)
Get-Process -ErrorAction SilentlyContinue | Where-Object {
    try {
        $processPath = [System.IO.Path]::GetFullPath($_.Path)
        $processPath.StartsWith($destination + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
    } catch { $false }
} | Stop-Process -Force
foreach ($shortcutPath in @(
    (Join-Path ([Environment]::GetFolderPath('Programs')) 'Window Lock.lnk'),
    (Join-Path ([Environment]::GetFolderPath('Startup')) 'Window Lock.lnk')
)) {
    if (Test-Path -LiteralPath $shortcutPath) { Remove-Item -LiteralPath $shortcutPath -Force }
}
if (Test-Path -LiteralPath $destination) { Remove-Item -LiteralPath $destination -Recurse -Force }

Write-Output 'Window Lock application files and shortcuts removed. Windows policy values were not changed.'
