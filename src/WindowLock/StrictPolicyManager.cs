using Microsoft.Win32;

namespace WindowLock;

public static class StrictPolicyManager
{
    private sealed record Setting(string Id, string Path, string Name, int Value);

    private const string BackupRoot = @"SOFTWARE\WindowLock\Backup\Strict";
    private static readonly Setting[] Settings =
    [
        new("Drivers", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1),
        new("UpdateMode", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2),
        new("StoreUpdates", @"SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", 2)
    ];

    public static bool OwnsAny => Settings.Any(Owns);
    public static bool AllProtected => Settings.All(setting => Read(setting) == setting.Value);
    public static bool OwnsSetting(string id) => Settings.Any(setting => setting.Id == id && Owns(setting));

    public static PolicyChangeResult Apply()
    {
        if (!PolicyManager.IsAdministrator) return PolicyChangeResult.NotAdministrator;
        if (!PolicyManager.IsSupportedEdition) return PolicyChangeResult.UnsupportedEdition;

        try
        {
            using (var updateKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
            {
                var noAutoUpdate = updateKey?.GetValue("NoAutoUpdate");
                if (noAutoUpdate is not null && Convert.ToInt32(noAutoUpdate) != 0)
                    return PolicyChangeResult.ExternallyConfigured;
            }
            foreach (var setting in Settings)
            {
                var current = Read(setting);
                if (current == setting.Value) continue;
                if (current is not null || Owns(setting)) return PolicyChangeResult.ExternallyConfigured;
            }

            var changed = new List<Setting>();
            try
            {
                foreach (var setting in Settings.Where(setting => Read(setting) != setting.Value))
                {
                    using (var backup = Registry.LocalMachine.CreateSubKey($@"{BackupRoot}\{setting.Id}", true))
                    {
                        backup.SetValue("Owned", 1, RegistryValueKind.DWord);
                        backup.SetValue("HadOriginal", 0, RegistryValueKind.DWord);
                        backup.SetValue("AppliedValue", setting.Value, RegistryValueKind.DWord);
                        backup.SetValue("CreatedUtc", DateTime.UtcNow.ToString("O"), RegistryValueKind.String);
                    }
                    changed.Add(setting);
                    using (var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true))
                        policy.SetValue(setting.Name, setting.Value, RegistryValueKind.DWord);
                    if (Read(setting) != setting.Value) throw new InvalidOperationException($"Windows did not retain {setting.Name}.");
                }
                return changed.Count == 0 ? PolicyChangeResult.AlreadyInDesiredState : PolicyChangeResult.Success;
            }
            catch
            {
                RollBackNewSettings(changed);
                return PolicyChangeResult.VerificationFailed;
            }
        }
        catch { return PolicyChangeResult.Failed; }
    }

    public static PolicyChangeResult ApplyOne(string id)
    {
        if (!PolicyManager.IsAdministrator) return PolicyChangeResult.NotAdministrator;
        if (!PolicyManager.IsSupportedEdition) return PolicyChangeResult.UnsupportedEdition;
        try
        {
            var setting = Settings.SingleOrDefault(candidate => candidate.Id == id);
            if (setting is null) return PolicyChangeResult.Failed;
            if (setting.Id == "UpdateMode")
            {
                using var updateKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
                var noAutoUpdate = updateKey?.GetValue("NoAutoUpdate");
                if (noAutoUpdate is not null && Convert.ToInt32(noAutoUpdate) != 0)
                    return PolicyChangeResult.ExternallyConfigured;
            }
            var current = Read(setting);
            if (current == setting.Value) return PolicyChangeResult.AlreadyInDesiredState;
            if (current is not null || Owns(setting)) return PolicyChangeResult.ExternallyConfigured;

            using (var backup = Registry.LocalMachine.CreateSubKey($@"{BackupRoot}\{setting.Id}", true))
            {
                backup.SetValue("Owned", 1, RegistryValueKind.DWord);
                backup.SetValue("HadOriginal", 0, RegistryValueKind.DWord);
                backup.SetValue("AppliedValue", setting.Value, RegistryValueKind.DWord);
                backup.SetValue("CreatedUtc", DateTime.UtcNow.ToString("O"), RegistryValueKind.String);
            }
            try
            {
                using var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true);
                policy.SetValue(setting.Name, setting.Value, RegistryValueKind.DWord);
                if (Read(setting) != setting.Value) throw new InvalidOperationException($"Windows did not retain {setting.Name}.");
                return PolicyChangeResult.Success;
            }
            catch
            {
                RollBackNewSettings([setting]);
                return PolicyChangeResult.VerificationFailed;
            }
        }
        catch { return PolicyChangeResult.Failed; }
    }

    public static PolicyChangeResult Restore()
    {
        if (!PolicyManager.IsAdministrator) return PolicyChangeResult.NotAdministrator;
        try
        {
            var owned = Settings.Where(Owns).ToList();
            if (owned.Count == 0) return PolicyChangeResult.NotOwned;
            if (owned.Any(setting => Read(setting) != setting.Value)) return PolicyChangeResult.ExternallyConfigured;

            var deleted = new List<Setting>();
            foreach (var setting in owned)
            {
                using var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true);
                policy.DeleteValue(setting.Name, false);
                deleted.Add(setting);
                if (Read(setting) is not null)
                {
                    Reapply(deleted);
                    return PolicyChangeResult.VerificationFailed;
                }
            }
            foreach (var setting in owned)
                Registry.LocalMachine.DeleteSubKeyTree($@"{BackupRoot}\{setting.Id}", false);
            using var backupRoot = Registry.LocalMachine.OpenSubKey(BackupRoot);
            if (backupRoot?.SubKeyCount == 0)
            {
                backupRoot.Dispose();
                Registry.LocalMachine.DeleteSubKeyTree(BackupRoot, false);
            }
            return PolicyChangeResult.Success;
        }
        catch { return PolicyChangeResult.Failed; }
    }

    public static PolicyChangeResult RestoreOne(string id)
    {
        if (!PolicyManager.IsAdministrator) return PolicyChangeResult.NotAdministrator;
        try
        {
            var setting = Settings.SingleOrDefault(candidate => candidate.Id == id);
            if (setting is null || !Owns(setting)) return PolicyChangeResult.NotOwned;
            if (Read(setting) != setting.Value) return PolicyChangeResult.ExternallyConfigured;

            using (var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true))
                policy.DeleteValue(setting.Name, false);
            if (Read(setting) is not null)
            {
                Reapply([setting]);
                return PolicyChangeResult.VerificationFailed;
            }
            Registry.LocalMachine.DeleteSubKeyTree($@"{BackupRoot}\{setting.Id}", false);
            using var backupRoot = Registry.LocalMachine.OpenSubKey(BackupRoot);
            if (backupRoot?.SubKeyCount == 0)
            {
                backupRoot.Dispose();
                Registry.LocalMachine.DeleteSubKeyTree(BackupRoot, false);
            }
            return PolicyChangeResult.Success;
        }
        catch { return PolicyChangeResult.Failed; }
    }

    private static int? Read(Setting setting)
    {
        using var key = Registry.LocalMachine.OpenSubKey(setting.Path);
        var value = key?.GetValue(setting.Name);
        return value is null ? null : Convert.ToInt32(value);
    }

    private static bool Owns(Setting setting)
    {
        using var backup = Registry.LocalMachine.OpenSubKey($@"{BackupRoot}\{setting.Id}");
        return Convert.ToInt32(backup?.GetValue("Owned") ?? 0) == 1;
    }

    private static void RollBackNewSettings(IEnumerable<Setting> settings)
    {
        foreach (var setting in settings.Reverse())
        {
            try
            {
                using var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true);
                if (Read(setting) == setting.Value) policy.DeleteValue(setting.Name, false);
                Registry.LocalMachine.DeleteSubKeyTree($@"{BackupRoot}\{setting.Id}", false);
            }
            catch { }
        }
    }

    private static void Reapply(IEnumerable<Setting> settings)
    {
        foreach (var setting in settings)
        {
            try
            {
                using var policy = Registry.LocalMachine.CreateSubKey(setting.Path, true);
                policy.SetValue(setting.Name, setting.Value, RegistryValueKind.DWord);
            }
            catch { }
        }
    }
}
