using Microsoft.Win32;
using System.Globalization;

namespace WindowLock;

public sealed record InstallActivity(string Installed, string Name, string Publisher, string Version, string Scope);

public static class ActivityScanner
{
    private const string UninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    public static IReadOnlyList<InstallActivity> ScanRecent(int days = 90)
    {
        var cutoff = DateTime.Today.AddDays(-days);
        var results = new List<InstallActivity>();
        ReadHive(RegistryHive.LocalMachine, RegistryView.Registry64, "All users", cutoff, results);
        ReadHive(RegistryHive.LocalMachine, RegistryView.Registry32, "All users", cutoff, results);
        ReadHive(RegistryHive.CurrentUser, RegistryView.Registry64, "Current user", cutoff, results);
        ReadHive(RegistryHive.CurrentUser, RegistryView.Registry32, "Current user", cutoff, results);
        return results
            .DistinctBy(item => (item.Name, item.Version, item.Scope))
            .OrderByDescending(item => item.Installed)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ReadHive(RegistryHive hive, RegistryView view, string scope, DateTime cutoff, List<InstallActivity> results)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var uninstall = baseKey.OpenSubKey(UninstallPath);
            if (uninstall is null) return;
            foreach (var subKeyName in uninstall.GetSubKeyNames())
            {
                using var item = uninstall.OpenSubKey(subKeyName);
                var name = item?.GetValue("DisplayName")?.ToString();
                var rawDate = item?.GetValue("InstallDate")?.ToString();
                if (string.IsNullOrWhiteSpace(name) || !DateTime.TryParseExact(rawDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var installed) || installed < cutoff)
                    continue;
                results.Add(new(installed.ToString("yyyy-MM-dd"), name, item?.GetValue("Publisher")?.ToString() ?? "Not listed", item?.GetValue("DisplayVersion")?.ToString() ?? "Not listed", scope));
            }
        }
        catch { }
    }
}
