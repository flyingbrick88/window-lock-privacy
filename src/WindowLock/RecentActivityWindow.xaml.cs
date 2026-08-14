using System.Windows;

namespace WindowLock;

public partial class RecentActivityWindow : Window
{
    public RecentActivityWindow()
    {
        InitializeComponent();
        var items = ActivityScanner.ScanRecent();
        ActivityGrid.ItemsSource = items;
        CountText.Text = items.Count == 0 ? "No dated installation records were found in this period." : $"{items.Count} dated installation record{(items.Count == 1 ? string.Empty : "s")} found.";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
