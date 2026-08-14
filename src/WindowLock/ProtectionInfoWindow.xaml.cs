using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;

namespace WindowLock;

public partial class ProtectionInfoWindow : Window
{
    private readonly string _documentationUri;

    public ProtectionInfoWindow(ProtectionCheck check)
    {
        InitializeComponent();
        _documentationUri = check.DocumentationUri;
        Title = $"{check.Title} — Window Lock";
        TitleText.Text = check.Title;
        SummaryText.Text = check.Description;
        ProtectedText.Text = check.ProtectedMeaning;
        UnprotectedText.Text = check.UnprotectedMeaning;
        GuidanceText.Text = check.Guidance;
        EvidenceText.Text = check.Evidence;
        SourceText.Text = check.PolicySource;
        ApplicabilityText.Text = check.Applicability;

        var protectedState = check.State == CheckState.Protected;
        var unavailable = check.State == CheckState.Unavailable;
        var unknown = check.State == CheckState.Unknown;
        var caution = unknown || unavailable;
        CurrentBadgeText.Text = unavailable ? "UNAVAILABLE ON THIS EDITION" : unknown ? "CURRENTLY UNKNOWN" : protectedState ? "CURRENTLY PROTECTED" : "CURRENTLY UNPROTECTED";
        CurrentBadge.Background = Brush(caution ? "#FFF8E1" : protectedState ? "#E6FCF5" : "#FFF0F0");
        CurrentBadgeText.Foreground = Brush(caution ? "#B26A00" : protectedState ? "#087F5B" : "#C92A2A");
    }

    private static SolidColorBrush Brush(string value) => new((WpfColor)WpfColorConverter.ConvertFromString(value));
    private void Documentation_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(_documentationUri) { UseShellExecute = true });
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
