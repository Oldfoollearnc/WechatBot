using System.IO;
using System.Text.Json;

namespace WechatBot;

// 运行设置
public class Settings
{
    public DelaySettings Delays { get; set; } = new();
    public ScrollSettings Scroll { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public ScheduleSettings Schedule { get; set; } = new();
    public RecognitionSettings Recognition { get; set; } = new();

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "猪猪工作室", "微信自动化", "settings.json");

    /// <summary>
    /// 加载设置，失败时返回默认值并通过 warn 回调通知调用方
    /// </summary>
    public static Settings Load(Action<string>? warn = null)
    {
        if (!File.Exists(SettingsPath)) return new Settings();
        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }
        catch (Exception ex)
        {
            warn?.Invoke($"配置文件加载失败，使用默认设置：{ex.Message}");
            return new Settings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}

public class DelaySettings
{
    public double Step { get; set; } = 1.5;
    public double Load { get; set; } = 3.0;
}

public class ScrollSettings
{
    public int Times { get; set; } = 5;
    public int Amount { get; set; } = 120;
}

public class RetrySettings
{
    public int MaxAttempts { get; set; } = 3;
    public double WaitBetween { get; set; } = 2.0;
}

public class ScheduleSettings
{
    public bool Enabled { get; set; } = false;
    public string Time { get; set; } = "10:00";
    public List<int> Days { get; set; } = [1, 2, 3, 4, 5, 6, 7];

    /// <summary>上次执行日期（yyyy-MM-dd），用于持久化防重复执行</summary>
    public string? LastRunDate { get; set; }
}

public class RecognitionSettings
{
    /// <summary>图像模板匹配阈值（0.0 ~ 1.0）</summary>
    public double MatchThreshold { get; set; } = 0.8;
}
