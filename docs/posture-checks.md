# Startup posture checks

| Check | Protected state | Action in 0.1.x |
| --- | --- | --- |
| Device-associated app downloads | Network device metadata retrieval blocked | Per-card PROTECT/UNPROTECT on supported editions |
| Device Installation Settings fallback | Metadata retrieval preference disabled | Read-only; unavailable when machine policy overrides it |
| Drivers in Windows Update | Driver-classified packages excluded from quality updates | Per-card PROTECT/UNPROTECT on supported editions |
| Windows Update approval | Notify before downloading available updates | Per-card PROTECT/UNPROTECT on supported editions |
| Store app auto-updates | Automatic Store app updating disabled | Per-card PROTECT/UNPROTECT on supported editions |
| Microsoft Store access | Store application blocked | Read-only and unavailable where Microsoft does not support the policy |

Every result includes a plain-language explanation, state, current evidence, policy source, Windows applicability, guidance, and Microsoft documentation. The scanner treats unreadable settings as unknown, never protected.

## Guardrails

- Device-metadata protection does not block the essential initial Plug and Play driver.
- Window Lock never removes a driver or vendor application.
- Recent installations are a limited, read-only view; uninstall remains a separate explicit action outside Window Lock.
- Window Lock refuses to overwrite policy it does not own.
- UNPROTECT is offered only when Window Lock recorded ownership and the current value still matches.
- Unsupported editions and policies are marked unavailable.
