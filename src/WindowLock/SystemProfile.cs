using Microsoft.Win32;

namespace WindowLock;

public sealed record SystemProfile(string Summary, string Detail);

public static class SystemProfileScanner
{
    public static string EditionId
    {
        get
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key?.GetValue("EditionID")?.ToString() ?? "Unknown edition";
        }
    }

    public static bool IsEnterpriseClassEdition =>
        EditionId.Contains("Enterprise", StringComparison.OrdinalIgnoreCase) ||
        EditionId.Contains("Education", StringComparison.OrdinalIgnoreCase) ||
        EditionId.Contains("IoT", StringComparison.OrdinalIgnoreCase);

    public static SystemProfile Scan()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var displayVersion = key?.GetValue("DisplayVersion")?.ToString() ?? "unknown version";
            var build = key?.GetValue("CurrentBuildNumber")?.ToString() ?? Environment.OSVersion.Version.Build.ToString();
            var product = int.TryParse(build, out var buildNumber) && buildNumber >= 22000 ? "Windows 11" : "Windows 10";
            var architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            var elevation = PolicyManager.IsAdministrator ? "Administrator" : "Standard user";
            return new($"{product} {displayVersion} · build {build} · {architecture}", $"Edition: {EditionId} · Current access: {elevation}");
        }
        catch (Exception ex)
        {
            return new("Windows details unavailable", ex.Message);
        }
    }
}
