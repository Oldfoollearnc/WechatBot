using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WechatBot.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Settings _settings;
    private readonly ImageRecognition _imageRec;
    private AutomationEngine? _engine;

    // --- 全局状态 ---
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _statusText = "就绪";
    [ObservableProperty] private string _statusIcon = "●";
    [ObservableProperty] private int _configuredCount;
    [ObservableProperty] private int _totalCount = AutomationEngine.Steps.Length;
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _currentStepName = "";
    [ObservableProperty] private int _currentStepIndex;
    [ObservableProperty] private string _lastRunTime = "从未执行";

    // --- 集合 ---
    public ObservableCollection<StepViewModel> Steps { get; } = [];
    public ObservableCollection<LogEntry> Logs { get; } = [];

    // --- 设置绑定 ---
    [ObservableProperty] private double _stepDelay = 1.5;
    [ObservableProperty] private double _loadDelay = 3.0;
    [ObservableProperty] private int _maxRetry = 3;
    [ObservableProperty] private double _retryWait = 2.0;
    [ObservableProperty] private double _matchThreshold = 0.8;
    [ObservableProperty] private string _theme = "Dark";
    [ObservableProperty] private bool _scheduleEnabled;
    [ObservableProperty] private int _scheduleHour = 10;
    [ObservableProperty] private int _scheduleMinute;
    [ObservableProperty] private ObservableCollection<bool> _scheduleDays = [true, true, true, true, true, true, true];

    public MainViewModel(Settings settings, ImageRecognition imageRec)
    {
        _settings = settings;
        _imageRec = imageRec;

        // 初始化步骤列表
        for (int i = 0; i < AutomationEngine.Steps.Length; i++)
        {
            var (key, name, desc, _, _) = AutomationEngine.Steps[i];
            var vm = new StepViewModel(_imageRec, AddLog)
            {
                Key = key, Name = name, Description = desc, Index = i
            };
            vm.RefreshConfigStatus();
            Steps.Add(vm);
        }

        LoadSettings();
        RefreshConfiguredCount();
        AddLog("🐷 排序小助手已启动");
    }

    // --- 日志 ---
    public void AddLog(string msg)
    {
        var level = msg.Contains("❌") || msg.Contains("ERROR") ? LogLevel.Error
            : msg.Contains("⚠") || msg.Contains("WARN") ? LogLevel.Warn
            : LogLevel.Info;

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Logs.Add(new LogEntry { Level = level, Message = msg });
            if (Logs.Count > 500) Logs.RemoveAt(0);
        });
    }

    // --- 配置进度 ---
    public void RefreshConfiguredCount()
    {
        ConfiguredCount = Steps.Count(s => s.IsConfigured);
        ProgressPercent = TotalCount > 0 ? (double)ConfiguredCount / TotalCount * 100 : 0;
        OnPropertyChanged(nameof(ConfiguredCount));
    }

    // --- 运行 ---
    [RelayCommand]
    private async Task RunAsync()
    {
        await RunAutomationAsync(false);
    }

    [RelayCommand]
    private async Task TestAsync()
    {
        await RunAutomationAsync(true);
    }

    [RelayCommand]
    private void Stop()
    {
        if (_engine is { IsRunning: true })
        {
            _engine.Stop();
            StatusText = "正在停止...";
            StatusIcon = "◌";
        }
    }

    private async Task RunAutomationAsync(bool isTest)
    {
        if (IsRunning)
        {
            AddLog("⚠️ 已有任务正在执行");
            return;
        }

        var missing = Steps.Where(s => !s.IsConfigured).ToList();
        if (missing.Count > 0)
        {
            AddLog($"⚠️ 还有 {missing.Count} 个步骤未配置：{string.Join(", ", missing.Select(s => s.Name))}");
            return;
        }

        IsRunning = true;
        StatusText = "正在执行...";
        StatusIcon = "⏳";

        // 重置步骤状态
        foreach (var step in Steps)
        {
            step.Status = StepStatus.Pending;
        }

        _engine = new AutomationEngine(_settings, _imageRec);
        _engine.OnLog += AddLog;
        _engine.OnProgress += (name, cur, total) =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                CurrentStepName = name;
                CurrentStepIndex = cur;
                StatusText = $"正在执行：{name} ({cur}/{total})";

                // 更新对应步骤状态
                if (cur - 1 < Steps.Count)
                {
                    for (int i = 0; i < cur - 1; i++)
                        Steps[i].Status = StepStatus.Success;
                    Steps[cur - 1].Status = StepStatus.Running;
                }
            });
        };
        _engine.OnComplete += success =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                IsRunning = false;
                if (success)
                {
                    StatusText = "✅ 执行完成";
                    StatusIcon = "✅";
                    foreach (var step in Steps) step.Status = StepStatus.Success;
                    LastRunTime = DateTime.Now.ToString("MM-dd HH:mm");
                }
                else
                {
                    StatusText = "❌ 执行失败";
                    StatusIcon = "❌";
                }
            });
        };

        _settings.Save();
        await _engine.RunAsync(isTest);
        _engine.Dispose();
        _engine = null;
    }

    // --- 截取模板 ---
    public async Task CaptureTemplateAsync(StepViewModel step)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow;
        mainWindow.WindowState = System.Windows.WindowState.Minimized;

        await Task.Delay(300);

        var selector = new RegionSelector();
        selector.ShowDialog();

        mainWindow.WindowState = System.Windows.WindowState.Normal;
        mainWindow.Activate();

        if (selector.IsConfirmed)
        {
            _imageRec.SaveTemplate(step.Key, selector.SelectedX, selector.SelectedY,
                selector.SelectedWidth, selector.SelectedHeight);

            step.RefreshConfigStatus();
            RefreshConfiguredCount();
            AddLog($"✅ 已保存模板：{step.Name} ({selector.SelectedWidth}x{selector.SelectedHeight})");
        }
    }

    // --- 重置模板 ---
    public void ResetTemplate(StepViewModel step)
    {
        _imageRec.DeleteTemplate(step.Key);
        step.RefreshConfigStatus();
        RefreshConfiguredCount();
        AddLog($"🗑 已重置模板：{step.Name}");
    }

    // --- 设置 ---
    private void LoadSettings()
    {
        StepDelay = _settings.Delays.Step;
        LoadDelay = _settings.Delays.Load;
        MaxRetry = _settings.Retry.MaxAttempts;
        RetryWait = _settings.Retry.WaitBetween;
        MatchThreshold = _settings.Recognition.MatchThreshold;
        Theme = _settings.Theme;
        ScheduleEnabled = _settings.Schedule.Enabled;

        if (_settings.Schedule.Time.Split(':') is [var h, var m]
            && int.TryParse(h, out int hour) && int.TryParse(m, out int minute))
        {
            ScheduleHour = hour;
            ScheduleMinute = minute;
        }

        ScheduleDays = new ObservableCollection<bool>(
            Enumerable.Range(1, 7).Select(d => _settings.Schedule.Days.Contains(d)));
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settings.Delays.Step = StepDelay;
        _settings.Delays.Load = LoadDelay;
        _settings.Retry.MaxAttempts = MaxRetry;
        _settings.Retry.WaitBetween = RetryWait;
        _settings.Recognition.MatchThreshold = MatchThreshold;
        _settings.Theme = Theme;
        _settings.Schedule.Enabled = ScheduleEnabled;
        _settings.Schedule.Time = $"{ScheduleHour:00}:{ScheduleMinute:00}";
        _settings.Schedule.Days = ScheduleDays
            .Select((selected, index) => selected ? index + 1 : 0)
            .Where(d => d > 0)
            .ToList();

        _imageRec.MatchThreshold = MatchThreshold;
        _settings.Save();
        AddLog("⚙️ 设置已保存");
    }
}
