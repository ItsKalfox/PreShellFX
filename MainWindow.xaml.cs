using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PreShellFX
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer loadingDotTimer, appCheckTimer, timeoutTimer;
        private int dotCount = 0;
        private List<AppStatus> appStatuses = new List<AppStatus>();
        private const int TIMEOUT_SECONDS = 15;
        private readonly string configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PreShellFX",
            "startupApps.json");

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            try
            {
                EnsureConfigDirectoryExists();

                if (!LoadStartupConfig())
                {
                    Close();
                    return;
                }

                InitUI();
                StartAllTimers();
            }
            catch (Exception ex)
            {
                ShowFatalError($"Initialization failed: {ex.Message}");
            }
        }

        private void EnsureConfigDirectoryExists()
        {
            var configDir = Path.GetDirectoryName(configPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
        }

        private bool LoadStartupConfig()
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Config file not found at {configPath}");
                }

                string json = File.ReadAllText(configPath);
                var configApps = JsonSerializer.Deserialize<List<StartupApp>>(json)
                    ?? throw new InvalidDataException("Invalid JSON structure");

                appStatuses = configApps
                    .Where(app => app.HoldLoader)
                    .Select(app => new AppStatus
                    {
                        Name = app.Name,
                        Path = app.Path,
                        IsInitialized = false
                    })
                    .ToList();

                return appStatuses.Any();
            }
            catch (Exception ex)
            {
                ShowError($"Config Error", $"Failed to load config:\n{ex.Message}");
                return false;
            }
        }

        private void InitUI()
        {
            StatusList.ItemsSource = appStatuses;
        }

        private void StartAllTimers()
        {
            // Loading dots animation
            loadingDotTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(500),
                DispatcherPriority.Normal,
                (s, e) => UpdateLoadingText(),
                Dispatcher);

            // App status checker
            appCheckTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.Normal,
                (s, e) => CheckRunningProcesses(),
                Dispatcher);

            // Timeout timer
            timeoutTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(TIMEOUT_SECONDS),
                DispatcherPriority.Normal,
                (s, e) => HandleTimeout(),
                Dispatcher);

            loadingDotTimer.Start();
            appCheckTimer.Start();
            timeoutTimer.Start();
        }

        private void UpdateLoadingText()
        {
            dotCount = (dotCount + 1) % 4;
            LoadingText.Text = $"Loading System ({GetProgressPercent()}%){new string('.', dotCount)}";
        }

        private void CheckRunningProcesses()
        {
            bool updated = false;
            var processes = Process.GetProcesses();

            foreach (var app in appStatuses.Where(a => !a.IsInitialized))
            {
                try
                {
                    if (IsProcessRunning(processes, app))
                    {
                        app.IsInitialized = true;
                        updated = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error] Checking {app.Name}: {ex.Message}");
                }
            }

            if (updated)
            {
                Dispatcher.Invoke(() => StatusList.Items.Refresh());
            }

            if (appStatuses.All(a => a.IsInitialized))
            {
                Dispatcher.Invoke(() => GracefulExit());
            }
        }

        private bool IsProcessRunning(Process[] processes, AppStatus app)
        {
            return processes.Any(p =>
                p.ProcessName.Equals(app.Name, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(app.Path) &&
                 processes.Any(p =>
                 {
                     try
                     {
                         return p.MainModule?.FileName?.Equals(app.Path, StringComparison.OrdinalIgnoreCase) == true;
                     }
                     catch (Win32Exception) { return false; }
                 }));
        }

        private void HandleTimeout()
        {
            if (!appStatuses.All(a => a.IsInitialized))
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingText.Text = "Timeout - Closing...";
                    GracefulExit();
                });
            }
        }

        private async void GracefulExit()
        {
            loadingDotTimer?.Stop();
            appCheckTimer?.Stop();
            timeoutTimer?.Stop();

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(500);
            Close();
        }

        private int GetProgressPercent() =>
            (int)((double)appStatuses.Count(a => a.IsInitialized) / appStatuses.Count * 100);

        private void ShowError(string title, string message)
        {
            Dispatcher.Invoke(() =>
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        private void ShowFatalError(string message)
        {
            ShowError("Fatal Error", message);
            Dispatcher.Invoke(Close);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt))
            {
                GracefulExit();
            }
        }
    }

    public class AppStatus
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsInitialized { get; set; }
    }

    public class StartupApp
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool HoldLoader { get; set; } = true;
    }

    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isDone)
                return isDone ? " ✅ Done" : " ⌛ Initializing...";
            return "❓ Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
