# SignPath Foundation readiness

This is the maintainer's application and release gate. A checked item records repository evidence; account-only items require manual confirmation because they cannot be proven from public source.

## Project eligibility

- [x] Public source repository with existing releases in the intended desktop-app form
- [x] Current source and future releases use the OSI-approved `GPL-3.0-only` licence
- [x] No commercial dual licence or proprietary maintainer-only component
- [x] Buildable public source, user documentation, contribution rules, security policy, and issue tracker
- [x] Privacy statement, system-change warning, limitations, and uninstall behavior are public
- [x] Code-signing policy names the provider, certificate organization, roles, approval process, and eligible artifacts

## Supply chain and release controls

- [x] .NET SDK pinned by `global.json`
- [x] GitHub Actions referenced by full commit hashes
- [x] Inno Setup, Microsoft SBOM tool, and its .NET 8 host versions pinned in CI
- [x] Automated release build, tests, SPDX SBOM, and SHA-256 checksums
- [x] Consistent `Window Lock` product name and 0.2.0 version metadata
- [ ] Configure a protected GitHub release environment or equivalent manual approval gate
- [ ] Connect the approved SignPath project to the public GitHub build
- [ ] Configure SignPath artifact configuration for the app and installer
- [ ] Configure SignPath policy so every signing request requires manual approval

## Account and application checks

- [ ] Maintainer manually confirms multi-factor authentication on GitHub
- [ ] Maintainer creates or confirms a SignPath account with multi-factor authentication
- [ ] Maintainer reviews and authorizes the personal and project information entered in the application
- [ ] Any identity details privately required by SignPath remain outside public repository and release literature
- [ ] SignPath Foundation accepts the application

## First signed release gate

- [ ] Signing input is the successful tagged public workflow artifact, not a local replacement
- [ ] Approver verifies source commit, version, filenames, hashes, SBOM, tests, and release notes
- [ ] Final executable and installer show a valid timestamped SignPath Foundation Authenticode signature
- [ ] Final files pass malware scanning, install, launch, policy read, guarded action, tray, and uninstall tests
- [ ] Release notes accurately label the signer and do not imply broader security guarantees
