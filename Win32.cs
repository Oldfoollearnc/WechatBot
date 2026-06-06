using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WechatBot;

public static class Win32
{
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);
    [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] public static extern int GetSystemMetrics(int nIndex);
    [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);
    [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
    [DllImport("gdi32.dll")] public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);
    [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr hObject);
    [DllImport("user32.dll")] public static extern bool MessageBeep(uint uType);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    // DPI 感知相关
    [DllImport("user32.dll")] public static extern uint GetDpiForSystem();
    [DllImport("user32.dll")] public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
    [DllImport("user32.dll")] public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const uint MOUSEEVENTF_LEFTUP = 0x0004;
    public const uint MOUSEEVENTF_WHEEL = 0x0800;
    public const int SW_RESTORE = 9;
    public const int SW_MINIMIZE = 6;
    public const int VK_F9 = 0x78;
    public const uint SRCCOPY = 0x00CC0020;
    public const uint MB_OK = 0x00000000;
    public const uint MB_ICONINFORMATION = 0x00000040;

    /// <summary>
    /// 查找微信窗口句柄。优先按窗口标题"微信"查找，兼容新旧版本进程名。
    /// </summary>
    public static IntPtr FindWeChatWindow()
    {
        // 新版微信（Weixin）和旧版微信（WeChat）窗口标题都是"微信"
        var hWnd = FindWindow(null, "微信");
        if (hWnd != IntPtr.Zero && IsWindow(hWnd)) return hWnd;
        return IntPtr.Zero;
    }

    public static bool IsWeChatRunning()
        => FindWeChatWindow() != IntPtr.Zero;

    public static bool ActivateWeChat()
    {
        var hWnd = FindWeChatWindow();
        if (hWnd == IntPtr.Zero) return false;
        ShowWindow(hWnd, SW_RESTORE);
        SetForegroundWindow(hWnd);
        return true;
    }

    /// <summary>
    /// 最小化所有微信窗口（不退出微信进程）
    /// </summary>
    public static bool CloseWeChat()
    {
        var windows = new List<IntPtr>();
        EnumWindows((hWnd, _) =>
        {
            if (!IsWindow(hWnd)) return true;
            var sb = new System.Text.StringBuilder(256);
            GetWindowText(hWnd, sb, 256);
            if (sb.ToString() == "微信") windows.Add(hWnd);
            return true;
        }, IntPtr.Zero);

        foreach (var hWnd in windows)
            ShowWindow(hWnd, SW_MINIMIZE);

        return windows.Count > 0;
    }

    /// <summary>
    /// 获取虚拟屏幕的物理像素尺寸（覆盖所有显示器，考虑 DPI）
    /// </summary>
    public static (int Width, int Height) GetVirtualScreenPhysicalSize()
    {
        // SM_CXVIRTUALSCREEN / SM_CYVIRTUALSCREEN 返回逻辑像素
        int w = GetSystemMetrics(78); // SM_CXVIRTUALSCREEN
        int h = GetSystemMetrics(79); // SM_CYVIRTUALSCREEN

        // 如果系统 DPI > 96，需要缩放
        uint dpi = GetDpiForSystem();
        if (dpi > 96)
        {
            w = (int)(w * dpi / 96.0);
            h = (int)(h * dpi / 96.0);
        }
        return (w, h);
    }

    /// <summary>
    /// 异步模拟鼠标点击（带短暂延时确保可靠性）
    /// </summary>
    public static async Task ClickAsync(int x, int y, CancellationToken ct = default)
    {
        SetCursorPos(x, y);
        await Task.Delay(50, ct);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        await Task.Delay(50, ct);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
    }

    /// <summary>
    /// 同步版本点击（保留兼容性）
    /// </summary>
    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
    }

    public static void Scroll(int amount) => mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)amount), IntPtr.Zero);
    public static bool IsKeyPressed(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;
    public static void Beep() => MessageBeep(MB_OK | MB_ICONINFORMATION);

    /// <summary>
    /// 获取当前前台窗口标题（调试用）
    /// </summary>
    public static string GetForegroundWindowTitle()
    {
        var hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return "(无)";
        var sb = new System.Text.StringBuilder(256);
        GetWindowText(hWnd, sb, 256);
        return sb.ToString();
    }
}
