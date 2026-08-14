# Installation

## Recommended installer

1. Download `WindowLock-Setup-0.1.1.exe` from the [GitHub releases page](https://github.com/flyingbrick88/window-lock-privacy/releases).
2. Compare its SHA-256 value with `SHA256SUMS.txt` on the release.
3. Run the installer. Choose whether Window Lock should start when you sign in and whether to create a desktop shortcut.
4. Launch Window Lock. It reads status without elevation; Windows asks for administrator approval only after you select **PROTECT** or **UNPROTECT**.

The release is not code signed, so SmartScreen may display an unknown-publisher warning. Download only from this repository and verify the checksum.

## Uninstalling

Use **Settings → Apps → Installed apps → Window Lock → Uninstall**. Uninstall removes application files and shortcuts but does not silently change Windows policies. To restore a setting owned by Window Lock, use that card's **UNPROTECT** button before uninstalling.

## Requirements

- Windows 10 version 1809 or later, or Windows 11
- x64-compatible processor
- Administrator approval when changing machine policy

Windows Home does not support every policy shown; unsupported options are marked **UNAVAILABLE**.
