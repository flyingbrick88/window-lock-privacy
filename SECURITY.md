# Security policy

Version 0.1.x receives security fixes while it is the latest published series.

Do not disclose a vulnerability in a public issue. Use GitHub's [private vulnerability reporting](https://github.com/flyingbrick88/window-lock-privacy/security/advisories/new) and include the affected version, Windows edition/build, impact, reproduction steps, and any suggested mitigation. Remove personal information and secrets.

Window Lock applies selected machine policies only after confirmation and a Windows administrator prompt. It stores minimum local ownership and backup values under `HKEY_LOCAL_MACHINE\SOFTWARE\WindowLock\Backup`; these values are required to avoid reversing policy owned by another tool. It has no telemetry or automatic network communication. Documentation and feedback links open in the default browser only after the user selects them.

The 0.1.0 executable and installer are not code signed. Verify release SHA-256 checksums and download only from this repository.
