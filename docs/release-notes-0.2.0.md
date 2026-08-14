# Window Lock 0.2.0 beta

This release moves the active project to the OSI-approved GNU General Public License version 3 only and prepares the public build and governance for a SignPath Foundation application.

## Files

- `WindowLock-Setup-0.2.0.exe` — self-contained per-user installer
- `WindowLock.exe` — self-contained portable application
- `SHA256SUMS.txt` — SHA-256 integrity values for released binaries and the SBOM
- `manifest.spdx.json` — machine-readable SPDX software bill of materials

## Licence transition

Current source and version 0.2.0 onward are licensed as `GPL-3.0-only`. The GPL permits use, modification, redistribution, and commercial activity when its terms are followed. Versions 0.1.0 and 0.1.1 remain governed by the licence shipped with those releases. No proprietary or commercial dual licence is offered.

## Signing status

This is an **unsigned SignPath application candidate**, not a signed release. Windows may show **Unknown publisher**. Download only from the official GitHub repository and verify `SHA256SUMS.txt`. If SignPath Foundation later accepts Window Lock, a subsequent release will explicitly identify and verify the expected Authenticode signer.

The app's policy scope and limitations are unchanged from 0.1.1. It is not a universal installation approval broker, and protection can delay trusted driver, Store, or security updates.
