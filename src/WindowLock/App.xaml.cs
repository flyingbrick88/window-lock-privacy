using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace WindowLock;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _tray;
    private MainWindow? _window;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length == 1 && e.Args[0] is "--apply-recommended" or "--restore-recommended" or "--apply-strict" or "--restore-strict" or "--protect-drivers" or "--protect-updates" or "--protect-store-updates" or "--unprotect-drivers" or "--unprotect-updates" or "--unprotect-store-updates")
        {
            var result = e.Args[0] switch
            {
                "--apply-recommended" => PolicyManager.ApplyRecommended(),
                "--restore-recommended" => PolicyManager.RestoreRecommended(),
                "--apply-strict" => StrictPolicyManager.Apply(),
                "--restore-strict" => StrictPolicyManager.Restore(),
                "--protect-drivers" => StrictPolicyManager.ApplyOne("Drivers"),
                "--protect-updates" => StrictPolicyManager.ApplyOne("UpdateMode"),
                "--protect-store-updates" => StrictPolicyManager.ApplyOne("StoreUpdates"),
                "--unprotect-drivers" => StrictPolicyManager.RestoreOne("Drivers"),
                "--unprotect-updates" => StrictPolicyManager.RestoreOne("UpdateMode"),
                _ => StrictPolicyManager.RestoreOne("StoreUpdates")
            };
            Environment.Exit((int)result);
        }
        base.OnStartup(e);
        _window = new MainWindow();
        _window.HideRequested += (_, _) => HideToTray();

        using var bitmap = new Bitmap(GetResourceStream(new Uri("pack://application:,,,/Assets/window-lock-logo.png"))!.Stream);
        using var generatedIcon = Icon.FromHandle(bitmap.GetHicon());
        _tray = new Forms.NotifyIcon
        {
            Icon = (Icon)generatedIcon.Clone(),
            Text = "Window Lock — checking protection",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        _tray.DoubleClick += (_, _) => ShowWindow();
        _window.StatusChanged += (_, protectedState) =>
            _tray.Text = protectedState ? "Window Lock — All available fields protected" : "Window Lock — Protection needs attention";
        _window.Show();
    }

    private Forms.ContextMenuStrip BuildMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open Window Lock", null, (_, _) => ShowWindow());
        menu.Items.Add("Run protection check", null, (_, _) => { ShowWindow(); _window?.RunChecks(); });
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());
        return menu;
    }

    private void ShowWindow()
    {
        _window?.Show();
        if (_window is not null) { _window.WindowState = WindowState.Normal; _window.Activate(); }
    }

    private void HideToTray()
    {
        _window?.Hide();
        _tray?.ShowBalloonTip(2000, "Window Lock is still running", "Protection status remains available from the system tray.", Forms.ToolTipIcon.Info);
    }

    private void ExitApp()
    {
        _window?.AllowExit();
        if (_tray is not null) { _tray.Visible = false; _tray.Dispose(); }
        Shutdown();
    }

}
