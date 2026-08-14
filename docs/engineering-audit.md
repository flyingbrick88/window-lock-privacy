# Engineering audit

## Corrected findings

- The original “nothing installs without approval” statement was not achievable with the four registry checks and is now labeled as a long-term vision.
- Device-metadata protection blocks applications associated with device metadata; it does not block essential Plug and Play drivers or every vendor delivery mechanism.
- `ExcludeWUDriversInQualityUpdate=1` excludes driver-classified content from quality updates; it does not prevent initial device-driver installation.
- `AUOptions=2` means notify before download in Configure Automatic Updates; it is not a per-update third-party approval broker.
- Store `AutoDownload=2` turns off automatic Store app updates for all Store apps; it does not create per-package approval and can delay security fixes.
- Microsoft documents the device-metadata policy for Pro, Enterprise, Education, and IoT Enterprise, not Home. Home must be shown as unavailable rather than protected.
- A present policy value owned by another administrator or management tool must not be overwritten.
- Tray exit now permits the WPF window to close, and the generated tray icon is cloned before its source bitmap is disposed.
- Earlier profile wording was replaced by independent per-card controls without implying complete installation control.
- Apply refuses to overwrite a changed or missing policy value when an old Window Lock ownership record exists.
- Upgrade and uninstall scripts identify the versioned installed executable by path instead of assuming its process name is `WindowLock`.
- The self-contained release returns exit code 40 immediately in non-elevated apply and restore modes, with monitored policy and backup values unchanged.
- Single-file publishing explicitly embeds native runtime libraries for extraction, preventing WPF startup failures on machines without a separate desktop runtime.
- Startup now reports Windows build, edition, architecture, and current privilege level; posture details identify source, applicability, and Microsoft documentation.
- A separate fallback check avoids confusing the user-facing Device Installation Settings preference with the overriding machine policy.
- Store access is separate from Store automatic updating and is marked unavailable outside Microsoft-documented supported editions.
- Recent-installation inventory is deliberately limited to dated traditional uninstall records and states that Store packages and undated installers are omitted.
- Strict controls have a distinct preview/apply flow for driver updates, update download notification, and Store updates; each owned protected field exposes its own red unprotect action instead of a global rollback button.
- Global Recommended and Strict buttons were removed; each supported card now applies or removes only its own policy through guarded PROTECT and UNPROTECT actions.
- Cards are sorted unprotected, unknown, protected, then unavailable; unavailable cards can be hidden, and state colors cover each complete card.
- All buttons share a rounded template, and user-facing Recommended terminology was removed from the dashboard and tray.
- Startup and manual scans set a live last-checked timestamp; manual refresh supplies a two-second completion confirmation without changing policy.
- The top panel identifies flyingbrick88, the creation date, free-use status, no-account design, local backup exception, and no-telemetry/no-remote-data behavior.
- Raw registry evidence remains in More information but is removed from card summaries; each card now begins with a large full-width state banner.

## Remaining release limitations

- No domain/MDM source attribution beyond refusing an explicitly configured unowned value.
- No Windows 10 Home fallback is implemented.
- No monitoring of actual package installation, services, scheduled tasks, or network telemetry yet.
- No production code-signing certificate or installer signing.
- No disposable-VM coverage across all supported Windows builds and editions yet.
- No per-item software, driver, Store, or package-manager approval broker.
