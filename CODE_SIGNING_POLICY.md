# Code signing policy

## Provider and current status

Window Lock is applying for the SignPath Foundation open-source code-signing program. The required provider disclosure is:

> Free code signing provided by SignPath.io, certificate by SignPath Foundation

This statement describes the planned signing service. It does not mean every Window Lock release is signed. Each GitHub release must state whether it is signed, identify the expected signer, and publish SHA-256 checksums. Until SignPath accepts the project and a signed release is verified, Windows may show **Unknown publisher**.

If approved, the Authenticode signer shown by Windows will be **SignPath Foundation**, not flyingbrick88. A signature authenticates the reviewed build and detects later alteration; it does not make the software risk-free or guarantee every Windows policy behaves identically on every edition.

## What is eligible to be signed

Only release artifacts built from the public repository at <https://github.com/flyingbrick88/window-lock-privacy> are eligible:

- `WindowLock.exe`, built from the tagged Window Lock source
- `WindowLock-Setup-<version>.exe`, built from the public Inno Setup script and the matching `WindowLock.exe`

The self-contained executable includes open-source Microsoft .NET runtime components described in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Inno Setup is a public build tool and is not shipped as a component of Window Lock. The signing process must not introduce private source, proprietary maintainer-only components, bundled offers, advertising, or an alternate commercial licence.

## Build and release provenance

1. A release version is set consistently in the project, installer, workflow, documentation, and Git tag.
2. GitHub Actions checks out the public commit using a commit-pinned action, uses the SDK pinned by `global.json`, restores from the configured public NuGet source, builds, runs tests, publishes the self-contained executable, and compiles the installer with a pinned Inno Setup package.
3. The workflow generates an SPDX SBOM and SHA-256 checksums. Its logs and artifacts provide a public, reproducible record of the source commit and build inputs.
4. A signing request may reference only the successful workflow artifact for that tagged commit. Maintainer-built replacement binaries are not eligible.
5. Every signing request and every GitHub release requires a manual approval. Signing and publishing must never be triggered solely by an unreviewed pull request or an automatic dependency update.
6. The approver verifies the tag, product name, version, commit, workflow result, file names, hashes, SBOM, malware scan result, and release notes before approval.
7. After signing, the maintainer verifies the Authenticode chain and timestamp, rechecks the hashes and metadata, scans the final files, tests installation and uninstall, and only then publishes them.

## Project roles

- **Author and committer:** `flyingbrick88` maintains the source and may merge reviewed changes.
- **Reviewer:** `flyingbrick88` reviews community pull requests for behavior, security, privacy, licensing, and build impact. Automated build and test checks must pass. A contributor cannot approve their own untrusted binary artifact.
- **Signing and release approver:** `flyingbrick88` manually approves each signing request and public release after completing the checks above.

Anyone later added to these roles must be named in this policy or a linked public governance document. All authors with commit access, reviewers, and signing approvers must use multi-factor authentication on GitHub and SignPath. Recovery credentials must be kept outside the repository. The SignPath application must not be submitted until the maintainer has manually confirmed MFA on both services.

Public project literature, application metadata, commit identity, website content, and release notes identify the maintainer only as `flyingbrick88`. Any legal identity privately required by the signing provider must not be copied into public project materials. The expected public certificate subject remains **SignPath Foundation**.

## Security and user-impact disclosure

Window Lock is a Windows policy-control application. Reading status is non-elevated. When the user selects **PROTECT** or **UNPROTECT** and approves Windows User Account Control, the app can change documented machine policy values under `HKEY_LOCAL_MACHINE` and stores limited backup and ownership metadata under `HKEY_LOCAL_MACHINE\SOFTWARE\WindowLock\Backup`. These changes can delay device companion apps, drivers, Microsoft Store app updates, or Windows Update downloads. The app explains each action before requesting elevation, verifies each write, refuses to overwrite an unowned conflicting policy, and provides a guarded per-card reversal where it owns the setting.

Uninstalling removes application files and shortcuts but intentionally leaves Windows policy values and backup metadata unchanged. Users should use the applicable **UNPROTECT** controls before uninstalling if they want Window Lock to restore owned values. This behavior is stated in the app, installer, privacy statement, and installation guide.

Window Lock has no account, analytics, advertising, telemetry, cloud sync, automatic update service, or remote reporting. This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it. Ko-fi, GitHub feedback, and documentation pages open only when the user selects their buttons or links.

## Reporting concerns

Report suspected vulnerabilities through [GitHub private vulnerability reporting](https://github.com/flyingbrick88/window-lock-privacy/security/advisories/new). Report signing or release-integrity problems privately before opening a public issue when disclosure could put users at risk.
