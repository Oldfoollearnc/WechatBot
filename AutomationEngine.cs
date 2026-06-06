using System.IO;

namespace WechatBot;

public class AutomationEngine : IDisposable
{
    // 步骤定义：Key, 名称, 描述, 延时类型, 是否在点击后滚动
    public static readonly (string Key, string Name, string Desc, string DelayType, bool ScrollAfter)[] Steps = [
        ("小程序面板",   "小程序面板",     "微信左侧的小程序面板入口",     "step", false),
        ("排序小助手",   "排序小助手",     "最近使用中的「排序小助手」",   "load", false),
        ("跳过广告",     "跳过广告",       "广告页面的「跳过」按钮",       "step", false),
        ("我的",         "我的",           "右下角的「我的」",             "step", false),
        ("创建的排序",   "创建的排序",     "我的页面中的「创建的排序」",   "load", false),
        ("第一个抽奖",   "第一个抽奖",     "列表中第一个抽奖项目",         "step", false),
        ("排序管理",     "排序管理",       "「排序管理」按钮",             "step", false),
        ("复制排序",     "复制排序",       "「复制排序」按钮",             "step", true),   // 点击后需要滚动
        ("确定1",        "确定(第一次)",   "第一个「确定」按钮",           "step", false),
        ("确定2",        "确定(第二次)",   "第二个「确定」按钮",           "step", false),
        ("分享",         "分享",           "「分享」按钮",                 "step", false),
        ("分享好友",     "分享好友",       "「分享好友」选项",             "step", false),
        ("目标群",       "目标群",         "要发送的目标群",               "step", false),
        ("发送",         "发送",           "「发送」按钮",                 "step", false),
    ];

    private readonly Settings _settings;
    private readonly ImageRecognition _imageRec;
    private readonly string _logDir;
    private CancellationTokenSource? _cts;

    public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

    public event Action<string>? OnLog;
    public event Action<string, int, int>? OnProgress;
    public event Action<bool>? OnComplete;

    public AutomationEngine(Settings settings, ImageRecognition imageRec)
    {
        _settings = settings;
        _imageRec = imageRec;
        _logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "猪猪工作室", "微信自动化", "logs");
        Directory.CreateDirectory(_logDir);
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    public List<string> GetMissingTemplates()
    {
        return Steps
            .Where(s => !_imageRec.HasTemplate(s.Key))
            .Select(s => s.Name)
            .ToList();
    }

    // 小程序内的步骤：点击后不应激活主窗口，否则会盖住小程序窗口
    private static readonly HashSet<string> MINI_PROGRAM_STEPS = new()
    {
        "排序小助手", "跳过广告", "我的", "创建的排序", "第一个抽奖",
        "排序管理", "复制排序", "确定1", "确定2", "分享", "分享好友"
    };

    public async Task RunAsync(bool isTest = false, CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = _cts.Token;

        var logFile = Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}.log");

        void Log(string level, string msg)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {msg}";
            try { File.AppendAllText(logFile, line + "\n"); }
            catch { /* 日志写入失败不应影响主流程 */ }
            OnLog?.Invoke(msg);
        }

        Log("INFO", $"🐷 ===== 开始执行 ===== 模式={(isTest ? "测试" : "正式")}");

        try
        {
            int stepMs = (int)(_settings.Delays.Step * 1000);
            int loadMs = (int)(_settings.Delays.Load * 1000);
            bool inMiniProgram = false;

            for (int i = 0; i < Steps.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var (key, name, desc, delayType, scrollAfter) = Steps[i];
                int delay = delayType == "load" ? loadMs : stepMs;

                Log("INFO", $"[{i + 1}/{Steps.Length}] 查找 {name}");
                OnProgress?.Invoke(name, i + 1, Steps.Length);

                // 确保微信在前台：未运行则终止，未激活则拉起
                if (!Win32.IsWeChatRunning())
                {
                    Log("ERROR", "微信未启动，请先打开微信");
                    throw new OperationCanceledException("微信未启动");
                }

                // 小程序模式下不激活主窗口，否则会盖住小程序窗口
                if (!inMiniProgram)
                {
                    if (!Win32.ActivateWeChat())
                        Log("WARN", "未能激活微信窗口，继续尝试…");
                    await Task.Delay(300, token);
                }

                // 统一重试策略：直接用 Find，外层控制重试次数
                bool success = false;
                for (int retry = 0; retry < _settings.Retry.MaxAttempts; retry++)
                {
                    token.ThrowIfCancellationRequested();

                    var result = _imageRec.Find(key);
                    if (result.HasValue)
                    {
                        Log("INFO", $"找到 {name} 于 ({result.Value.X}, {result.Value.Y}), 置信度={result.Value.Confidence:F2}");
                        await Win32.ClickAsync(result.Value.X, result.Value.Y, token);
                        inMiniProgram = MINI_PROGRAM_STEPS.Contains(key);
                        success = true;
                        break;
                    }

                    Log("WARN", $"未找到 {name}，重试 {retry + 1}/{_settings.Retry.MaxAttempts}");
                    if (retry < _settings.Retry.MaxAttempts - 1)
                        await Task.Delay((int)(_settings.Retry.WaitBetween * 1000), token);
                }

                if (!success)
                {
                    Log("ERROR", $"步骤 [{name}] 失败，终止执行");
                    throw new OperationCanceledException($"步骤 [{name}] 找不到对应按钮");
                }

                // 等待页面响应
                await Task.Delay(delay, token);

                // 点击后需要滚动
                if (scrollAfter)
                {
                    Log("INFO", $"向下滚动 {_settings.Scroll.Times} 次");
                    for (int s = 0; s < _settings.Scroll.Times; s++)
                    {
                        Win32.Scroll(-_settings.Scroll.Amount);
                        await Task.Delay(300, token);
                    }
                    await Task.Delay(1000, token);
                }
            }

            Log("INFO", "🐷 ===== 执行完成 =====");
            Win32.Beep();

            // 执行成功后关闭微信窗口
            if (Win32.CloseWeChat())
                Log("INFO", "已最小化微信窗口");
            else
                Log("WARN", "未能最小化微信窗口");

            OnComplete?.Invoke(true);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            Log("WARN", "用户中断执行");
            OnComplete?.Invoke(false);
        }
        catch (Exception ex)
        {
            Log("ERROR", $"执行异常：{ex.Message}");
            OnComplete?.Invoke(false);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Dispose() { /* ImageRecognition 由调用方管理，此处无需释放 */ }
}
