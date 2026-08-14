# Window Lock — Windows privacy & update control

[![Build and test](https://github.com/flyingbrick88/window-lock-privacy/actions/workflows/build.yml/badge.svg)](https://github.com/flyingbrick88/window-lock-privacy/actions/workflows/build.yml)
[![Documentation](https://img.shields.io/badge/docs-GitHub%20Pages-087F5B)](https://flyingbrick88.github.io/window-lock-privacy/)
[![Licence: PolyForm Noncommercial](https://img.shields.io/badge/licence-PolyForm%20Noncommercial-blue)](LICENSE)

Window Lock is a Windows 10 and 11 desktop app that makes selected built-in privacy and update policies clear and easy to control. Its main purpose is to reduce optional manufacturer companion apps, promotional utilities, and telemetry software appearing simply because hardware such as a display, printer, or USB device was connected.

![Window Lock 0.1.1 interface](docs/images/window-lock-gui-preview.png)

## Download

Download `WindowLock-Setup-0.1.1.exe` from the [GitHub releases page](https://github.com/flyingbrick88/window-lock-privacy/releases). The installer is self-contained and installs only for the current Windows account. See [installation guidance](docs/installation.md), including checksum verification and the unsigned-app warning.

## What it checks

- device-associated app downloads triggered through Windows device metadata
- the fallback Device Installation Settings preference
- driver-classified packages in routine Windows quality updates
- notify-before-download behavior for Windows Update
- automatic Microsoft Store app updates
- Microsoft Store access where the policy is supported

Unprotected cards appear first, protected cards are pale green, and unsupported options appear last and can be hidden. Each actionable card has its own **PROTECT** or **UNPROTECT** button. **More information** explains the effect, applicability, current Windows evidence, and Microsoft documentation.

Window Lock will not overwrite a conflicting policy set by another administrator, organization, or tool. It records local backup and ownership metadata before applying a new value, verifies every change, and allows UNPROTECT only while the value still matches what Window Lock set.

## Important limitation

Windows has no universal third-party callback that pauses every possible installation and asks another app for approval. Window Lock does **not** guarantee that nothing can be installed without permission. It does not intercept every traditional installer, initial Plug and Play driver, package manager, script, enterprise-management action, or Windows servicing path. Some controls block automatic delivery and require a later manual update rather than presenting a per-item approval prompt.

Protection can delay driver, Store-app, or security updates. Review and install trusted security updates regularly. See the [user guide](docs/user-guide.md) and [posture details](docs/posture-checks.md).

## Privacy

No account is required. Window Lock contains no analytics, advertising, telemetry, cloud sync, or remote reporting and sends no application data anywhere. It does not save user identity, installed-app inventory, or recent-installation results. It stores only local policy backup values needed for safe UNPROTECT operations. The feedback and documentation buttons merely open the selected website in the default browser. Read the [complete privacy statement](PRIVACY.md).

## Build

Requirements: Windows and .NET SDK 10.0. From PowerShell:

```powershell
dotnet restore .\WindowLock.slnx
dotnet build .\WindowLock.slnx --configuration Release --no-restore
dotnet run --project .\tests\WindowLock.Tests\WindowLock.Tests.csproj --configuration Release --no-build
dotnet publish .\src\WindowLock\WindowLock.csproj --configuration Release --property:PublishProfile=win-x64 --output .\dist\win-x64
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" .\installer\WindowLock.iss
```

The GitHub Actions workflow repeats the build, test, publish, and installer compilation on a clean Windows runner.

## Feedback and security

Use the app's **Send feedback** button or [GitHub Issues](https://github.com/flyingbrick88/window-lock-privacy/issues/new/choose). Please report vulnerabilities privately as described in [SECURITY.md](SECURITY.md).

Development is funded voluntarily through [Ko-fi](https://ko-fi.com/flyingbrick88). Support is optional and never affects app features, privacy, or access.

## Licence

Copyright © 2026 flyingbrick88. Window Lock is free for personal and other qualifying noncommercial use under the [PolyForm Noncommercial License 1.0.0](LICENSE). Commercial use or sale is not permitted without separate permission. This is a source-available licence, not an OSI-approved open-source licence. Third-party runtime and installer notices are listed separately in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
