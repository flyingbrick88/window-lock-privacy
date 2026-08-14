using Microsoft.Win32;
using System.Security.Principal;

namespace WindowLock;

public enum PolicyChangeResult
{
    Success = 0,
    AlreadyInDesiredState = 10,
    NotOwned = 11,
    ExternallyConfigured = 20,
    VerificationFailed = 30,
    NotAdministrator = 40,
    UnsupportedEdition = 50,
    Failed = 100
}

public static class PolicyManager
{
    public const string PolicyPath = @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata";
    public const string PolicyName = "PreventDeviceMetadataFromNetwork";
    private const string BackupPath = @"SOFTWARE\WindowLock\Backup\DeviceMetadata";
    private const int ProtectedValue = 1;

    public static bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    public static bool IsSupportedEdition
    {
        get
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var edition = key?.GetValue("EditionID")?.ToString() ?? string.Empty;
            return !edition.Contains("Core", StringComparison.OrdinalIgnoreCase) &&
                   !edition.Contains("Home", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static bool OwnsRecommendedPolicy()
    {
        using var backup = Registry.LocalMachine.OpenSubKey(BackupPath);
        return Convert.ToInt32(backup?.GetValue("Owned") ?? 0) == 1;
    }

    public static PolicyChangeResult ApplyRecommended()
    {
        if (!IsAdministrator) return PolicyChangeResult.NotAdministrator;
        if (!IsSupportedEdition) return PolicyChangeResult.UnsupportedEdition;

        try
        {
            using var existingKey = Registry.LocalMachine.OpenSubKey(PolicyPath);
            var existing = existingKey?.GetValue(PolicyName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            var existingKind = existing is null ? RegistryValueKind.None : existingKey!.GetValueKind(PolicyName);

            if (existing is not null && existingKind == RegistryValueKind.DWord && Convert.ToInt32(existing) == ProtectedValue)
                return PolicyChangeResult.AlreadyInDesiredState;

            var ownsPolicy = OwnsRecommendedPolicy();
            if (ownsPolicy)
            {
                using var backup = Registry.LocalMachine.OpenSubKey(BackupPath);
                var appliedValue = Convert.ToInt32(backup?.GetValue("AppliedValue") ?? ProtectedValue);
                if (existing is null || existingKind != RegistryValueKind.DWord || Convert.ToInt32(existing) != appliedValue)
                    return PolicyChangeResult.ExternallyConfigured;
            }
            else if (existing is not null)
                return PolicyChangeResult.ExternallyConfigured;

            using (var backup = Registry.LocalMachine.CreateSubKey(BackupPath, true))
            {
                if (Convert.ToInt32(backup.GetValue("Owned") ?? 0) != 1)
                {
                    backup.SetValue("HadOriginal", existing is null ? 0 : 1, RegistryValueKind.DWord);
                    if (existing is not null)
                    {
                        backup.SetValue("OriginalKind", (int)existingKind, RegistryValueKind.DWord);
                        backup.SetValue("OriginalValue", existing, existingKind);
                    }
                    backup.SetValue("Owned", 1, RegistryValueKind.DWord);
                    backup.SetValue("AppliedValue", ProtectedValue, RegistryValueKind.DWord);
                    backup.SetValue("CreatedUtc", DateTime.UtcNow.ToString("O"), RegistryValueKind.String);
                }
            }

            using (var policy = Registry.LocalMachine.CreateSubKey(PolicyPath, true))
                policy.SetValue(PolicyName, ProtectedValue, RegistryValueKind.DWord);

            return ReadPolicyValue() == ProtectedValue ? PolicyChangeResult.Success : RollbackAfterFailure();
        }
        catch
        {
            try
            {
                if (OwnsRecommendedPolicy()) RestoreRecommended();
            }
            catch { }
            return PolicyChangeResult.Failed;
        }
    }

    public static PolicyChangeResult RestoreRecommended()
    {
        if (!IsAdministrator) return PolicyChangeResult.NotAdministrator;
        try
        {
            using var backup = Registry.LocalMachine.OpenSubKey(BackupPath);
            if (Convert.ToInt32(backup?.GetValue("Owned") ?? 0) != 1) return PolicyChangeResult.NotOwned;
            if (ReadPolicyValue() != Convert.ToInt32(backup!.GetValue("AppliedValue") ?? ProtectedValue))
                return PolicyChangeResult.ExternallyConfigured;

            var hadOriginal = Convert.ToInt32(backup.GetValue("HadOriginal") ?? 0) == 1;
            using (var policy = Registry.LocalMachine.CreateSubKey(PolicyPath, true))
            {
                if (hadOriginal)
                {
                    var kind = (RegistryValueKind)Convert.ToInt32(backup.GetValue("OriginalKind"));
                    policy.SetValue(PolicyName, backup.GetValue("OriginalValue")!, kind);
                }
                else policy.DeleteValue(PolicyName, false);
            }

            var restored = hadOriginal ? ReadPolicyValue() == Convert.ToInt32(backup.GetValue("OriginalValue")) : ReadPolicyValue() is null;
            if (!restored) return PolicyChangeResult.VerificationFailed;
            Registry.LocalMachine.DeleteSubKeyTree(BackupPath, false);
            return PolicyChangeResult.Success;
        }
        catch { return PolicyChangeResult.Failed; }
    }

    public static int? ReadPolicyValue()
    {
        using var key = Registry.LocalMachine.OpenSubKey(PolicyPath);
        var value = key?.GetValue(PolicyName);
        return value is null ? null : Convert.ToInt32(value);
    }

    private static PolicyChangeResult RollbackAfterFailure()
    {
        var restored = RestoreRecommended();
        return restored == PolicyChangeResult.Success ? PolicyChangeResult.VerificationFailed : PolicyChangeResult.Failed;
    }
}
