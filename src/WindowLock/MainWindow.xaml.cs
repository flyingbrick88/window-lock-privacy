using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Threading;
using WpfBrush = System.Windows.Media.Brush;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfMessageBox = System.Windows.MessageBox;

namespace WindowLock;

public partial class MainWindow : Window
{
    private const string FeedbackUri = "https://github.com/flyingbrick88/window-lock-privacy/issues/new/choose";
    private const string SupportUri = "https://ko-fi.com/flyingbrick88";
    public event EventHandler? HideRequested;
    public event EventHandler<bool>? StatusChanged;
    private bool _allowExit;
    private bool _showUnavailable = true;
    private readonly DispatcherTimer _lastCheckedTimer;
    private DateTimeOffset? _lastCheckedAt;
    private int _feedbackGeneration;

    public MainWindow()
    {
        InitializeComponent();
        _lastCheckedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _lastCheckedTimer.Tick += (_, _) => UpdateLastCheckedDisplay();
        _lastCheckedTimer.Start();
        Loaded += (_, _) => RunChecks();
        Closing += (_, e) => { if (!_allowExit) { e.Cancel = true; HideRequested?.Invoke(this, EventArgs.Empty); } };
    }

    public void RunChecks(bool updateTimestamp = true)
    {
        var profile = SystemProfileScanner.Scan();
        SystemSummaryText.Text = profile.Summary;
        SystemDetailText.Text = profile.Detail;
        var checks = ProtectionScanner.Scan();
        var orderedChecks = checks
            .Where(check => _showUnavailable || check.State != CheckState.Unavailable)
            .OrderBy(check => check.State switch { CheckState.Unprotected => 0, CheckState.Unknown => 1, CheckState.Protected => 2, _ => 3 });
        ChecksPanel.Children.Clear();
        foreach (var check in orderedChecks) ChecksPanel.Children.Add(CreateCheckCard(check));
        var unprotectedCount = checks.Count(check => check.State == CheckState.Unprotected);
        var protectedCount = checks.Count(check => check.State == CheckState.Protected);
        SetHero(unprotectedCount, protectedCount);
        StatusChanged?.Invoke(this, unprotectedCount == 0);
        if (updateTimestamp)
        {
            _lastCheckedAt = DateTimeOffset.Now;
            UpdateLastCheckedDisplay();
        }
    }

    private Border CreateCheckCard(ProtectionCheck check)
    {
        var protectedState = check.State == CheckState.Protected;
        var unavailable = check.State == CheckState.Unavailable;
        var unknown = check.State == CheckState.Unknown;
        var caution = unknown || unavailable;
        var color = (WpfColor)WpfColorConverter.ConvertFromString(caution ? "#B26A00" : protectedState ? "#087F5B" : "#C92A2A");
        var soft = new SolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString(caution ? "#FFF8E1" : protectedState ? "#E6FCF5" : "#FFF0F0"));
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var info = new StackPanel();
        info.Children.Add(new TextBlock { Text = check.Title, FontSize = 17, FontWeight = FontWeights.SemiBold, Foreground = (WpfBrush)FindResource("Navy") });
        info.Children.Add(new TextBlock { Text = check.Description, FontSize = 14, Foreground = (WpfBrush)FindResource("Muted"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 20, 3) });
        var badge = new Border { Background = new SolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString(caution ? "#FFE8A3" : protectedState ? "#B7E4D5" : "#FFD1D1")), CornerRadius = new CornerRadius(12), Padding = new Thickness(16, 11, 16, 11), HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 16) };
        badge.Child = new TextBlock { Text = unavailable ? "UNAVAILABLE" : unknown ? "STATUS UNKNOWN" : protectedState ? "PROTECTED" : "UNPROTECTED", Foreground = new SolidColorBrush(color), FontWeight = FontWeights.Bold, FontSize = 16, TextAlignment = TextAlignment.Center };
        var moreInfo = new System.Windows.Controls.Button { Content = "More information", Background = new SolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString("#E9EEF3")), Foreground = (WpfBrush)FindResource("Navy"), FontSize = 13, Padding = new Thickness(14, 8, 14, 8), Margin = new Thickness(0, 9, 0, 0) };
        moreInfo.Click += (_, _) => new ProtectionInfoWindow(check) { Owner = this }.ShowDialog();
        var actions = new StackPanel { VerticalAlignment = VerticalAlignment.Center, MinWidth = 150 };
        actions.Children.Add(moreInfo);
        var protectCommand = GetProtectCommand(check);
        if (!protectedState && !caution && protectCommand is not null)
        {
            var protect = new System.Windows.Controls.Button { Content = "PROTECT", Background = (WpfBrush)FindResource("Green"), Foreground = System.Windows.Media.Brushes.White, FontSize = 12, Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 8, 0, 0) };
            protect.Click += (_, _) => Protect(check.Title, protectCommand);
            actions.Children.Add(protect);
        }
        var unprotectCommand = GetUnprotectCommand(check);
        if (protectedState && unprotectCommand is not null)
        {
            var unprotect = new System.Windows.Controls.Button { Content = "UNPROTECT", Background = new SolidColorBrush((WpfColor)WpfColorConverter.ConvertFromString("#C92A2A")), Foreground = System.Windows.Media.Brushes.White, FontSize = 12, Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 8, 0, 0) };
            unprotect.Click += (_, _) => Unprotect(check.Title, unprotectCommand);
            actions.Children.Add(unprotect);
        }
        grid.Children.Add(badge); Grid.SetColumnSpan(badge, 2);
        Grid.SetRow(info, 1); grid.Children.Add(info);
        Grid.SetRow(actions, 1); Grid.SetColumn(actions, 1); grid.Children.Add(actions);
        return new Border { Style = (Style)FindResource("Card"), Background = soft, BorderBrush = new SolidColorBrush(color), BorderThickness = new Thickness(1), Child = grid };
    }

    private void SetHero(int unprotectedCount, int protectedCount)
    {
        var protectedState = unprotectedCount == 0;
        HeroLabel.Text = "PROTECTION STATUS";
        HeroTitle.Text = protectedState ? "All available fields are protected" : $"{unprotectedCount} field{(unprotectedCount == 1 ? " needs" : "s need")} attention";
        HeroText.Text = $"{protectedCount} protected · {unprotectedCount} unprotected";
        HeroBadge.Background = (WpfBrush)FindResource(protectedState ? "GreenSoft" : "RedSoft");
        HeroBadgeText.Foreground = (WpfBrush)FindResource(protectedState ? "Green" : "Red");
        HeroBadgeText.Text = protectedState ? "PROTECTED" : "ACTION NEEDED";
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RunChecks();
        var generation = ++_feedbackGeneration;
        UpdateStatusButton.Content = "Protection status updated ✓";
        await Task.Delay(2000);
        if (generation == _feedbackGeneration) UpdateStatusButton.Content = "Update protection status";
    }

    private void UpdateLastCheckedDisplay()
    {
        if (_lastCheckedAt is null) return;
        var elapsed = DateTimeOffset.Now - _lastCheckedAt.Value;
        var age = elapsed.TotalSeconds < 5 ? "just now" : elapsed.TotalMinutes < 1 ? $"{(int)elapsed.TotalSeconds} seconds ago" : elapsed.TotalHours < 1 ? $"{(int)elapsed.TotalMinutes} minute{((int)elapsed.TotalMinutes == 1 ? string.Empty : "s")} ago" : $"at {_lastCheckedAt.Value:t}";
        LastCheckedText.Text = $"Last checked {age} · {_lastCheckedAt.Value:t}";
    }
    private void ToggleUnavailable_Click(object sender, RoutedEventArgs e)
    {
        _showUnavailable = !_showUnavailable;
        ToggleUnavailableButton.Content = _showUnavailable ? "Hide unavailable options" : "Show unavailable options";
        RunChecks(updateTimestamp: false);
    }
    private void RecentInstallations_Click(object sender, RoutedEventArgs e) => new RecentActivityWindow { Owner = this }.ShowDialog();
    private void Feedback_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(FeedbackUri) { UseShellExecute = true }); }
        catch (Exception ex) { WpfMessageBox.Show($"Windows could not open the feedback page.\n\n{FeedbackUri}\n\n{ex.Message}", "Send feedback", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void SupportDevelopment_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(SupportUri) { UseShellExecute = true }); }
        catch (Exception ex) { WpfMessageBox.Show($"Windows could not open the Ko-fi page.\n\n{SupportUri}\n\n{ex.Message}", "Support development", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private static string? GetProtectCommand(ProtectionCheck check) => check.Title switch
    {
        "Device-associated app downloads" when PolicyManager.IsSupportedEdition => "--apply-recommended",
        "Drivers in Windows Update" when PolicyManager.IsSupportedEdition => "--protect-drivers",
        "Windows Update approval" when PolicyManager.IsSupportedEdition => "--protect-updates",
        "Store app auto-updates" when PolicyManager.IsSupportedEdition => "--protect-store-updates",
        _ => null
    };

    private void Protect(string title, string command)
    {
        var answer = WpfMessageBox.Show($"Turn on protection for ‘{title}’?\n\nWindow Lock will change only this field, verify it, and will not overwrite a conflicting administrator or organization policy.", "PROTECT", MessageBoxButton.OKCancel, MessageBoxImage.Information);
        if (answer == MessageBoxResult.OK) RunElevatedPolicyCommand(command, $"protect {title}");
    }

    private static string? GetUnprotectCommand(ProtectionCheck check) => check.Title switch
    {
        "Device-associated app downloads" when PolicyManager.OwnsRecommendedPolicy() => "--restore-recommended",
        "Drivers in Windows Update" when StrictPolicyManager.OwnsSetting("Drivers") => "--unprotect-drivers",
        "Windows Update approval" when StrictPolicyManager.OwnsSetting("UpdateMode") => "--unprotect-updates",
        "Store app auto-updates" when StrictPolicyManager.OwnsSetting("StoreUpdates") => "--unprotect-store-updates",
        _ => null
    };

    private void Unprotect(string title, string command)
    {
        var answer = WpfMessageBox.Show($"Turn off Window Lock protection for ‘{title}’?\n\nWindows may resume automatic downloads or updates through this channel.", "UNPROTECT", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        if (answer == MessageBoxResult.OK) RunElevatedPolicyCommand(command, $"unprotect {title}");
    }

    private void RunElevatedPolicyCommand(string argument, string action)
    {
        try
        {
            var executable = Environment.ProcessPath ?? throw new InvalidOperationException("The application executable path is unavailable.");
            var process = Process.Start(new ProcessStartInfo(executable, argument) { UseShellExecute = true, Verb = "runas" });
            process?.WaitForExit();
            var result = process is null ? PolicyChangeResult.Failed : (PolicyChangeResult)process.ExitCode;
            var message = result switch
            {
                PolicyChangeResult.Success => $"Window Lock successfully completed the request to {action} and verified the resulting Windows policy value.",
                PolicyChangeResult.AlreadyInDesiredState => "Windows was already protected. Window Lock did not claim ownership or alter the existing setting.",
                PolicyChangeResult.NotOwned => "Window Lock does not own this setting, so it cannot unprotect it.",
                PolicyChangeResult.ExternallyConfigured => "The value is explicitly configured or changed by another administrator, organization, or tool. Window Lock left it untouched.",
                PolicyChangeResult.UnsupportedEdition => "Microsoft does not document this policy as supported on this Windows edition. Window Lock made no change.",
                PolicyChangeResult.NotAdministrator => "Administrator approval was not granted. No setting was changed.",
                PolicyChangeResult.VerificationFailed => "Windows did not retain the expected value. Window Lock attempted to restore the original setting.",
                _ => "The operation failed. Window Lock could not verify a safe completed change."
            };
            WpfMessageBox.Show(message, "Window Lock", MessageBoxButton.OK, result is PolicyChangeResult.Success or PolicyChangeResult.AlreadyInDesiredState ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            WpfMessageBox.Show("Administrator approval was cancelled. No setting was changed.", "Window Lock", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        finally { RunChecks(); }
    }

    public void AllowExit() => _allowExit = true;
}
