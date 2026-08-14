using WindowLock;

if (args.Length == 1 && args[0] == "--smoke-apply")
    return (int)PolicyManager.ApplyRecommended();
if (args.Length == 1 && args[0] == "--smoke-restore")
    return (int)PolicyManager.RestoreRecommended();
if (args.Length == 1 && args[0] == "--smoke-strict-apply")
    return (int)StrictPolicyManager.Apply();
if (args.Length == 1 && args[0] == "--smoke-strict-restore")
    return (int)StrictPolicyManager.Restore();

var failures = new List<string>();
var checks = ProtectionScanner.Scan();
var activities = ActivityScanner.ScanRecent();
var systemProfile = SystemProfileScanner.Scan();

Assert(checks.Count == 6, "exactly six posture checks are returned");
Assert(checks.Select(c => c.Title).Distinct(StringComparer.OrdinalIgnoreCase).Count() == checks.Count, "check titles are unique");
Assert(checks.Count(c => c.Recommended) == 1, "exactly one check belongs to Recommended Protection");
foreach (var check in checks)
{
    Assert(!string.IsNullOrWhiteSpace(check.Description), $"{check.Title} has a description");
    Assert(!string.IsNullOrWhiteSpace(check.ProtectedMeaning), $"{check.Title} explains Protected");
    Assert(!string.IsNullOrWhiteSpace(check.UnprotectedMeaning), $"{check.Title} explains Unprotected");
    Assert(!string.IsNullOrWhiteSpace(check.Guidance), $"{check.Title} provides guidance");
    Assert(!string.IsNullOrWhiteSpace(check.Evidence), $"{check.Title} provides evidence");
    Assert(!string.IsNullOrWhiteSpace(check.PolicySource), $"{check.Title} identifies its policy source");
    Assert(!string.IsNullOrWhiteSpace(check.Applicability), $"{check.Title} explains applicability");
    Assert(Uri.TryCreate(check.DocumentationUri, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps, $"{check.Title} has an HTTPS documentation link");
}
Assert(checks.Any(c => c.Title == "Device Installation Settings fallback"), "fallback device setting is reported");
Assert(checks.Any(c => c.Title == "Microsoft Store access"), "Store access is reported separately from updates");
Assert(activities.All(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Installed)), "recent installation records have names and dates");
Assert(systemProfile.Summary.StartsWith("Windows 10", StringComparison.Ordinal) || systemProfile.Summary.StartsWith("Windows 11", StringComparison.Ordinal), "system profile identifies Windows 10 or Windows 11 from its build");
Assert(systemProfile.Detail.Contains("Edition:", StringComparison.Ordinal) && systemProfile.Detail.Contains("Current access:", StringComparison.Ordinal), "system profile reports edition and access level");
Assert(Enum.GetValues<PolicyChangeResult>().Distinct().Count() == Enum.GetValues<PolicyChangeResult>().Length, "policy result codes are unique");
Assert((int)PolicyChangeResult.NotAdministrator == 40, "non-elevated command mode has stable exit code 40");

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var mainXaml = File.ReadAllText(Path.Combine(projectRoot, "src", "WindowLock", "MainWindow.xaml"));
var mainCode = File.ReadAllText(Path.Combine(projectRoot, "src", "WindowLock", "MainWindow.xaml.cs"));
var infoXaml = File.ReadAllText(Path.Combine(projectRoot, "src", "WindowLock", "ProtectionInfoWindow.xaml"));
foreach (var expected in new[] { "Control selected automatic install paths.", "Recent installations", "Support development on Ko-fi", "Send feedback", "Update protection status", "Hide unavailable options", "ABOUT &amp; PRIVACY", "Built by flyingbrick88", "Created August 2026", "Licence: GNU GPL v3", "Open source", "No account required", "Compatibility: Windows 10 version 1809 or later and Windows 11 · x64", "No user identity or installed-app inventory is saved", "Window Lock sends no data" })
    Assert(mainXaml.Contains(expected, StringComparison.Ordinal), $"main interface contains {expected}");
Assert(mainCode.Contains("More information", StringComparison.Ordinal), "each generated posture card includes More information");
Assert(mainCode.Contains("https://github.com/flyingbrick88/window-lock-privacy/issues/new/choose", StringComparison.Ordinal), "feedback button opens the repository issue chooser");
Assert(mainCode.Contains("https://ko-fi.com/flyingbrick88", StringComparison.Ordinal), "support button opens the creator's Ko-fi page");
Assert(mainCode.Contains("Content = \"PROTECT\"", StringComparison.Ordinal), "actionable unprotected cards provide a per-field protect action");
Assert(mainCode.Contains("Content = \"UNPROTECT\"", StringComparison.Ordinal), "owned protected cards provide the requested per-field unprotect action");
Assert(!mainCode.Contains("Text = check.Evidence", StringComparison.Ordinal), "raw registry evidence is not shown on card summaries");
Assert(mainCode.Contains("Grid.SetColumnSpan(badge, 2)", StringComparison.Ordinal), "status banner spans the full card width");
Assert(mainCode.Contains("FontSize = 16", StringComparison.Ordinal) && mainCode.Contains("TextAlignment = TextAlignment.Center", StringComparison.Ordinal), "status banner is large and visually distinct");
Assert(!mainXaml.Contains("Recommended", StringComparison.OrdinalIgnoreCase) && !mainCode.Contains("Recommended protection", StringComparison.OrdinalIgnoreCase), "main interface contains no Recommended protection wording");
Assert(mainCode.Contains("CheckState.Unprotected => 0", StringComparison.Ordinal) && mainCode.Contains("CheckState.Protected => 2", StringComparison.Ordinal), "cards sort unprotected before protected and unavailable");
Assert(mainCode.Contains("_showUnavailable", StringComparison.Ordinal) && mainCode.Contains("ToggleUnavailable_Click", StringComparison.Ordinal), "unavailable cards can be shown or hidden");
Assert(mainCode.Contains("DispatcherTimer", StringComparison.Ordinal) && mainCode.Contains("Last checked", StringComparison.Ordinal), "last-checked age updates continuously");
Assert(mainCode.Contains("Protection status updated ✓", StringComparison.Ordinal), "manual status update provides completion feedback");
Assert(mainCode.Contains("RunChecks(updateTimestamp: false)", StringComparison.Ordinal), "display-only unavailable toggle does not reset the check timer");
Assert(!mainXaml.Contains("Restore previous setting", StringComparison.Ordinal) && !mainXaml.Contains("Restore strict settings", StringComparison.Ordinal), "global restore buttons are removed");
Assert(!mainXaml.Contains("Turn on protection", StringComparison.Ordinal) && !mainXaml.Contains("Turn on Strict controls", StringComparison.Ordinal) && !mainXaml.Contains("Preview changes", StringComparison.Ordinal), "global protection buttons are removed");
Assert(mainCode.Contains("All available fields are protected", StringComparison.Ordinal), "hero describes aggregate available protection without Recommended wording");
Assert(mainCode.Contains("ACTION NEEDED", StringComparison.Ordinal), "hero clearly reports when unprotected fields remain");
Assert(!mainCode.Contains("Windows information and unsupported options remain available below", StringComparison.Ordinal), "removed explanatory sentence is absent");
foreach (var expected in new[] { "When PROTECTED", "When UNPROTECTED", "Guidance", "Current Windows evidence", "Policy source", "Applies to", "Microsoft documentation", "Close" })
    Assert(infoXaml.Contains(expected, StringComparison.Ordinal), $"information dialog contains {expected}");
var activityXaml = File.ReadAllText(Path.Combine(projectRoot, "src", "WindowLock", "RecentActivityWindow.xaml"));
foreach (var expected in new[] { "Recent installations", "last 90 days", "Store packages", "Installed", "Application", "Publisher", "Version", "Scope" })
    Assert(activityXaml.Contains(expected, StringComparison.Ordinal), $"activity window contains {expected}");
var appXaml = File.ReadAllText(Path.Combine(projectRoot, "src", "WindowLock", "App.xaml"));
Assert(appXaml.Contains("CornerRadius=\"10\"", StringComparison.Ordinal), "shared button template uses rounded corners");

Exception? dialogConstructionError = null;
var dialogThread = new Thread(() =>
{
    try
    {
        var activityWindow = new RecentActivityWindow();
        var informationWindow = new ProtectionInfoWindow(checks[0]);
        activityWindow.Close();
        informationWindow.Close();
    }
    catch (Exception ex) { dialogConstructionError = ex; }
});
dialogThread.SetApartmentState(ApartmentState.STA);
dialogThread.Start();
dialogThread.Join();
Assert(dialogConstructionError is null, $"information and activity dialogs construct successfully: {dialogConstructionError?.Message}");

var installScript = File.ReadAllText(Path.Combine(projectRoot, "installer", "Install-WindowLock.ps1"));
var uninstallScript = File.ReadAllText(Path.Combine(projectRoot, "installer", "Uninstall-WindowLock.ps1"));
Assert(installScript.Contains("$_.Path", StringComparison.Ordinal), "installer stops the versioned executable by its path");
Assert(uninstallScript.Contains("StartsWith($destination", StringComparison.Ordinal), "uninstaller stops executables inside the install directory");

if (failures.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
    return 1;
}

Console.WriteLine($"Passed {checks.Count} check definitions and policy result validation.");
return 0;

void Assert(bool condition, string message)
{
    if (!condition) failures.Add($"FAIL: {message}");
}
