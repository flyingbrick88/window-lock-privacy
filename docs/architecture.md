# Proposed architecture

## Components

### Future protected Windows service

Runs with the minimum required system privileges, owns policy enforcement, validates approval responses, performs narrowly scoped installs, restores the locked state, and writes tamper-evident audit events.

### Current standard-user application and future approval UI

The current UI displays status and starts a narrow elevated command in the same application after explicit confirmation. A future approval UI would send package-specific decisions to a protected service over authenticated local IPC.

### Policy adapters

- **Windows Update adapter:** configures notification-before-download behavior and separates driver updates.
- **Device adapter:** uses device-installation restrictions with an allow-list based on device instance IDs, hardware IDs, and setup classes.
- **Application-control adapter:** uses AppLocker or Windows Defender Application Control where supported to constrain executables, scripts, Windows Installer files, and packaged apps.
- **Store adapter:** blocks Store acquisition and automatic updates while locked.
- **Elevation adapter:** ensures ordinary users cannot bypass control through an administrator token or an alternate installer path.
- **Inventory adapter:** compares installed software, services, drivers, scheduled tasks, and packages before and after an approved operation.

### Posture engine

Runs on every application launch. Each check returns its effective value, policy source when identifiable, Windows-edition support, evidence, state, and remediation availability. Results are displayed before any elevation request.

The first and highest-confidence check is:

- Group Policy: `Computer Configuration > Administrative Templates > System > Device Installation > Prevent automatic download of applications associated with device metadata`
- Policy registry value: `HKLM\SOFTWARE\Policies\Microsoft\Windows\Device Metadata\PreventDeviceMetadataFromNetwork`
- Protected value: enabled / `1`

Window Lock must prefer supported policy locations over undocumented service disabling or permission changes.

### Remediation engine

The current build confirms the selected field and launches the same application binary in a narrow command mode through UAC. It refuses to overwrite an explicitly configured unowned value, records ownership and backup metadata, applies one policy, reads it back, and offers guarded restoration. Current beta builds are not code signed; a signed executable and separately signed privileged helper remain future hardening work.

## Future per-item approval sequence

1. Detect an offered update, blocked installer, package request, or newly connected device.
2. Resolve its identity and collect signature, publisher, source, version, and hash where available.
3. Show a non-elevated approval request.
4. On approval, create a narrow, expiring authorization bound to that identity.
5. Permit or initiate only the approved operation through the protected service.
6. Verify the result and immediately restore the locked policy.
7. Record success, failure, side effects, and the final enforcement state.

## Engineering constraints

- Built-in Windows policy is the enforcement foundation; filesystem watching alone is insufficient and occurs too late.
- Blocking Microsoft Store does not block `winget` or every other acquisition path, so application control and least privilege are also required.
- Device policy can prevent unapproved devices and driver updates, but broad rules must be tested carefully to avoid disabling essential keyboard, storage, display, or network hardware.
- Policy deployment must start in audit/test mode on a disposable Windows virtual machine before enforcement is offered on a real computer.
- Windows edition differences require capability detection and may require different enforcement backends.

## First implementation milestone

Complete Windows 10/11 Home and Pro VM coverage for the startup scanner and reversible per-card actions. Then add managed-device detection, broader automated integration tests, signed packaging, and only afterward consider device allow-listing or application-control enforcement.
