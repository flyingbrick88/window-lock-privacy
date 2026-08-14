using Microsoft.Win32;

namespace WindowLock;

public enum CheckState { Protected, Unprotected, Unavailable, Unknown }

public sealed record ProtectionCheck(
    string Title,
    string Description,
    CheckState State,
    string Evidence,
    bool Recommended,
    string ProtectedMeaning,
    string UnprotectedMeaning,
    string Guidance,
    string PolicySource,
    string Applicability,
    string DocumentationUri);

public static class ProtectionScanner
{
    private const string DeviceMetadataKey = @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata";
    private const string DeviceMetadataFallbackKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata";
    private const string UpdateKey = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
    private const string AutoUpdateKey = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
    private const string StoreKey = @"SOFTWARE\Policies\Microsoft\WindowsStore";

    public static IReadOnlyList<ProtectionCheck> Scan() =>
    [
        DwordCheck("Device-associated app downloads", "Checks Microsoft’s policy for applications associated with device metadata.", DeviceMetadataKey, "PreventDeviceMetadataFromNetwork", 1, true,
            "Windows is prevented from retrieving device metadata that can trigger optional manufacturer companion-app downloads. Your display, keyboard, mouse, storage, and other hardware can still use essential Plug and Play drivers.",
            "Connecting hardware may allow Windows to retrieve manufacturer metadata and silently acquire an associated Store app. That app may add background services, startup behavior, promotions, account sign-in, or telemetry beyond what is required for the hardware to operate.",
            "Turn this protection on unless you deliberately want Windows to obtain manufacturer companion apps automatically. You can still install a trusted vendor utility manually later.",
            "Windows 10 version 1809 and later; Pro, Enterprise, Education, and IoT Enterprise",
            "https://learn.microsoft.com/windows/client-management/mdm/policy-csp-deviceinstallation#preventdevicemetadatafromnetwork"),
        DeviceInstallationFallbackCheck(),
        DwordCheck("Drivers in Windows Update", "Keeps driver packages separate from routine quality updates.", UpdateKey, "ExcludeWUDriversInQualityUpdate", 1, false,
            "Driver-classified packages are excluded from normal Windows quality updates. Security and operating-system updates remain available, while driver changes can be considered separately.",
            "Windows Update may include and install new driver versions alongside routine updates. Updated drivers can fix problems, but they may also change device behavior or bring vendor components without a separate decision.",
            "Use this when you prefer to review driver updates separately. Keep existing working drivers, and obtain necessary updates from Windows Optional updates or the hardware manufacturer after checking the publisher and purpose.",
            "Windows 10 version 1607 and later; Pro, Enterprise, Education, and IoT Enterprise",
            "https://learn.microsoft.com/windows/client-management/mdm/policy-csp-update#excludewudriversinqualityupdate"),
        DwordCheck("Windows Update approval", "Reports whether Windows asks before downloading available updates.", AutoUpdateKey, "AUOptions", 2, false,
            "Windows notifies you before downloading available updates. You decide when the download begins, which reduces unexpected network use and unattended change.",
            "Windows can download updates automatically according to its configured servicing behavior. Installation and restart behavior may still depend on active hours and other Windows policies.",
            "Notification-before-download provides control, but do not postpone security updates indefinitely. Review and install Microsoft security updates promptly at a convenient time.",
            "Windows 10 and Windows 11 where Configure Automatic Updates policy applies",
            "https://learn.microsoft.com/windows/deployment/update/waas-wu-settings"),
        DwordCheck("Store app auto-updates", "Reports whether Microsoft Store apps can change automatically in the background.", StoreKey, "AutoDownload", 2, false,
            "Microsoft Store apps do not update automatically in the background. Version changes occur when you deliberately check for and approve updates.",
            "Installed Store apps may update automatically. Updates can contain important security fixes, but they can also alter features, permissions, background behavior, or user experience without a separate prompt.",
            "Choose protection if you want to review Store changes manually, then check for updates regularly so security fixes are not missed. This setting does not by itself block every Store acquisition or package-manager installation path.",
            "Windows 10 and Windows 11; Pro, Enterprise, Education, and IoT Enterprise",
            "https://learn.microsoft.com/windows/client-management/mdm/policy-csp-applicationmanagement#allowappstoreautoupdate"),
        StoreAccessCheck()
    ];

    private static ProtectionCheck DwordCheck(string title, string description, string path, string name, int protectedValue, bool recommended, string protectedMeaning, string unprotectedMeaning, string guidance, string applicability, string documentationUri)
    {
        try
        {
            if (recommended && !PolicyManager.IsSupportedEdition)
                return new(title, description, CheckState.Unavailable, "Microsoft documents this policy for Pro, Enterprise, Education, and IoT Enterprise editions", recommended, protectedMeaning, unprotectedMeaning, guidance, "Not configured by Window Lock", applicability, documentationUri);
            using var key = Registry.LocalMachine.OpenSubKey(path);
            var value = key?.GetValue(name);
            if (value is null)
                return new(title, description, CheckState.Unprotected, $"{name} is not configured", recommended, protectedMeaning, unprotectedMeaning, guidance, "Windows default or user preference", applicability, documentationUri);
            var actual = Convert.ToInt32(value);
            var source = recommended && PolicyManager.OwnsRecommendedPolicy() ? "Machine policy set by Window Lock" : "Machine policy set by an administrator, organization, or another tool";
            return new(title, description, actual == protectedValue ? CheckState.Protected : CheckState.Unprotected, $"{name} = {actual}", recommended, protectedMeaning, unprotectedMeaning, guidance, source, applicability, documentationUri);
        }
        catch (Exception ex)
        {
            return new(title, description, CheckState.Unknown, ex.Message, recommended, protectedMeaning, unprotectedMeaning, guidance, "Could not determine", applicability, documentationUri);
        }
    }

    private static ProtectionCheck DeviceInstallationFallbackCheck()
    {
        const string title = "Device Installation Settings fallback";
        const string description = "Checks the built-in Hardware dialog preference used when no overriding machine policy exists.";
        const string protectedMeaning = "The fallback preference prevents retrieval of device metadata from the network when no machine policy overrides it.";
        const string unprotectedMeaning = "Without an overriding policy, the fallback preference allows Windows to retrieve device metadata and associated device applications.";
        const string guidance = "The supported machine policy is stronger because it overrides this preference. This card is read-only and helps explain the effective state before that policy is enabled.";
        const string applicability = "Windows 10 and Windows 11; overridden when the device-metadata machine policy is configured";
        const string docs = "https://learn.microsoft.com/windows/client-management/mdm/policy-csp-deviceinstallation#preventdevicemetadatafromnetwork";
        try
        {
            using var policy = Registry.LocalMachine.OpenSubKey(DeviceMetadataKey);
            var policyValue = policy?.GetValue("PreventDeviceMetadataFromNetwork");
            if (policyValue is not null)
                return new(title, description, CheckState.Unavailable, $"Overridden by machine policy value {Convert.ToInt32(policyValue)}", false, protectedMeaning, unprotectedMeaning, guidance, "Machine policy takes precedence", applicability, docs);
            using var fallback = Registry.LocalMachine.OpenSubKey(DeviceMetadataFallbackKey);
            var value = fallback?.GetValue("PreventDeviceMetadataFromNetwork");
            var actual = value is null ? 0 : Convert.ToInt32(value);
            return new(title, description, actual == 1 ? CheckState.Protected : CheckState.Unprotected, value is null ? "Fallback value is not configured" : $"Fallback value = {actual}", false, protectedMeaning, unprotectedMeaning, guidance, "Local Device Installation Settings preference", applicability, docs);
        }
        catch (Exception ex)
        {
            return new(title, description, CheckState.Unknown, ex.Message, false, protectedMeaning, unprotectedMeaning, guidance, "Could not determine", applicability, docs);
        }
    }

    private static ProtectionCheck StoreAccessCheck()
    {
        const string title = "Microsoft Store access";
        const string description = "Reports whether supported Windows policy denies access to the Microsoft Store application.";
        const string protectedMeaning = "The Store application is blocked, reducing one acquisition path. This does not block winget, sideloading, or traditional installers.";
        const string unprotectedMeaning = "Users can access the Microsoft Store and deliberately acquire applications. Automatic updating is reported separately.";
        const string guidance = "This is an advanced control. Blocking the Store can also prevent Store-delivered application updates.";
        const string applicability = "Enterprise, Education, and IoT Enterprise; not supported as an enforced machine policy on Pro";
        const string docs = "https://learn.microsoft.com/windows/client-management/mdm/policy-csp-admx-windowsstore#removewindowsstore_2";
        if (!SystemProfileScanner.IsEnterpriseClassEdition)
            return new(title, description, CheckState.Unavailable, "Microsoft does not document Store blocking as supported on this edition", false, protectedMeaning, unprotectedMeaning, guidance, "Not applicable", applicability, docs);
        return DwordCheck(title, description, StoreKey, "RemoveWindowsStore", 1, false, protectedMeaning, unprotectedMeaning, guidance, applicability, docs);
    }
}
