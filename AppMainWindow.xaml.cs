using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WechatBot.Pages;
using WechatBot.ViewModels;

namespace WechatBot;

public partial class AppMainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly Settings _settings;
    private readonly ImageRecognition _imageRec;
    private TrayIcon? _trayIcon;
    private HwndSource? _hwndSource;
    private bool _forceClose;
    private DispatcherTimer? _scheduleTimer;

    // 页面实例（缓存）
    private HomePage? _homePage;
    private StepsPage? _stepsPage;
    private LogsPage? _logsPage;
    private SettingsPage? _settingsPage;

    private const int WM_COMMAND = 0x0111;

    public AppMainWindow()
    {
        // 先加载主题，再初始化 UI
        _settings = Settings.Load(warn => { });
        ThemeManager.ApplyTheme(_settings.Theme);

        InitializeComponent();

        _imageRec = new ImageRecognition();
        _viewModel = new MainViewModel(_settings, _imageRec);
        DataContext = _viewModel;

        // 初始化页面并默认显示首页
        _homePage = new HomePage(_viewModel);
        _stepsPage = new StepsPage(_viewModel);
        _logsPage = new LogsPage(_viewModel);
        _settingsPage = new SettingsPage(_viewModel);
        PageContent.Content = _homePage;

        CleanOldLogs();
        StartScheduleTimer();
        StartLogoBreathAnimation();

        // 窗口启动动画
        Opacity = 0;
        Loaded += (_, _) =>
        {
            InitTrayIcon();
            AnimateWindowStartup();
        };
        Closed += (_, _) =>
        {
            _scheduleTimer?.Stop();
            _trayIcon?.Dispose();
            _hwndSource?.RemoveHook(WndProc);
            _imageRec.Dispose();
        };
    }

    #region 窗口动画

    private void AnimateWindowStartup()
    {
        // 淡入动画
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // 缩放动画（从中心弹出）
        var scaleTransform = new ScaleTransform(0.95, 0.95, ActualWidth / 2, ActualHeight / 2);
        RenderTransform = scaleTransform;

        var scaleX = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1 }
        };
        var scaleY = new DoubleAnimation(0.95, 1, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 1 }
        };

        BeginAnimation(OpacityProperty, fadeIn);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
    }

    #endregion

    #region 窗口按钮

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            BtnMaximize_Click(sender, e);
        else
            DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void BtnClose_Click(object sender, RoutedEventArgs e)
        => Close();

    #endregion

    #region 导航 + 页面切换动画

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton rb || _homePage == null) return;
        UserControl? newPage = rb.Name switch
        {
            "NavHome" => _homePage,
            "NavSteps" => _stepsPage,
            "NavLogs" => _logsPage,
            "NavSettings" => _settingsPage,
            _ => _homePage
        };
        if (newPage == null || PageContent.Content == newPage) return;
        AnimatePageSwitch(newPage);
    }

    private void AnimatePageSwitch(UserControl newPage)
    {
        // 淡出旧页面
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
        fadeOut.Completed += (_, _) =>
        {
            PageContent.Content = newPage;
            // 淡入新页面
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var slideIn = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

            PageContent.BeginAnimation(OpacityProperty, fadeIn);
            var transform = PageContent.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                PageContent.RenderTransform = transform;
            }
            transform.BeginAnimation(TranslateTransform.YProperty, slideIn);
        };
        PageContent.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void StartLogoBreathAnimation()
    {
        var breathAnim = new DoubleAnimation(1, 0.6, TimeSpan.FromSeconds(1.5))
        {
            AutoReverse = true,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        LogoEmoji.BeginAnimation(OpacityProperty, breathAnim);
    }

    #endregion

    #region 系统托盘

    private void InitTrayIcon()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        _trayIcon = new TrayIcon(hwnd, "排序小助手 - 猪猪工作室");

        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
        _trayIcon.LoadIconFromFile(iconPath);

        _trayIcon.OnDoubleClick += RestoreFromTray;
        _trayIcon.OnShowClick += RestoreFromTray;
        _trayIcon.OnExitClick += () => { _forceClose = true; Close(); };

        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_trayIcon != null)
        {
            if (_trayIcon.HandleMessage((uint)msg, wParam, lParam))
            {
                handled = true;
                return IntPtr.Zero;
            }
            if (msg == WM_COMMAND && _trayIcon.HandleCommand((uint)wParam.ToInt32()))
            {
                handled = true;
                return IntPtr.Zero;
            }
        }
        return IntPtr.Zero;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_forceClose)
        {
            e.Cancel = true;
            MinimizeToTray();
            return;
        }
        base.OnClosing(e);
    }

    private void MinimizeToTray()
    {
        Hide();
        _trayIcon?.Show();
        _trayIcon?.ShowBalloon("排序小助手", "已最小化到系统托盘，双击恢复");
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        _trayIcon?.Hide();
    }

    #endregion

    #region 定时

    private void StartScheduleTimer()
    {
        _scheduleTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _scheduleTimer.Tick += (_, _) => CheckSchedule();
        _scheduleTimer.Start();
    }

    private void CheckSchedule()
    {
        if (!_settings.Schedule.Enabled) return;
        try
        {
            var now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");
            int todayDayOfWeek = (int)now.DayOfWeek;
            if (todayDayOfWeek == 0) todayDayOfWeek = 7;

            if (!_settings.Schedule.Days.Contains(todayDayOfWeek)) return;
            if (_settings.Schedule.LastRunDate == todayStr) return;

            var timeParts = _settings.Schedule.Time.Split(':');
            if (timeParts.Length != 2) return;

            int targetHour = int.Parse(timeParts[0]);
            int targetMinute = int.Parse(timeParts[1]);

            if (now.Hour == targetHour && now.Minute == targetMinute)
            {
                var stepKeys = AutomationEngine.Steps.Select(s => s.Key).ToArray();
                if (!ImageRecognition.AllTemplatesExist(stepKeys))
                {
                    _viewModel.AddLog("⏰ 定时任务触发但模板未全部配置，跳过");
                    _settings.Schedule.LastRunDate = todayStr;
                    _settings.Save();
                    return;
                }

                _settings.Schedule.LastRunDate = todayStr;
                _settings.Save();
                _viewModel.AddLog("⏰ 定时任务触发");
                _ = _viewModel.RunCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _viewModel.AddLog($"⏰ 定时检查异常：{ex.Message}");
        }
    }

    #endregion

    #region 日志清理

    private static void CleanOldLogs()
    {
        try
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir)) return;
            var cutoff = DateTime.Now.AddDays(-7);
            foreach (var file in Directory.GetFiles(logDir, "*.log"))
            {
                if (File.GetLastWriteTime(file) < cutoff)
                {
                    try { File.Delete(file); }
                    catch { /* 文件被占用，忽略 */ }
                }
            }
        }
        catch { /* 日志清理失败不应影响主流程 */ }
    }

    #endregion
}
