using System.IO;
using OpenCvSharp;

namespace WechatBot;

public class ImageRecognition : IDisposable
{
    private readonly string _templateDir;
    private readonly Dictionary<string, Mat> _templates = [];

    public double MatchThreshold { get; set; } = 0.8;

    public ImageRecognition()
    {
        _templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
        Directory.CreateDirectory(_templateDir);
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        _templates.Clear();
        foreach (var file in Directory.GetFiles(_templateDir, "*.png"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var mat = Cv2.ImRead(file, ImreadModes.Color);
            if (!mat.Empty()) _templates[name] = mat;
        }
    }

    public bool HasTemplate(string name) => _templates.ContainsKey(name);
    public int TemplateCount => _templates.Count;

    // 静态方法：检查所有步骤的模板是否存在（无需创建实例）
    public static bool AllTemplatesExist(string[] keys)
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
        if (!Directory.Exists(dir)) return false;
        foreach (var key in keys)
        {
            if (!File.Exists(Path.Combine(dir, $"{key}.png")))
                return false;
        }
        return true;
    }

    // 截取屏幕（带 GDI 资源安全释放）
    public Mat CaptureScreen()
    {
        int w = Win32.GetSystemMetrics(0);
        int h = Win32.GetSystemMetrics(1);
        IntPtr hdc = IntPtr.Zero;
        IntPtr memDc = IntPtr.Zero;
        IntPtr hBmp = IntPtr.Zero;

        try
        {
            hdc = Win32.GetDC(IntPtr.Zero);
            memDc = Win32.CreateCompatibleDC(hdc);
            hBmp = Win32.CreateCompatibleBitmap(hdc, w, h);
            var old = Win32.SelectObject(memDc, hBmp);
            Win32.BitBlt(memDc, 0, 0, w, h, hdc, 0, 0, Win32.SRCCOPY);
            Win32.SelectObject(memDc, old);

            using var bmp = System.Drawing.Image.FromHbitmap(hBmp);
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
        }
        finally
        {
            if (hBmp != IntPtr.Zero) Win32.DeleteObject(hBmp);
            if (memDc != IntPtr.Zero) Win32.DeleteDC(memDc);
            if (hdc != IntPtr.Zero) Win32.ReleaseDC(IntPtr.Zero, hdc);
        }
    }

    // 保存模板
    public void SaveTemplate(string name, int x, int y, int width, int height)
    {
        using var screen = CaptureScreen();
        x = Math.Max(0, x);
        y = Math.Max(0, y);
        width = Math.Min(width, screen.Width - x);
        height = Math.Min(height, screen.Height - y);

        using var cropped = new Mat(screen, new Rect(x, y, width, height));
        var path = Path.Combine(_templateDir, $"{name}.png");
        Cv2.ImWrite(path, cropped);

        if (_templates.ContainsKey(name)) _templates[name].Dispose();
        _templates[name] = cropped.Clone();
    }

    // 删除模板
    public void DeleteTemplate(string name)
    {
        var path = Path.Combine(_templateDir, $"{name}.png");
        if (File.Exists(path)) File.Delete(path);
        if (_templates.Remove(name, out var mat)) mat.Dispose();
    }

    // 查找模板
    public (int X, int Y, double Confidence)? Find(string name)
    {
        if (!_templates.TryGetValue(name, out var template)) return null;
        using var screen = CaptureScreen();
        return FindInImage(screen, template);
    }

    private (int X, int Y, double Confidence)? FindInImage(Mat source, Mat template)
    {
        if (source.Empty() || template.Empty()) return null;
        using var result = new Mat();
        Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
        Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out var maxLoc);
        if (maxVal >= MatchThreshold)
            return (maxLoc.X + template.Width / 2, maxLoc.Y + template.Height / 2, maxVal);
        return null;
    }

    // 带重试查找
    public async Task<(int X, int Y, double Confidence)?> FindWithRetry(string name, int maxRetry = 3, int delayMs = 500)
    {
        for (int i = 0; i < maxRetry; i++)
        {
            var result = Find(name);
            if (result.HasValue) return result;
            if (i < maxRetry - 1) await Task.Delay(delayMs);
        }
        return null;
    }

    // 等待元素出现
    public async Task<(int X, int Y, double Confidence)?> WaitFor(string name, int timeoutMs = 10000, int intervalMs = 500)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            var result = Find(name);
            if (result.HasValue) return result;
            await Task.Delay(intervalMs);
        }
        return null;
    }

    public void Dispose()
    {
        foreach (var mat in _templates.Values) mat.Dispose();
        _templates.Clear();
    }
}
