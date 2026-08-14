# Privacy

Window Lock has no accounts, analytics, advertising, telemetry, cloud sync, or remote reporting. It does not send user, application, device, or diagnostic information to flyingbrick88 or any third party.

The app reads Windows version information, selected policy values, and limited local installation records to display status. Recent-installation results remain in memory and are not saved. When Window Lock applies a policy, it stores only the original policy value and ownership metadata under `HKEY_LOCAL_MACHINE\SOFTWARE\WindowLock\Backup`; this makes the matching **UNPROTECT** operation safe. These backups remain on the computer.

The **Support development**, **Send feedback**, and Microsoft documentation buttons open pages in the default browser. Nothing is submitted automatically. Ko-fi, GitHub, and Microsoft apply their own privacy terms once their sites are opened.

Uninstalling Window Lock removes the app but deliberately leaves Windows policy values and local backup metadata unchanged. Reinstall Window Lock and use the relevant **UNPROTECT** button before uninstalling if you want it to restore a setting that it owns.
