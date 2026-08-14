# Requirements

## Product vision

Window Lock aims to make it easy to prevent Windows from silently installing optional manufacturer companion apps associated with newly connected hardware. It separately reports broader Store-app, driver, and Windows Update exposure. Reporting a channel does not mean the current release controls it.

The application must not describe all vendor utilities as malicious. It reports package identity, publisher, permissions, startup behavior, services, and network activity where evidence is available so the user can make an informed decision.

## Startup posture test

On every application startup, run a read-only assessment before offering changes:

1. Detect Windows version, build, edition, architecture, and whether the current process is elevated.
2. Check whether automatic applications associated with device metadata are blocked.
3. Check the user-facing Device Installation Settings state.
4. Check whether drivers are included with Windows quality updates.
5. Check Windows Update download/install notification policy.
6. Check Microsoft Store access and automatic-update policy.
7. Check application-control availability and enforcement state.
8. Inventory recently installed AppX/MSIX packages, traditional applications, drivers, services, and scheduled tasks when accessible.
9. Label every check as Protected, Attention, Unavailable on this edition, or Unknown/error.
10. Never treat an unreadable setting as protected.

**Current implementation:** reports Windows version, display version, build, edition, architecture, elevation state, six policy/fallback checks, policy source, applicability, and dated traditional application records from the preceding 90 days. AppX/MSIX, driver, service, and scheduled-task history remain future inventory work.

The startup test must not require elevation for checks that an ordinary user can perform. Elevation is requested only when the user chooses to apply or restore a machine-wide setting.

## Independent controls and implementation status

### Device-associated app downloads

- Enable the Windows policy that prevents automatic download of applications associated with device metadata.
- Disable automatic manufacturer apps and custom device metadata through the corresponding built-in Device Installation setting.
- Leave essential Plug and Play driver installation available.
- Keep normal Microsoft security updates available.
- Explain that this targets optional companion software rather than the basic driver needed to operate the hardware.
- **Current implementation:** supported on Microsoft-documented Pro, Enterprise, Education, and IoT Enterprise editions; one elevated policy change with backup, verification, and guarded restoration.

### Other individually selectable controls

- Exclude driver-classified updates from Windows quality updates.
- Use notify-before-download behavior for Windows Update where the installed edition supports it.
- Disable automatic Microsoft Store app updates.
- Offer device allow-listing and broader application-control only through an advanced flow with recovery checks.
- **Current implementation:** separate guarded card actions apply the driver-update, notify-before-download, and Store auto-update controls on supported editions. Each previously unconfigured value is backed up, verified, and unprotected only when owned by Window Lock. Device allow-listing and application control remain advanced future work.

### Per-field unprotect

- Unprotect only settings previously changed by Window Lock.
- Show the exact field and a clear consequence warning before elevation.
- Do not overwrite unrelated administrator or organization policy.

## Future approval-broker safety properties

1. **Fail closed:** a crashed UI, stopped service, timeout, reboot, or loss of connectivity must not silently authorize an installation.
2. **Specific consent:** approval identifies the package, update, driver, or device. A generic unlimited “allow installs” mode is not acceptable.
3. **Short-lived authority:** an approval expires and cannot be reused for unrelated software.
4. **Authentic prompt:** only the protected local service can create an actionable approval request. The UI must resist prompt spoofing.
5. **Full audit trail:** record the request source, package identity, publisher/signature, decision, approving identity, policy changes, installation result, and restoration of the locked state.
6. **Safe recovery:** provide an offline, administrator-controlled recovery procedure that cannot become a routine bypass.
7. **No surprise reboot:** update approval and restart approval are separate decisions unless the user explicitly combines them.

## Roadmap installation channels

| Channel | Locked behavior | Approval behavior |
| --- | --- | --- |
| Windows Update | Notify before download; do not install automatically | User selects specific offered updates for download and installation |
| Windows Update drivers | Excluded from ordinary quality updates | Driver is reviewed and approved separately |
| Newly connected hardware | Unknown devices are denied by device-installation policy | Approve a device ID or instance ID and its driver package |
| Traditional installers | Unapproved executables, MSI packages, and scripts cannot run with install capability | Permit a verified installer for one execution |
| Microsoft Store / MSIX | Acquisition and automatic updating are blocked | Approve the identified package and version |
| Package managers | Unapproved package-manager execution or elevation is blocked | Approve the named package, source, and version |
| Application self-updaters | Updater executables and services cannot install unapproved code | Approve a signed update with an identified publisher and version |

## User-visible approval information

- Product and version
- Publisher and signature status
- Download or device source
- Hash when a concrete package is available
- Requested privileges
- Whether drivers, services, scheduled tasks, or restart are involved
- Clear choices: deny, approve once, or review details

## Compatibility target

- Windows 10 and Windows 11
- Edition and build capability detection at setup time
- A least-privilege daily user account; administrative operations go through the Window Lock broker

## Non-goals for the first version

- Malware detection or replacement of Microsoft Defender
- Silent remote approval
- Permanent disabling of Windows security updates
- Kernel modification or unsupported patching of Windows components
