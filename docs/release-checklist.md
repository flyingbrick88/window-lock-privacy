# Release checklist

Publication was explicitly authorized on 14 August 2026. This checklist records the pre-release review and known limitations.

- [x] Release build succeeds without warnings
- [x] Startup posture check implemented
- [x] System-tray behavior implemented
- [x] Original logo asset added
- [x] Build, contribution, and security documentation added
- [x] Implement narrow elevated Recommended policy command mode
- [x] Implement backup, verification, and guarded rollback
- [x] Add automated definition, text, and non-elevated command tests
- [x] Produce a self-contained Windows x64 executable
- [x] Install per-user Start Menu and startup shortcuts
- [x] Verify installed executable matches reviewed build by SHA-256
- [x] Exercise accessible GUI controls and rerun the read-only scanner in the self-contained build
- [x] Verify non-elevated apply/restore return code 40 without changing monitored registry values
- [x] Add Windows system profile, policy source, applicability, and Microsoft documentation links
- [x] Add effective Device Installation Settings fallback and edition-aware Store access checks
- [x] Add read-only recent traditional installation inventory with explicit coverage limits
- [x] Add guarded Strict apply/restore for driver updates, update download notification, and Store updates
- [x] Replace global protection controls with independent per-card PROTECT and UNPROTECT actions
- [x] Sort cards by action priority, add unavailable visibility control, and apply full-card state colors
- [x] Apply shared rounded button styling and remove user-facing Recommended terminology
- [x] Add live last-checked age and manual refresh completion feedback
- [x] Add creator, licence, copyright, account, and accurate privacy disclosure
- [x] Remove raw registry evidence from card faces and add full-width status banners
- [ ] Complete elevated Strict apply/restore integration test with locally approved UAC
- [ ] Complete elevated apply/restore integration test with locally approved UAC
- [ ] Test Windows 10/11 Home and Pro in disposable virtual machines
- [x] Add executable icon and user-friendly Inno Setup packaging
- [x] Add PolyForm Noncommercial 1.0.0 and accurately describe it as source-available, not OSI open source
- [ ] Configure private vulnerability reporting after repository creation
- [ ] Code-sign production executables and installer
- [ ] Create reviewed local commit
- [ ] Create GitHub repository and push after all local release checks pass
