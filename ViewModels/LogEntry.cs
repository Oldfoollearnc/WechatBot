using System.Windows.Media;

namespace WechatBot.ViewModels;

public enum LogLevel { Info, Warn, Error }

public partial class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = "";

    public string TimeText => Timestamp.ToString("HH:mm:ss");
    public string LevelText => Level switch
    {
        LogLevel.Info => "INFO",
        LogLevel.Warn => "WARN",
        LogLevel.Error => "ERROR",
        _ => ""
    };
    public string Icon => Level switch
    {
        LogLevel.Info => "✔",
        LogLevel.Warn => "⚠",
        LogLevel.Error => "✖",
        _ => ""
    };
    public Brush LevelColor => Level switch
    {
        LogLevel.Info => Brushes.LimeGreen,
        LogLevel.Warn => Brushes.Gold,
        LogLevel.Error => Brushes.Tomato,
        _ => Brushes.Gray
    };
}
