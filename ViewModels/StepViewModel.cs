using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WechatBot.ViewModels;

public enum StepStatus { Pending, Running, Success, Failed }

public partial class StepViewModel : ObservableObject
{
    private readonly ImageRecognition _imageRec;
    private readonly Action<string> _log;

    [ObservableProperty] private string _key = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private int _index;
    [ObservableProperty] private bool _isConfigured;
    [ObservableProperty] private StepStatus _status = StepStatus.Pending;

    public string IndexText => $"{Index + 1}";
    public string StatusIcon => Status switch
    {
        StepStatus.Success => "✔",
        StepStatus.Failed => "✖",
        StepStatus.Running => "⏳",
        _ => IsConfigured ? "●" : "○"
    };
    public string StatusText => Status switch
    {
        StepStatus.Success => "已完成",
        StepStatus.Failed => "失败",
        StepStatus.Running => "执行中...",
        _ => IsConfigured ? "已配置" : "未配置"
    };

    public StepViewModel(ImageRecognition imageRec, Action<string> log)
    {
        _imageRec = imageRec;
        _log = log;
    }

    public void RefreshConfigStatus()
    {
        IsConfigured = _imageRec.HasTemplate(Key);
        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusText));
    }

    [RelayCommand]
    private async Task TestRecognitionAsync()
    {
        Status = StepStatus.Running;
        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusText));

        try
        {
            var result = await _imageRec.FindWithRetry(Key, 3, 500);
            if (result.HasValue)
            {
                Status = StepStatus.Success;
                _log($"✅ 识别成功：{Name} → ({result.Value.X}, {result.Value.Y}), 置信度={result.Value.Confidence:F2}");
                Win32.SetCursorPos(result.Value.X, result.Value.Y);
            }
            else
            {
                Status = StepStatus.Failed;
                _log($"❌ 识别失败：{Name}（未在屏幕上找到匹配区域）");
            }
        }
        catch (Exception ex)
        {
            Status = StepStatus.Failed;
            _log($"❌ 识别异常：{Name} - {ex.Message}");
        }

        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusText));

        await Task.Delay(2000);
        Status = StepStatus.Pending;
        OnPropertyChanged(nameof(StatusIcon));
        OnPropertyChanged(nameof(StatusText));
    }
}
